using MediatR;
using HangfireJobsSys.Application.Queries;
using System;

namespace HangfireJobsSys.Application.Queries.Logs
{
    /// <summary>
    /// 操作日志查询参数
    /// </summary>
    public class OperationLogQuery : BaseQuery, IRequest<PagedResult<OperationLogDto>>
    {
        /// <summary>
        /// 操作人ID
        /// </summary>
        public string OperatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        public string OperatorName { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// 操作模块
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 操作状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 开始操作时间
        /// </summary>
        public DateTime? StartOperationTime { get; set; }

        /// <summary>
        /// 结束操作时间
        /// </summary>
        public DateTime? EndOperationTime { get; set; }
    }
}