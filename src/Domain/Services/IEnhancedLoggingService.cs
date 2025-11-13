using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using System.Linq;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 增强的日志记录服务接口
    /// </summary>
    public interface IEnhancedLoggingService
    {
        /// <summary>
        /// 记录任务执行开始
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="backgroundJobId">Hangfire任务ID</param>
        /// <param name="metadata">元数据</param>
        Task<Guid> LogJobExecutionStartAsync(Guid jobId, string backgroundJobId, IDictionary<string, object> metadata = null);
        
        /// <summary>
        /// 记录任务执行完成
        /// </summary>
        /// <param name="logId">日志ID</param>
        /// <param name="result">执行结果</param>
        Task LogJobExecutionCompleteAsync(Guid logId, string result = null);
        
        /// <summary>
        /// 记录任务执行失败
        /// </summary>
        /// <param name="logId">日志ID</param>
        /// <param name="errorMessage">错误消息</param>
        /// <param name="errorDetails">错误详情</param>
        Task LogJobExecutionFailureAsync(Guid logId, string errorMessage, string errorDetails = null);
        
        /// <summary>
        /// 记录任务执行中间状态
        /// </summary>
        /// <param name="logId">日志ID</param>
        /// <param name="status">状态</param>
        /// <param name="message">消息</param>
        Task LogJobExecutionStatusAsync(Guid logId, ExecutionStatus status, string message = null);
        
        /// <summary>
        /// 记录审计日志
        /// </summary>
        /// <param name="action">操作</param>
        /// <param name="entityType">实体类型</param>
        /// <param name="entityId">实体ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="details">详情</param>
        Task LogAuditAsync(string action, string entityType, Guid entityId, string userId = null, IDictionary<string, object> details = null);
        
        /// <summary>
        /// 获取任务执行历史
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页大小</param>
        Task<PagedResult<JobExecutionLog>> GetJobExecutionHistoryAsync(Guid jobId, int pageIndex = 1, int pageSize = 20);
        
        /// <summary>
        /// 开始性能跟踪
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="tags">标签</param>
        PerformanceTracker BeginPerformanceTrack(string operationName, IDictionary<string, string> tags = null);
        
        /// <summary>
        /// 结束性能跟踪
        /// </summary>
        /// <param name="tracker">性能跟踪器</param>
        Task EndPerformanceTrackAsync(PerformanceTracker tracker);
    }
    
    /// <summary>
    /// 性能跟踪器
    /// </summary>
    public class PerformanceTracker : IDisposable
    {
        public string OperationName { get; }
        public DateTime StartTime { get; }
        public IDictionary<string, string> Tags { get; }
        public IDictionary<string, double> Metrics { get; } = new Dictionary<string, double>();
        public IDictionary<string, object> Context { get; } = new Dictionary<string, object>();
        
        public PerformanceTracker(string operationName, IDictionary<string, string> tags = null)
        {
            OperationName = operationName;
            StartTime = DateTime.UtcNow;
            Tags = tags ?? new Dictionary<string, string>();
        }
        
        public void AddMetric(string name, double value)
        {
            Metrics[name] = value;
        }
        
        public void AddContext(string key, object value)
        {
            Context[key] = value;
        }
        
        public double ElapsedMilliseconds => (DateTime.UtcNow - StartTime).TotalMilliseconds;
        
        public void Dispose()
        {
            // 简化实现，实际项目中可能需要自动结束跟踪
        }
    }
    
    /// <summary>
    /// 分页结果
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}