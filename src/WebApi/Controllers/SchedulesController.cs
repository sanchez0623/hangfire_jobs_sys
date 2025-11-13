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
    /// 调度管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // 添加授权属性，确保只有已认证用户才能访问
    public class SchedulesController : ControllerBase
    {
        /// <summary>
        /// 获取当前登录用户ID
        /// </summary>
        /// <returns>用户ID，如果未认证则抛出异常</returns>
        protected Guid GetCurrentUserId()
        {
            // 从ClaimsPrincipal中获取用户ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("无法获取用户身份信息");
            }
            
            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new InvalidOperationException("用户ID格式无效");
            }
            
            return userId;
        }
        
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobRepository _jobRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SchedulesController(IScheduleRepository scheduleRepository, IJobRepository jobRepository)
        {
            _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
            _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        }

        /// <summary>
        /// 获取调度列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedules()
        {
            try
            {
                var schedules = await _scheduleRepository.GetListAsync();
                var scheduleDtos = schedules.Select(schedule => new ScheduleDto
                {
                    Id = schedule.Id,
                    JobId = schedule.JobId,
                    CronExpression = schedule.CronExpression,
                    IsEnabled = schedule.Status == ScheduleStatus.Active,
                    // 移除不存在的属性映射
                    // LastExecutionTime 和 NextExecutionTime 在Schedule实体中不存在
                    CreatedAt = schedule.CreatedAt,
                    UpdatedAt = schedule.UpdatedAt
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取调度列表失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 获取任务的调度列表
        /// </summary>
        [HttpGet("job/{jobId}")]
        public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetSchedulesByJobId(Guid jobId)
        {
            try
            {
                var schedules = await _scheduleRepository.GetByJobIdAsync(jobId);
                var scheduleDtos = schedules.Select(schedule => new ScheduleDto
                {
                    Id = schedule.Id,
                    JobId = schedule.JobId,
                    CronExpression = schedule.CronExpression,
                    IsEnabled = schedule.Status == ScheduleStatus.Active,
                    // 移除不存在的属性映射
                    // LastExecutionTime 和 NextExecutionTime 在Schedule实体中不存在
                    CreatedAt = schedule.CreatedAt,
                    UpdatedAt = schedule.UpdatedAt
                }).ToList();

                return Ok(scheduleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取任务调度列表失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 更新调度状态
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ScheduleDto>> UpdateScheduleStatus(Guid id, [FromBody] bool isEnabled)
        {
            try
            {
                var schedule = await _scheduleRepository.GetByIdAsync(id);
                if (schedule == null)
                {
                    return NotFound();
                }

                // 由于Status是只读属性，我们需要使用正确的方法来修改状态
                // 从认证上下文获取当前登录用户ID
                var updatedBy = GetCurrentUserId();
                
                if (isEnabled)
                {
                    schedule.Activate(updatedBy);
                }
                else
                {
                    schedule.Pause(updatedBy);
                }
                await _scheduleRepository.UpdateAsync(schedule);

                var scheduleDto = new ScheduleDto
                {
                    Id = schedule.Id,
                    JobId = schedule.JobId,
                    CronExpression = schedule.CronExpression,
                    IsEnabled = schedule.Status == ScheduleStatus.Active,
                    // 移除不存在的属性映射
                    // LastExecutionTime 和 NextExecutionTime 在Schedule实体中不存在
                    CreatedAt = schedule.CreatedAt,
                    UpdatedAt = schedule.UpdatedAt
                };

                return Ok(scheduleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新调度状态失败：" + ex.Message });
            }
        }

        /// <summary>
        /// 删除调度
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSchedule(Guid id)
        {
            try
            {
                var schedule = await _scheduleRepository.GetByIdAsync(id);
                if (schedule == null)
                {
                    return NotFound();
                }

                await _scheduleRepository.DeleteAsync(schedule.Id);
                return Ok(new { message = "调度删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "删除调度失败：" + ex.Message });
            }
        }
    }
}