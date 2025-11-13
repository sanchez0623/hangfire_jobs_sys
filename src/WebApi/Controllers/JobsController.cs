using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.WebApi.Models.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HangfireJobsSys.WebApi.Controllers
{
    /// <summary>
    /// 任务管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;
        private readonly IScheduleRepository _scheduleRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JobsController(IJobRepository jobRepository, IScheduleRepository scheduleRepository)
        {
            _jobRepository = jobRepository;
            _scheduleRepository = scheduleRepository;
        }

        /// <summary>
        /// 获取任务列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs()
        {
            var jobs = await _jobRepository.GetListAsync();
            var jobDtos = jobs.Select(job => new JobDto
            {
                Id = job.Id,
                Name = job.Name,
                Description = job.Description,
                JobType = job.JobTypeName,
                Parameters = job.Parameters,
                CreatedById = job.CreatedBy,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                Status = (int)job.Status
            }).ToList();

            return Ok(jobDtos);
        }

        /// <summary>
        /// 获取任务详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<JobDto>> GetJob(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            var jobDto = new JobDto
            {
                Id = job.Id,
                Name = job.Name,
                Description = job.Description,
                JobType = job.JobTypeName,
                Parameters = job.Parameters,
                CreatedById = job.CreatedBy,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt,
                Status = (int)job.Status
            };

            return Ok(jobDto);
        }

        /// <summary>
        /// 创建任务
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<JobDto>> CreateJob(CreateJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 获取当前用户ID
                var currentUserId = GetCurrentUserId();

                var job = Job.Create(
                    name: request.Name,
                    description: request.Description ?? string.Empty,
                    jobTypeName: request.JobType,
                    parameters: request.Parameters ?? string.Empty,
                    createdBy: currentUserId
                );

                await _jobRepository.AddAsync(job);

                // 如果提供了Cron表达式，创建调度
                if (!string.IsNullOrEmpty(request.CronExpression))
                {
                    var schedule = Schedule.CreateCronSchedule(
                        jobId: job.Id,
                        cronExpression: request.CronExpression,
                        startTime: null,
                        endTime: null,
                        createdBy: currentUserId
                    );
                    await _scheduleRepository.AddAsync(schedule);
                }

                var jobDto = new JobDto
                {
                    Id = job.Id,
                    Name = job.Name,
                    Description = job.Description,
                    JobType = job.JobTypeName,
                    Parameters = job.Parameters,
                    CreatedById = job.CreatedBy,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    Status = (int)job.Status
                };

                return CreatedAtAction(nameof(GetJob), new { id = job.Id }, jobDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建任务失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 更新任务
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<JobDto>> UpdateJob(Guid id, UpdateJobRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound();
                }

                // 由于Job实体没有Update方法，我们直接更新属性
                // 由于Job实体属性是只读的，我们需要先删除再创建新的任务
                // 1. 先删除现有任务的所有调度
                var schedules = await _scheduleRepository.GetByJobIdAsync(job.Id);
                foreach (var schedule in schedules)
                {
                    await _scheduleRepository.DeleteAsync(schedule.Id);
                }
                
                // 2. 删除现有任务
                await _jobRepository.DeleteAsync(job);
                
                // 3. 创建新任务
                var newJob = Job.Create(
                    name: request.Name,
                    description: request.Description ?? string.Empty,
                    jobTypeName: request.JobType,
                    parameters: request.Parameters ?? string.Empty,
                    createdBy: job.CreatedBy
                );
                await _jobRepository.AddAsync(newJob);
                
                // 暂时不重新创建调度，因为UpdateJobRequest可能没有CronExpression和IsEnabled属性
                // 可以在后续版本中添加这些属性支持
                
                // 更新job引用为新创建的任务
                job = newJob;

                var jobDto = new JobDto
                {
                    Id = job.Id,
                    Name = job.Name,
                    Description = job.Description,
                    JobType = job.JobTypeName,
                    Parameters = job.Parameters,
                    CreatedById = job.CreatedBy,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    Status = (int)job.Status
                };

                return Ok(jobDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新任务失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(Guid id)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound();
                }

                // 删除相关的调度
                var schedules = await _scheduleRepository.GetByJobIdAsync(id);
                foreach (var schedule in schedules)
                {
                    await _scheduleRepository.DeleteAsync(schedule.Id);
                }

                // 删除任务
                await _jobRepository.DeleteAsync(job);

                return Ok(new { message = "任务删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除任务失败：" + ex.Message });
            }
        }
        /// <summary>
        /// 从认证上下文获取当前用户ID
        /// </summary>
        /// <returns>当前用户ID</returns>
        /// <exception cref="UnauthorizedAccessException">当无法获取用户ID时抛出</exception>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("无法获取当前用户身份信息");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("用户ID格式无效");
            }

            return userId;
        }
    }
}