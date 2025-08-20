using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using FluentAssertions;
using MediaBrowser.Model.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 性能基准测试，测试关键操作的性能指标
    /// </summary>
    public class PerformanceBenchmarkTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public PerformanceBenchmarkTests()
        {
            _mockLogger = new Mock<ILogger>();
            // 注意：这些测试将专注于数据处理性能，而不是服务集成
        }

        [Fact]
        public async Task ThemeSerialization_Performance_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var theme = TestConfiguration.CreateTestTheme();
            var iterations = 100;
            var maxAllowedTimeMs = 50; // 每次序列化不超过50ms

            // Act
            var stopwatch = Stopwatch.StartNew();
            var results = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var iterationStopwatch = Stopwatch.StartNew();
                var json = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(theme));
                var deserializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<Theme>(json));
                iterationStopwatch.Stop();
                
                results.Add(iterationStopwatch.ElapsedMilliseconds);
                deserializedTheme.Should().NotBeNull();
                deserializedTheme.Id.Should().Be(theme.Id);
            }

            stopwatch.Stop();

            // Assert
            var averageTime = results.Average();
            var maxTime = results.Max();
            var minTime = results.Min();

            averageTime.Should().BeLessThan(maxAllowedTimeMs, 
                $"平均序列化时间应小于 {maxAllowedTimeMs}ms，实际为 {averageTime:F2}ms");
            maxTime.Should().BeLessThan(maxAllowedTimeMs * 2, 
                $"最大序列化时间应小于 {maxAllowedTimeMs * 2}ms，实际为 {maxTime}ms");
        }

        [Fact]
        public async Task ThemeValidation_Performance_ShouldHandleMultipleValidationsEfficiently()
        {
            // Arrange
            var themes = TestConfiguration.CreateTestThemeList();
            var iterations = 50;
            var maxAllowedTimeMs = 20; // 每次验证不超过20ms

            // Act
            var results = new List<long>();
            
            for (int i = 0; i < iterations; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                foreach (var theme in themes)
                {
                    var validationErrors = await Task.FromResult(theme.Validate());
                    validationErrors.Should().NotBeNull();
                }
                stopwatch.Stop();
                
                results.Add(stopwatch.ElapsedMilliseconds);
            }

            // Assert
            var averageTime = results.Average();
            var maxTime = results.Max();

            averageTime.Should().BeLessThan(maxAllowedTimeMs, 
                $"平均验证时间应小于 {maxAllowedTimeMs}ms，实际为 {averageTime:F2}ms");
            maxTime.Should().BeLessThan(maxAllowedTimeMs * 3, 
                $"最大验证时间应小于 {maxAllowedTimeMs * 3}ms，实际为 {maxTime}ms");
        }

        [Fact]
        public async Task ConfigurationSerialization_Performance_ShouldBeOptimal()
        {
            // Arrange
            var config = TestConfiguration.CreateDefaultTestConfig();
            var iterations = 200;
            var maxAllowedTimeMs = 5; // 配置序列化不超过5ms

            // Act
            var results = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                var json = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(config));
                var deserializedConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<BeautifyConfig>(json));
                stopwatch.Stop();

                results.Add(stopwatch.ElapsedMilliseconds);
                deserializedConfig.Should().NotBeNull();
            }

            // Assert
            var averageTime = results.Average();
            var maxTime = results.Max();

            averageTime.Should().BeLessThan(maxAllowedTimeMs, 
                $"平均序列化时间应小于 {maxAllowedTimeMs}ms，实际为 {averageTime:F2}ms");
        }

        [Fact]
        public async Task LargeDataStructure_Performance_ShouldHandleComplexObjects()
        {
            // Arrange
            var largeConfig = TestConfiguration.CreateDefaultTestConfig();
            
            // 添加大量自定义设置来模拟复杂数据
            for (int i = 0; i < 1000; i++)
            {
                largeConfig.CustomSettings[$"setting_{i}"] = $"value_{i}";
            }
            
            var maxAllowedTimeMs = 50; // 大型对象处理不超过50ms

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            var json = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(largeConfig));
            var deserializedConfig = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<BeautifyConfig>(json));
            
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxAllowedTimeMs, 
                $"大型对象处理时间应小于 {maxAllowedTimeMs}ms，实际为 {stopwatch.ElapsedMilliseconds}ms");
            
            deserializedConfig.Should().NotBeNull();
            deserializedConfig.CustomSettings.Should().HaveCount(largeConfig.CustomSettings.Count);
        }

        [Fact]
        public async Task MemoryUsage_DuringIntensiveOperations_ShouldRemainStable()
        {
            // Arrange
            var themes = TestConfiguration.CreateTestThemeList();
            var iterations = 100;
            
            // 记录初始内存使用
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var initialMemory = GC.GetTotalMemory(false);

            // Act - 执行大量序列化操作
            for (int i = 0; i < iterations; i++)
            {
                foreach (var theme in themes)
                {
                    var json = await Task.FromResult(Newtonsoft.Json.JsonConvert.SerializeObject(theme));
                    var deserializedTheme = await Task.FromResult(Newtonsoft.Json.JsonConvert.DeserializeObject<Theme>(json));
                    deserializedTheme.Should().NotBeNull();
                }
                
                // 每20次操作检查一次内存
                if (i % 20 == 0)
                {
                    var currentMemory = GC.GetTotalMemory(false);
                    var memoryIncrease = currentMemory - initialMemory;
                    
                    // 内存增长不应超过50MB
                    memoryIncrease.Should().BeLessThan(50 * 1024 * 1024, 
                        $"内存使用增长过多: {memoryIncrease / 1024 / 1024}MB");
                }
            }

            // Assert - 最终内存检查
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(false);
            var totalMemoryIncrease = finalMemory - initialMemory;

            totalMemoryIncrease.Should().BeLessThan(100 * 1024 * 1024, 
                $"总内存增长应小于100MB，实际增长: {totalMemoryIncrease / 1024 / 1024}MB");
        }
    }
}