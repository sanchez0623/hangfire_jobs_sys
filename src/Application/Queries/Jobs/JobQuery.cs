using MediatR;
using HangfireJobsSys.Application.Queries;

namespace HangfireJobsSys.Application.Queries.Jobs
{
    /// <summary>
    /// 任务查询参数
    /// </summary>
    public class JobQuery : BaseQuery, IRequest<PagedResult<JobDto>>
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 开始创建时间
        /// </summary>
        public DateTime? StartCreateTime { get; set; }

        /// <summary>
        /// 结束创建时间
        /// </summary>
        public DateTime? EndCreateTime { get; set; }
    }
}