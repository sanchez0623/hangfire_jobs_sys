using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using HangfireJobsSys.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HangfireJobsSys.Infrastructure.Repositories
{
    /// <summary>
    /// 任务仓储EF Core实现
    /// </summary>
    public class JobRepository : IJobRepository
    {
        private readonly HangfireJobsSysDbContext _dbContext;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库上下文</param>
        /// <param name="cacheService">缓存服务</param>
        public JobRepository(HangfireJobsSysDbContext dbContext, ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }
        
        /// <summary>
        /// 清除任务相关的所有缓存
        /// </summary>
        private async Task ClearJobCacheAsync(Guid jobId)
        {
            // 清除单个任务缓存
            await _cacheService.RemoveAsync(CacheKeyFactory.GetJobKey(jobId));
            
            // 清除任务列表缓存（使用模式匹配）
            await _cacheService.RemoveByPatternAsync($"{CacheKeyFactory.GetPatternKey("jobs:*")}");
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
        {
            await _dbContext.Jobs.AddAsync(job, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量添加任务（单次SaveChanges）
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<Job> jobs, CancellationToken cancellationToken = default)
        {
            await _dbContext.Jobs.AddRangeAsync(jobs, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 更新任务
        /// </summary>
        public async Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
        {
            _dbContext.Jobs.Update(job);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量更新任务（单次SaveChanges）
        /// </summary>
        public async Task UpdateRangeAsync(IEnumerable<Job> jobs, CancellationToken cancellationToken = default)
        {
            _dbContext.Jobs.UpdateRange(jobs);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        public async Task DeleteAsync(Job job, CancellationToken cancellationToken = default)
        {
            _dbContext.Jobs.Remove(job);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量删除任务（单次SaveChanges）
        /// </summary>
        public async Task DeleteRangeAsync(IEnumerable<Job> jobs, CancellationToken cancellationToken = default)
        {
            _dbContext.Jobs.RemoveRange(jobs);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 使用SQL批量删除任务（高效）
        /// </summary>
        public async Task BulkDeleteAsync(IEnumerable<Guid> jobIds, CancellationToken cancellationToken = default)
        {
            var ids = jobIds.ToList();
            if (ids.Count == 0) return;

            // 使用ExecuteSqlInterpolated进行高效的批量删除
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM Jobs WHERE Id IN ({string.Join(",", ids.Select(id => $"'{id}'"))})",
                cancellationToken);
        }

        /// <summary>
        /// 根据ID获取任务
        /// </summary>
        public async Task<Job> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // 尝试从缓存获取
            var cacheKey = CacheKeyFactory.GetJobKey(id);
            var cachedJob = await _cacheService.GetAsync<Job>(cacheKey);
            
            if (cachedJob != null)
            {
                return cachedJob;
            }

            // 从数据库获取
            var job = await _dbContext.Jobs.FindAsync([id], cancellationToken);
            if (job == null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found");
            }

            // 设置缓存，缓存5分钟
            await _cacheService.SetAsync(cacheKey, job, TimeSpan.FromMinutes(5));
            
            return job;
        }

        /// <summary>
        /// 根据ID获取任务（包含调度计划）
        /// </summary>
        public async Task<Job> GetByIdWithSchedulesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var job = await _dbContext.Jobs
                .Include(j => j.Schedules)
                .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
            if (job == null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found");
            }
            return job;
        }

        /// <summary>
        /// 根据ID获取任务（包含执行日志）
        /// </summary>
        public async Task<Job> GetByIdWithLogsAsync(Guid id, int logPage = 1, int logPageSize = 20, CancellationToken cancellationToken = default)
        {
            var job = await _dbContext.Jobs.FindAsync([id], cancellationToken);
            if (job == null)
            {
                throw new KeyNotFoundException($"Task with id {id} not found");
            }
            return job;
        }

        /// <summary>
        /// 获取任务列表
        /// </summary>
        public async Task<IEnumerable<Job>> GetListAsync(
            JobStatus? status = null,
            string nameKeyword = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // 尝试从缓存获取
            var cacheKey = CacheKeyFactory.GetJobListKey(status?.ToString(), nameKeyword, page, pageSize);
            var cachedResult = await _cacheService.GetAsync<List<Job>>(cacheKey);
            
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var query = _dbContext.Jobs.AsQueryable();

            // 应用过滤条件
            if (status.HasValue)
            {
                query = query.Where(j => j.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(nameKeyword))
            {
                query = query.Where(j => j.Name.Contains(nameKeyword));
            }

            // 排序和分页
            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
                
            // 设置缓存，缓存2分钟（列表数据更新频率较高）
            await _cacheService.SetAsync(cacheKey, jobs, TimeSpan.FromMinutes(2));
                
            return jobs;
        }

        /// <summary>
        /// 获取任务总数
        /// </summary>
        public async Task<int> GetTotalCountAsync(JobStatus? status = null, string nameKeyword = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Jobs.AsQueryable();

            // 应用过滤条件
            if (status.HasValue)
            {
                query = query.Where(j => j.Status == status.Value);
            }

            if (!string.IsNullOrEmpty(nameKeyword))
            {
                query = query.Where(j => j.Name.Contains(nameKeyword));
            }

            return await query.CountAsync(cancellationToken);
        }

        /// <summary>
        /// 批量更新任务状态（使用EF Core批量更新）
        /// </summary>
        public async Task BatchUpdateStatusAsync(IEnumerable<Guid> jobIds, JobStatus status, Guid updatedBy, CancellationToken cancellationToken = default)
        {
            var jobsToUpdate = await _dbContext.Jobs
                .Where(j => jobIds.Contains(j.Id))
                .ToListAsync(cancellationToken);

            foreach (var job in jobsToUpdate)
            {
                // 根据status值调用相应的方法
                switch (status)
                {   
                    case JobStatus.Active:
                        job.Activate(updatedBy);
                        break;
                    case JobStatus.Paused:
                        job.Pause(updatedBy);
                        break;
                    case JobStatus.Deleted:
                        job.Delete(updatedBy);
                        break;
                }
            }

            // 使用UpdateRange优化批量更新性能
            _dbContext.Jobs.UpdateRange(jobsToUpdate);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 批量更新任务状态（使用SQL直接更新，高效）
        /// </summary>
        public async Task BulkUpdateStatusAsync(IEnumerable<Guid> jobIds, JobStatus status, Guid updatedBy, CancellationToken cancellationToken = default)
        {
            var ids = jobIds.ToList();
            if (ids.Count == 0) return;

            // 获取当前时间
            var now = DateTime.UtcNow;
            
            // 构建IN子句的参数
            var idList = string.Join(",", ids.Select(id => $"'{id}'"));
            
            // 构建UPDATE语句
            var sql = $@"UPDATE Jobs 
                         SET Status = {(int)status}, 
                             UpdatedAt = '{now}', 
                             UpdatedBy = '{updatedBy}' 
                         WHERE Id IN ({idList})";
            
            // 执行批量更新
            await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
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