using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Enums;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using Microsoft.Extensions.Logging;

namespace HangfireJobsSys.Infrastructure.Services
{
    /// <summary>
    /// 增强的日志记录服务实现
    /// </summary>
    public class EnhancedLoggingService : IEnhancedLoggingService
    {
        private readonly ILogger<EnhancedLoggingService> _logger;
        private readonly IJobExecutionLogRepository _jobExecutionLogRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IPerformanceMonitoringService _performanceMonitoringService;

        public EnhancedLoggingService(
            ILogger<EnhancedLoggingService> logger,
            IJobExecutionLogRepository jobExecutionLogRepository,
            IAuditLogRepository auditLogRepository,
            IPerformanceMonitoringService performanceMonitoringService)
        {
            _logger = logger;
            _jobExecutionLogRepository = jobExecutionLogRepository;
            _auditLogRepository = auditLogRepository;
            _performanceMonitoringService = performanceMonitoringService;
        }

        public async Task<Guid> LogJobExecutionStartAsync(Guid jobId, string backgroundJobId, IDictionary<string, object> metadata = null)
        {
            // 创建执行日志
            var log = JobExecutionLog.Create(jobId, backgroundJobId);
            
            // 保存到数据库
            await _jobExecutionLogRepository.AddAsync(log);
            
            // 记录结构化日志
            _logger.LogInformation(
                new EventId(1001, "JobExecutionStarted"),
                "任务执行开始 | JobId: {JobId} | BackgroundJobId: {BackgroundJobId} | Metadata: {@Metadata}",
                jobId, backgroundJobId, metadata);
            
            return log.Id;
        }

        public async Task LogJobExecutionCompleteAsync(Guid logId, string result = null)
        {
            // 获取日志
            var log = await _jobExecutionLogRepository.GetByIdAsync(logId);
            if (log == null)
            {
                _logger.LogWarning("找不到执行日志: {LogId}", logId);
                return;
            }
            
            // 更新状态
            log.Complete(result);
            await _jobExecutionLogRepository.UpdateAsync(log);
            
            // 计算执行时间
            var executionTime = log.DurationMs ?? 0;
            
            // 记录结构化日志
            _logger.LogInformation(
                new EventId(1002, "JobExecutionCompleted"),
                "任务执行完成 | LogId: {LogId} | JobId: {JobId} | ExecutionTime: {ExecutionTime}ms | Result: {Result}",
                logId, log.JobId, executionTime, result);
            
            // 记录性能指标
            await _performanceMonitoringService.RecordJobPerformanceAsync(
                log.JobId,
                log.StartedAt,
                log.EndedAt ?? DateTime.UtcNow,
                ExecutionStatus.Succeeded,
                new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", executionTime },
                    { "Result", result }
                });
        }

        public async Task LogJobExecutionFailureAsync(Guid logId, string errorMessage, string errorDetails = null)
        {
            // 获取日志
            var log = await _jobExecutionLogRepository.GetByIdAsync(logId);
            if (log == null)
            {
                _logger.LogWarning("找不到执行日志: {LogId}", logId);
                return;
            }
            
            // 更新状态
            log.Fail(errorMessage, errorDetails);
            await _jobExecutionLogRepository.UpdateAsync(log);
            
            // 计算执行时间
            var executionTime = log.DurationMs ?? 0;
            
            // 记录结构化错误日志
            _logger.LogError(
                new EventId(1003, "JobExecutionFailed"),
                "任务执行失败 | LogId: {LogId} | JobId: {JobId} | ExecutionTime: {ExecutionTime}ms | Error: {ErrorMessage} | Details: {ErrorDetails}",
                logId, log.JobId, executionTime, errorMessage, errorDetails);
            
            // 记录性能指标
            await _performanceMonitoringService.RecordJobPerformanceAsync(
                log.JobId,
                log.StartedAt,
                log.EndedAt ?? DateTime.UtcNow,
                ExecutionStatus.Failed,
                new Dictionary<string, object>
                {
                    { "ExecutionTimeMs", executionTime },
                    { "ErrorMessage", errorMessage }
                });
        }

        public async Task LogJobExecutionStatusAsync(Guid logId, ExecutionStatus status, string message = null)
        {
            // 获取日志
            var log = await _jobExecutionLogRepository.GetByIdAsync(logId);
            if (log == null)
            {
                _logger.LogWarning("找不到执行日志: {LogId}", logId);
                return;
            }
            
            // 简单处理状态更新
            await _jobExecutionLogRepository.UpdateAsync(log);
            
            // 记录结构化日志
            _logger.LogInformation(
                new EventId(1004, "JobStatusUpdated"),
                "任务状态更新 | LogId: {LogId} | JobId: {JobId} | Status: {Status} | Message: {Message}",
                logId, log.JobId, status, message);
        }

        public async Task LogAuditAsync(string action, string entityType, Guid entityId, string userId = null, IDictionary<string, object> details = null)
        {
            // 创建审计日志
            var auditLog = AuditLog.Create(action, entityType, entityId, userId);
            
            // 添加详情
            if (details != null)
            {
                foreach (var kvp in details)
                {
                    auditLog.AddDetail(kvp.Key, kvp.Value);
                }
            }
            
            // 保存到数据库
            await _auditLogRepository.AddAsync(auditLog);
            
            // 记录结构化日志
            _logger.LogInformation(
                new EventId(2001, "AuditLogCreated"),
                "审计日志记录 | Action: {Action} | EntityType: {EntityType} | EntityId: {EntityId} | UserId: {UserId} | Details: {@Details}",
                action, entityType, entityId, userId, details);
        }

        public async Task<PagedResult<JobExecutionLog>> GetJobExecutionHistoryAsync(Guid jobId, int pageIndex = 1, int pageSize = 20)
        {
            // 验证参数
            if (jobId == Guid.Empty)
                throw new ArgumentNullException(nameof(jobId));
            
            // 验证分页参数
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Max(1, Math.Min(100, pageSize));
            
            // 获取所有日志
            var allLogs = await _jobExecutionLogRepository.GetRecentFailedLogsAsync();
            var jobLogs = allLogs.Where(log => log.JobId == jobId).ToList();
            
            // 简单分页
            var totalCount = jobLogs.Count;
            var pagedLogs = jobLogs
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return new PagedResult<JobExecutionLog>
            {
                Items = pagedLogs,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public PerformanceTracker BeginPerformanceTrack(string operationName, IDictionary<string, string> tags = null)
        {
            // 创建性能跟踪器
            var tracker = new PerformanceTracker(operationName, tags);
            
            // 记录开始日志
            _logger.LogDebug(
                new EventId(3001, "PerformanceTrackStarted"),
                "性能跟踪开始 | Operation: {OperationName} | Tags: {@Tags}",
                operationName, tags);
            
            return tracker;
        }

        public async Task EndPerformanceTrackAsync(PerformanceTracker tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException(nameof(tracker));
            
            // 计算耗时
            var elapsedMs = tracker.ElapsedMilliseconds;
            
            // 确定日志级别
            if (elapsedMs > 1000) // 超过1秒
            {
                _logger.LogWarning(
                    new EventId(3002, "PerformanceTrackEndedSlow"),
                    "性能跟踪结束(慢) | Operation: {OperationName} | Time: {Elapsed}ms | Tags: {@Tags} | Metrics: {@Metrics}",
                    tracker.OperationName, elapsedMs, tracker.Tags, tracker.Metrics);
            }
            else
            {
                _logger.LogDebug(
                    new EventId(3003, "PerformanceTrackEnded"),
                    "性能跟踪结束 | Operation: {OperationName} | Time: {Elapsed}ms | Tags: {@Tags} | Metrics: {@Metrics}",
                    tracker.OperationName, elapsedMs, tracker.Tags, tracker.Metrics);
            }
            
            // 记录性能指标
            await _performanceMonitoringService.RecordJobPerformanceAsync(
                Guid.Empty, // 非任务相关的性能跟踪
                tracker.StartTime,
                DateTime.UtcNow,
                ExecutionStatus.Succeeded,
                new Dictionary<string, object>
                {
                    { "OperationName", tracker.OperationName },
                    { "ElapsedMs", elapsedMs },
                    { "Tags", tracker.Tags },
                    { "Metrics", tracker.Metrics }
                });
        }
    }
}