using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireJobsSys.Application.Handlers
{
    /// <summary>
    /// 查询处理程序基类
    /// </summary>
    /// <typeparam name="TQuery">查询类型</typeparam>
    /// <typeparam name="TResult">结果类型</typeparam>
    public abstract class QueryHandlerBase<TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IRequest<TResult>
    {
        protected readonly ILogger Logger;

        protected QueryHandlerBase(ILogger logger)
        {
            Logger = logger;
        }

        public abstract Task<TResult> Handle(TQuery request, CancellationToken cancellationToken);

        /// <summary>
        /// 记录查询处理开始日志
        /// </summary>
        protected virtual void LogQueryStart(TQuery request)
        {
            var queryName = typeof(TQuery).Name;
            var queryParameters = GetQueryParameters(request);
            
            // 记录详细的查询参数，使用Debug级别
            Logger.LogDebug(
                "开始处理查询: {QueryName}, 查询参数: {QueryParameters}", 
                queryName, 
                queryParameters);
                
            // 对于可能影响性能的复杂查询，记录Information级别日志
            if (IsComplexQuery(request))
            {
                Logger.LogInformation(
                    "执行复杂查询: {QueryName}", 
                    queryName);
            }
        }

        /// <summary>
        /// 记录查询处理完成日志
        /// </summary>
        protected virtual void LogQueryCompleted(TQuery request, TResult result)
        {
            var queryName = typeof(TQuery).Name;
            
            // 获取结果的基本信息，避免日志过大
            string resultInfo = GetResultInfo(result);
            
            // 记录详细的查询结果，使用Debug级别
            Logger.LogDebug(
                "查询处理完成: {QueryName}, 结果信息: {ResultInfo}", 
                queryName, 
                resultInfo);
                
            // 对于大型结果集，记录Warning级别日志，提示可能的性能问题
            if (IsLargeResult(result))
            {
                Logger.LogWarning(
                    "查询返回大型结果集: {QueryName}, 结果记录数: {ResultCount}", 
                    queryName, 
                    GetResultCount(result));
            }
        }

        /// <summary>
        /// 记录查询处理异常日志
        /// </summary>
        protected virtual void LogQueryError(TQuery request, System.Exception ex)
        {
            var queryName = typeof(TQuery).Name;
            var queryParameters = GetQueryParameters(request);
            
            // 记录详细的错误信息，使用Error级别
            Logger.LogError(
                ex, 
                "查询处理失败: {QueryName}, 查询参数: {QueryParameters}, 错误消息: {ErrorMessage}", 
                queryName, 
                queryParameters,
                ex.Message);
                
            // 对于频繁执行的关键查询失败，记录Warning级别日志
            if (IsCriticalQuery(request))
            {
                Logger.LogWarning(
                    "关键查询失败: {QueryName}, 请检查系统状态", 
                    queryName);
            }
        }
        
        /// <summary>
        /// 判断是否为复杂查询
        /// </summary>
        protected virtual bool IsComplexQuery(TQuery request)
        {
            // 根据查询类型判断是否为复杂查询
            var queryName = typeof(TQuery).Name;
            
            // 包含以下关键词的查询被认为是复杂查询
            var complexQueryKeywords = new[]
            {
                "Statistics",
                "Report",
                "Summary",
                "Analytics"
            };
            
            return complexQueryKeywords.Any(k => queryName.Contains(k));
        }
        
        /// <summary>
        /// 判断是否为大型结果集
        /// </summary>
        protected virtual bool IsLargeResult(TResult result)
        {
            int resultCount = GetResultCount(result);
            return resultCount > 1000; // 超过1000条记录视为大型结果集
        }
        
        /// <summary>
        /// 获取结果集数量
        /// </summary>
        protected virtual int GetResultCount(TResult result)
        {
            if (result == null) return 0;
            
            var resultType = result.GetType();
            
            // 检查是否有Count属性
            var countProperty = resultType.GetProperty("Count");
            if (countProperty != null && countProperty.PropertyType == typeof(int))
            {
                return (int)countProperty.GetValue(result);
            }
            
            // 检查是否为分页结果
            if (resultType.Name.Contains("PagedResult"))
            {
                var totalCountProperty = resultType.GetProperty("TotalCount");
                if (totalCountProperty != null && totalCountProperty.PropertyType == typeof(int))
                {
                    return (int)totalCountProperty.GetValue(result);
                }
            }
            
            return 0;
        }
        
        /// <summary>
        /// 判断是否为关键查询
        /// </summary>
        protected virtual bool IsCriticalQuery(TQuery request)
        {
            // 根据查询类型判断是否为关键查询
            var queryName = typeof(TQuery).Name;
            
            // 以下类型的查询被认为是关键查询
            var criticalQueryKeywords = new[]
            {
                "Statistics",
                "ExecutionLog",
                "OperationLog"
            };
            
            return criticalQueryKeywords.Any(k => queryName.Contains(k));
        }
        
        /// <summary>
        /// 获取查询参数信息
        /// </summary>
        protected virtual string GetQueryParameters(TQuery request)
        {
            if (request == null) return "null";
            
            try
            {
                var properties = typeof(TQuery).GetProperties()
                    .Select(p => $"{p.Name}: {p.GetValue(request)}")
                    .Take(5) // 只记录前5个属性，避免日志过长
                    .ToList();
                
                return string.Join(", ", properties);
            }
            catch
            {
                return "[无法序列化参数]";
            }
        }
        
        /// <summary>
        /// 获取结果的基本信息
        /// </summary>
        protected virtual string GetResultInfo(TResult result)
        {
            if (result == null) return "null";
            
            var resultType = result.GetType();
            
            // 如果是集合类型，记录元素数量
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                var countProperty = resultType.GetProperty("Count");
                if (countProperty != null)
                {
                    var count = countProperty.GetValue(result);
                    return $"List<{resultType.GetGenericArguments()[0].Name}> 包含 {count} 条记录";
                }
            }
            
            // 如果是分页结果，记录总页数和总记录数
            if (resultType.Name.Contains("PagedResult"))
            {
                var totalCountProperty = resultType.GetProperty("TotalCount");
                var itemsProperty = resultType.GetProperty("Items");
                
                if (totalCountProperty != null && itemsProperty != null)
                {
                    var totalCount = totalCountProperty.GetValue(result);
                    var items = itemsProperty.GetValue(result);
                    var itemCount = items?.GetType().GetProperty("Count")?.GetValue(items) ?? 0;
                    
                    return $"分页结果，总记录数: {totalCount}, 当前页记录数: {itemCount}";
                }
            }
            
            // 对于其他类型，记录类型名称
            return $"{resultType.Name}";
        }
    }
}