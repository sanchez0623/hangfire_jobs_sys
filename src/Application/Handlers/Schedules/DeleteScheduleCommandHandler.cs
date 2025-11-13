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
    /// 删除调度计划命令处理程序
    /// </summary>
    public class DeleteScheduleCommandHandler : CommandHandlerBase<DeleteScheduleCommand, bool>
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public DeleteScheduleCommandHandler(
            ILogger<DeleteScheduleCommandHandler> logger,
            IScheduleRepository scheduleRepository,
            IJobService jobService,
            IOperationLogService operationLogService)
            : base(logger)
        {
            _scheduleRepository = scheduleRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
        }

        public override async Task<bool> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
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

                // 取消Hangfire中的调度
                if (!string.IsNullOrEmpty(schedule.HangfireJobId))
                {
                    await _jobService.CancelScheduledJobAsync(schedule.HangfireJobId);
                }

                // 删除调度计划
                await _scheduleRepository.DeleteAsync(request.ScheduleId, cancellationToken);

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    null, // 使用null代替不存在的OperatorId属性
                    "系统管理员",
                    OperationType.Delete,
                    "Schedule",
                    $"删除调度计划",
                    request.ScheduleId,
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
                    null, // 使用null代替不存在的OperatorId属性
                    "系统管理员",
                    OperationType.Delete,
                    "Schedule",
                    $"删除调度计划失败",
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