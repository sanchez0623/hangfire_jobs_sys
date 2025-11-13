using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Jobs
{
    /// <summary>
    /// 更新任务命令
    /// </summary>
    public class UpdateJobCommand : IRequest<bool>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 任务类型名称
        /// </summary>
        public string JobTypeName { get; set; }

        /// <summary>
        /// 任务参数（JSON格式）
        /// </summary>
        public string Parameters { get; set; }

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