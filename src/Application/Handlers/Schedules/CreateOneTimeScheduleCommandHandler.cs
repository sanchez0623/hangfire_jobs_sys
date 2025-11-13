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
    /// 创建一次性调度计划命令处理程序
    /// </summary>
    public class CreateOneTimeScheduleCommandHandler : CommandHandlerBase<CreateOneTimeScheduleCommand, Guid>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public CreateOneTimeScheduleCommandHandler(
            ILogger<CreateOneTimeScheduleCommandHandler> logger,
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

        public override async Task<Guid> Handle(CreateOneTimeScheduleCommand request, CancellationToken cancellationToken)
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

                // 验证执行时间
                if (request.ExecuteTime <= DateTime.UtcNow)
                {
                    throw new System.Exception("执行时间必须大于当前时间");
                }

                // 创建一次性调度计划（使用Cron方式）
                // 为一次性执行创建特定的Cron表达式
                var cronExpression = $"{request.ExecuteTime.Second} {request.ExecuteTime.Minute} {request.ExecuteTime.Hour} {request.ExecuteTime.Day} {request.ExecuteTime.Month} ? {request.ExecuteTime.Year}";
                var schedule = Schedule.CreateCronSchedule(
                    request.JobId,
                    cronExpression,
                    request.ExecuteTime,
                    null,
                    request.CreatedBy);
                
                // 激活调度计划
                schedule.Activate(request.CreatedBy);

                // 保存调度计划
                await _scheduleRepository.AddAsync(schedule, cancellationToken);

                // 如果任务是激活状态，立即调度
                if (job.Status == JobStatus.Active)
                {
                    // 计算执行时间与当前时间的差值
                    var delay = request.ExecuteTime - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                    {
                    // 直接使用schedule中的Cron表达式
                    var hangfireJobId = await _jobService.ScheduleJobAsync(
                        request.JobId,
                        schedule.CronExpression,
                        request.ExecuteTime,
                        null);
                    schedule.AssociateHangfireJobId(hangfireJobId);
                    await _scheduleRepository.UpdateAsync(schedule, cancellationToken);
                    }
                }

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.CreatedBy,
                    "系统管理员",
                    OperationType.Create,
                    "Schedule",
                    $"创建一次性调度计划: {job.Name}",
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
                    $"创建一次性调度计划失败",
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