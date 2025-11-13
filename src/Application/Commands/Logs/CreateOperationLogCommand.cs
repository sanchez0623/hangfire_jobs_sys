using MediatR;
using System;

namespace HangfireJobsSys.Application.Commands.Logs
{
    /// <summary>
    /// 创建操作日志命令
    /// </summary>
    public class CreateOperationLogCommand : IRequest<bool>
    {
        /// <summary>
        /// 操作人ID
        /// </summary>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        public string OperatorName { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// 操作模块
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string RequestParams { get; set; }

        /// <summary>
        /// 响应结果
        /// </summary>
        public string ResponseResult { get; set; }

        /// <summary>
        /// 执行时间（毫秒）
        /// </summary>
        public long ExecutionTime { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// 客户端浏览器
        /// <summary>
        public string ClientBrowser { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}