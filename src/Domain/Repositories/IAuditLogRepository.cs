using System;using System.Collections.Generic;using System.Threading;using System.Threading.Tasks;using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Repositories
{
    /// <summary>
    /// 审计日志仓储接口
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>
        /// 添加审计日志
        /// </summary>
        Task AddAsync(AuditLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ID获取审计日志
        /// </summary>
        Task<AuditLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取审计日志列表
        /// </summary>
        Task<IEnumerable<AuditLog>> GetListAsync(
            string action = null,
            string entityType = null,
            Guid? entityId = null,
            string userId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}