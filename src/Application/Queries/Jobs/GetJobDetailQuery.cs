using MediatR;
using System;

namespace HangfireJobsSys.Application.Queries.Jobs
{
    /// <summary>
    /// 获取任务详情查询
    /// </summary>
    public class GetJobDetailQuery : IRequest<JobDetailDto>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid JobId { get; set; }
    }
}