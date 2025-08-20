using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using EmbyBeautifyPlugin.Models;
using EmbyBeautifyPlugin.Services;
using EmbyBeautifyPlugin.Interfaces;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 样式缓存服务测试类
    /// 测试样式缓存的功能和性能优化
    /// </summary>
    public class StyleCacheServiceTests : IDisposable
    {
        private readonly Mock<ILogger<StyleCacheService>> _mockLogger;
        private readonly Mock<IConfigurationManager> _mockConfigManager;
        private readonly StyleCacheService _cacheService;
        private readonly string _testCacheDirectory;

        public StyleCacheServiceTests()
        {
            _mockLogger = new Mock<ILogger<StyleCacheService>>();
            _mockConfigManager = new Mock<IConfigurationManager>();
            
            _cacheService = new StyleCacheService(_mockLogger.Object, _mockConfigManager.Object);
            
            // 设置测试缓存目录
            _testCacheDirectory = Path.Combine(Path.GetTempPath(), "EmbyBeautifyPlugin_Test", Guid.NewGuid().ToString());
        }

        [Fact]
        public async Task SetCachedStyleAsync_ShouldCacheContent()
        {
            // Arrange
            var cacheKey = "test-key";
            var content = "body { color: red; }";

            // Act
            await _cacheService.SetCachedStyleAsync(cacheKey, content);

            // Assert
            var cachedContent = await _cacheService.GetCachedStyleAsync(cacheKey);
            cachedContent.Should().Be(content);
        }

        [Fact]
        public async Task GetCachedStyleAsync_WithNonExistentKey_ShouldReturnNull()
        {
            // Arrange
            var cacheKey = "non-existent-key";

            // Act
            var result = await _cacheService.GetCachedStyleAsync(cacheKey);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SetCachedStyleAsync_WithEmptyContent_ShouldNotCache()
        {
            // Arrange
            var cacheKey = "test-key";
            var content = "";

            // Act
            await _cacheService.SetCachedStyleAsync(cacheKey, content);

            // Assert
            var cachedContent = await _cacheService.GetCachedStyleAsync(cacheKey);
            cachedContent.Should().BeNull();
        }

        [Fact]
        public async Task RemoveCachedStyleAsync_ShouldRemoveFromCache()
        {
            // Arrange
            var cacheKey = "test-key";
            var content = "body { color: blue; }";
            await _cacheService.SetCachedStyleAsync(cacheKey, content);

            // Act
            await _cacheService.RemoveCachedStyleAsync(cacheKey);

            // Assert
            var cachedContent = await _cacheService.GetCachedStyleAsync(cacheKey);
            cachedContent.Should().BeNull();
        }

        [Fact]
        public async Task GenerateCacheKey_WithSameInputs_ShouldReturnSameKey()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var config = TestConfiguration.GetSampleBeautifyConfig();

            // Act
            var key1 = _cacheService.GenerateCacheKey(theme, config);
            var key2 = _cacheService.GenerateCacheKey(theme, config);

            // Assert
            key1.Should().Be(key2);
            key1.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateCacheKey_WithDifferentInputs_ShouldReturnDifferentKeys()
        {
            // Arrange
            var theme1 = TestConfiguration.GetSampleTheme();
            var theme2 = TestConfiguration.GetSampleTheme();
            theme2.Id = "different-theme";
            var config = TestConfiguration.GetSampleBeautifyConfig();

            // Act
            var key1 = _cacheService.GenerateCacheKey(theme1, config);
            var key2 = _cacheService.GenerateCacheKey(theme2, config);

            // Assert
            key1.Should().NotBe(key2);
        }

        [Fact]
        public async Task SetCachedStyleAsync_WithExpiration_ShouldExpireAfterTime()
        {
            // Arrange
            var cacheKey = "expiring-key";
            var content = "body { color: green; }";
            var expiration = TimeSpan.FromMilliseconds(100);

            // Act
            await _cacheService.SetCachedStyleAsync(cacheKey, content, expiration);
            
            // 立即获取应该成功
            var immediateResult = await _cacheService.GetCachedStyleAsync(cacheKey);
            immediateResult.Should().Be(content);

            // 等待过期
            await Task.Delay(200);

            // Assert
            var expiredResult = await _cacheService.GetCachedStyleAsync(cacheKey);
            expiredResult.Should().BeNull();
        }

        [Fact]
        public async Task GetCacheStatisticsAsync_ShouldReturnValidStatistics()
        {
            // Arrange
            var cacheKey = "stats-test-key";
            var content = "body { color: purple; }";
            await _cacheService.SetCachedStyleAsync(cacheKey, content);

            // Act
            var statistics = await _cacheService.GetCacheStatisticsAsync();

            // Assert
            statistics.Should().NotBeNull();
            statistics.MemoryCacheEntries.Should().BeGreaterThan(0);
            statistics.MemoryCacheSize.Should().BeGreaterThan(0);
            statistics.CacheDirectory.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ClearAllCacheAsync_ShouldRemoveAllCachedItems()
        {
            // Arrange
            var keys = new[] { "key1", "key2", "key3" };
            var content = "body { color: orange; }";

            foreach (var key in keys)
            {
                await _cacheService.SetCachedStyleAsync(key, content);
            }

            // Verify items are cached
            foreach (var key in keys)
            {
                var cachedContent = await _cacheService.GetCachedStyleAsync(key);
                cachedContent.Should().Be(content);
            }

            // Act
            await _cacheService.ClearAllCacheAsync();

            // Assert
            foreach (var key in keys)
            {
                var cachedContent = await _cacheService.GetCachedStyleAsync(key);
                cachedContent.Should().BeNull();
            }
        }

        [Fact]
        public async Task CleanupExpiredCacheAsync_ShouldRemoveExpiredItems()
        {
            // Arrange
            var expiredKey = "expired-key";
            var validKey = "valid-key";
            var content = "body { color: yellow; }";

            // 添加一个很快过期的项目
            await _cacheService.SetCachedStyleAsync(expiredKey, content, TimeSpan.FromMilliseconds(50));
            
            // 添加一个长期有效的项目
            await _cacheService.SetCachedStyleAsync(validKey, content, TimeSpan.FromHours(1));

            // 等待第一个项目过期
            await Task.Delay(100);

            // Act
            await _cacheService.CleanupExpiredCacheAsync();

            // Assert
            var expiredResult = await _cacheService.GetCachedStyleAsync(expiredKey);
            var validResult = await _cacheService.GetCachedStyleAsync(validKey);

            expiredResult.Should().BeNull();
            validResult.Should().Be(content);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task GetCachedStyleAsync_WithInvalidKey_ShouldReturnNull(string invalidKey)
        {
            // Act
            var result = await _cacheService.GetCachedStyleAsync(invalidKey);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task SetCachedStyleAsync_WithInvalidKey_ShouldNotThrow(string invalidKey)
        {
            // Arrange
            var content = "body { color: black; }";

            // Act & Assert
            await _cacheService.Invoking(s => s.SetCachedStyleAsync(invalidKey, content))
                .Should().NotThrowAsync();
        }

        [Fact]
        public async Task GenerateCacheKey_WithNullInputs_ShouldReturnValidKey()
        {
            // Act
            var key = _cacheService.GenerateCacheKey(null, null);

            // Assert
            key.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GenerateCacheKey_WithAdditionalData_ShouldIncludeInKey()
        {
            // Arrange
            var theme = TestConfiguration.GetSampleTheme();
            var config = TestConfiguration.GetSampleBeautifyConfig();

            // Act
            var key1 = _cacheService.GenerateCacheKey(theme, config);
            var key2 = _cacheService.GenerateCacheKey(theme, config, "additional-data");

            // Assert
            key1.Should().NotBe(key2);
        }

        [Fact]
        public async Task CacheService_ShouldHandleConcurrentAccess()
        {
            // Arrange
            var tasks = new Task[10];
            var cacheKey = "concurrent-key";
            var content = "body { color: cyan; }";

            // Act - 并发设置和获取缓存
            for (int i = 0; i < tasks.Length; i++)
            {
                var index = i;
                tasks[i] = Task.Run(async () =>
                {
                    await _cacheService.SetCachedStyleAsync($"{cacheKey}-{index}", content);
                    var result = await _cacheService.GetCachedStyleAsync($"{cacheKey}-{index}");
                    result.Should().Be(content);
                });
            }

            // Assert
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task CacheService_ShouldHandleLargeContent()
        {
            // Arrange
            var cacheKey = "large-content-key";
            var largeContent = new string('x', 100000); // 100KB content

            // Act
            await _cacheService.SetCachedStyleAsync(cacheKey, largeContent);
            var result = await _cacheService.GetCachedStyleAsync(cacheKey);

            // Assert
            result.Should().Be(largeContent);
        }

        [Fact]
        public async Task CacheService_Performance_ShouldBeFast()
        {
            // Arrange
            var cacheKey = "performance-key";
            var content = "body { color: magenta; }";
            var iterations = 1000;

            // Act - 测试设置性能
            var startTime = DateTime.UtcNow;
            
            for (int i = 0; i < iterations; i++)
            {
                await _cacheService.SetCachedStyleAsync($"{cacheKey}-{i}", content);
            }
            
            var setTime = DateTime.UtcNow - startTime;

            // Act - 测试获取性能
            startTime = DateTime.UtcNow;
            
            for (int i = 0; i < iterations; i++)
            {
                var result = await _cacheService.GetCachedStyleAsync($"{cacheKey}-{i}");
                result.Should().Be(content);
            }
            
            var getTime = DateTime.UtcNow - startTime;

            // Assert - 性能应该在合理范围内
            setTime.TotalMilliseconds.Should().BeLessThan(5000); // 5秒内完成1000次设置
            getTime.TotalMilliseconds.Should().BeLessThan(1000); // 1秒内完成1000次获取
        }

        public void Dispose()
        {
            _cacheService?.Dispose();
            
            // 清理测试缓存目录
            if (Directory.Exists(_testCacheDirectory))
            {
                try
                {
                    Directory.Delete(_testCacheDirectory, true);
                }
                catch
                {
                    // 忽略清理错误
                }
            }
        }
    }
}