using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Repositories
{
    /// <summary>
    /// 操作日志仓储接口
    /// </summary>
    public interface IOperationLogRepository
    {
        /// <summary>
        /// 添加操作日志
        /// </summary>
        Task AddAsync(OperationLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新操作日志
        /// </summary>
        Task UpdateAsync(OperationLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取操作日志
        /// </summary>
        Task<OperationLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取操作日志列表
        /// </summary>
        Task<IEnumerable<OperationLog>> GetListAsync(
            Guid? operatorId = null,
            OperationType? type = null,
            string module = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            OperationStatus? status = null,
            string keyword = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取操作日志总数
        /// </summary>
        Task<int> GetTotalCountAsync(
            Guid? operatorId = null,
            OperationType? type = null,
            string module = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            OperationStatus? status = null,
            string keyword = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 清理指定时间之前的操作日志
        /// </summary>
        Task<int> CleanupLogsAsync(DateTime beforeTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取操作类型统计
        /// </summary>
        Task<IDictionary<string, int>> GetTypeStatisticsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取模块操作统计
        /// </summary>
        Task<IDictionary<string, int>> GetModuleStatisticsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取用户操作统计
        /// </summary>
        Task<IDictionary<string, int>> GetUserStatisticsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    }
}