using HangfireJobsSys.Application.Commands.Jobs;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireJobsSys.Application.Handlers.Jobs
{
    /// <summary>
    /// 更新任务状态命令处理程序
    /// </summary>
    public class UpdateJobStatusCommandHandler : CommandHandlerBase<UpdateJobStatusCommand, bool>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public UpdateJobStatusCommandHandler(
            ILogger<UpdateJobStatusCommandHandler> logger,
            IJobRepository jobRepository,
            IScheduleRepository scheduleRepository,
            IJobService jobService,
            IOperationLogService operationLogService)
            : base(logger)
        {
            _jobRepository = jobRepository;
            _scheduleRepository = scheduleRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
        }

        public override async Task<bool> Handle(UpdateJobStatusCommand request, CancellationToken cancellationToken)
        {
            LogCommandStart(request);

            try
            {
                // 获取任务
                var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
                if (job == null)
                {
                    throw new System.Exception($"任务不存在: {request.JobId}");
                }

                // 记录旧状态
                string previousStatus = job.Status.ToString();

                // 更新任务状态
                if (request.Status == "Active")
                {
                    job.Activate(request.UpdatedBy);
                }
                else if (request.Status == "Paused")
                {
                    job.Pause(request.UpdatedBy);
                }
                else if (request.Status == "Deleted")
                {
                    job.Delete(request.UpdatedBy);
                }
                await _jobRepository.UpdateAsync(job, cancellationToken);

                // 获取关联的调度计划
                var schedules = await _scheduleRepository.GetByJobIdAsync(request.JobId, cancellationToken);

                if (request.Status == "Active")
                {
                    // 激活任务，重新调度所有激活状态的调度计划
                    foreach (var schedule in schedules.Where(s => s.Status == ScheduleStatus.Active))
                    {
                        if (!string.IsNullOrEmpty(schedule.HangfireJobId))
                        {
                            await _jobService.CancelScheduledJobAsync(schedule.HangfireJobId);
                        }
                        
                        // 根据调度类型调用不同的创建方法
                        if (schedule.Type == ScheduleType.Cron)
                        {
                            var updatedSchedule = await _jobService.ActivateScheduleAsync(schedule.Id, request.UpdatedBy);
                            // 实际的调度逻辑会在服务层处理
                        }
                    }
                }
                else if (request.Status == "Paused" || request.Status == "Deleted")
                {
                    // 暂停或删除任务，取消所有调度
                    foreach (var schedule in schedules.Where(s => !string.IsNullOrEmpty(s.HangfireJobId)))
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
                    "Job",
                    $"更新任务状态: {job.Name} ({previousStatus} -> {request.Status})",
                    job.Id,
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
                    "Job",
                    $"更新任务状态失败: {request.JobId}",
                    ex.Message,
                    request.JobId,
                    JsonSerializer.Serialize(request),
                    request.ClientIp,
                    request.ClientBrowser);

                throw;
            }
        }
    }
}