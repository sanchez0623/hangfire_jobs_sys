using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Repositories
{
    /// <summary>
    /// 调度计划仓储接口
    /// </summary>
    public interface IScheduleRepository
    {
        /// <summary>
        /// 添加调度计划
        /// </summary>
        Task AddAsync(Schedule schedule, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新调度计划
        /// </summary>
        Task UpdateAsync(Schedule schedule, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID删除调度计划
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取调度计划
        /// </summary>
        Task<Schedule> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据任务ID获取调度计划列表
        /// </summary>
        Task<IEnumerable<Schedule>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取调度计划列表
        /// </summary>
        Task<IEnumerable<Schedule>> GetListAsync(
            Guid? jobId = null,
            ScheduleType? type = null,
            ScheduleStatus? status = null,
            string createdBy = null,
            DateTime? startCreateTime = null,
            DateTime? endCreateTime = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取调度计划总数
        /// </summary>
        Task<int> GetTotalCountAsync(
            Guid? jobId = null,
            ScheduleType? type = null,
            ScheduleStatus? status = null,
            string createdBy = null,
            DateTime? startCreateTime = null,
            DateTime? endCreateTime = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量更新调度计划状态
        /// </summary>
        Task BatchUpdateStatusAsync(IEnumerable<Guid> scheduleIds, ScheduleStatus status, Guid updatedBy, CancellationToken cancellationToken = default);
    }
}