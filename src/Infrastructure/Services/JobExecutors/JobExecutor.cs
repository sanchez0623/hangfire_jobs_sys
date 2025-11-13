using System;
using System.Threading;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;
using HangfireJobsSys.Domain.Repositories;
using HangfireJobsSys.Domain.Services;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HangfireJobsSys.Infrastructure.Services.JobExecutors
{
    /// <summary>
    /// 任务执行配置
    /// </summary>
    public class JobExecutionOptions
    {
        /// <summary>
        /// 默认超时时间（秒）
        /// </summary>
        public const int DefaultTimeoutSeconds = 300;
        
        /// <summary>
        /// 默认最大重试次数
        /// </summary>
        public const int DefaultMaxRetries = 3;
        
        /// <summary>
        /// 默认重试间隔（秒）
        /// </summary>
        public const int DefaultRetryIntervalSeconds = 5;
    }

    /// <summary>
    /// 任务执行器
    /// </summary>
    public class JobExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobExecutor> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JobExecutor(
            IServiceProvider serviceProvider,
            ILogger<JobExecutor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="jobId">任务ID</param>
        /// <param name="parameters">额外参数</param>
        public async Task ExecuteAsync(Guid jobId, string parameters)
        {
            // 创建一个新的服务作用域，确保正确的依赖注入生命周期管理
            using var scope = _serviceProvider.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
            
            // 创建执行日志（使用空字符串作为执行ID，简化实现）
            var log = await jobService.LogJobExecutionStartAsync(jobId, string.Empty);

            int retryCount = 0;
            bool isRetry = false;
            Job job = null;
            int timeoutSeconds = JobExecutionOptions.DefaultTimeoutSeconds;
            int maxRetries = JobExecutionOptions.DefaultMaxRetries;

            try
            {
                // 获取任务信息
                job = await jobRepository.GetByIdAsync(jobId);
                if (job == null)
                {
                    throw new ArgumentException($"任务不存在: {jobId}");
                }

                // 检查任务状态
                if (job.Status != JobStatus.Active)
                {
                    throw new InvalidOperationException($"任务未激活，无法执行: {jobId}");
                }

                // 根据任务优先级调整配置
                timeoutSeconds = GetTimeoutByPriority(job.Priority);
                maxRetries = GetMaxRetriesByPriority(job.Priority);

                // 获取任务类型
                var jobType = Type.GetType(job.JobTypeName);
                if (jobType == null)
                {
                    throw new InvalidOperationException($"找不到任务类型: {job.JobTypeName}");
                }

                // 检查任务类型是否实现了IHangfireJob接口
                if (!typeof(IHangfireJob).IsAssignableFrom(jobType))
                {
                    throw new InvalidOperationException($"任务类型未实现IHangfireJob接口: {job.JobTypeName}");
                }

                // 执行任务（带重试机制）
                while (retryCount <= maxRetries)
                {
                    if (retryCount > 0)
                    {
                        isRetry = true;
                        _logger.LogInformation("重试执行任务: {JobId}, 第 {RetryCount}/{MaxRetries} 次尝试", 
                            jobId, retryCount, maxRetries);
                        
                        // 重试间隔（指数退避）
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(JobExecutionOptions.DefaultRetryIntervalSeconds, retryCount)), CancellationToken.None);
                    }
                    
                    try
                    {
                        _logger.LogInformation("开始执行任务: {JobId}, 超时设置: {Timeout}秒", jobId, timeoutSeconds);
                        
                        // 创建任务实例
                        var jobInstance = scope.ServiceProvider.GetRequiredService(jobType) as IHangfireJob;
                        if (jobInstance == null)
                        {
                            throw new InvalidOperationException($"无法创建任务实例: {job.JobTypeName}");
                        }

                        // 使用CancellationTokenSource实现超时控制
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                        
                        // 执行任务
                        _logger.LogDebug("执行任务逻辑: {JobId}, 类型: {JobType}", jobId, job.JobTypeName);
                        
                        // 定义带取消令牌的任务执行方法
                        async Task<string> ExecuteWithCancellation()
                        {
                            // 创建一个任务，在其中执行原始方法
                            var executionTask = jobInstance.ExecuteAsync(job.Parameters ?? parameters);
                            
                            // 等待任务完成或取消令牌触发
                            var completedTask = await Task.WhenAny(executionTask, Task.Delay(Timeout.Infinite, cts.Token));
                            
                            // 如果是取消令牌触发，则抛出操作取消异常
                            if (completedTask == Task.Delay(Timeout.Infinite, cts.Token))
                            {
                                throw new OperationCanceledException(cts.Token);
                            }
                            
                            // 返回执行结果
                            return await executionTask;
                        }
                        
                        var result = await ExecuteWithCancellation();

                        // 记录任务执行成功
                        await jobService.LogJobExecutionCompleteAsync(log.Id, isRetry ? 
                            $"成功(第{retryCount}次重试): {result}" : result);
                        
                        _logger.LogInformation("任务执行成功: {JobId}", jobId);
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        // 超时异常
                        _logger.LogWarning("任务执行超时: {JobId}, 超时时间: {Timeout}秒", jobId, timeoutSeconds);
                        throw new TimeoutException($"任务执行超时: {timeoutSeconds}秒");
                    }
                    catch (Exception ex) when (retryCount < maxRetries && IsRetryableException(ex))
                    {
                        // 可重试的异常，增加重试计数并继续
                        retryCount++;
                        _logger.LogWarning(ex, "任务执行失败，将进行重试: {JobId}, 第 {RetryCount}/{MaxRetries} 次尝试", 
                            jobId, retryCount, maxRetries);
                    }
                    catch (Exception ex)
                    {
                        // 不可重试的异常或已达到最大重试次数
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务执行失败: {JobId}", jobId);
                
                // 记录任务执行失败
                string errorMessage = ex.Message;
                string detailedError = ex.ToString();
                
                if (ex is TimeoutException)
                {
                    await jobService.LogJobExecutionFailureAsync(log.Id, 
                        $"任务执行超时: {errorMessage}", detailedError);
                }
                else
                {
                    await jobService.LogJobExecutionFailureAsync(log.Id, errorMessage, detailedError);
                }
                
                // 对于关键任务，可以考虑发送告警通知
                if (job?.Priority == JobPriority.Critical)
                {
                    await NotifyCriticalJobFailureAsync(job, ex);
                }
                
                throw; // 重新抛出异常，让Hangfire知道任务失败
            }
        }
        
        /// <summary>
        /// 根据优先级获取超时时间
        /// </summary>
        private int GetTimeoutByPriority(JobPriority priority)
        {
            return priority switch
            {
                JobPriority.Critical => 60,  // 关键任务超时时间较短，快速失败
                JobPriority.High => 300,
                JobPriority.Default => JobExecutionOptions.DefaultTimeoutSeconds,
                JobPriority.Low => 600,      // 低优先级任务超时时间较长
                _ => JobExecutionOptions.DefaultTimeoutSeconds
            };
        }
        
        /// <summary>
        /// 根据优先级获取最大重试次数
        /// </summary>
        private int GetMaxRetriesByPriority(JobPriority priority)
        {
            return priority switch
            {
                JobPriority.Critical => 5,   // 关键任务重试次数更多
                JobPriority.High => 3,
                JobPriority.Default => JobExecutionOptions.DefaultMaxRetries,
                JobPriority.Low => 1,        // 低优先级任务重试次数较少
                _ => JobExecutionOptions.DefaultMaxRetries
            };
        }
        
        /// <summary>
        /// 判断异常是否可重试
        /// </summary>
        private bool IsRetryableException(Exception ex)
        {
            // 网络相关异常、超时异常等可以重试
            return ex is System.Net.Http.HttpRequestException ||
                   ex is System.Net.Sockets.SocketException ||
                   ex is TimeoutException ||
                   ex is System.Data.Common.DbException ||
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// 通知关键任务失败（可扩展为邮件、短信等通知）
        /// </summary>
        private async Task NotifyCriticalJobFailureAsync(Job job, Exception ex)
        {
            try
            {
                _logger.LogCritical("关键任务执行失败: {JobId} - {JobName}. 错误信息: {ErrorMessage}", 
                    job.Id, job.Name, ex.Message);
                
                // 这里可以扩展为发送邮件、短信、消息队列等通知机制
                // await _notificationService.SendCriticalAlertAsync("Job Failure Alert", alertMessage);
            }
            catch (Exception notifyEx)
            {
                _logger.LogError(notifyEx, "发送关键任务失败通知失败");
            }
        }
    }

    /// <summary>
    /// Hangfire任务接口
    /// </summary>
    public interface IHangfireJob
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="parameters">任务参数</param>
        /// <returns>执行结果</returns>
        Task<string> ExecuteAsync(string parameters);
    }
}