using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HangfireJobsSys.Domain.Services
{
    /// <summary>
    /// 缓存服务接口
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <typeparam name="T">缓存数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存数据</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 设置缓存数据
        /// </summary>
        /// <typeparam name="T">缓存数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiry">过期时间</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <param name="keys">缓存键列表</param>
        Task RemoveRangeAsync(IEnumerable<string> keys);

        /// <summary>
        /// 根据模式删除缓存
        /// </summary>
        /// <param name="pattern">缓存键模式</param>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        Task ClearAsync();
    }
}