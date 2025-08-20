using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 内存优化服务，负责监控和优化内存使用
    /// </summary>
    public class MemoryOptimizer
    {
        private readonly ILogger<MemoryOptimizer> _logger;
        private readonly Timer _monitoringTimer;
        private readonly Timer _optimizationTimer;
        
        private long _lastMemoryUsage;
        private DateTime _lastOptimizationTime;
        private readonly List<MemorySnapshot> _memoryHistory;
        private readonly object _lockObject = new object();
        
        // 配置参数
        private const long HighMemoryThresholdMB = 200; // 200MB
        private const long CriticalMemoryThresholdMB = 500; // 500MB
        private const int MonitoringIntervalSeconds = 30; // 30秒监控一次
        private const int OptimizationIntervalMinutes = 10; // 10分钟优化一次
        private const int MaxHistoryEntries = 100;

        public MemoryOptimizer(ILogger<MemoryOptimizer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _memoryHistory = new List<MemorySnapshot>();
            _lastOptimizationTime = DateTime.UtcNow;
            
            // 启动内存监控定时器
            _monitoringTimer = new Timer(MonitorMemoryUsage, null, 
                TimeSpan.Zero, TimeSpan.FromSeconds(MonitoringIntervalSeconds));
            
            // 启动内存优化定时器
            _optimizationTimer = new Timer(PerformMemoryOptimization, null,
                TimeSpan.FromMinutes(OptimizationIntervalMinutes), 
                TimeSpan.FromMinutes(OptimizationIntervalMinutes));
        }

        /// <summary>
        /// 获取当前内存使用情况
        /// </summary>
        /// <returns>内存使用信息</returns>
        public MemoryUsageInfo GetCurrentMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            var gcMemory = GC.GetTotalMemory(false);
            
            return new MemoryUsageInfo
            {
                Timestamp = DateTime.UtcNow,
                WorkingSetMB = process.WorkingSet64 / 1024.0 / 1024.0,
                PrivateMemoryMB = process.PrivateMemorySize64 / 1024.0 / 1024.0,
                GCMemoryMB = gcMemory / 1024.0 / 1024.0,
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2)
            };
        }

        /// <summary>
        /// 执行内存优化
        /// </summary>
        /// <param name="force">是否强制执行优化</param>
        /// <returns>优化结果</returns>
        public async Task<MemoryOptimizationResult> OptimizeMemoryAsync(bool force = false)
        {
            var beforeOptimization = GetCurrentMemoryUsage();
            var optimizationSteps = new List<string>();
            
            _logger.LogInformation("开始内存优化 - 当前内存使用: {WorkingSetMB:F2} MB, GC内存: {GCMemoryMB:F2} MB", 
                beforeOptimization.WorkingSetMB, beforeOptimization.GCMemoryMB);

            try
            {
                // 步骤1: 清理弱引用和终结器队列
                optimizationSteps.Add("清理弱引用和终结器队列");
                GC.WaitForPendingFinalizers();
                
                // 步骤2: 执行垃圾回收
                optimizationSteps.Add("执行垃圾回收");
                GC.Collect(0, GCCollectionMode.Optimized);
                await Task.Delay(100); // 给GC一些时间
                
                GC.Collect(1, GCCollectionMode.Optimized);
                await Task.Delay(100);
                
                // 如果内存使用过高或强制执行，进行更激进的优化
                if (force || beforeOptimization.GCMemoryMB > HighMemoryThresholdMB)
                {
                    optimizationSteps.Add("执行完整垃圾回收");
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

                // 步骤3: 压缩大对象堆（如果内存使用过高）
                if (force || beforeOptimization.GCMemoryMB > CriticalMemoryThresholdMB)
                {
                    optimizationSteps.Add("压缩大对象堆");
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                }

                // 步骤4: 清理字符串池（如果可能）
                optimizationSteps.Add("清理内部缓存");
                await ClearInternalCachesAsync();

                // 等待优化完成
                await Task.Delay(500);
                
                var afterOptimization = GetCurrentMemoryUsage();
                var memoryFreed = beforeOptimization.GCMemoryMB - afterOptimization.GCMemoryMB;
                
                var result = new MemoryOptimizationResult
                {
                    OptimizationTime = DateTime.UtcNow,
                    BeforeOptimization = beforeOptimization,
                    AfterOptimization = afterOptimization,
                    MemoryFreedMB = memoryFreed,
                    OptimizationSteps = optimizationSteps,
                    Success = true
                };

                _logger.LogInformation("内存优化完成 - 释放内存: {MemoryFreedMB:F2} MB, 当前内存: {CurrentMemoryMB:F2} MB", 
                    memoryFreed, afterOptimization.GCMemoryMB);

                _lastOptimizationTime = DateTime.UtcNow;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "内存优化过程中发生错误");
                
                return new MemoryOptimizationResult
                {
                    OptimizationTime = DateTime.UtcNow,
                    BeforeOptimization = beforeOptimization,
                    AfterOptimization = GetCurrentMemoryUsage(),
                    OptimizationSteps = optimizationSteps,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 获取内存使用历史
        /// </summary>
        /// <param name="hours">获取最近几小时的历史，默认为24小时</param>
        /// <returns>内存使用历史</returns>
        public List<MemorySnapshot> GetMemoryHistory(int hours = 24)
        {
            lock (_lockObject)
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-hours);
                return _memoryHistory
                    .Where(snapshot => snapshot.Timestamp >= cutoffTime)
                    .OrderBy(snapshot => snapshot.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取内存使用趋势分析
        /// </summary>
        /// <returns>趋势分析结果</returns>
        public MemoryTrendAnalysis AnalyzeMemoryTrends()
        {
            lock (_lockObject)
            {
                if (_memoryHistory.Count < 2)
                {
                    return new MemoryTrendAnalysis
                    {
                        HasSufficientData = false,
                        Message = "数据不足，无法进行趋势分析"
                    };
                }

                var recentHistory = _memoryHistory.TakeLast(20).ToList();
                var averageMemory = recentHistory.Average(h => h.GCMemoryMB);
                var maxMemory = recentHistory.Max(h => h.GCMemoryMB);
                var minMemory = recentHistory.Min(h => h.GCMemoryMB);
                
                // 计算内存增长趋势
                var firstHalf = recentHistory.Take(recentHistory.Count / 2).Average(h => h.GCMemoryMB);
                var secondHalf = recentHistory.Skip(recentHistory.Count / 2).Average(h => h.GCMemoryMB);
                var growthTrend = secondHalf - firstHalf;

                // 分析GC频率
                var totalGCCollections = recentHistory.Last().Gen0Collections + 
                                       recentHistory.Last().Gen1Collections + 
                                       recentHistory.Last().Gen2Collections;
                
                var firstGCCollections = recentHistory.First().Gen0Collections + 
                                       recentHistory.First().Gen1Collections + 
                                       recentHistory.First().Gen2Collections;
                
                var gcFrequency = totalGCCollections - firstGCCollections;

                return new MemoryTrendAnalysis
                {
                    HasSufficientData = true,
                    AverageMemoryMB = averageMemory,
                    MaxMemoryMB = maxMemory,
                    MinMemoryMB = minMemory,
                    MemoryGrowthTrendMB = growthTrend,
                    GCFrequency = gcFrequency,
                    IsMemoryGrowing = growthTrend > 5, // 增长超过5MB认为是增长趋势
                    IsHighMemoryUsage = averageMemory > HighMemoryThresholdMB,
                    IsCriticalMemoryUsage = maxMemory > CriticalMemoryThresholdMB,
                    Recommendations = GenerateMemoryRecommendations(averageMemory, growthTrend, gcFrequency)
                };
            }
        }

        /// <summary>
        /// 监控内存使用（定时器回调）
        /// </summary>
        private void MonitorMemoryUsage(object state)
        {
            try
            {
                var currentUsage = GetCurrentMemoryUsage();
                var snapshot = new MemorySnapshot
                {
                    Timestamp = currentUsage.Timestamp,
                    WorkingSetMB = currentUsage.WorkingSetMB,
                    PrivateMemoryMB = currentUsage.PrivateMemoryMB,
                    GCMemoryMB = currentUsage.GCMemoryMB,
                    Gen0Collections = currentUsage.Gen0Collections,
                    Gen1Collections = currentUsage.Gen1Collections,
                    Gen2Collections = currentUsage.Gen2Collections
                };

                lock (_lockObject)
                {
                    _memoryHistory.Add(snapshot);
                    
                    // 限制历史记录数量
                    while (_memoryHistory.Count > MaxHistoryEntries)
                    {
                        _memoryHistory.RemoveAt(0);
                    }
                }

                // 检查是否需要警告
                if (currentUsage.GCMemoryMB > CriticalMemoryThresholdMB)
                {
                    _logger.LogWarning("内存使用过高: {MemoryMB:F2} MB，建议执行内存优化", currentUsage.GCMemoryMB);
                }
                else if (currentUsage.GCMemoryMB > HighMemoryThresholdMB)
                {
                    _logger.LogInformation("内存使用较高: {MemoryMB:F2} MB", currentUsage.GCMemoryMB);
                }

                _lastMemoryUsage = (long)currentUsage.GCMemoryMB;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "监控内存使用时发生错误");
            }
        }

        /// <summary>
        /// 执行内存优化（定时器回调）
        /// </summary>
        private async void PerformMemoryOptimization(object state)
        {
            try
            {
                var currentUsage = GetCurrentMemoryUsage();
                
                // 只有在内存使用较高时才执行自动优化
                if (currentUsage.GCMemoryMB > HighMemoryThresholdMB)
                {
                    await OptimizeMemoryAsync(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自动内存优化时发生错误");
            }
        }

        /// <summary>
        /// 清理内部缓存
        /// </summary>
        private async Task ClearInternalCachesAsync()
        {
            // 这里可以添加清理各种内部缓存的逻辑
            // 例如：清理CSS缓存、主题缓存等
            
            await Task.CompletedTask; // 占位符，实际实现时可以调用其他服务的清理方法
        }

        /// <summary>
        /// 生成内存优化建议
        /// </summary>
        private List<string> GenerateMemoryRecommendations(double averageMemory, double growthTrend, int gcFrequency)
        {
            var recommendations = new List<string>();

            if (averageMemory > CriticalMemoryThresholdMB)
            {
                recommendations.Add("内存使用过高，建议立即执行内存优化");
                recommendations.Add("考虑减少缓存大小或清理不必要的数据");
            }
            else if (averageMemory > HighMemoryThresholdMB)
            {
                recommendations.Add("内存使用较高，建议定期执行内存优化");
            }

            if (growthTrend > 10)
            {
                recommendations.Add("检测到内存持续增长，可能存在内存泄漏");
                recommendations.Add("建议检查长期持有的对象引用");
            }

            if (gcFrequency > 50)
            {
                recommendations.Add("GC频率较高，建议优化对象分配策略");
                recommendations.Add("考虑使用对象池或减少临时对象创建");
            }

            if (!recommendations.Any())
            {
                recommendations.Add("内存使用正常，无需特殊优化");
            }

            return recommendations;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _monitoringTimer?.Dispose();
            _optimizationTimer?.Dispose();
            
            lock (_lockObject)
            {
                _memoryHistory.Clear();
            }
            
            _logger.LogDebug("内存优化器已释放");
        }
    }

    /// <summary>
    /// 内存使用信息
    /// </summary>
    public class MemoryUsageInfo
    {
        public DateTime Timestamp { get; set; }
        public double WorkingSetMB { get; set; }
        public double PrivateMemoryMB { get; set; }
        public double GCMemoryMB { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
    }

    /// <summary>
    /// 内存快照
    /// </summary>
    public class MemorySnapshot
    {
        public DateTime Timestamp { get; set; }
        public double WorkingSetMB { get; set; }
        public double PrivateMemoryMB { get; set; }
        public double GCMemoryMB { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
    }

    /// <summary>
    /// 内存优化结果
    /// </summary>
    public class MemoryOptimizationResult
    {
        public DateTime OptimizationTime { get; set; }
        public MemoryUsageInfo BeforeOptimization { get; set; }
        public MemoryUsageInfo AfterOptimization { get; set; }
        public double MemoryFreedMB { get; set; }
        public List<string> OptimizationSteps { get; set; } = new List<string>();
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    /// <summary>
    /// 内存趋势分析结果
    /// </summary>
    public class MemoryTrendAnalysis
    {
        public bool HasSufficientData { get; set; }
        public string Message { get; set; }
        public double AverageMemoryMB { get; set; }
        public double MaxMemoryMB { get; set; }
        public double MinMemoryMB { get; set; }
        public double MemoryGrowthTrendMB { get; set; }
        public int GCFrequency { get; set; }
        public bool IsMemoryGrowing { get; set; }
        public bool IsHighMemoryUsage { get; set; }
        public bool IsCriticalMemoryUsage { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}