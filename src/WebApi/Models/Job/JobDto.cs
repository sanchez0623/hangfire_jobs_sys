namespace HangfireJobsSys.WebApi.Models.Job
{
    /// <summary>
    /// 任务DTO
    /// </summary>
    public class JobDto
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        public required string JobType { get; set; }

        /// <summary>
        /// 任务参数
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatedById { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }
}