using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireJobsSys.Application.Handlers
{
    /// <summary>
    /// 命令处理程序基类
    /// </summary>
    /// <typeparam name="TCommand">命令类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    public abstract class CommandHandlerBase<TCommand, TResult> : IRequestHandler<TCommand, TResult> where TCommand : IRequest<TResult>
    {
        protected readonly ILogger Logger;

        protected CommandHandlerBase(ILogger logger)
        {
            Logger = logger;
        }

        public abstract Task<TResult> Handle(TCommand request, CancellationToken cancellationToken);

        /// <summary>
        /// 记录命令处理开始日志
        /// </summary>
        protected virtual void LogCommandStart(TCommand request)
        {
            var commandName = typeof(TCommand).Name;
            var commandProperties = GetRequestProperties(request);
            
            // 记录详细的命令参数和上下文信息，使用Debug级别
            Logger.LogDebug(
                "开始处理命令: {CommandName}, 请求ID: {RequestId}, 用户ID: {UserId}, 参数: {CommandParams}", 
                commandName, 
                GetRequestId(request), 
                GetUserId(request),
                commandProperties);
            
            // 记录简要的命令开始信息，使用Information级别
            Logger.LogInformation(
                "命令开始: {CommandName}, 请求ID: {RequestId}", 
                commandName, 
                GetRequestId(request));
        }

        /// <summary>
        /// 记录命令处理完成日志
        /// </summary>
        protected virtual void LogCommandCompleted(TCommand request, TResult result)
        {
            var commandName = typeof(TCommand).Name;
            
            // 记录详细的完成信息，使用Debug级别
            Logger.LogDebug(
                "命令处理完成: {CommandName}, 请求ID: {RequestId}, 结果类型: {ResultType}, 结果值: {ResultValue}", 
                commandName, 
                GetRequestId(request),
                result?.GetType().Name ?? "null",
                result?.ToString() ?? "null");
            
            // 记录简要的完成信息，使用Information级别
            Logger.LogInformation(
                "命令完成: {CommandName}, 请求ID: {RequestId}", 
                commandName, 
                GetRequestId(request));
        }

        /// <summary>
        /// 记录命令处理异常日志
        /// </summary>
        protected virtual void LogCommandError(TCommand request, System.Exception ex)
        {
            var commandName = typeof(TCommand).Name;
            
            // 记录详细的错误信息，包含异常栈，使用Error级别
            Logger.LogError(
                ex, 
                "命令处理失败: {CommandName}, 请求ID: {RequestId}, 用户ID: {UserId}, 错误消息: {ErrorMessage}", 
                commandName, 
                GetRequestId(request),
                GetUserId(request),
                ex.Message);
                
            // 对于关键业务操作失败，记录Warning级别日志
            if (IsCriticalOperation(request))
            {
                Logger.LogWarning(
                    "关键操作失败: {CommandName}, 请求ID: {RequestId}, 用户ID: {UserId}",
                    commandName,
                    GetRequestId(request),
                    GetUserId(request));
            }
        }
        
        /// <summary>
        /// 判断是否为关键业务操作
        /// </summary>
        protected virtual bool IsCriticalOperation(TCommand request)
        {
            // 根据命令类型判断是否为关键操作
            var commandName = typeof(TCommand).Name;
            
            // 以下类型的命令被认为是关键操作
            var criticalCommands = new[]
            {
                "DeleteJobCommand",
                "DeleteScheduleCommand",
                "CreateJobCommand",
                "UpdateJobStatusCommand"
            };
            
            return criticalCommands.Any(c => commandName.Contains(c));
        }
        
        /// <summary>
        /// 获取请求ID
        /// </summary>
        protected virtual string GetRequestId(TCommand request)
        {
            // 尝试从请求对象中获取RequestId属性
            var property = typeof(TCommand).GetProperty("RequestId");
            if (property != null)
            {
                var value = property.GetValue(request);
                return value?.ToString() ?? Guid.NewGuid().ToString();
            }
            return Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// 获取用户ID
        /// </summary>
        protected virtual string GetUserId(TCommand request)
        {
            // 尝试从请求对象中获取用户相关属性
            var userIdProperty = typeof(TCommand).GetProperty("CreatedBy") ?? 
                                typeof(TCommand).GetProperty("UpdatedBy") ?? 
                                typeof(TCommand).GetProperty("UserId");
            
            if (userIdProperty != null)
            {
                var value = userIdProperty.GetValue(request);
                return value?.ToString() ?? "Unknown";
            }
            return "Unknown";
        }
        
        /// <summary>
        /// 获取请求的关键属性信息
        /// </summary>
        protected virtual string GetRequestProperties(TCommand request)
        {
            if (request == null) return "null";
            
            try
            {
                var properties = typeof(TCommand).GetProperties()
                    .Where(p => p.Name != "RequestId" && p.Name != "CreatedBy" && p.Name != "UpdatedBy" && p.Name != "UserId")
                    .Select(p => $"{p.Name}: {p.GetValue(request)}")
                    .Take(5) // 只记录前5个属性，避免日志过长
                    .ToList();
                
                return string.Join(", ", properties);
            }
            catch
            {
                return "[无法序列化属性]";
            }
        }
    }
}