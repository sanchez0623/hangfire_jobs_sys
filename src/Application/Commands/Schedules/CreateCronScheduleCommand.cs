using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Schedules
{
    /// <summary>
    /// 创建Cron调度计划命令
    /// </summary>
    public class CreateCronScheduleCommand : IRequest<Guid>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// 客户端浏览器
        /// </summary>
        public string ClientBrowser { get; set; }
    }
}