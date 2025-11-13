using MediatR;

namespace HangfireJobsSys.Application.Commands.Schedules
{
    /// <summary>
    /// 创建一次性调度计划命令
    /// </summary>
    public class CreateOneTimeScheduleCommand : IRequest<Guid>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime ExecuteTime { get; set; }

        /// <summary>
        /// 创建人
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