using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Jobs
{
    /// <summary>
    /// 删除任务命令
    /// </summary>
    public class DeleteJobCommand : IRequest<bool>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

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