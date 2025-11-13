using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HangfireJobsSys.Infrastructure.Repositories
{
    /// <summary>
    /// 审计日志仓储实现
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly HangfireJobsSysDbContext _context;

        public AuditLogRepository(HangfireJobsSysDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
        {
            await _context.AuditLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<AuditLog> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetListAsync(
            string action = null,
            string entityType = null,
            Guid? entityId = null,
            string userId = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action.Contains(action));

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(l => l.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(l => l.EntityId == entityId.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (startTime.HasValue)
                query = query.Where(l => l.CreatedAt >= startTime.Value);

            if (endTime.HasValue)
                query = query.Where(l => l.CreatedAt <= endTime.Value);

            // 分页
            var skip = (page - 1) * pageSize;
            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
    }
}