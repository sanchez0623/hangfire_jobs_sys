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
    /// 创建间隔调度计划命令处理程序
    /// </summary>
    public class CreateIntervalScheduleCommandHandler : CommandHandlerBase<CreateIntervalScheduleCommand, Guid>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public CreateIntervalScheduleCommandHandler(
            ILogger<CreateIntervalScheduleCommandHandler> logger,
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

        public override async Task<Guid> Handle(CreateIntervalScheduleCommand request, CancellationToken cancellationToken)
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

                // 验证执行间隔
                if (request.IntervalSeconds < 5)
                {
                    throw new System.Exception("执行间隔不能小于5秒");
                }

                // 创建间隔调度计划
                var schedule = Schedule.CreateIntervalSchedule(
                    request.JobId,
                    request.IntervalSeconds,
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
                    // 使用服务层方法创建间隔调度
                    var cronExpression = $"*/{request.IntervalSeconds} * * * * *";
                    var hangfireJobId = await _jobService.ScheduleJobAsync(request.JobId, cronExpression, request.StartTime, request.EndTime);
                    schedule.AssociateHangfireJobId(hangfireJobId);
                    await _scheduleRepository.UpdateAsync(schedule, cancellationToken);
                }

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.CreatedBy,
                    "系统管理员",
                    OperationType.Create,
                    "Schedule",
                    $"创建间隔调度计划: {job.Name}",
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
                    $"创建间隔调度计划失败",
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