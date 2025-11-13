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
    /// 更新任务命令处理程序
    /// </summary>
    public class UpdateJobCommandHandler : CommandHandlerBase<UpdateJobCommand, bool>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public UpdateJobCommandHandler(
            ILogger<UpdateJobCommandHandler> logger,
            IJobRepository jobRepository,
            IJobService jobService,
            IOperationLogService operationLogService)
            : base(logger)
        {
            _jobRepository = jobRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
        }

        public override async Task<bool> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
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

                // 跳过任务类型验证，因为IJobService中没有ValidateJobTypeAsync方法

                // 更新任务信息
                job.Update(
                    request.Name,
                    request.Description,
                    request.JobTypeName,
                    request.Parameters,
                    request.UpdatedBy);

                // 保存更新
                await _jobRepository.UpdateAsync(job, cancellationToken);

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.UpdatedBy,
                    "系统管理员",
                    OperationType.Update,
                    "Job",
                    $"更新任务: {job.Name}",
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
                    $"更新任务失败: {request.JobId}",
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