using EmbyBeautifyPlugin.Interfaces;
using EmbyBeautifyPlugin.Models;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbyBeautifyPlugin.Controllers
{
    /// <summary>
    /// API控制器，用于管理插件配置相关的操作
    /// </summary>
    [Route("/emby-beautify/configuration", "GET", Summary = "获取插件配置")]
    [Route("/emby-beautify/configuration", "POST", Summary = "更新插件配置")]
    [Route("/emby-beautify/configuration/validate", "POST", Summary = "验证插件配置")]
    public class ConfigurationApiController : IService
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly ILogger _logger;

        public ConfigurationApiController(IConfigurationManager configurationManager, ILogManager logManager)
        {
            _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
            _logger = logManager?.GetLogger(GetType().Name) ?? throw new ArgumentNullException(nameof(logManager));
        }

        /// <summary>
        /// 获取当前插件配置
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>当前配置</returns>
        public async Task<object> Get(GetConfigurationRequest request)
        {
            try
            {
                _logger.Debug("开始获取插件配置");

                var configuration = await _configurationManager.LoadConfigurationAsync();
                
                var response = new GetConfigurationResponse
                {
                    Configuration = configuration,
                    LoadedAt = DateTime.UtcNow
                };

                _logger.Debug("成功获取插件配置，活动主题: {0}", configuration.ActiveThemeId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("获取配置失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 更新插件配置
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>操作结果</returns>
        public async Task<object> Post(UpdateConfigurationRequest request)
        {
            try
            {
                _logger.Debug("开始更新插件配置，活动主题: {0}", request.Configuration?.ActiveThemeId);

                if (request.Configuration == null)
                {
                    throw new ArgumentException("配置不能为空", nameof(request.Configuration));
                }

                // 验证配置
                var isValid = await _configurationManager.ValidateConfigurationAsync(request.Configuration);
                if (!isValid)
                {
                    throw new ArgumentException("提供的配置无效");
                }

                // 保存配置
                await _configurationManager.SaveConfigurationAsync(request.Configuration);

                var response = new UpdateConfigurationResponse
                {
                    Success = true,
                    Message = "配置已成功更新",
                    Configuration = request.Configuration,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.Info("配置已成功更新，活动主题: {0}", request.Configuration.ActiveThemeId);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("更新配置失败", ex);
                
                return new UpdateConfigurationResponse
                {
                    Success = false,
                    Message = $"更新配置失败: {ex.Message}",
                    Configuration = null,
                    UpdatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 验证插件配置
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns>验证结果</returns>
        public async Task<object> Post(ValidateConfigurationRequest request)
        {
            try
            {
                _logger.Debug("开始验证插件配置");

                if (request.Configuration == null)
                {
                    throw new ArgumentException("配置不能为空", nameof(request.Configuration));
                }

                var isValid = await _configurationManager.ValidateConfigurationAsync(request.Configuration);
                
                var response = new ValidateConfigurationResponse
                {
                    IsValid = isValid,
                    Message = isValid ? "配置验证通过" : "配置验证失败",
                    ValidationErrors = isValid ? new string[0] : GetValidationErrors(request.Configuration),
                    ValidatedAt = DateTime.UtcNow
                };

                _logger.Debug("配置验证完成，结果: {0}", isValid);
                return response;
            }
            catch (Exception ex)
            {
                _logger.ErrorException("验证配置失败", ex);
                
                return new ValidateConfigurationResponse
                {
                    IsValid = false,
                    Message = $"验证配置时发生错误: {ex.Message}",
                    ValidationErrors = new[] { ex.Message },
                    ValidatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 获取配置验证错误信息
        /// </summary>
        /// <param name="configuration">要验证的配置</param>
        /// <returns>验证错误列表</returns>
        private string[] GetValidationErrors(BeautifyConfig configuration)
        {
            var errors = new List<string>();

            try
            {
                // 验证主题ID
                if (string.IsNullOrWhiteSpace(configuration.ActiveThemeId))
                {
                    errors.Add("活动主题ID不能为空");
                }

                // 验证动画持续时间
                if (configuration.AnimationDuration < 0 || configuration.AnimationDuration > 5000)
                {
                    errors.Add("动画持续时间必须在0-5000毫秒之间");
                }

                // 验证响应式设置
                if (configuration.ResponsiveSettings != null)
                {
                    if (configuration.ResponsiveSettings.Desktop?.MaxWidth <= 0)
                    {
                        errors.Add("桌面端最大宽度必须大于0");
                    }
                    
                    if (configuration.ResponsiveSettings.Tablet?.MaxWidth <= 0)
                    {
                        errors.Add("平板端最大宽度必须大于0");
                    }
                    
                    if (configuration.ResponsiveSettings.Mobile?.MaxWidth <= 0)
                    {
                        errors.Add("移动端最大宽度必须大于0");
                    }
                }

                // 验证自定义设置
                if (configuration.CustomSettings != null)
                {
                    foreach (var setting in configuration.CustomSettings)
                    {
                        if (string.IsNullOrWhiteSpace(setting.Key))
                        {
                            errors.Add("自定义设置的键不能为空");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"验证过程中发生错误: {ex.Message}");
            }

            return errors.ToArray();
        }
    }

    #region Request/Response Models

    /// <summary>
    /// 获取配置请求
    /// </summary>
    public class GetConfigurationRequest
    {
        // 空请求类，用于路由匹配
    }

    /// <summary>
    /// 获取配置响应
    /// </summary>
    public class GetConfigurationResponse
    {
        public BeautifyConfig Configuration { get; set; }
        public DateTime LoadedAt { get; set; }
    }

    /// <summary>
    /// 更新配置请求
    /// </summary>
    public class UpdateConfigurationRequest
    {
        public BeautifyConfig Configuration { get; set; }
    }

    /// <summary>
    /// 更新配置响应
    /// </summary>
    public class UpdateConfigurationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BeautifyConfig Configuration { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 验证配置请求
    /// </summary>
    public class ValidateConfigurationRequest
    {
        public BeautifyConfig Configuration { get; set; }
    }

    /// <summary>
    /// 验证配置响应
    /// </summary>
    public class ValidateConfigurationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string[] ValidationErrors { get; set; }
        public DateTime ValidatedAt { get; set; }
    }

    #endregion
}