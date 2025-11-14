using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Enums;
using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.Infrastructure.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HangfireJobsSys.Infrastructure.Services
{
    /// <summary>
    /// 性能监控服务实现（基于数据库存储）
    /// </summary>
    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly HangfireJobsSysDbContext _dbContext;
        private readonly PerformanceMonitoringOptions _options;
        

        public PerformanceMonitoringService(HangfireJobsSysDbContext dbContext,
                                            ILogger<PerformanceMonitoringService> logger,
                                            PerformanceMonitoringOptions options)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            _logger.LogInformation("性能监控服务初始化完成，数据保留天数: {RetentionDays}, 历史迁移阈值: {MigrationThresholdDays}", 
                _options.DataRetentionDays, _options.HistoryMigrationThresholdDays);
            
            // 配置定期清理和迁移作业（使用每天凌晨2点的默认Cron表达式）
            RecurringJob.AddOrUpdate("CleanupPerformanceData",
                () => CleanupAndMigrateOldDataAsync(),
                "0 2 * * *");
            
            // 注册定期清理任务 - 已通过Hangfire实现
        _logger.LogInformation("性能监控数据清理任务已注册，Cron表达式: {Cron}", "0 2 * * *");
        }
        
        /// <summary>
        /// 注册定期清理任务
        /// </summary>
        private void RegisterPeriodicCleanupTask()
        {
            try
            {
                // 使用Hangfire注册定期清理任务
                RecurringJob.AddOrUpdate(
                    "performance-data-cleanup",
                    () => PerformPeriodicCleanupAsync(),
                    _options.CleanupCronExpression,
                    TimeZoneInfo.Utc);
                
                _logger.LogInformation("Registered periodic performance data cleanup task with cron expression: {CronExpression}", _options.CleanupCronExpression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register periodic cleanup task");
            }
        }
        
        /// <summary>
        /// 执行定期清理
        /// </summary>
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task PerformPeriodicCleanupAsync()
        {
            try
            {
                // 调用迁移和清理方法
                await CleanupAndMigrateOldDataAsync();
                
                _logger.LogInformation("Periodic cleanup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic cleanup of performance data");
                throw; // 重新抛出异常以便Hangfire重试
            }
        }

        public async Task RecordJobPerformanceAsync(Guid jobId, DateTime startTime, DateTime endTime, ExecutionStatus status, IDictionary<string, object> metrics = null)
        {
            try
            {
                var executionTimeMs = (endTime - startTime).TotalMilliseconds;
                
                // 获取作业名称（这里简化处理，实际应该从作业存储中获取）
                var jobName = metrics != null && metrics.TryGetValue("JobName", out var jobNameValue) && jobNameValue != null ? jobNameValue.ToString()! : $"Job-{jobId}";
                var jobType = metrics != null && metrics.TryGetValue("JobType", out var jobTypeValue) && jobTypeValue != null ? jobTypeValue.ToString()! : $"Type-{jobId.GetHashCode() % 100}";
                
                // 创建性能数据对象 - 使用实体类
                var performanceData = new Domain.Entities.JobPerformanceData
                {
                    JobId = jobId,
                    JobName = jobName,
                    JobType = jobType,
                    ExecutionTimeMs = executionTimeMs,
                    Status = status,
                    Timestamp = DateTime.UtcNow,
                    CustomMetrics = metrics != null ? System.Text.Json.JsonSerializer.Serialize(metrics) : null
                };
                
                // 保存到数据库
                await _dbContext.JobPerformanceData.AddAsync(performanceData);
                await _dbContext.SaveChangesAsync();
                
                // 检查是否需要触发告警
                await CheckAlertThresholdsAsync(jobId, performanceData);
                
                _logger.LogDebug("任务性能数据记录成功: JobId={JobId}, 执行时间={ExecutionTime}ms", jobId, executionTimeMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录任务性能数据时出错");
                // 不抛出异常，避免影响主流程
            }
        }

        public async Task<JobPerformanceStatistics> GetJobPerformanceStatisticsAsync(Guid? jobId = null,
            TimeSpan? timeRange = null)
        {
            var endTime = DateTime.UtcNow;
            var startTime = timeRange.HasValue ? endTime - timeRange.Value : endTime.AddDays(-1);
            
            // 限制统计数据的最大样本数，避免处理过多数据导致性能问题
            List<Domain.Entities.JobPerformanceData> filteredData;
            
            // 使用数据库查询代替内存存储
            var query = _dbContext.JobPerformanceData
                .Where(d => d.Timestamp >= startTime && d.Timestamp <= endTime)
                .Where(d => !jobId.HasValue || d.JobId == jobId.Value);
            
            // 对于大型数据集，只取最近的N条记录进行统计
            // 使用BatchSize作为最大样本数
            if (await query.CountAsync() > _options.BatchSize)
            {
                filteredData = await query
                    .OrderByDescending(d => d.Timestamp)
                    .Take(_options.BatchSize)
                    .ToListAsync();
            }
            else
            {
                filteredData = await query.ToListAsync();
            }
            
            var stats = new JobPerformanceStatistics
            {
                PeriodStart = startTime,
                PeriodEnd = endTime,
                TotalJobsExecuted = filteredData.Count,
                SuccessfulJobs = filteredData.Count(d => d.Status == ExecutionStatus.Succeeded),
                FailedJobs = filteredData.Count(d => d.Status == ExecutionStatus.Failed),
                TimedOutJobs = filteredData.Count(d => d.Status == ExecutionStatus.Canceled),
                JobTypeStats = new Dictionary<string, JobTypeStatistics>()
            };
            
            if (filteredData.Any())
            {
                // 计算执行时间统计
                var executionTimes = filteredData.Select(d => d.ExecutionTimeMs).ToList();
                stats.AverageExecutionTimeMs = executionTimes.Average();
                stats.MinExecutionTimeMs = executionTimes.Min();
                stats.MaxExecutionTimeMs = executionTimes.Max();
                
                // 计算中位数
                var sortedTimes = executionTimes.OrderBy(t => t).ToList();
                var midIndex = sortedTimes.Count / 2;
                stats.MedianExecutionTimeMs = sortedTimes.Count % 2 == 0
                    ? (sortedTimes[midIndex - 1] + sortedTimes[midIndex]) / 2
                    : sortedTimes[midIndex];
                
                // 按任务类型分组统计
                foreach (var group in filteredData.GroupBy(d => d.JobType))
                {
                    stats.JobTypeStats[group.Key] = new JobTypeStatistics
                    {
                        JobType = group.Key,
                        ExecutionCount = group.Count(),
                        AverageExecutionTimeMs = group.Average(d => d.ExecutionTimeMs),
                        SuccessRate = (double)group.Count(d => d.Status == ExecutionStatus.Succeeded) / group.Count() * 100
                    };
                }
            }
            
            return stats;
        }

        public Task<SystemLoadMetrics> GetSystemLoadMetricsAsync()
        {
            try
            {
                // 获取CPU和内存使用情况
                using var process = Process.GetCurrentProcess();
                process.Refresh();
                
                // 计算内存使用百分比（简化版，实际项目可能需要更复杂的计算）
                var memoryUsage = process.WorkingSet64 / (1024.0 * 1024.0); // MB
                
                // 简化的系统负载指标
                var activeJobsCount = 0; // 实际项目中应从Hangfire获取
                var queuedJobsCount = 0; // 实际项目中应从Hangfire获取
                var scheduledJobsCount = 0; // 实际项目中应从Hangfire获取
                
                var metrics = new SystemLoadMetrics
                {
                    ActiveJobs = activeJobsCount,
                    QueuedJobs = queuedJobsCount,
                    ScheduledJobs = scheduledJobsCount,
                    CpuUsage = GetCurrentCpuUsage(), // 简化实现
                    MemoryUsage = memoryUsage,
                    WorkerCount = GetWorkerCount(), // 简化实现
                    AvailableWorkers = GetAvailableWorkerCount(), // 简化实现
                    Timestamp = DateTime.UtcNow
                };
                
                _logger.LogDebug("System load metrics: CPU={Cpu}%, Memory={Memory}MB, ActiveJobs={ActiveJobs}",
                    metrics.CpuUsage, metrics.MemoryUsage, metrics.ActiveJobs);
                
                return Task.FromResult(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system load metrics");
                // 返回默认值
                return Task.FromResult(new SystemLoadMetrics
                {
                    ActiveJobs = 0,
                    QueuedJobs = 0,
                    ScheduledJobs = 0,
                    CpuUsage = 0,
                    MemoryUsage = 0,
                    WorkerCount = 0,
                    AvailableWorkers = 0,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        public async Task<bool> CheckAlertThresholdsAsync(Guid jobId, Domain.Entities.JobPerformanceData performanceData)
        {
            try
            {
                // 获取任务的告警阈值
                var thresholds = _options.AlertThresholds;
                
                bool shouldAlert = false;
                List<string> alerts = new();
                
                // 检查执行时间
                if (performanceData.ExecutionTimeMs > thresholds.MaxExecutionTimeMs)
                {
                    shouldAlert = true;
                    alerts.Add($"执行时间过长: {performanceData.ExecutionTimeMs}ms > {thresholds.MaxExecutionTimeMs}ms");
                }
                
                // 检查失败状态
                if (performanceData.Status == ExecutionStatus.Failed || 
                    performanceData.Status == ExecutionStatus.Canceled)
                {
                    // 获取最近的失败率 - 使用数据库而不是内存存储
                    var oneHourAgo = DateTime.UtcNow.AddHours(-1);
                    var recentJobs = await _dbContext.JobPerformanceData
                        .Where(j => j.JobId == jobId && j.Timestamp >= oneHourAgo)
                        .ToListAsync();
                    
                    if (recentJobs.Count > 0)
                    {
                        var failureRate = (double)recentJobs.Count(j => j.Status == ExecutionStatus.Failed || j.Status == ExecutionStatus.Canceled) / recentJobs.Count * 100;
                        if (failureRate > thresholds.FailureRatePercentage)
                        {
                            shouldAlert = true;
                            alerts.Add($"失败率过高: {failureRate:F2}% > {thresholds.FailureRatePercentage}%");
                        }
                    }
                }
                
                // 如果需要告警
                if (shouldAlert)
                {
                    foreach (var alert in alerts)
                    {
                        _logger.LogWarning("Alert triggered for job {JobId}: {Alert}", jobId, alert);
                    }
                    
                    // 发送告警通知（可扩展为邮件、短信等）
                    // 创建实体类对象用于传递
                    var entityData = new Domain.Entities.JobPerformanceData
                    {
                        JobId = performanceData.JobId,
                        JobName = performanceData.JobName,
                        JobType = performanceData.JobType,
                        ExecutionTimeMs = performanceData.ExecutionTimeMs,
                        Status = performanceData.Status,
                        Timestamp = performanceData.Timestamp,
                        CustomMetrics = performanceData.CustomMetrics != null ? System.Text.Json.JsonSerializer.Serialize(performanceData.CustomMetrics) : string.Empty
                    };
                    // 同步调用通知方法，因为它不再是异步的
                    SendAlertNotificationAsync(jobId, entityData, alerts);
                }
                
                return shouldAlert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking alert thresholds for job {JobId}", jobId);
                return false;
            }
        }
        
        /// <summary>
        /// 获取分页性能数据
        /// </summary>
        /// <param name="pageIndex">页码索引</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="jobId">作业ID（可选）</param>
        /// <param name="jobName">作业名称（可选）</param>
        /// <param name="jobType">作业类型（可选）</param>
        /// <param name="status">执行状态（可选）</param>
        /// <returns>分页结果</returns>
        public async Task<PagedResult<Domain.Services.JobPerformanceData>> GetPagedPerformanceDataAsync(
            int pageIndex,
            int pageSize,
            Guid? jobId = null,
            string jobName = null,
            string jobType = null,
            ExecutionStatus? status = null)
        {
            try
            {
                // 验证参数
                if (pageIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index must be non-negative");
                if (pageSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be positive");
                if (pageSize > 100)
                    pageSize = 100; // 限制最大页面大小
                
                // 构建查询
                var query = _dbContext.JobPerformanceData.AsQueryable();
                
                // 应用过滤条件
                if (jobId.HasValue)
                {
                    query = query.Where(d => d.JobId == jobId.Value);
                }
                
                if (!string.IsNullOrEmpty(jobName))
                {
                    query = query.Where(d => d.JobName.Contains(jobName));
                }
                
                if (!string.IsNullOrEmpty(jobType))
                {
                    query = query.Where(d => d.JobType == jobType);
                }
                
                if (status.HasValue)
                {
                    query = query.Where(d => d.Status == status.Value);
                }
                
                // 获取总数
                var totalCount = await query.CountAsync();
                
                // 执行分页查询
                var items = await query
                    .OrderByDescending(d => d.Timestamp)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                // 转换为服务层的JobPerformanceData类型
                var resultItems = items.Select(item => new Domain.Services.JobPerformanceData
                {
                    JobId = item.JobId,
                    JobName = item.JobName,
                    JobType = item.JobType,
                    ExecutionTimeMs = item.ExecutionTimeMs,
                    Status = item.Status,
                    Timestamp = item.Timestamp,
                    CustomMetrics = !string.IsNullOrEmpty(item.CustomMetrics)
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(item.CustomMetrics) ?? new Dictionary<string, object> {}
                        : new Dictionary<string, object>()
                }).ToList();
                
                return new PagedResult<Domain.Services.JobPerformanceData>
                {
                    Items = resultItems,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分页性能数据时出错");
                return new PagedResult<Domain.Services.JobPerformanceData>
                {
                    Items = new List<Domain.Services.JobPerformanceData>(),
                    TotalCount = 0,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
        }
        
        #region 辅助方法
        
        /// <summary>
        /// 清理过期数据并迁移到历史表
        /// </summary>
        /// <returns>迁移的记录数量</returns>
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task<int> CleanupAndMigrateOldDataAsync()
        {
            try
            {
                _logger.LogInformation("开始清理和迁移旧性能数据");
                
                // 计算 cutoff 时间 - 数据保留天数前
                var cutoffTime = DateTime.UtcNow.AddDays(-_options.DataRetentionDays);
                
                // 分批处理旧数据，避免一次查询过多记录
                const int batchSize = 1000;
                int totalMigratedCount = 0;
                
                while (true)
                {
                    // 查询需要迁移的数据
                    var oldData = await _dbContext.JobPerformanceData
                        .Where(d => d.Timestamp < cutoffTime)
                        .Take(batchSize)
                        .ToListAsync();
                    
                    if (oldData.Count == 0)
                    {
                        break; // 没有更多数据需要迁移
                    }
                    
                    // 转换为历史记录
                    var historyRecords = oldData.Select(d => new Domain.Entities.JobPerformanceDataHistory
                    {
                        JobId = d.JobId,
                        JobName = d.JobName,
                        JobType = d.JobType,
                        ExecutionTimeMs = d.ExecutionTimeMs,
                        Status = d.Status,
                        Timestamp = d.Timestamp,
                        CustomMetrics = d.CustomMetrics,
                        MigratedAt = DateTime.UtcNow
                    }).ToList();
                    
                    // 添加到历史表
                    await _dbContext.JobPerformanceDataHistory.AddRangeAsync(historyRecords);
                    
                    // 从主表删除
                    _dbContext.JobPerformanceData.RemoveRange(oldData);
                    
                    // 保存更改
                    await _dbContext.SaveChangesAsync();
                    
                    totalMigratedCount += oldData.Count;
                    _logger.LogInformation("已迁移 {BatchCount} 条记录，累计迁移 {TotalCount} 条", 
                        oldData.Count, totalMigratedCount);
                    
                    // 短暂暂停，避免对数据库造成过大压力
                    await Task.Delay(100);
                }
                
                _logger.LogInformation("性能数据清理和迁移完成，共迁移 {TotalCount} 条记录", totalMigratedCount);
                return totalMigratedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理和迁移旧性能数据时出错");
                throw;
            }
        }
        
        private async Task<double> CalculateRecentFailureRate(Guid jobId)
        {
            try
            {
                // 从数据库获取最近100次执行
                var recentData = await _dbContext.JobPerformanceData
                    .Where(d => d.JobId == jobId)
                    .OrderByDescending(d => d.Timestamp)
                    .Take(100)
                    .ToListAsync();
                
                if (!recentData.Any())
                    return 0;
                
                int failedCount = recentData.Count(d => 
                    d.Status == ExecutionStatus.Failed || 
                    d.Status == ExecutionStatus.Canceled);
                
                return (double)failedCount / recentData.Count * 100;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "计算任务失败率时出错，JobId: {JobId}", jobId);
                return 0;
            }
        }
        
        private void SendAlertNotificationAsync(Guid jobId, Domain.Entities.JobPerformanceData data, List<string> alerts)
        {
            // 这里可以扩展为发送邮件、短信、消息队列等
            _logger.LogError("Job alert notification for {JobId} ({JobName}): {Alerts}",
                jobId, data.JobName, string.Join(", ", alerts));
            
            // 示例：可集成第三方告警系统
            // await _notificationService.SendAlertAsync("Job Performance Alert", alertMessage);
        }
        
        private double GetCurrentCpuUsage()
        {
            // 简化实现，实际项目中可能需要更复杂的计算
            return 0; // 占位
        }
        
        private int GetWorkerCount()
        {
            // 简化实现，实际项目中应从Hangfire获取
            return 5; // 占位
        }
        
        private int GetAvailableWorkerCount()
        {
            // 简化实现，实际项目中应从Hangfire获取
            return 3; // 占位
        }
        
        #endregion
        
        /// <summary>
        /// 统一查询性能数据（从当前表和历史表）
        /// </summary>
        /// <param name="queryOptions">查询选项</param>
        /// <returns>查询结果</returns>
        public async Task<PagedResult<Domain.Services.JobPerformanceData>> GetUnifiedPerformanceDataAsync(
            Domain.Services.PerformanceDataQueryOptions queryOptions)
        {
            try
            {
                // 验证参数
                if (queryOptions == null)
                    throw new ArgumentNullException(nameof(queryOptions));
                
                if (queryOptions.PageIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(queryOptions.PageIndex), "Page index must be non-negative");
                if (queryOptions.PageSize <= 0)
                    throw new ArgumentOutOfRangeException(nameof(queryOptions.PageSize), "Page size must be positive");
                if (queryOptions.PageSize > 100)
                    queryOptions.PageSize = 100; // 限制最大页面大小
                
                // 计算时间范围
                var endTime = DateTime.UtcNow;
                var startTime = queryOptions.TimeRange.HasValue 
                    ? endTime.Subtract(queryOptions.TimeRange.Value) 
                    : endTime.AddDays(-30); // 默认查询最近30天
                
                // 查询当前表和历史表中的数据
                var currentDataQuery = _dbContext.JobPerformanceData
                    .Where(d => d.Timestamp >= startTime && d.Timestamp <= endTime);
                
                var historyDataQuery = _dbContext.JobPerformanceDataHistory
                    .Where(d => d.Timestamp >= startTime && d.Timestamp <= endTime);
                
                // 应用过滤条件
                if (queryOptions.JobId.HasValue)
                {
                    currentDataQuery = currentDataQuery.Where(d => d.JobId == queryOptions.JobId.Value);
                    historyDataQuery = historyDataQuery.Where(d => d.JobId == queryOptions.JobId.Value);
                }
                
                if (!string.IsNullOrEmpty(queryOptions.JobName))
                {
                    currentDataQuery = currentDataQuery.Where(d => d.JobName.Contains(queryOptions.JobName));
                    historyDataQuery = historyDataQuery.Where(d => d.JobName.Contains(queryOptions.JobName));
                }
                
                if (!string.IsNullOrEmpty(queryOptions.JobType))
                {
                    currentDataQuery = currentDataQuery.Where(d => d.JobType == queryOptions.JobType);
                    historyDataQuery = historyDataQuery.Where(d => d.JobType == queryOptions.JobType);
                }
                
                if (queryOptions.Status.HasValue)
                {
                    currentDataQuery = currentDataQuery.Where(d => d.Status == queryOptions.Status.Value);
                    historyDataQuery = historyDataQuery.Where(d => d.Status == queryOptions.Status.Value);
                }
                
                // 获取总数
                var currentCount = await currentDataQuery.CountAsync();
                var historyCount = await historyDataQuery.CountAsync();
                var totalCount = currentCount + historyCount;
                
                // 查询当前表数据
                var currentItems = await currentDataQuery
                    .OrderByDescending(d => d.Timestamp)
                    .ToListAsync();
                
                // 查询历史表数据
                var historyItems = await historyDataQuery
                    .OrderByDescending(d => d.Timestamp)
                    .ToListAsync();
                
                // 获取所有需要的数据并转换为统一格式
                var allItems = new List<Domain.Services.JobPerformanceData>();
                
                // 添加当前表数据
                allItems.AddRange(currentItems.Select(item => new Domain.Services.JobPerformanceData
                {
                    JobId = item.JobId,
                    JobName = item.JobName,
                    JobType = item.JobType,
                    ExecutionTimeMs = item.ExecutionTimeMs,
                    Status = item.Status,
                    Timestamp = item.Timestamp,
                    CustomMetrics = !string.IsNullOrEmpty(item.CustomMetrics)
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(item.CustomMetrics) ?? new Dictionary<string, object>()
                        : new Dictionary<string, object>()
                }));
                
                // 添加历史表数据
                allItems.AddRange(historyItems.Select(item => new Domain.Services.JobPerformanceData
                {
                    JobId = item.JobId,
                    JobName = item.JobName,
                    JobType = item.JobType,
                    ExecutionTimeMs = item.ExecutionTimeMs,
                    Status = item.Status,
                    Timestamp = item.Timestamp,
                    CustomMetrics = !string.IsNullOrEmpty(item.CustomMetrics)
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(item.CustomMetrics) ?? new Dictionary<string, object>()
                        : new Dictionary<string, object>()
                }));
                
                // 排序并分页
                var pagedItems = allItems
                    .OrderByDescending(d => d.Timestamp)
                    .Skip(queryOptions.PageIndex * queryOptions.PageSize)
                    .Take(queryOptions.PageSize)
                    .ToList();
                
                // 已在前面的步骤中转换为正确的类型
                var resultItems = pagedItems;
                
                return new PagedResult<Domain.Services.JobPerformanceData>
                {
                    Items = resultItems,
                    TotalCount = totalCount,
                    PageIndex = queryOptions.PageIndex,
                    PageSize = queryOptions.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "统一查询性能数据时出错");
                return new PagedResult<Domain.Services.JobPerformanceData>
                {
                    Items = new List<Domain.Services.JobPerformanceData>(),
                    TotalCount = 0,
                    PageIndex = queryOptions.PageIndex,
                    PageSize = queryOptions.PageSize
                };
            }
        }
    }
    
    
    /// <summary>
    /// 性能数据查询选项
    /// </summary>
    public class PerformanceDataQueryOptions
    {
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public Guid? JobId { get; set; }
        public string? JobName { get; set; }
        public string? JobType { get; set; }
        public ExecutionStatus? Status { get; set; }
        public TimeSpan? TimeRange { get; set; }
        public bool IncludeHistory { get; set; } = true;
    }
}