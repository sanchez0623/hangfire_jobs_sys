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
    /// 创建Cron调度计划命令处理程序
    /// </summary>
    public class CreateCronScheduleCommandHandler : CommandHandlerBase<CreateCronScheduleCommand, Guid>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public CreateCronScheduleCommandHandler(
            ILogger<CreateCronScheduleCommandHandler> logger,
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

        public override async Task<Guid> Handle(CreateCronScheduleCommand request, CancellationToken cancellationToken)
        {
            LogCommandStart(request);

            try
            {
                // 验证任务是否存在
                var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
                if (job == null)
                {
                    throw new System.Exception($"任务不存在: {request.JobId}");
                }

                // 验证Cron表达式（简单验证非空）
                if (string.IsNullOrEmpty(request.CronExpression))
                {
                    throw new System.Exception("Cron表达式不能为空");
                }

                // 创建Cron调度计划
                var schedule = Schedule.CreateCronSchedule(
                    request.JobId,
                    request.CronExpression,
                    request.StartTime,
                    request.EndTime,
                    request.CreatedBy);
                
                // 激活调度计划
                schedule.Activate(request.CreatedBy);

                // 保存调度计划
                await _scheduleRepository.AddAsync(schedule, cancellationToken);

                // 如果任务是激活状态，立即调度
                if (job.Status == JobStatus.Active)
                {
                    var hangfireJobId = await _jobService.ScheduleJobAsync(job.Id, schedule.CronExpression, request.StartTime, request.EndTime);
                    schedule.AssociateHangfireJobId(hangfireJobId);
                    await _scheduleRepository.UpdateAsync(schedule, cancellationToken);
                }

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.CreatedBy,
                    "系统管理员",
                    OperationType.Create,
                    "Schedule",
                    $"创建Cron调度计划: {job.Name}",
                    schedule.Id,
                    JsonSerializer.Serialize(request),
                    request.ClientIp,
                    request.ClientBrowser);

                LogCommandCompleted(request, schedule.Id);
                return schedule.Id;
            }
            catch (System.Exception ex)
            {
                LogCommandError(request, ex);
                
                // 记录失败的操作日志
                await _operationLogService.LogFailedOperationAsync(
                    request.CreatedBy,
                    "系统管理员",
                    OperationType.Create,
                    "Schedule",
                    $"创建Cron调度计划失败",
                    ex.Message,
                    null,
                    JsonSerializer.Serialize(request),
                    request.ClientIp,
                    request.ClientBrowser);

                throw;
            }
        }
    }
}