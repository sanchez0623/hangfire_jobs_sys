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
    /// 立即执行任务命令处理程序
    /// </summary>
    public class ExecuteJobImmediatelyCommandHandler : CommandHandlerBase<ExecuteJobImmediatelyCommand, string>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public ExecuteJobImmediatelyCommandHandler(
            ILogger<ExecuteJobImmediatelyCommandHandler> logger,
            IJobRepository jobRepository,
            IJobService jobService,
            IOperationLogService operationLogService)
            : base(logger)
        {
            _jobRepository = jobRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
        }

        public override async Task<string> Handle(ExecuteJobImmediatelyCommand request, CancellationToken cancellationToken)
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

                // 验证任务是否可执行
                if (job.Status != JobStatus.Active)
                {
                    throw new System.Exception($"任务未激活，无法执行: {job.Name}");
                }

                // 立即执行任务
                var jobId = await _jobService.ExecuteJobImmediatelyAsync(request.JobId, Guid.Empty); // 使用Guid.Empty代替字符串

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.ExecutorId,
                    "系统管理员",
                    OperationType.Execute,
                    "Job",
                    $"立即执行任务: {job.Name}",
                    job.Id,
                    JsonSerializer.Serialize(request),
                    request.ClientIp,
                    request.ClientBrowser);

                LogCommandCompleted(request, jobId);
                return jobId;
            }
            catch (System.Exception ex)
            {
                LogCommandError(request, ex);
                
                // 记录失败的操作日志
                await _operationLogService.LogFailedOperationAsync(
                    request.ExecutorId,
                    "系统管理员",
                    OperationType.Execute,
                    "Job",
                    $"立即执行任务失败: {request.JobId}",
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