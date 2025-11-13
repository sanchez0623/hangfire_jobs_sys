using HangfireJobsSys.Application.Commands.Jobs;
using HangfireJobsSys.Domain.Entities;
// OperationType is defined in HangfireJobsSys.Domain.Entities namespace
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
    /// 激活任务命令处理程序
    /// </summary>
    public class ActivateJobCommandHandler : CommandHandlerBase<ActivateJobCommand, bool>
    {
        private readonly IJobRepository _jobRepository;
    private readonly IJobService _jobService;
    private readonly IOperationLogService _operationLogService;
    private readonly IScheduleRepository _scheduleRepository;

        public ActivateJobCommandHandler(
            ILogger<ActivateJobCommandHandler> logger,
            IJobRepository jobRepository,
            IJobService jobService,
            IOperationLogService operationLogService,
            IScheduleRepository scheduleRepository)
            : base(logger)
        {
            _jobRepository = jobRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
            _scheduleRepository = scheduleRepository;
        }

        public override async Task<bool> Handle(ActivateJobCommand request, CancellationToken cancellationToken)
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

                // 激活任务
                job.Activate(request.UpdatedBy);

                // 保存更新
                await _jobRepository.UpdateAsync(job, cancellationToken);

                // 获取并激活相关的调度计划
                var schedules = await _scheduleRepository.GetByJobIdAsync(job.Id, cancellationToken);
                foreach (var scheduleItem in schedules.Where(s => s.Status != ScheduleStatus.Deleted))
                {
                    // 直接更新状态为Active
                    scheduleItem.Activate(request.UpdatedBy);
                    await _scheduleRepository.UpdateAsync(scheduleItem, cancellationToken);
                }

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.UpdatedBy,
                    "系统管理员",
                    OperationType.Activate,
                    "Job",
                    $"激活任务: {job.Name}",
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
                    OperationType.Activate,
                    "Job",
                    $"激活任务失败: {request.JobId}",
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