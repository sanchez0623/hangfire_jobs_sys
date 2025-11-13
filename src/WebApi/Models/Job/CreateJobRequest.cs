using System.ComponentModel.DataAnnotations;

namespace HangfireJobsSys.WebApi.Models.Job
{
    /// <summary>
    /// 创建任务请求模型
    /// </summary>
    public class CreateJobRequest
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        [Required(ErrorMessage = "任务名称不能为空")]
        [StringLength(100, ErrorMessage = "任务名称长度不能超过100个字符")]
        public required string Name { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        [StringLength(500, ErrorMessage = "任务描述长度不能超过500个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 任务类型
        /// </summary>
        [Required(ErrorMessage = "任务类型不能为空")]
        [StringLength(200, ErrorMessage = "任务类型长度不能超过200个字符")]
        public required string JobType { get; set; }

        /// <summary>
        /// 任务参数（JSON格式）
        /// </summary>
        public string? Parameters { get; set; }

        /// <summary>
        /// 调度表达式（Cron表达式）
        /// </summary>
        public string? CronExpression { get; set; }

        /// <summary>
        /// 是否启用调度
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}