using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Enums;

using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 性能监控服务接口
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        /// <summary>
        /// 记录任务执行性能指标
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="status">执行状态</param>
        /// <param name="metrics">自定义指标</param>
        Task RecordJobPerformanceAsync(Guid jobId, DateTime startTime, DateTime endTime, ExecutionStatus status, IDictionary<string, object> metrics = null);
        
        /// <summary>
        /// 获取任务性能统计
        /// </summary>
        /// <param name="jobId">任务ID（可选）</param>
        /// <param name="timeRange">时间范围</param>
        Task<JobPerformanceStatistics> GetJobPerformanceStatisticsAsync(Guid? jobId = null, 
            TimeSpan? timeRange = null);
        
        /// <summary>
        /// 获取系统负载指标
        /// </summary>
        Task<SystemLoadMetrics> GetSystemLoadMetricsAsync();
        
        /// <summary>
        /// 检查是否需要触发告警
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="performanceData">性能数据</param>
        Task<bool> CheckAlertThresholdsAsync(Guid jobId, Domain.Entities.JobPerformanceData performanceData);
        
        /// <summary>
    /// 获取分页的性能数据列表
    /// </summary>
    /// <param name="pageIndex">页码（从0开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="jobId">作业ID（可选）</param>
    /// <param name="jobName">作业名称（可选）</param>
    /// <param name="jobType">作业类型（可选）</param>
    /// <param name="status">执行状态（可选）</param>
    /// <returns>分页的性能数据</returns>
    Task<PagedResult<JobPerformanceData>> GetPagedPerformanceDataAsync(
        int pageIndex,
        int pageSize,
        Guid? jobId = null,
        string jobName = null,
        string jobType = null,
        ExecutionStatus? status = null);
        
    /// <summary>
    /// 统一查询性能数据（从当前表和历史表）
    /// </summary>
    /// <param name="queryOptions">查询选项</param>
    /// <returns>查询结果</returns>
    Task<PagedResult<JobPerformanceData>> GetUnifiedPerformanceDataAsync(
        PerformanceDataQueryOptions queryOptions);
}

/// <summary>
/// 性能数据查询选项
/// </summary>
public class PerformanceDataQueryOptions
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 20;
    public Guid? JobId { get; set; }
    public string JobName { get; set; }
    public string JobType { get; set; }
    public ExecutionStatus? Status { get; set; }
    public TimeSpan? TimeRange { get; set; }
    public bool IncludeHistory { get; set; } = true;
}
    

    
    /// <summary>
    /// 任务性能统计
    /// </summary>
    public class JobPerformanceStatistics
    {
        public int TotalJobsExecuted { get; set; }
        public int SuccessfulJobs { get; set; }
        public int FailedJobs { get; set; }
        public int TimedOutJobs { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public double MedianExecutionTimeMs { get; set; }
        public double MinExecutionTimeMs { get; set; }
        public double MaxExecutionTimeMs { get; set; }
        public IDictionary<string, JobTypeStatistics> JobTypeStats { get; set; } = new Dictionary<string, JobTypeStatistics>();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
    
    /// <summary>
    /// 任务类型统计
    /// </summary>
    public class JobTypeStatistics
    {
        public string JobType { get; set; }
        public int ExecutionCount { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public double SuccessRate { get; set; }
    }
    
    /// <summary>
    /// 系统负载指标
    /// </summary>
    public class SystemLoadMetrics
    {
        public int ActiveJobs { get; set; }
        public int QueuedJobs { get; set; }
        public int ScheduledJobs { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int WorkerCount { get; set; }
        public int AvailableWorkers { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// 任务性能数据
    /// </summary>
    public class JobPerformanceData
    {
        public Guid JobId { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public double ExecutionTimeMs { get; set; }
        public ExecutionStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
        public IDictionary<string, object> CustomMetrics { get; set; } = new Dictionary<string, object>();
    }
}