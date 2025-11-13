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
    /// 删除任务命令处理程序
    /// </summary>
    public class DeleteJobCommandHandler : CommandHandlerBase<DeleteJobCommand, bool>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public DeleteJobCommandHandler(
            ILogger<DeleteJobCommandHandler> logger,
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

        public override async Task<bool> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
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

                // 获取任务关联的调度计划
                var schedules = await _scheduleRepository.GetByJobIdAsync(request.JobId, cancellationToken);

                // 删除相关的调度计划（取消Hangfire调度）
                foreach (var schedule in schedules)
                {
                    await _jobService.DeleteScheduleAsync(schedule.Id, request.UpdatedBy);
                }

                // 删除任务
                await _jobRepository.DeleteAsync(job, cancellationToken);

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.UpdatedBy,
                    "系统管理员",
                    OperationType.Delete,
                    "Job",
                    $"删除任务: {job.Name}",
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
                    OperationType.Delete,
                    "Job",
                    $"删除任务失败: {request.JobId}",
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