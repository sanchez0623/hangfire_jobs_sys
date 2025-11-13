using MediatR;
using HangfireJobsSys.Application.Queries;
using System;

namespace HangfireJobsSys.Application.Queries.Schedules
{
    /// <summary>
    /// 调度计划查询参数
    /// </summary>
    public class ScheduleQuery : BaseQuery, IRequest<PagedResult<ScheduleDto>>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid? JobId { get; set; }

        /// <summary>
        /// 调度类型（Cron/Interval）
        /// </summary>
        public string ScheduleType { get; set; }

        /// <summary>
        /// 调度状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 开始创建时间
        /// </summary>
        public DateTime? StartCreateTime { get; set; }

        /// <summary>
        /// 结束创建时间
        /// </summary>
        public DateTime? EndCreateTime { get; set; }
    }
}