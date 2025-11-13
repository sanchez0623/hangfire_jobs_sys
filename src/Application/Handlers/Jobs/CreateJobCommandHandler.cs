using HangfireJobsSys.Application.Commands.Jobs;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireJobsSys.Application.Handlers.Jobs
{
    /// <summary>
    /// 创建任务命令处理程序
    /// </summary>
    public class CreateJobCommandHandler : CommandHandlerBase<CreateJobCommand, Guid>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobService _jobService;
        private readonly IOperationLogService _operationLogService;

        public CreateJobCommandHandler(
            ILogger<CreateJobCommandHandler> logger,
            IJobRepository jobRepository,
            IJobService jobService,
            IOperationLogService operationLogService)
            : base(logger)
        {
            _jobRepository = jobRepository;
            _jobService = jobService;
            _operationLogService = operationLogService;
        }

        public override async Task<Guid> Handle(CreateJobCommand request, CancellationToken cancellationToken)
        {
            LogCommandStart(request);

            try
            {
                // 任务类型验证将在保存时进行

                // 创建任务
                var job = Job.Create(
                    request.Name,
                    request.Description ?? string.Empty,
                    request.JobTypeName,
                    request.Parameters ?? string.Empty,
                    request.CreatedBy);

                // 保存任务
                await _jobRepository.AddAsync(job, cancellationToken);

                // 记录操作日志
                await _operationLogService.LogOperationAsync(
                    request.CreatedBy,
                    "系统管理员",
                    OperationType.Create,
                    "Job",
                    $"创建任务: {request.Name}",
                    job.Id,
                    JsonSerializer.Serialize(request),
                    request.ClientIp ?? string.Empty,
                    request.ClientBrowser ?? string.Empty);

                LogCommandCompleted(request, job.Id);
                return job.Id;
            }
            catch (System.Exception ex)
            {
                LogCommandError(request, ex);
                
                // 记录失败的操作日志
                await _operationLogService.LogFailedOperationAsync(
                    request.CreatedBy,
                    "系统管理员",
                    OperationType.Create,
                    "Job",
                    $"创建任务失败: {request.Name}",
                    ex.Message,
                    null,
                    JsonSerializer.Serialize(request),
                    request.ClientIp ?? string.Empty,
                    request.ClientBrowser ?? string.Empty);

                throw;
            }
        }
    }
}