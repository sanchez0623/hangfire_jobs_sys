using MediatR;
using HangfireJobsSys.Application.Queries;
using System;

namespace HangfireJobsSys.Application.Queries.Logs
{
    /// <summary>
    /// 任务执行日志查询参数
    /// </summary>
    public class JobExecutionLogQuery : BaseQuery, IRequest<PagedResult<JobExecutionLogDto>>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid? JobId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public string ExecutionStatus { get; set; }

        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime? StartExecutionTime { get; set; }

        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime? EndExecutionTime { get; set; }
    }
}