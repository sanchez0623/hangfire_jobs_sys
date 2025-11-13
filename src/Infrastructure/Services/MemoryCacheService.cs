using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HangfireJobsSys.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HangfireJobsSys.Infrastructure.Services
{
    /// <summary>
    /// 内存缓存服务实现
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, object> _cacheKeys;
        private readonly object _lock = new object();

        // 默认缓存过期时间
        private const int DefaultCacheExpiryMinutes = 5;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheKeys = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return default;

                if (_memoryCache.TryGetValue(key, out T value))
                {
                    _logger.LogTrace("Cache hit for key: {Key}", key);
                    return value;
                }

                _logger.LogTrace("Cache miss for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for key: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// 设置缓存数据
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return;

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(DefaultCacheExpiryMinutes),
                    Priority = CacheItemPriority.Normal
                };

                // 添加回调，当缓存过期时从集合中移除
                cacheEntryOptions.RegisterPostEvictionCallback((cacheKey, value, reason, state) =>
                {
                    if (reason != EvictionReason.Replaced)
                    {
                        _cacheKeys.TryRemove(cacheKey.ToString(), out _);
                    }
                });

                _memoryCache.Set(key, value, cacheEntryOptions);
                _cacheKeys.TryAdd(key, null);
                
                _logger.LogTrace("Cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return;

                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
                
                _logger.LogTrace("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        /// <summary>
        /// 批量删除缓存
        /// </summary>
        public async Task RemoveRangeAsync(IEnumerable<string> keys)
        {
            try
            {
                foreach (var key in keys)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                }
                
                _logger.LogTrace("Cache range removed for {Count} keys", keys.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache range");
            }
        }

        /// <summary>
        /// 根据模式删除缓存
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var matchingKeys = _cacheKeys.Keys.Where(key => regex.IsMatch(key)).ToList();

                await RemoveRangeAsync(matchingKeys);
                
                _logger.LogTrace("Cache removed by pattern: {Pattern}, {Count} keys affected", pattern, matchingKeys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public async Task ClearAsync()
        {
            try
            {
                // 由于IMemoryCache没有直接Clear方法，我们只能逐个移除
                var keys = _cacheKeys.Keys.ToList();
                await RemoveRangeAsync(keys);
                
                _logger.LogInformation("All cache cleared, {Count} keys removed", keys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }
    }
}