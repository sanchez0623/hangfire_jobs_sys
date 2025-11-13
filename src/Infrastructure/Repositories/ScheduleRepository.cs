using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HangfireJobsSys.Infrastructure.Repositories
{
    /// <summary>
    /// 调度计划仓储EF Core实现
    /// </summary>
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly HangfireJobsSysDbContext _dbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        public ScheduleRepository(HangfireJobsSysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 添加调度计划
        /// </summary>
        public async Task AddAsync(Schedule schedule, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _dbContext.Schedules.AddAsync(schedule, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量添加调度计划（单次SaveChanges）
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<Schedule> schedules, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _dbContext.Schedules.AddRangeAsync(schedules, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 更新调度计划
        /// </summary>
        public async Task UpdateAsync(Schedule schedule, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _dbContext.Schedules.Update(schedule);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量更新调度计划（单次SaveChanges）
        /// </summary>
        public async Task UpdateRangeAsync(IEnumerable<Schedule> schedules, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _dbContext.Schedules.UpdateRange(schedules);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 根据ID删除调度计划
        /// </summary>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var schedule = await _dbContext.Schedules.FindAsync([id], cancellationToken);
            if (schedule != null)
            {
                _dbContext.Schedules.Remove(schedule);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 批量删除调度计划（单次SaveChanges）
        /// </summary>
        public async Task DeleteRangeAsync(IEnumerable<Guid> scheduleIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var schedules = await _dbContext.Schedules
                .Where(s => scheduleIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            
            _dbContext.Schedules.RemoveRange(schedules);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 使用SQL批量删除调度计划（高效）
        /// </summary>
        public async Task BulkDeleteAsync(IEnumerable<Guid> scheduleIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = scheduleIds.ToList();
            if (ids.Count == 0) return;

            // 使用ExecuteSqlInterpolated进行高效的批量删除
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM Schedules WHERE Id IN ({string.Join(",", ids.Select(id => $"'{id}'"))})",
                cancellationToken);
        }

        /// <summary>
        /// 根据ID获取调度计划
        /// </summary>
        public async Task<Schedule> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var schedule = await _dbContext.Schedules.FindAsync([id], cancellationToken);
            if (schedule == null)
            {
                throw new KeyNotFoundException($"Schedule with id {id} not found");
            }
            return schedule;
        }
        
        /// <summary>
        /// 根据作业ID获取调度计划列表
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _dbContext.Schedules
                .Where(s => s.JobId == jobId)
                .AsNoTracking() // 只读查询使用NoTracking提升性能
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 根据多个作业ID批量获取调度计划
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetByJobIdsAsync(IEnumerable<Guid> jobIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = jobIds.ToList();
            if (ids.Count == 0) return new List<Schedule>();

            return await _dbContext.Schedules
                .Where(s => ids.Contains(s.JobId))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取调度计划列表
        /// </summary>
        public async Task<IEnumerable<Schedule>> GetListAsync(
            Guid? jobId = null,
            ScheduleType? type = null,
            ScheduleStatus? status = null,
            string createdBy = null,
            DateTime? startCreateTime = null,
            DateTime? endCreateTime = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var query = _dbContext.Schedules.AsNoTracking().AsQueryable();
            
            // 应用过滤条件
            if (jobId.HasValue)
            {
                query = query.Where(s => s.JobId == jobId.Value);
            }
            
            if (type.HasValue)
            {
                query = query.Where(s => s.Type == type.Value);
            }
            
            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }
            
            if (!string.IsNullOrEmpty(createdBy))
            {
                query = query.Where(s => s.CreatedBy.ToString() == createdBy);
            }
            
            if (startCreateTime.HasValue)
            {
                query = query.Where(s => s.CreatedAt >= startCreateTime.Value);
            }
            
            if (endCreateTime.HasValue)
            {
                query = query.Where(s => s.CreatedAt <= endCreateTime.Value);
            }
            
            // 排序和分页
            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取调度计划总数
        /// </summary>
        public async Task<int> GetTotalCountAsync(
            Guid? jobId = null,
            ScheduleType? type = null,
            ScheduleStatus? status = null,
            string createdBy = null,
            DateTime? startCreateTime = null,
            DateTime? endCreateTime = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var query = _dbContext.Schedules.AsNoTracking().AsQueryable();
            
            // 应用过滤条件
            if (jobId.HasValue)
            {
                query = query.Where(s => s.JobId == jobId.Value);
            }
            
            if (type.HasValue)
            {
                query = query.Where(s => s.Type == type.Value);
            }
            
            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }
            
            if (!string.IsNullOrEmpty(createdBy))
            {
                query = query.Where(s => s.CreatedBy.ToString() == createdBy);
            }
            
            if (startCreateTime.HasValue)
            {
                query = query.Where(s => s.CreatedAt >= startCreateTime.Value);
            }
            
            if (endCreateTime.HasValue)
            {
                query = query.Where(s => s.CreatedAt <= endCreateTime.Value);
            }
            
            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// 批量更新调度计划状态（使用EF Core批量更新）
        /// </summary>
        public async Task BatchUpdateStatusAsync(IEnumerable<Guid> scheduleIds, ScheduleStatus status, Guid updatedBy, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var ids = scheduleIds.ToHashSet();
            var schedulesToUpdate = await _dbContext.Schedules
                .Where(s => ids.Contains(s.Id))
                .ToListAsync(cancellationToken);
            
            foreach (var schedule in schedulesToUpdate)
            {
                // 根据状态调用相应的方法
                if (status == ScheduleStatus.Active)
                {
                    schedule.Activate(updatedBy);
                }
                else if (status == ScheduleStatus.Paused)
                {
                    schedule.Pause(updatedBy);
                }
                else if (status == ScheduleStatus.Deleted)
                {
                    schedule.Delete(updatedBy);
                }
            }
            
            // 使用UpdateRange优化批量更新性能
            _dbContext.Schedules.UpdateRange(schedulesToUpdate);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量更新调度计划状态（使用SQL直接更新，高效）
        /// </summary>
        public async Task BulkUpdateStatusAsync(IEnumerable<Guid> scheduleIds, ScheduleStatus status, Guid updatedBy, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = scheduleIds.ToList();
            if (ids.Count == 0) return;

            // 获取当前时间
            var now = DateTime.UtcNow;
            
            // 构建IN子句的参数
            var idList = string.Join(",", ids.Select(id => $"'{id}'"));
            
            // 构建UPDATE语句
            var sql = $@"UPDATE Schedules 
                         SET Status = {(int)status}, 
                             UpdatedAt = '{{{now}}}', 
                             UpdatedBy = '{{{updatedBy}}}' 
                         WHERE Id IN ({idList})";
            
            // 执行批量更新
            await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            try
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}