using EmbyBeautifyPlugin.Controllers;
using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using FluentAssertions;
using MediaBrowser.Model.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EmbyBeautifyPlugin.Tests
{
    /// <summary>
    /// 配置API控制器的单元测试
    /// </summary>
    public class ConfigurationApiControllerTests
    {
        private readonly Mock<IConfigurationManager> _mockConfigurationManager;
        private readonly Mock<ILogManager> _mockLogManager;
        private readonly Mock<ILogger> _mockLogger;
        private readonly ConfigurationApiController _controller;

        public ConfigurationApiControllerTests()
        {
            _mockConfigurationManager = new Mock<IConfigurationManager>();
            _mockLogManager = new Mock<ILogManager>();
            _mockLogger = new Mock<ILogger>();
            
            _mockLogManager.Setup(x => x.GetLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
            
            _controller = new ConfigurationApiController(_mockConfigurationManager.Object, _mockLogManager.Object);
        }

        [Fact]
        public async Task Get_GetConfigurationRequest_ReturnsConfiguration()
        {
            // Arrange
            var expectedConfig = TestConfiguration.GetSampleBeautifyConfig();
            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ReturnsAsync(expectedConfig);

            var request = new GetConfigurationRequest();

            // Act
            var result = await _controller.Get(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as GetConfigurationResponse;
            response.Should().NotBeNull();
            response.Configuration.Should().NotBeNull();
            response.Configuration.ActiveThemeId.Should().Be(expectedConfig.ActiveThemeId);
            response.Configuration.EnableAnimations.Should().Be(expectedConfig.EnableAnimations);
            response.LoadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            _mockConfigurationManager.Verify(x => x.LoadConfigurationAsync(), Times.Once);
        }

        [Fact]
        public async Task Get_GetConfigurationRequest_ConfigurationManagerThrows_ThrowsException()
        {
            // Arrange
            _mockConfigurationManager
                .Setup(x => x.LoadConfigurationAsync())
                .ThrowsAsync(new InvalidOperationException("Configuration load failed"));

            var request = new GetConfigurationRequest();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.Get(request));
            
            exception.Message.Should().Contain("Configuration load failed");
        }

        [Fact]
        public async Task Post_UpdateConfigurationRequest_ValidConfiguration_ReturnsSuccessResponse()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            _mockConfigurationManager
                .Setup(x => x.ValidateConfigurationAsync(config))
                .ReturnsAsync(true);
            
            _mockConfigurationManager
                .Setup(x => x.SaveConfigurationAsync(config))
                .Returns(Task.CompletedTask);

            var request = new UpdateConfigurationRequest { Configuration = config };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as UpdateConfigurationResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Message.Should().Contain("配置已成功更新");
            response.Configuration.Should().NotBeNull();
            response.Configuration.ActiveThemeId.Should().Be(config.ActiveThemeId);
            response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            _mockConfigurationManager.Verify(x => x.ValidateConfigurationAsync(config), Times.Once);
            _mockConfigurationManager.Verify(x => x.SaveConfigurationAsync(config), Times.Once);
        }

        [Fact]
        public async Task Post_UpdateConfigurationRequest_NullConfiguration_ReturnsFailureResponse()
        {
            // Arrange
            var request = new UpdateConfigurationRequest { Configuration = null };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as UpdateConfigurationResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("更新配置失败");
            response.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task Post_UpdateConfigurationRequest_InvalidConfiguration_ReturnsFailureResponse()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            _mockConfigurationManager
                .Setup(x => x.ValidateConfigurationAsync(config))
                .ReturnsAsync(false);

            var request = new UpdateConfigurationRequest { Configuration = config };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as UpdateConfigurationResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("更新配置失败");
            response.Configuration.Should().BeNull();
            
            _mockConfigurationManager.Verify(x => x.ValidateConfigurationAsync(config), Times.Once);
            _mockConfigurationManager.Verify(x => x.SaveConfigurationAsync(It.IsAny<BeautifyConfig>()), Times.Never);
        }

        [Fact]
        public async Task Post_UpdateConfigurationRequest_SaveThrows_ReturnsFailureResponse()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            _mockConfigurationManager
                .Setup(x => x.ValidateConfigurationAsync(config))
                .ReturnsAsync(true);
            
            _mockConfigurationManager
                .Setup(x => x.SaveConfigurationAsync(config))
                .ThrowsAsync(new InvalidOperationException("Save failed"));

            var request = new UpdateConfigurationRequest { Configuration = config };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as UpdateConfigurationResponse;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("更新配置失败");
            response.Configuration.Should().BeNull();
        }

        [Fact]
        public async Task Post_ValidateConfigurationRequest_ValidConfiguration_ReturnsValidResponse()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            _mockConfigurationManager
                .Setup(x => x.ValidateConfigurationAsync(config))
                .ReturnsAsync(true);

            var request = new ValidateConfigurationRequest { Configuration = config };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as ValidateConfigurationResponse;
            response.Should().NotBeNull();
            response.IsValid.Should().BeTrue();
            response.Message.Should().Contain("配置验证通过");
            response.ValidationErrors.Should().BeEmpty();
            response.ValidatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            
            _mockConfigurationManager.Verify(x => x.ValidateConfigurationAsync(config), Times.Once);
        }

        [Fact]
        public async Task Post_ValidateConfigurationRequest_InvalidConfiguration_ReturnsInvalidResponse()
        {
            // Arrange
            var config = new BeautifyConfig
            {
                ActiveThemeId = "", // 无效的空主题ID
                AnimationDuration = -100 // 无效的负数持续时间
            };
            
            _mockConfigurationManager
                .Setup(x => x.ValidateConfigurationAsync(config))
                .ReturnsAsync(false);

            var request = new ValidateConfigurationRequest { Configuration = config };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as ValidateConfigurationResponse;
            response.Should().NotBeNull();
            response.IsValid.Should().BeFalse();
            response.Message.Should().Contain("配置验证失败");
            response.ValidationErrors.Should().NotBeEmpty();
            response.ValidationErrors.Should().Contain(error => error.Contains("活动主题ID不能为空"));
            response.ValidationErrors.Should().Contain(error => error.Contains("动画持续时间必须在0-5000毫秒之间"));
        }

        [Fact]
        public async Task Post_ValidateConfigurationRequest_NullConfiguration_ReturnsErrorResponse()
        {
            // Arrange
            var request = new ValidateConfigurationRequest { Configuration = null };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as ValidateConfigurationResponse;
            response.Should().NotBeNull();
            response.IsValid.Should().BeFalse();
            response.Message.Should().Contain("验证配置时发生错误");
            response.ValidationErrors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Post_ValidateConfigurationRequest_ValidationThrows_ReturnsErrorResponse()
        {
            // Arrange
            var config = TestConfiguration.GetSampleBeautifyConfig();
            _mockConfigurationManager
                .Setup(x => x.ValidateConfigurationAsync(config))
                .ThrowsAsync(new InvalidOperationException("Validation error"));

            var request = new ValidateConfigurationRequest { Configuration = config };

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().NotBeNull();
            var response = result as ValidateConfigurationResponse;
            response.Should().NotBeNull();
            response.IsValid.Should().BeFalse();
            response.Message.Should().Contain("验证配置时发生错误");
            response.ValidationErrors.Should().Contain("Validation error");
        }

        [Fact]
        public void Constructor_NullConfigurationManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ConfigurationApiController(null, _mockLogManager.Object));
            
            exception.ParamName.Should().Be("configurationManager");
        }

        [Fact]
        public void Constructor_NullLogManager_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ConfigurationApiController(_mockConfigurationManager.Object, null));
            
            exception.ParamName.Should().Be("logManager");
        }

        [Theory]
        [InlineData("", true, "活动主题ID不能为空")]
        [InlineData("valid-theme", true, null)]
        [InlineData("valid-theme", false, null)]
        public void GetValidationErrors_VariousConfigurations_ReturnsExpectedErrors(
            string activeThemeId, bool enableAnimations, string expectedError)
        {
            // Arrange
            var config = new BeautifyConfig
            {
                ActiveThemeId = activeThemeId,
                EnableAnimations = enableAnimations,
                AnimationDuration = 300
            };

            var request = new ValidateConfigurationRequest { Configuration = config };

            // Act
            var result = _controller.Post(request).Result;

            // Assert
            var response = result as ValidateConfigurationResponse;
            if (expectedError != null)
            {
                response.ValidationErrors.Should().Contain(error => error.Contains(expectedError));
            }
        }
    }
}