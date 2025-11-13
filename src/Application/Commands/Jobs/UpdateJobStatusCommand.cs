using MediatR;

namespace HangfireJobsSys.Application.Commands.Jobs
{
    /// <summary>
    /// 更新任务状态命令
    /// </summary>
    public class UpdateJobStatusCommand : IRequest<bool>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// 新状态 (Active/Inactive)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 更新人ID
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