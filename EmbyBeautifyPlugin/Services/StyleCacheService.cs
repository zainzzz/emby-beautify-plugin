using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 样式缓存服务
    /// 负责缓存生成的CSS样式，提高性能并减少重复计算
    /// </summary>
    public class StyleCacheService
    {
        private readonly ILogger<StyleCacheService> _logger;
        private readonly IConfigurationManager _configurationManager;
        private readonly ConcurrentDictionary<string, CacheEntry> _memoryCache;
        private readonly SemaphoreSlim _cacheSemaphore;
        private readonly Timer _cleanupTimer;
        private readonly string _cacheDirectory;

        // 缓存配置
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30);
        private readonly int _maxMemoryCacheSize = 100;
        private readonly long _maxFileCacheSize = 50 * 1024 * 1024; // 50MB

        public StyleCacheService(
            ILogger<StyleCacheService> logger,
            IConfigurationManager configurationManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            
            _memoryCache = new ConcurrentDictionary<string, CacheEntry>();
            _cacheSemaphore = new SemaphoreSlim(1, 1);
            
            // 设置缓存目录
            _cacheDirectory = Path.Combine(Path.GetTempPath(), "EmbyBeautifyPlugin", "StyleCache");
            EnsureCacheDirectoryExists();
            
            // 启动定期清理定时器
            _cleanupTimer = new Timer(PerformCleanup, null, _cleanupInterval, _cleanupInterval);
            
            _logger.LogDebug("样式缓存服务已初始化，缓存目录: {CacheDirectory}", _cacheDirectory);
        }

        /// <summary>
        /// 获取缓存的样式
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>缓存的CSS内容，如果不存在则返回null</returns>
        public async Task<string> GetCachedStyleAsync(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return null;
            }

            try
            {
                // 首先检查内存缓存
                if (_memoryCache.TryGetValue(cacheKey, out var memoryEntry))
                {
                    if (!memoryEntry.IsExpired)
                    {
                        memoryEntry.LastAccessed = DateTime.UtcNow;
                        _logger.LogDebug("从内存缓存获取样式: {CacheKey}", cacheKey);
                        return memoryEntry.Content;
                    }
                    else
                    {
                        // 移除过期的内存缓存
                        _memoryCache.TryRemove(cacheKey, out _);
                    }
                }

                // 检查文件缓存
                var filePath = GetCacheFilePath(cacheKey);
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < _defaultExpiration)
                    {
                        var content = await File.ReadAllTextAsync(filePath);
                        
                        // 将文件缓存加载到内存缓存
                        await AddToMemoryCacheAsync(cacheKey, content);
                        
                        _logger.LogDebug("从文件缓存获取样式: {CacheKey}", cacheKey);
                        return content;
                    }
                    else
                    {
                        // 删除过期的文件缓存
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "删除过期缓存文件失败: {FilePath}", filePath);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存样式时发生错误: {CacheKey}", cacheKey);
                return null;
            }
        }

        /// <summary>
        /// 缓存样式内容
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="content">CSS内容</param>
        /// <param name="expiration">过期时间，如果为null则使用默认过期时间</param>
        public async Task SetCachedStyleAsync(string cacheKey, string content, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(cacheKey) || string.IsNullOrEmpty(content))
            {
                return;
            }

            var expirationTime = expiration ?? _defaultExpiration;

            try
            {
                await _cacheSemaphore.WaitAsync();

                // 添加到内存缓存
                await AddToMemoryCacheAsync(cacheKey, content, expirationTime);

                // 添加到文件缓存
                await AddToFileCacheAsync(cacheKey, content);

                _logger.LogDebug("样式已缓存: {CacheKey}, 内容长度: {ContentLength}", cacheKey, content.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "缓存样式时发生错误: {CacheKey}", cacheKey);
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// 移除缓存的样式
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        public async Task RemoveCachedStyleAsync(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return;
            }

            try
            {
                // 从内存缓存移除
                _memoryCache.TryRemove(cacheKey, out _);

                // 从文件缓存移除
                var filePath = GetCacheFilePath(cacheKey);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _logger.LogDebug("缓存已移除: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除缓存时发生错误: {CacheKey}", cacheKey);
            }
        }

        /// <summary>
        /// 清理过期的缓存
        /// </summary>
        public async Task CleanupExpiredCacheAsync()
        {
            try
            {
                await _cacheSemaphore.WaitAsync();

                var cleanupTasks = new List<Task>
                {
                    CleanupMemoryCacheAsync(),
                    CleanupFileCacheAsync()
                };

                await Task.WhenAll(cleanupTasks);

                _logger.LogDebug("缓存清理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理缓存时发生错误");
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>缓存统计信息</returns>
        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            try
            {
                var memoryCacheCount = _memoryCache.Count;
                var memoryCacheSize = CalculateMemoryCacheSize();

                var fileCacheInfo = await GetFileCacheInfoAsync();

                return new CacheStatistics
                {
                    MemoryCacheEntries = memoryCacheCount,
                    MemoryCacheSize = memoryCacheSize,
                    FileCacheEntries = fileCacheInfo.Count,
                    FileCacheSize = fileCacheInfo.Size,
                    CacheDirectory = _cacheDirectory,
                    LastCleanupTime = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取缓存统计信息时发生错误");
                return new CacheStatistics();
            }
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public async Task ClearAllCacheAsync()
        {
            try
            {
                await _cacheSemaphore.WaitAsync();

                // 清空内存缓存
                _memoryCache.Clear();

                // 清空文件缓存
                if (Directory.Exists(_cacheDirectory))
                {
                    var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "删除缓存文件失败: {FilePath}", file);
                        }
                    }
                }

                _logger.LogInformation("所有缓存已清空");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空缓存时发生错误");
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// 生成缓存键
        /// </summary>
        /// <param name="theme">主题对象</param>
        /// <param name="config">配置对象</param>
        /// <param name="additionalData">额外数据</param>
        /// <returns>缓存键</returns>
        public string GenerateCacheKey(Theme theme, BeautifyConfig config, params object[] additionalData)
        {
            try
            {
                var keyBuilder = new StringBuilder();
                
                // 添加主题信息
                if (theme != null)
                {
                    keyBuilder.Append($"theme:{theme.Id}:{theme.Version}");
                }

                // 添加配置信息
                if (config != null)
                {
                    keyBuilder.Append($":config:{config.EnableAnimations}:{config.AnimationDuration}");
                    
                    if (config.ResponsiveSettings != null)
                    {
                        keyBuilder.Append($":responsive:{config.ResponsiveSettings.Desktop?.MaxWidth}");
                        keyBuilder.Append($":{config.ResponsiveSettings.Tablet?.MaxWidth}");
                        keyBuilder.Append($":{config.ResponsiveSettings.Mobile?.MaxWidth}");
                    }
                }

                // 添加额外数据
                if (additionalData != null)
                {
                    foreach (var data in additionalData)
                    {
                        keyBuilder.Append($":{data?.GetHashCode()}");
                    }
                }

                // 生成哈希值作为缓存键
                var keyString = keyBuilder.ToString();
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成缓存键时发生错误");
                return Guid.NewGuid().ToString("N");
            }
        }

        /// <summary>
        /// 添加到内存缓存
        /// </summary>
        private async Task AddToMemoryCacheAsync(string cacheKey, string content, TimeSpan? expiration = null)
        {
            var expirationTime = expiration ?? _defaultExpiration;
            var entry = new CacheEntry
            {
                Content = content,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expirationTime),
                LastAccessed = DateTime.UtcNow,
                Size = Encoding.UTF8.GetByteCount(content)
            };

            _memoryCache.AddOrUpdate(cacheKey, entry, (key, existing) => entry);

            // 检查内存缓存大小限制
            if (_memoryCache.Count > _maxMemoryCacheSize)
            {
                await TrimMemoryCacheAsync();
            }
        }

        /// <summary>
        /// 添加到文件缓存
        /// </summary>
        private async Task AddToFileCacheAsync(string cacheKey, string content)
        {
            try
            {
                var filePath = GetCacheFilePath(cacheKey);
                await File.WriteAllTextAsync(filePath, content);

                // 检查文件缓存大小限制
                await CheckFileCacheSizeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "写入文件缓存失败: {CacheKey}", cacheKey);
            }
        }

        /// <summary>
        /// 清理内存缓存
        /// </summary>
        private async Task CleanupMemoryCacheAsync()
        {
            var expiredKeys = new List<string>();
            var now = DateTime.UtcNow;

            foreach (var kvp in _memoryCache)
            {
                if (kvp.Value.IsExpired)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }

            foreach (var key in expiredKeys)
            {
                _memoryCache.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("清理了 {Count} 个过期的内存缓存条目", expiredKeys.Count);
            }
        }

        /// <summary>
        /// 清理文件缓存
        /// </summary>
        private async Task CleanupFileCacheAsync()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    return;
                }

                var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                var expiredFiles = new List<string>();
                var now = DateTime.UtcNow;

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (now - fileInfo.LastWriteTimeUtc > _defaultExpiration)
                    {
                        expiredFiles.Add(file);
                    }
                }

                foreach (var file in expiredFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "删除过期缓存文件失败: {FilePath}", file);
                    }
                }

                if (expiredFiles.Count > 0)
                {
                    _logger.LogDebug("清理了 {Count} 个过期的文件缓存", expiredFiles.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理文件缓存时发生错误");
            }
        }

        /// <summary>
        /// 修剪内存缓存
        /// </summary>
        private async Task TrimMemoryCacheAsync()
        {
            var entries = new List<KeyValuePair<string, CacheEntry>>();
            foreach (var kvp in _memoryCache)
            {
                entries.Add(kvp);
            }

            // 按最后访问时间排序，移除最旧的条目
            entries.Sort((a, b) => a.Value.LastAccessed.CompareTo(b.Value.LastAccessed));

            var removeCount = entries.Count - _maxMemoryCacheSize + 10; // 多移除10个以避免频繁修剪
            for (int i = 0; i < removeCount && i < entries.Count; i++)
            {
                _memoryCache.TryRemove(entries[i].Key, out _);
            }

            _logger.LogDebug("修剪了 {Count} 个内存缓存条目", removeCount);
        }

        /// <summary>
        /// 检查文件缓存大小
        /// </summary>
        private async Task CheckFileCacheSizeAsync()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    return;
                }

                var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                long totalSize = 0;

                var fileInfos = new List<FileInfo>();
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                    fileInfos.Add(fileInfo);
                }

                if (totalSize > _maxFileCacheSize)
                {
                    // 按最后写入时间排序，删除最旧的文件
                    fileInfos.Sort((a, b) => a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc));

                    long removedSize = 0;
                    int removedCount = 0;

                    foreach (var fileInfo in fileInfos)
                    {
                        if (totalSize - removedSize <= _maxFileCacheSize * 0.8) // 保持在80%以下
                        {
                            break;
                        }

                        try
                        {
                            removedSize += fileInfo.Length;
                            File.Delete(fileInfo.FullName);
                            removedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "删除缓存文件失败: {FilePath}", fileInfo.FullName);
                        }
                    }

                    _logger.LogDebug("清理了 {Count} 个文件缓存，释放了 {Size} 字节", removedCount, removedSize);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查文件缓存大小时发生错误");
            }
        }

        /// <summary>
        /// 获取缓存文件路径
        /// </summary>
        private string GetCacheFilePath(string cacheKey)
        {
            return Path.Combine(_cacheDirectory, $"{cacheKey}.cache");
        }

        /// <summary>
        /// 确保缓存目录存在
        /// </summary>
        private void EnsureCacheDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    Directory.CreateDirectory(_cacheDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建缓存目录失败: {CacheDirectory}", _cacheDirectory);
            }
        }

        /// <summary>
        /// 计算内存缓存大小
        /// </summary>
        private long CalculateMemoryCacheSize()
        {
            long totalSize = 0;
            foreach (var entry in _memoryCache.Values)
            {
                totalSize += entry.Size;
            }
            return totalSize;
        }

        /// <summary>
        /// 获取文件缓存信息
        /// </summary>
        private async Task<(int Count, long Size)> GetFileCacheInfoAsync()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    return (0, 0);
                }

                var files = Directory.GetFiles(_cacheDirectory, "*.cache");
                long totalSize = 0;

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }

                return (files.Length, totalSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件缓存信息时发生错误");
                return (0, 0);
            }
        }

        /// <summary>
        /// 定期清理回调
        /// </summary>
        private async void PerformCleanup(object state)
        {
            try
            {
                await CleanupExpiredCacheAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "定期清理缓存时发生错误");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _cacheSemaphore?.Dispose();
        }
    }

    /// <summary>
    /// 缓存条目
    /// </summary>
    internal class CacheEntry
    {
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public long Size { get; set; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// 缓存统计信息
    /// </summary>
    public class CacheStatistics
    {
        public int MemoryCacheEntries { get; set; }
        public long MemoryCacheSize { get; set; }
        public int FileCacheEntries { get; set; }
        public long FileCacheSize { get; set; }
        public string CacheDirectory { get; set; }
        public DateTime LastCleanupTime { get; set; }
    }
}