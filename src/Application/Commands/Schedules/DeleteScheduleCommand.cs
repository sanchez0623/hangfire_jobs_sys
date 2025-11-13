using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Schedules
{
    /// <summary>
    /// 删除调度计划命令
    /// </summary>
    public class DeleteScheduleCommand : IRequest<bool>
    {
        /// <summary>
        /// 调度计划ID
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public Guid UpdatedBy { get; set; }

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