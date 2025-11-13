using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Jobs
{
    /// <summary>
    /// 创建任务命令
    /// </summary>
    public class CreateJobCommand : IRequest<Guid>
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 任务类型名称
        /// </summary>
        public required string JobTypeName { get; set; }

        /// <summary>
        /// 任务参数（JSON格式）
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public required Guid CreatedBy { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// 客户端浏览器
        /// </summary>
        public string? ClientBrowser { get; set; }
    }
}