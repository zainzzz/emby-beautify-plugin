using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// 性能监控服务，用于跟踪和分析插件性能
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly ILogger<PerformanceMonitor> _logger;
        private readonly ConcurrentDictionary<string, PerformanceMetric> _metrics;
        private readonly ConcurrentQueue<PerformanceEvent> _events;
        private readonly Timer _reportTimer;
        private readonly object _lockObject = new object();
        
        private long _totalMemoryUsage;
        private int _activeOperations;
        private DateTime _lastReportTime;

        public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = new ConcurrentDictionary<string, PerformanceMetric>();
            _events = new ConcurrentQueue<PerformanceEvent>();
            _lastReportTime = DateTime.UtcNow;
            
            // 每5分钟生成一次性能报告
            _reportTimer = new Timer(GeneratePerformanceReport, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// 开始监控一个操作
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="context">操作上下文</param>
        /// <returns>性能跟踪器</returns>
        public IPerformanceTracker StartOperation(string operationName, Dictionary<string, object> context = null)
        {
            Interlocked.Increment(ref _activeOperations);
            
            var tracker = new PerformanceTracker(operationName, context, this);
            
            _logger.LogDebug("开始监控操作: {OperationName}", operationName);
            
            return tracker;
        }

        /// <summary>
        /// 记录操作完成
        /// </summary>
        /// <param name="operationName">操作名称</param>
        /// <param name="duration">持续时间</param>
        /// <param name="success">是否成功</param>
        /// <param name="context">操作上下文</param>
        internal void RecordOperation(string operationName, TimeSpan duration, bool success, Dictionary<string, object> context)
        {
            Interlocked.Decrement(ref _activeOperations);
            
            // 更新性能指标
            var metric = _metrics.AddOrUpdate(operationName, 
                new PerformanceMetric(operationName),
                (key, existing) => existing);
            
            metric.RecordExecution(duration, success);
            
            // 记录性能事件
            var performanceEvent = new PerformanceEvent
            {
                OperationName = operationName,
                Duration = duration,
                Success = success,
                Timestamp = DateTime.UtcNow,
                Context = context,
                MemoryUsage = GC.GetTotalMemory(false)
            };
            
            _events.Enqueue(performanceEvent);
            
            // 限制事件队列大小
            while (_events.Count > 1000)
            {
                _events.TryDequeue(out _);
            }
            
            // 记录慢操作
            if (duration.TotalMilliseconds > 1000) // 超过1秒的操作
            {
                _logger.LogWarning("检测到慢操作: {OperationName}, 耗时: {Duration}ms", 
                    operationName, duration.TotalMilliseconds);
            }
        }

        /// <summary>
        /// 记录内存使用情况
        /// </summary>
        /// <param name="memoryUsage">内存使用量（字节）</param>
        public void RecordMemoryUsage(long memoryUsage)
        {
            Interlocked.Exchange(ref _totalMemoryUsage, memoryUsage);
        }

        /// <summary>
        /// 获取性能统计信息
        /// </summary>
        /// <returns>性能统计</returns>
        public PerformanceStatistics GetStatistics()
        {
            var stats = new PerformanceStatistics
            {
                ActiveOperations = _activeOperations,
                TotalMemoryUsage = _totalMemoryUsage,
                GeneratedAt = DateTime.UtcNow
            };

            // 复制指标数据
            foreach (var metric in _metrics.Values)
            {
                stats.Metrics[metric.OperationName] = new PerformanceMetricSnapshot
                {
                    OperationName = metric.OperationName,
                    TotalExecutions = metric.TotalExecutions,
                    SuccessfulExecutions = metric.SuccessfulExecutions,
                    FailedExecutions = metric.FailedExecutions,
                    AverageExecutionTime = metric.AverageExecutionTime,
                    MinExecutionTime = metric.MinExecutionTime,
                    MaxExecutionTime = metric.MaxExecutionTime,
                    LastExecutionTime = metric.LastExecutionTime,
                    SuccessRate = metric.SuccessRate
                };
            }

            // 获取最近的事件
            var recentEvents = _events.ToArray()
                .Where(e => e.Timestamp > DateTime.UtcNow.AddMinutes(-10))
                .OrderByDescending(e => e.Timestamp)
                .Take(50)
                .ToList();

            stats.RecentEvents = recentEvents;

            return stats;
        }

        /// <summary>
        /// 获取性能报告
        /// </summary>
        /// <returns>性能报告</returns>
        public PerformanceReport GenerateReport()
        {
            var stats = GetStatistics();
            var report = new PerformanceReport
            {
                GeneratedAt = DateTime.UtcNow,
                ReportPeriod = DateTime.UtcNow - _lastReportTime,
                Statistics = stats
            };

            // 分析性能趋势
            AnalyzePerformanceTrends(report);

            // 生成建议
            GenerateRecommendations(report);

            return report;
        }

        /// <summary>
        /// 分析性能趋势
        /// </summary>
        /// <param name="report">性能报告</param>
        private void AnalyzePerformanceTrends(PerformanceReport report)
        {
            var trends = new List<string>();

            // 分析慢操作
            var slowOperations = report.Statistics.Metrics.Values
                .Where(m => m.AverageExecutionTime.TotalMilliseconds > 500)
                .OrderByDescending(m => m.AverageExecutionTime.TotalMilliseconds)
                .ToList();

            if (slowOperations.Any())
            {
                trends.Add($"检测到 {slowOperations.Count} 个慢操作，平均耗时超过500ms");
            }

            // 分析失败率
            var highFailureOperations = report.Statistics.Metrics.Values
                .Where(m => m.SuccessRate < 0.95 && m.TotalExecutions > 10)
                .OrderBy(m => m.SuccessRate)
                .ToList();

            if (highFailureOperations.Any())
            {
                trends.Add($"检测到 {highFailureOperations.Count} 个高失败率操作，成功率低于95%");
            }

            // 分析内存使用
            var currentMemoryMB = report.Statistics.TotalMemoryUsage / 1024.0 / 1024.0;
            if (currentMemoryMB > 100) // 超过100MB
            {
                trends.Add($"内存使用较高: {currentMemoryMB:F2} MB");
            }

            report.Trends = trends;
        }

        /// <summary>
        /// 生成性能优化建议
        /// </summary>
        /// <param name="report">性能报告</param>
        private void GenerateRecommendations(PerformanceReport report)
        {
            var recommendations = new List<string>();

            // 基于慢操作的建议
            var slowOperations = report.Statistics.Metrics.Values
                .Where(m => m.AverageExecutionTime.TotalMilliseconds > 1000)
                .ToList();

            foreach (var operation in slowOperations)
            {
                recommendations.Add($"优化 '{operation.OperationName}' 操作，当前平均耗时 {operation.AverageExecutionTime.TotalMilliseconds:F0}ms");
            }

            // 基于内存使用的建议
            var memoryMB = report.Statistics.TotalMemoryUsage / 1024.0 / 1024.0;
            if (memoryMB > 200)
            {
                recommendations.Add("考虑实施内存优化策略，当前内存使用过高");
            }

            // 基于失败率的建议
            var unreliableOperations = report.Statistics.Metrics.Values
                .Where(m => m.SuccessRate < 0.9 && m.TotalExecutions > 5)
                .ToList();

            foreach (var operation in unreliableOperations)
            {
                recommendations.Add($"改进 '{operation.OperationName}' 操作的可靠性，当前成功率 {operation.SuccessRate:P1}");
            }

            report.Recommendations = recommendations;
        }

        /// <summary>
        /// 生成性能报告（定时器回调）
        /// </summary>
        /// <param name="state">状态对象</param>
        private void GeneratePerformanceReport(object state)
        {
            try
            {
                var report = GenerateReport();
                
                _logger.LogInformation("性能报告生成完成 - 活跃操作: {ActiveOperations}, 内存使用: {MemoryMB:F2} MB, 监控指标: {MetricsCount}",
                    report.Statistics.ActiveOperations,
                    report.Statistics.TotalMemoryUsage / 1024.0 / 1024.0,
                    report.Statistics.Metrics.Count);

                if (report.Trends.Any())
                {
                    _logger.LogInformation("性能趋势: {Trends}", string.Join("; ", report.Trends));
                }

                if (report.Recommendations.Any())
                {
                    _logger.LogInformation("优化建议: {Recommendations}", string.Join("; ", report.Recommendations));
                }

                _lastReportTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成性能报告时发生错误");
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            _reportTimer?.Dispose();
            _logger.LogDebug("性能监控器已释放");
        }
    }

    /// <summary>
    /// 性能跟踪器接口
    /// </summary>
    public interface IPerformanceTracker : IDisposable
    {
        /// <summary>
        /// 完成操作
        /// </summary>
        /// <param name="success">是否成功</param>
        void Complete(bool success = true);
    }

    /// <summary>
    /// 性能跟踪器实现
    /// </summary>
    internal class PerformanceTracker : IPerformanceTracker
    {
        private readonly string _operationName;
        private readonly Dictionary<string, object> _context;
        private readonly PerformanceMonitor _monitor;
        private readonly Stopwatch _stopwatch;
        private bool _completed;

        public PerformanceTracker(string operationName, Dictionary<string, object> context, PerformanceMonitor monitor)
        {
            _operationName = operationName;
            _context = context;
            _monitor = monitor;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Complete(bool success = true)
        {
            if (_completed) return;

            _stopwatch.Stop();
            _monitor.RecordOperation(_operationName, _stopwatch.Elapsed, success, _context);
            _completed = true;
        }

        public void Dispose()
        {
            if (!_completed)
            {
                Complete(false); // 如果没有显式完成，标记为失败
            }
        }
    }

    /// <summary>
    /// 性能指标
    /// </summary>
    public class PerformanceMetric
    {
        private readonly object _lock = new object();
        private long _totalExecutions;
        private long _successfulExecutions;
        private long _totalExecutionTime; // 毫秒
        private long _minExecutionTime = long.MaxValue;
        private long _maxExecutionTime;
        private DateTime _lastExecutionTime;

        public string OperationName { get; }
        
        public long TotalExecutions => _totalExecutions;
        public long SuccessfulExecutions => _successfulExecutions;
        public long FailedExecutions => _totalExecutions - _successfulExecutions;
        
        public TimeSpan AverageExecutionTime => _totalExecutions > 0 
            ? TimeSpan.FromMilliseconds((double)_totalExecutionTime / _totalExecutions)
            : TimeSpan.Zero;
        
        public TimeSpan MinExecutionTime => _minExecutionTime == long.MaxValue 
            ? TimeSpan.Zero 
            : TimeSpan.FromMilliseconds(_minExecutionTime);
        
        public TimeSpan MaxExecutionTime => TimeSpan.FromMilliseconds(_maxExecutionTime);
        public DateTime LastExecutionTime => _lastExecutionTime;
        
        public double SuccessRate => _totalExecutions > 0 
            ? (double)_successfulExecutions / _totalExecutions 
            : 0.0;

        public PerformanceMetric(string operationName)
        {
            OperationName = operationName;
        }

        public void RecordExecution(TimeSpan duration, bool success)
        {
            var durationMs = (long)duration.TotalMilliseconds;
            
            lock (_lock)
            {
                _totalExecutions++;
                if (success) _successfulExecutions++;
                
                _totalExecutionTime += durationMs;
                _minExecutionTime = Math.Min(_minExecutionTime, durationMs);
                _maxExecutionTime = Math.Max(_maxExecutionTime, durationMs);
                _lastExecutionTime = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// 性能事件
    /// </summary>
    public class PerformanceEvent
    {
        public string OperationName { get; set; }
        public TimeSpan Duration { get; set; }
        public bool Success { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Context { get; set; }
        public long MemoryUsage { get; set; }
    }

    /// <summary>
    /// 性能指标快照
    /// </summary>
    public class PerformanceMetricSnapshot
    {
        public string OperationName { get; set; }
        public long TotalExecutions { get; set; }
        public long SuccessfulExecutions { get; set; }
        public long FailedExecutions { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public TimeSpan MinExecutionTime { get; set; }
        public TimeSpan MaxExecutionTime { get; set; }
        public DateTime LastExecutionTime { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// 性能统计信息
    /// </summary>
    public class PerformanceStatistics
    {
        public int ActiveOperations { get; set; }
        public long TotalMemoryUsage { get; set; }
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, PerformanceMetricSnapshot> Metrics { get; set; } = new Dictionary<string, PerformanceMetricSnapshot>();
        public List<PerformanceEvent> RecentEvents { get; set; } = new List<PerformanceEvent>();
    }

    /// <summary>
    /// 性能报告
    /// </summary>
    public class PerformanceReport
    {
        public DateTime GeneratedAt { get; set; }
        public TimeSpan ReportPeriod { get; set; }
        public PerformanceStatistics Statistics { get; set; }
        public List<string> Trends { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }
}