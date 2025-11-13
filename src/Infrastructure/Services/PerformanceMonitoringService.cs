using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Services;
using Microsoft.Extensions.Logging;

namespace HangfireJobsSys.Infrastructure.Services
{
    /// <summary>
    /// 性能监控服务实现
    /// </summary>
    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly List<JobPerformanceData> _performanceDataStore = new();
        private readonly Dictionary<Guid, AlertThresholds> _jobAlertThresholds = new();
        private readonly object _lockObject = new();
        
        // 默认告警阈值
        private readonly AlertThresholds _defaultThresholds = new()
        {
            MaxExecutionTimeMs = 30000,  // 30秒
            FailureRatePercentage = 10,  // 10%
            TimeoutPercentage = 5        // 5%
        };

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;
        }

        public async Task RecordJobPerformanceAsync(Guid jobId, DateTime startTime, DateTime endTime, 
            ExecutionStatus status, IDictionary<string, object> metrics = null)
        {
            var executionTimeMs = (endTime - startTime).TotalMilliseconds;
            
            var performanceData = new JobPerformanceData
            {
                JobId = jobId,
                JobName = metrics != null && metrics.TryGetValue("JobName", out var jobNameValue) && jobNameValue != null ? jobNameValue.ToString()! : string.Empty,
                JobType = metrics != null && metrics.TryGetValue("JobType", out var jobTypeValue) && jobTypeValue != null ? jobTypeValue.ToString()! : string.Empty,
                ExecutionTimeMs = executionTimeMs,
                Status = status,
                Timestamp = DateTime.UtcNow,
                CustomMetrics = metrics ?? new Dictionary<string, object>()
            };
            
            // 添加到内存存储（线程安全）
            lock (_lockObject)
            {
                _performanceDataStore.Add(performanceData);
                
                // 清理过期数据（保留最近7天的数据）
                CleanupExpiredData();
            }
            
            // 检查告警
            await CheckAlertThresholdsAsync(jobId, performanceData);
            
            _logger.LogDebug("Performance recorded: JobId={JobId}, Time={ExecutionTime}ms, Status={Status}",
                jobId, executionTimeMs, status);
        }

        public async Task<JobPerformanceStatistics> GetJobPerformanceStatisticsAsync(Guid? jobId = null,
            TimeSpan? timeRange = null)
        {
            var endTime = DateTime.UtcNow;
            var startTime = timeRange.HasValue ? endTime - timeRange.Value : endTime.AddDays(-1);
            
            List<JobPerformanceData> filteredData;
            
            lock (_lockObject)
            {
                filteredData = _performanceDataStore
                    .Where(d => d.Timestamp >= startTime && d.Timestamp <= endTime)
                    .Where(d => !jobId.HasValue || d.JobId == jobId.Value)
                    .ToList();
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

        public async Task<SystemLoadMetrics> GetSystemLoadMetricsAsync()
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
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system load metrics");
                // 返回默认值
                return new SystemLoadMetrics
                {
                    ActiveJobs = 0,
                    QueuedJobs = 0,
                    ScheduledJobs = 0,
                    CpuUsage = 0,
                    MemoryUsage = 0,
                    WorkerCount = 0,
                    AvailableWorkers = 0,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<bool> CheckAlertThresholdsAsync(Guid jobId, JobPerformanceData performanceData)
        {
            try
            {
                // 获取任务的告警阈值
                var thresholds = _jobAlertThresholds.GetValueOrDefault(jobId, _defaultThresholds);
                
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
                    // 获取最近的失败率
                    var recentFailureRate = await CalculateRecentFailureRate(jobId);
                    if (recentFailureRate > thresholds.FailureRatePercentage)
                    {
                        shouldAlert = true;
                        alerts.Add($"失败率过高: {recentFailureRate:F2}% > {thresholds.FailureRatePercentage}%");
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
                    await SendAlertNotificationAsync(jobId, performanceData, alerts);
                }
                
                return shouldAlert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking alert thresholds for job {JobId}", jobId);
                return false;
            }
        }
        
        #region 辅助方法
        
        private void CleanupExpiredData()
        {
            // 清理7天前的数据
            var cutoffTime = DateTime.UtcNow.AddDays(-7);
            _performanceDataStore.RemoveAll(d => d.Timestamp < cutoffTime);
        }
        
        private async Task<double> CalculateRecentFailureRate(Guid jobId)
        {
            // 获取最近100次执行
            List<JobPerformanceData> recentData;
            lock (_lockObject)
            {
                recentData = _performanceDataStore
                    .Where(d => d.JobId == jobId)
                    .OrderByDescending(d => d.Timestamp)
                    .Take(100)
                    .ToList();
            }
            
            if (!recentData.Any())
                return 0;
            
            int failedCount = recentData.Count(d => 
                d.Status == ExecutionStatus.Failed || 
                d.Status == ExecutionStatus.Canceled);
            
            return (double)failedCount / recentData.Count * 100;
        }
        
        private async Task SendAlertNotificationAsync(Guid jobId, JobPerformanceData data, List<string> alerts)
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
    }
    
    /// <summary>
    /// 告警阈值配置
    /// </summary>
    public class AlertThresholds
    {
        public double MaxExecutionTimeMs { get; set; }
        public double FailureRatePercentage { get; set; }
        public double TimeoutPercentage { get; set; }
    }
}