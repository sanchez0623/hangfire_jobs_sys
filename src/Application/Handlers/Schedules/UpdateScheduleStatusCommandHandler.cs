using HangfireJobsSys.Application.Commands.Schedules;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireJobsSys.Application.Handlers.Schedules
{
    /// <summary>
    /// 更新调度计划状态命令处理程序
    /// </summary>
    public class UpdateScheduleStatusCommandHandler : CommandHandlerBase<UpdateScheduleStatusCommand, bool>
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public UpdateScheduleStatusCommandHandler(
            ILogger<UpdateScheduleStatusCommandHandler> logger,
            IScheduleRepository scheduleRepository,
            IJobRepository jobRepository,
            IJobService jobService,
            IOperationLogService operationLogService)
            : base(logger)
        {
            _scheduleRepository = scheduleRepository;
            _jobRepository = jobRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
        }

        public override async Task<bool> Handle(UpdateScheduleStatusCommand request, CancellationToken cancellationToken)
        {
            LogCommandStart(request);

            try
            {
                // 获取调度计划
                var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
                if (schedule == null)
                {
                    throw new System.Exception($"调度计划不存在: {request.ScheduleId}");
                }

                // 获取关联的任务
                var job = await _jobRepository.GetByIdAsync(schedule.JobId, cancellationToken);
                if (job == null)
                {
                    throw new System.Exception($"任务不存在: {schedule.JobId}");
                }

                // 更新状态
                string previousStatus = schedule.Status.ToString();
                if (request.Status == "Active")
                {
                    schedule.Activate(request.UpdatedBy);
                }
                else if (request.Status == "Paused")
                {
                    schedule.Pause(request.UpdatedBy);
                }
                else if (request.Status == "Deleted")
                {
                    schedule.Delete(request.UpdatedBy);
                }
                await _scheduleRepository.UpdateAsync(schedule, cancellationToken);

                // 根据状态执行相应操作
                if (request.Status == "Active")
                {
                    // 激活调度计划
                    if (!string.IsNullOrEmpty(schedule.HangfireJobId))
                    {
                        await _jobService.CancelScheduledJobAsync(schedule.HangfireJobId);
                    }
                    
                    // 如果任务也是激活状态，调用服务层方法激活调度计划
                    if (job.Status == JobStatus.Active)
                    {
                        var updatedSchedule = await _jobService.ActivateScheduleAsync(schedule.Id, request.UpdatedBy);
                        // 实际的调度逻辑会在服务层处理
                    }
                }
                else if (request.Status == "Paused" || request.Status == "Deleted")
                {
                    // 暂停或删除调度计划，取消Hangfire中的调度
                    if (!string.IsNullOrEmpty(schedule.HangfireJobId))
                    {
                        await _jobService.CancelScheduledJobAsync(schedule.HangfireJobId);
                        schedule.ClearHangfireJobId();
                        await _scheduleRepository.UpdateAsync(schedule, cancellationToken);
                    }
                }

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.UpdatedBy,
                    "系统管理员",
                    OperationType.Update,
                    "Schedule",
                    $"更新调度计划状态: {previousStatus} -> {request.Status}",
                    schedule.Id,
                    JsonSerializer.Serialize(request),
                    request.ClientIp,
                    request.ClientBrowser);

                LogCommandCompleted(request, true);
                return true;
            }
            catch (System.Exception ex)
            {
                LogCommandError(request, ex);
                
                // 记录失败的操作日志
                await _operationLogService.LogFailedOperationAsync(
                    request.UpdatedBy,
                    "系统管理员",
                    OperationType.Update,
                    "Schedule",
                    $"更新调度计划状态失败",
                    ex.Message,
                    request.ScheduleId,
                    JsonSerializer.Serialize(request),
                    request.ClientIp,
                    request.ClientBrowser);

                throw;
            }
        }
    }
}