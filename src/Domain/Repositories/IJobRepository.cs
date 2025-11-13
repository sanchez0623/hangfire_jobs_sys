using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Repositories
{
    /// <summary>
    /// 任务仓储接口
    /// </summary>
    public interface IJobRepository
    {
        /// <summary>
        /// 添加任务
        /// </summary>
        Task AddAsync(Job job, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新任务
        /// </summary>
        Task UpdateAsync(Job job, CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除任务
        /// </summary>
        Task DeleteAsync(Job job, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取任务
        /// </summary>
        Task<Job> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取任务（包含调度计划）
        /// </summary>
        Task<Job> GetByIdWithSchedulesAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取任务（包含执行日志）
        /// </summary>
        Task<Job> GetByIdWithLogsAsync(Guid id, int logPage = 1, int logPageSize = 20, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取任务列表
        /// </summary>
        Task<IEnumerable<Job>> GetListAsync(
            JobStatus? status = null,
            string nameKeyword = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取任务总数
        /// </summary>
        Task<int> GetTotalCountAsync(JobStatus? status = null, string nameKeyword = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量更新任务状态
        /// </summary>
        Task BatchUpdateStatusAsync(IEnumerable<Guid> jobIds, JobStatus status, Guid updatedBy, CancellationToken cancellationToken = default);
    }
}