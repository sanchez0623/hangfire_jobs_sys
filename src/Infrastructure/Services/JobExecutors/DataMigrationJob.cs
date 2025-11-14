using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HangfireJobsSys.Infrastructure.Services.JobExecutors
{
    /// <summary>
    /// 数据迁移工作项
    /// 用于将数据从主表迁移到历史表，支持增量迁移
    /// </summary>
    public class DataMigrationJob : IHangfireJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataMigrationJob> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DataMigrationJob(
            IServiceProvider serviceProvider,
            ILogger<DataMigrationJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 执行数据迁移
        /// </summary>
        /// <param name="parameters">迁移参数（JSON格式）</param>
        /// <returns>迁移结果</returns>
        public async Task<string> ExecuteAsync(string parameters)
        {
            _logger.LogInformation("开始执行数据迁移任务");
            
            try
            {
                // 解析迁移参数
                var migrationParams = ParseMigrationParameters(parameters);
                
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HangfireJobsSysDbContext>();
                var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                
                // 执行迁移
                var migrationResult = await MigrateDataAsync(context, migrationParams);
                
                _logger.LogInformation($"数据迁移完成: 迁移了{migrationResult.RecordsMigrated}条记录，耗时{migrationResult.ExecutionTime.TotalSeconds}秒");
                return $"迁移成功: 共迁移{migrationResult.RecordsMigrated}条记录，耗时{migrationResult.ExecutionTime.TotalSeconds:F2}秒";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据迁移任务执行失败");
                throw new InvalidOperationException("数据迁移失败: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 解析迁移参数
        /// </summary>
        private MigrationParameters ParseMigrationParameters(string parameters)
        {
            try
            {
                // 默认参数
                var defaultParams = new MigrationParameters
                {
                    BatchSize = 1000,
                    RetentionDays = 30,
                    MaxRecords = 100000,
                    EntityTypes = new List<string> { "JobExecutionLog", "JobPerformanceData" }
                };

                // 如果没有参数，返回默认值
                if (string.IsNullOrEmpty(parameters))
                {
                    _logger.LogInformation("使用默认迁移参数");
                    return defaultParams;
                }

                // 解析JSON参数
                var parsedParams = JsonConvert.DeserializeObject<MigrationParameters>(parameters);
                
                // 验证必要的字段，如果缺失则使用默认值
                if (parsedParams.EntityTypes == null || parsedParams.EntityTypes.Count == 0)
                {
                    parsedParams.EntityTypes = new List<string> { "JobExecutionLog", "JobPerformanceData" };
                }
                
                // 确保批次大小合理
                parsedParams.BatchSize = Math.Max(100, Math.Min(parsedParams.BatchSize, 10000));
                
                // 确保保留天数合理
                parsedParams.RetentionDays = Math.Max(1, Math.Min(parsedParams.RetentionDays, 365));
                
                // 确保最大记录数合理
                parsedParams.MaxRecords = Math.Max(1000, Math.Min(parsedParams.MaxRecords, 1000000));
                
                _logger.LogInformation("使用自定义迁移参数: 批次大小={BatchSize}, 保留天数={RetentionDays}, 最大记录数={MaxRecords}, 实体类型数量={EntityTypeCount}",
                    parsedParams.BatchSize, parsedParams.RetentionDays, parsedParams.MaxRecords, parsedParams.EntityTypes.Count);
                
                return parsedParams;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "解析迁移参数失败，使用默认参数");
                return new MigrationParameters
                {
                    BatchSize = 1000,
                    RetentionDays = 30,
                    MaxRecords = 100000,
                    EntityTypes = new List<string> { "JobExecutionLog", "JobPerformanceData" }
                };
            }
        }

        /// <summary>
        /// 执行数据迁移
        /// </summary>
        private async Task<MigrationResult> MigrateDataAsync(HangfireJobsSysDbContext context, MigrationParameters parameters)
        {
            var startTime = DateTime.UtcNow;
            var totalRecordsMigrated = 0;
            
            _logger.LogInformation("开始数据迁移: 保留天数={RetentionDays}, 批次大小={BatchSize}, 最大记录数={MaxRecords}",
                parameters.RetentionDays, parameters.BatchSize, parameters.MaxRecords);

            // 计算截止日期
            var cutoffDate = DateTime.UtcNow.AddDays(-parameters.RetentionDays);
            
            // 迁移每个实体类型
            foreach (var entityType in parameters.EntityTypes)
            {
                switch (entityType.ToLower())
                {
                    case "jobexecutionlog":
                        totalRecordsMigrated += await MigrateJobExecutionLogsAsync(context, cutoffDate, parameters.BatchSize, parameters.MaxRecords);
                        break;
                    case "jobperformancedata":
                        totalRecordsMigrated += await MigrateJobPerformanceDataAsync(context, cutoffDate, parameters.BatchSize, parameters.MaxRecords);
                        break;
                    default:
                        _logger.LogWarning("不支持的实体类型: {EntityType}", entityType);
                        break;
                }
            }

            return new MigrationResult
            {
                RecordsMigrated = totalRecordsMigrated,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }

        /// <summary>
        /// 迁移任务执行日志
        /// </summary>
        private async Task<int> MigrateJobExecutionLogsAsync(HangfireJobsSysDbContext context, DateTime cutoffDate, int batchSize, int maxRecords)
        {
            var recordsMigrated = 0;
            bool hasMoreRecords = true;

            _logger.LogInformation("开始迁移任务执行日志，截止日期: {CutoffDate}", cutoffDate);

            while (hasMoreRecords && recordsMigrated < maxRecords)
            {
                try
                {
                    // 查找需要迁移的记录
                    var logsToMigrate = await context.JobExecutionLogs
                        .Where(j => j.EndedAt.HasValue && j.EndedAt.Value < cutoffDate)
                        .OrderBy(j => j.EndedAt)
                        .Take(batchSize)
                        .ToListAsync();

                    if (logsToMigrate.Count == 0)
                    {
                        hasMoreRecords = false;
                        break;
                    }

                    // 开始事务
                    using var transaction = await context.Database.BeginTransactionAsync();

                    try
                    {
                        // 创建历史记录
                        var historyRecords = logsToMigrate.Select(j => new JobExecutionLogHistory
                        {
                            JobId = j.JobId,
                            ExecutionTime = j.EndedAt.Value,
                            Status = j.Status,
                            ResultMessage = j.Result,
                            ErrorMessage = j.ErrorMessage,
                            ErrorDetails = j.ErrorStack,
                            ExecutionDurationMs = j.DurationMs ?? 0,
                            CreatedAt = j.CreatedAt,
                            MigratedAt = DateTime.UtcNow
                        }).ToList();

                        // 批量插入历史表
                        await context.JobExecutionLogHistory.AddRangeAsync(historyRecords);
                        
                        // 批量删除主表记录
                        context.JobExecutionLogs.RemoveRange(logsToMigrate);
                        
                        // 提交事务
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        recordsMigrated += logsToMigrate.Count;
                        _logger.LogInformation("已迁移{Count}条任务执行日志，累计{Total}条", logsToMigrate.Count, recordsMigrated);

                        // 为了避免长时间锁定数据库，每批处理后短暂暂停
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "迁移批次失败，已回滚");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "迁移任务执行日志时出错");
                    // 继续尝试下一批次
                    await Task.Delay(1000); // 出错后暂停一秒再尝试
                }
            }

            _logger.LogInformation("任务执行日志迁移完成，共迁移{Count}条记录", recordsMigrated);
            return recordsMigrated;
        }

        /// <summary>
        /// 迁移任务性能数据
        /// </summary>
        private async Task<int> MigrateJobPerformanceDataAsync(HangfireJobsSysDbContext context, DateTime cutoffDate, int batchSize, int maxRecords)
        {
            var recordsMigrated = 0;
            bool hasMoreRecords = true;

            _logger.LogInformation("开始迁移任务性能数据，截止日期: {CutoffDate}", cutoffDate);

            while (hasMoreRecords && recordsMigrated < maxRecords)
            {
                try
                {
                    // 查找需要迁移的记录
                    var performanceDataToMigrate = await context.JobPerformanceData
                        .Where(p => p.Timestamp < cutoffDate)
                        .OrderBy(p => p.Timestamp)
                        .Take(batchSize)
                        .ToListAsync();

                    if (performanceDataToMigrate.Count == 0)
                    {
                        hasMoreRecords = false;
                        break;
                    }

                    // 开始事务
                    using var transaction = await context.Database.BeginTransactionAsync();

                    try
                    {
                        // 创建历史记录
                        var historyRecords = performanceDataToMigrate.Select(p => new JobPerformanceDataHistory
                        {
                            Id = p.Id,
                            JobId = p.JobId,
                            JobName = p.JobName,
                            Timestamp = p.Timestamp,
                            ExecutionTimeMs = p.ExecutionTimeMs,
                            CustomMetrics = p.CustomMetrics,
                            MigratedAt = DateTime.UtcNow
                        }).ToList();

                        // 批量插入历史表
                        await context.JobPerformanceDataHistory.AddRangeAsync(historyRecords);
                        
                        // 批量删除主表记录
                        context.JobPerformanceData.RemoveRange(performanceDataToMigrate);
                        
                        // 提交事务
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        recordsMigrated += performanceDataToMigrate.Count;
                        _logger.LogInformation("已迁移{Count}条任务性能数据，累计{Total}条", performanceDataToMigrate.Count, recordsMigrated);

                        // 为了避免长时间锁定数据库，每批处理后短暂暂停
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "迁移批次失败，已回滚");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "迁移任务性能数据时出错");
                    // 继续尝试下一批次
                    await Task.Delay(1000); // 出错后暂停一秒再尝试
                }
            }

            _logger.LogInformation("任务性能数据迁移完成，共迁移{Count}条记录", recordsMigrated);
            return recordsMigrated;
        }

        /// <summary>
        /// 迁移参数
        /// </summary>
        private class MigrationParameters
        {
            public int BatchSize { get; set; } = 1000;
            public int RetentionDays { get; set; } = 30;
            public int MaxRecords { get; set; } = 100000;
            public List<string> EntityTypes { get; set; } = new List<string> { "JobExecutionLog", "JobPerformanceData" };
        }

        /// <summary>
        /// 迁移结果
        /// </summary>
        private class MigrationResult
        {
            public int RecordsMigrated { get; set; }
            public TimeSpan ExecutionTime { get; set; }
        }
    }
}