using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Entities;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 操作日志服务接口
    /// </summary>
    public interface IOperationLogService
    {
        /// <summary>
        /// 记录操作日志
        /// </summary>
        Task LogOperationAsync(
            Guid? operatorId,
            string operatorName,
            OperationType type,
            string module,
            string description,
            Guid? entityId = null,
            string parameters = null,
            string clientIp = null,
            string clientBrowser = null);

        /// <summary>
        /// 记录失败的操作日志
        /// </summary>
        Task LogFailedOperationAsync(
            Guid? operatorId,
            string operatorName,
            OperationType type,
            string module,
            string description,
            string errorMessage,
            Guid? entityId = null,
            string parameters = null,
            string clientIp = null,
            string clientBrowser = null);

        /// <summary>
        /// 获取操作日志列表
        /// </summary>
        Task<IEnumerable<OperationLog>> GetOperationLogsAsync(
            Guid? operatorId = null,
            OperationType? type = null,
            string module = null,
            DateTime? startTime = null,
            DateTime? endTime = null,
            OperationStatus? status = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// 根据ID获取操作日志
        /// </summary>
        Task<OperationLog> GetOperationLogByIdAsync(Guid logId);

        /// <summary>
        /// 清理指定时间之前的操作日志
        /// </summary>
        Task<int> CleanupLogsAsync(DateTime beforeTime);

        /// <summary>
        /// 获取操作统计信息
        /// </summary>
        Task<IDictionary<string, int>> GetOperationStatisticsAsync(DateTime startTime, DateTime endTime);
    }
}