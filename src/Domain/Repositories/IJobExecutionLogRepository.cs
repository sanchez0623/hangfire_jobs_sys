using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Enums;

namespace HangfireJobsSys.Domain.Repositories
{
    /// <summary>
    /// 任务执行日志仓储接口
    /// </summary>
    public interface IJobExecutionLogRepository
    {
        /// <summary>
        /// 添加执行日志
        /// </summary>
        Task AddAsync(JobExecutionLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新执行日志
        /// </summary>
        Task UpdateAsync(JobExecutionLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取执行日志
        /// </summary>
        Task<JobExecutionLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据Hangfire执行ID获取执行日志
        /// </summary>
        Task<JobExecutionLog> GetByHangfireExecutionIdAsync(string hangfireExecutionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据任务ID获取执行日志列表
        /// </summary>
        Task<IEnumerable<JobExecutionLog>> GetByJobIdAsync(
            Guid jobId,
            ExecutionStatus? status = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取任务执行日志总数
        /// </summary>
        Task<int> GetTotalCountByJobIdAsync(Guid jobId, ExecutionStatus? status = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取最近执行失败的日志
        /// </summary>
        Task<IEnumerable<JobExecutionLog>> GetRecentFailedLogsAsync(int hours = 24, int limit = 50, CancellationToken cancellationToken = default);

        /// <summary>
        /// 清理指定时间之前的执行日志
        /// </summary>
        Task<int> CleanupLogsAsync(DateTime beforeTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取任务执行统计信息
        /// </summary>
        Task<IDictionary<ExecutionStatus, int>> GetExecutionStatisticsAsync(Guid? jobId = null, DateTime? startTime = null, DateTime? endTime = null, CancellationToken cancellationToken = default);
    }
}