using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Interfaces;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 样式懒加载服务，按需加载和缓存样式
    /// </summary>
    public class LazyStyleLoader
    {
        private readonly ILogger<LazyStyleLoader> _logger;
        private readonly IThemeManager _themeManager;
        private readonly CssGenerationEngine _cssGenerator;
        private readonly ConcurrentDictionary<string, LazyStyleEntry> _styleCache;
        private readonly SemaphoreSlim _loadingSemaphore;
        private readonly Timer _cleanupTimer;
        
        private const int MaxCacheSize = 50;
        private const int CleanupIntervalMinutes = 30;
        private const int MaxUnusedMinutes = 60;

        public LazyStyleLoader(
            ILogger<LazyStyleLoader> logger,
            IThemeManager themeManager,
            CssGenerationEngine cssGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
            _cssGenerator = cssGenerator ?? throw new ArgumentNullException(nameof(cssGenerator));
            
            _styleCache = new ConcurrentDictionary<string, LazyStyleEntry>();
            _loadingSemaphore = new SemaphoreSlim(3, 3); // 最多3个并发加载
            
            // 定期清理缓存
            _cleanupTimer = new Timer(CleanupCache, null, 
                TimeSpan.FromMinutes(CleanupIntervalMinutes), 
                TimeSpan.FromMinutes(CleanupIntervalMinutes));
        }

        /// <summary>
        /// 异步加载主题样式
        /// </summary>
        /// <param name="themeId">主题ID</param>
        /// <param name="options">CSS生成选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>CSS内容</returns>
        public async Task<string> LoadThemeStyleAsync(string themeId, CssGenerationOptions options = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(themeId))
                throw new ArgumentException("主题ID不能为空", nameof(themeId));

            options ??= new CssGenerationOptions();
            var cacheKey = GenerateCacheKey(themeId, options);

            // 检查缓存
            if (_styleCache.TryGetValue(cacheKey, out var cachedEntry))
            {
                cachedEntry.UpdateLastAccessed();
                
                if (cachedEntry.IsLoaded)
                {
                    _logger.LogDebug("从缓存返回主题样式: {ThemeId}", themeId);
                    return cachedEntry.CssContent;
                }
                
                // 如果正在加载，等待完成
                if (cachedEntry.IsLoading)
                {
                    _logger.LogDebug("等待主题样式加载完成: {ThemeId}", themeId);
                    return await cachedEntry.LoadingTask;
                }
            }

            // 创建新的加载任务
            var entry = new LazyStyleEntry(cacheKey);
            _styleCache.TryAdd(cacheKey, entry);

            try
            {
                await _loadingSemaphore.WaitAsync(cancellationToken);
                
                // 双重检查，防止重复加载
                if (entry.IsLoaded)
                {
                    return entry.CssContent;
                }

                _logger.LogDebug("开始加载主题样式: {ThemeId}", themeId);
                
                // 创建加载任务
                entry.LoadingTask = LoadStyleInternalAsync(themeId, options, cancellationToken);
                var cssContent = await entry.LoadingTask;
                
                entry.SetLoaded(cssContent);
                
                _logger.LogDebug("主题样式加载完成: {ThemeId}, 大小: {Size} 字节", themeId, cssContent?.Length ?? 0);
                
                return cssContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载主题样式失败: {ThemeId}", themeId);
                entry.SetError(ex);
                
                // 从缓存中移除失败的条目
                _styleCache.TryRemove(cacheKey, out _);
                
                throw;
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        /// <summary>
        /// 预加载主题样式
        /// </summary>
        /// <param name="themeIds">主题ID列表</param>
        /// <param name="options">CSS生成选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>预加载任务</returns>
        public async Task PreloadThemeStylesAsync(IEnumerable<string> themeIds, CssGenerationOptions options = null, CancellationToken cancellationToken = default)
        {
            if (themeIds == null)
                throw new ArgumentNullException(nameof(themeIds));

            var themeIdList = themeIds.ToList();
            _logger.LogInformation("开始预加载 {Count} 个主题样式", themeIdList.Count);

            var preloadTasks = themeIdList.Select(async themeId =>
            {
                try
                {
                    await LoadThemeStyleAsync(themeId, options, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "预加载主题样式失败: {ThemeId}", themeId);
                }
            });

            await Task.WhenAll(preloadTasks);
            
            _logger.LogInformation("主题样式预加载完成");
        }

        /// <summary>
        /// 获取响应式样式
        /// </summary>
        /// <param name="themeId">主题ID</param>
        /// <param name="breakpointSettings">断点设置</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>响应式CSS内容</returns>
        public async Task<string> LoadResponsiveStyleAsync(string themeId, BreakpointSettings breakpointSettings, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(themeId))
                throw new ArgumentException("主题ID不能为空", nameof(themeId));
            
            if (breakpointSettings == null)
                throw new ArgumentNullException(nameof(breakpointSettings));

            var cacheKey = GenerateResponsiveCacheKey(themeId, breakpointSettings);

            // 检查缓存
            if (_styleCache.TryGetValue(cacheKey, out var cachedEntry))
            {
                cachedEntry.UpdateLastAccessed();
                
                if (cachedEntry.IsLoaded)
                {
                    return cachedEntry.CssContent;
                }
                
                if (cachedEntry.IsLoading)
                {
                    return await cachedEntry.LoadingTask;
                }
            }

            // 创建新的加载任务
            var entry = new LazyStyleEntry(cacheKey);
            _styleCache.TryAdd(cacheKey, entry);

            try
            {
                await _loadingSemaphore.WaitAsync(cancellationToken);
                
                if (entry.IsLoaded)
                {
                    return entry.CssContent;
                }

                _logger.LogDebug("开始加载响应式样式: {ThemeId}, 断点: {MinWidth}-{MaxWidth}", 
                    themeId, breakpointSettings.MinWidth, breakpointSettings.MaxWidth);
                
                entry.LoadingTask = LoadResponsiveStyleInternalAsync(themeId, breakpointSettings, cancellationToken);
                var cssContent = await entry.LoadingTask;
                
                entry.SetLoaded(cssContent);
                
                return cssContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载响应式样式失败: {ThemeId}", themeId);
                entry.SetError(ex);
                _styleCache.TryRemove(cacheKey, out _);
                throw;
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="themeId">主题ID，为空则清除所有缓存</param>
        public void ClearCache(string themeId = null)
        {
            if (string.IsNullOrEmpty(themeId))
            {
                var count = _styleCache.Count;
                _styleCache.Clear();
                _logger.LogInformation("已清除所有样式缓存，共 {Count} 项", count);
            }
            else
            {
                var keysToRemove = _styleCache.Keys
                    .Where(key => key.StartsWith($"{themeId}:"))
                    .ToList();
                
                foreach (var key in keysToRemove)
                {
                    _styleCache.TryRemove(key, out _);
                }
                
                _logger.LogInformation("已清除主题 {ThemeId} 的样式缓存，共 {Count} 项", themeId, keysToRemove.Count);
            }
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>缓存统计</returns>
        public LazyStyleCacheStatistics GetCacheStatistics()
        {
            var entries = _styleCache.Values.ToList();
            
            return new LazyStyleCacheStatistics
            {
                TotalEntries = entries.Count,
                LoadedEntries = entries.Count(e => e.IsLoaded),
                LoadingEntries = entries.Count(e => e.IsLoading),
                ErrorEntries = entries.Count(e => e.HasError),
                TotalCacheSize = entries.Where(e => e.IsLoaded).Sum(e => e.CssContent?.Length ?? 0),
                OldestEntry = entries.Any() ? entries.Min(e => e.CreatedAt) : (DateTime?)null,
                NewestEntry = entries.Any() ? entries.Max(e => e.CreatedAt) : (DateTime?)null,
                MostRecentlyAccessed = entries.Any() ? entries.Max(e => e.LastAccessedAt) : (DateTime?)null
            };
        }

        /// <summary>
        /// 内部样式加载方法
        /// </summary>
        private async Task<string> LoadStyleInternalAsync(string themeId, CssGenerationOptions options, CancellationToken cancellationToken)
        {
            var theme = await _themeManager.GetThemeByIdAsync(themeId);
            if (theme == null)
            {
                throw new ArgumentException($"未找到主题: {themeId}");
            }

            return await _cssGenerator.GenerateThemeCssAsync(theme, options);
        }

        /// <summary>
        /// 内部响应式样式加载方法
        /// </summary>
        private async Task<string> LoadResponsiveStyleInternalAsync(string themeId, BreakpointSettings breakpointSettings, CancellationToken cancellationToken)
        {
            var theme = await _themeManager.GetThemeByIdAsync(themeId);
            if (theme == null)
            {
                throw new ArgumentException($"未找到主题: {themeId}");
            }

            // 生成响应式CSS
            var cssBuilder = new System.Text.StringBuilder();
            
            cssBuilder.AppendLine($"@media (min-width: {breakpointSettings.MinWidth}px) and (max-width: {breakpointSettings.MaxWidth}px) {{");
            cssBuilder.AppendLine("  :root {");
            cssBuilder.AppendLine($"    --responsive-columns: {breakpointSettings.GridColumns};");
            cssBuilder.AppendLine($"    --responsive-gap: {breakpointSettings.GridGap};");
            cssBuilder.AppendLine($"    --responsive-font-scale: {breakpointSettings.FontScale};");
            cssBuilder.AppendLine("  }");
            
            // 添加网格布局
            cssBuilder.AppendLine("  .grid-container {");
            cssBuilder.AppendLine($"    grid-template-columns: repeat({breakpointSettings.GridColumns}, 1fr);");
            cssBuilder.AppendLine($"    gap: {breakpointSettings.GridGap};");
            cssBuilder.AppendLine("  }");
            
            // 添加字体缩放
            if (breakpointSettings.FontScale != 1.0)
            {
                cssBuilder.AppendLine("  body {");
                cssBuilder.AppendLine($"    font-size: calc(var(--font-size) * {breakpointSettings.FontScale});");
                cssBuilder.AppendLine("  }");
            }
            
            cssBuilder.AppendLine("}");

            return cssBuilder.ToString();
        }

        /// <summary>
        /// 生成缓存键
        /// </summary>
        private string GenerateCacheKey(string themeId, CssGenerationOptions options)
        {
            var optionsHash = options.GetHashCode();
            return $"{themeId}:theme:{optionsHash}";
        }

        /// <summary>
        /// 生成响应式缓存键
        /// </summary>
        private string GenerateResponsiveCacheKey(string themeId, BreakpointSettings breakpointSettings)
        {
            var settingsHash = $"{breakpointSettings.MinWidth}_{breakpointSettings.MaxWidth}_{breakpointSettings.GridColumns}_{breakpointSettings.GridGap}_{breakpointSettings.FontScale}".GetHashCode();
            return $"{themeId}:responsive:{settingsHash}";
        }

        /// <summary>
        /// 清理缓存（定时器回调）
        /// </summary>
        private void CleanupCache(object state)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-MaxUnusedMinutes);
                var keysToRemove = new List<string>();

                foreach (var kvp in _styleCache)
                {
                    var entry = kvp.Value;
                    
                    // 移除长时间未访问的条目
                    if (entry.LastAccessedAt < cutoffTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                // 如果缓存过大，移除最旧的条目
                if (_styleCache.Count > MaxCacheSize)
                {
                    var oldestEntries = _styleCache
                        .OrderBy(kvp => kvp.Value.LastAccessedAt)
                        .Take(_styleCache.Count - MaxCacheSize)
                        .Select(kvp => kvp.Key)
                        .ToList();
                    
                    keysToRemove.AddRange(oldestEntries);
                }

                // 执行清理
                foreach (var key in keysToRemove.Distinct())
                {
                    _styleCache.TryRemove(key, out _);
                }

                if (keysToRemove.Any())
                {
                    _logger.LogDebug("样式缓存清理完成，移除 {Count} 项", keysToRemove.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "样式缓存清理时发生错误");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _loadingSemaphore?.Dispose();
            _styleCache.Clear();
            _logger.LogDebug("懒加载样式服务已释放");
        }
    }

    /// <summary>
    /// 懒加载样式条目
    /// </summary>
    internal class LazyStyleEntry
    {
        private readonly object _lock = new object();
        
        public string CacheKey { get; }
        public DateTime CreatedAt { get; }
        public DateTime LastAccessedAt { get; private set; }
        public bool IsLoaded { get; private set; }
        public bool IsLoading => LoadingTask != null && !LoadingTask.IsCompleted;
        public bool HasError { get; private set; }
        public string CssContent { get; private set; }
        public Task<string> LoadingTask { get; set; }
        public Exception Error { get; private set; }

        public LazyStyleEntry(string cacheKey)
        {
            CacheKey = cacheKey;
            CreatedAt = DateTime.UtcNow;
            LastAccessedAt = DateTime.UtcNow;
        }

        public void UpdateLastAccessed()
        {
            lock (_lock)
            {
                LastAccessedAt = DateTime.UtcNow;
            }
        }

        public void SetLoaded(string cssContent)
        {
            lock (_lock)
            {
                CssContent = cssContent;
                IsLoaded = true;
                HasError = false;
                Error = null;
                LastAccessedAt = DateTime.UtcNow;
            }
        }

        public void SetError(Exception error)
        {
            lock (_lock)
            {
                Error = error;
                HasError = true;
                IsLoaded = false;
                CssContent = null;
            }
        }
    }

    /// <summary>
    /// 懒加载样式缓存统计信息
    /// </summary>
    public class LazyStyleCacheStatistics
    {
        public int TotalEntries { get; set; }
        public int LoadedEntries { get; set; }
        public int LoadingEntries { get; set; }
        public int ErrorEntries { get; set; }
        public long TotalCacheSize { get; set; }
        public DateTime? OldestEntry { get; set; }
        public DateTime? NewestEntry { get; set; }
        public DateTime? MostRecentlyAccessed { get; set; }
    }
}