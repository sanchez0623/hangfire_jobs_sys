using MediatR;

namespace HangfireJobsSys.Application.Commands.Schedules
{
    /// <summary>
    /// 更新调度计划状态命令
    /// </summary>
    public class UpdateScheduleStatusCommand : IRequest<bool>
    {
        /// <summary>
        /// 调度计划ID
        /// </summary>
        public Guid ScheduleId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 更新人
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