using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Jobs
{
    /// <summary>
    /// 立即执行任务命令
    /// </summary>
    public class ExecuteJobImmediatelyCommand : IRequest<string>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// 执行人ID
        /// </summary>
        public Guid ExecutorId { get; set; }

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