/**
 * Emby 美化插件 - 客户端样式注入器
 * 负责在浏览器端动态注入和管理样式
 */

(function(window, document) {
    'use strict';

    // 命名空间
    window.EmbyBeautifyStyleInjector = window.EmbyBeautifyStyleInjector || {};

    const StyleInjector = {
        // 配置选项
        config: {
            stylePrefix: 'emby-beautify-',
            maxRetries: 3,
            retryDelay: 1000,
            updateInterval: 5000,
            debugMode: false
        },

        // 内部状态
        state: {
            injectedStyles: new Map(),
            observers: new Map(),
            isInitialized: false,
            lastUpdateTime: 0,
            retryCount: 0
        },

        // 浏览器兼容性检测
        compatibility: {
            supportsCustomProperties: false,
            supportsObserver: false,
            supportsPromises: false,
            browserInfo: {}
        },

        /**
         * 初始化样式注入器
         */
        init: function() {
            if (this.state.isInitialized) {
                this.log('样式注入器已经初始化');
                return Promise.resolve();
            }

            this.log('初始化样式注入器...');
            
            return this.detectBrowserCompatibility()
                .then(() => this.setupStyleContainer())
                .then(() => this.setupMutationObserver())
                .then(() => this.loadInitialStyles())
                .then(() => {
                    this.state.isInitialized = true;
                    this.log('样式注入器初始化完成');
                    this.startPeriodicUpdate();
                })
                .catch(error => {
                    this.error('初始化失败:', error);
                    throw error;
                });
        },

        /**
         * 检测浏览器兼容性
         */
        detectBrowserCompatibility: function() {
            return new Promise((resolve) => {
                // 检测 CSS 自定义属性支持
                this.compatibility.supportsCustomProperties = 
                    window.CSS && window.CSS.supports && window.CSS.supports('color', 'var(--test)');

                // 检测 MutationObserver 支持
                this.compatibility.supportsObserver = 
                    typeof window.MutationObserver !== 'undefined';

                // 检测 Promise 支持
                this.compatibility.supportsPromises = 
                    typeof Promise !== 'undefined';

                // 获取浏览器信息
                this.compatibility.browserInfo = this.getBrowserInfo();

                this.log('浏览器兼容性检测完成:', this.compatibility);
                resolve();
            });
        },

        /**
         * 获取浏览器信息
         */
        getBrowserInfo: function() {
            const ua = navigator.userAgent;
            const info = {
                isChrome: /Chrome/.test(ua) && /Google Inc/.test(navigator.vendor),
                isFirefox: /Firefox/.test(ua),
                isSafari: /Safari/.test(ua) && /Apple Computer/.test(navigator.vendor),
                isEdge: /Edge/.test(ua),
                isIE: /Trident/.test(ua),
                isMobile: /Mobile|Android|iPhone|iPad/.test(ua)
            };

            // 获取版本号
            if (info.isChrome) {
                const match = ua.match(/Chrome\/(\d+)/);
                info.version = match ? parseInt(match[1]) : 0;
            } else if (info.isFirefox) {
                const match = ua.match(/Firefox\/(\d+)/);
                info.version = match ? parseInt(match[1]) : 0;
            } else if (info.isSafari) {
                const match = ua.match(/Version\/(\d+)/);
                info.version = match ? parseInt(match[1]) : 0;
            }

            return info;
        },

        /**
         * 设置样式容器
         */
        setupStyleContainer: function() {
            return new Promise((resolve) => {
                // 创建或获取样式容器
                let container = document.getElementById(this.config.stylePrefix + 'container');
                
                if (!container) {
                    container = document.createElement('div');
                    container.id = this.config.stylePrefix + 'container';
                    container.style.display = 'none';
                    
                    // 插入到 head 中
                    const head = document.head || document.getElementsByTagName('head')[0];
                    head.appendChild(container);
                    
                    this.log('创建样式容器');
                }

                this.state.styleContainer = container;
                resolve();
            });
        },

        /**
         * 设置 DOM 变化监听器
         */
        setupMutationObserver: function() {
            return new Promise((resolve) => {
                if (!this.compatibility.supportsObserver) {
                    this.log('浏览器不支持 MutationObserver，跳过设置');
                    resolve();
                    return;
                }

                const observer = new MutationObserver((mutations) => {
                    let shouldUpdate = false;
                    
                    mutations.forEach((mutation) => {
                        if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                            // 检查是否有新的 Emby 内容节点
                            for (let node of mutation.addedNodes) {
                                if (node.nodeType === Node.ELEMENT_NODE && 
                                    (node.classList.contains('page') || 
                                     node.classList.contains('view') ||
                                     node.querySelector('.page, .view'))) {
                                    shouldUpdate = true;
                                    break;
                                }
                            }
                        }
                    });

                    if (shouldUpdate) {
                        this.log('检测到页面内容变化，准备更新样式');
                        this.debounce('updateStyles', () => this.updateStyles(), 500);
                    }
                });

                // 开始观察
                observer.observe(document.body, {
                    childList: true,
                    subtree: true
                });

                this.state.observers.set('mutation', observer);
                this.log('DOM 变化监听器设置完成');
                resolve();
            });
        },

        /**
         * 加载初始样式
         */
        loadInitialStyles: function() {
            return this.fetchCurrentTheme()
                .then(theme => {
                    if (theme) {
                        return this.applyTheme(theme);
                    }
                })
                .catch(error => {
                    this.error('加载初始样式失败:', error);
                    // 应用默认样式作为回退
                    return this.applyDefaultStyles();
                });
        },

        /**
         * 获取当前主题
         */
        fetchCurrentTheme: function() {
            return this.makeRequest('/emby-beautify/theme/current')
                .then(response => response.json())
                .then(data => {
                    if (data.Success) {
                        return data.Theme;
                    } else {
                        throw new Error(data.Message || '获取主题失败');
                    }
                });
        },

        /**
         * 应用主题
         */
        applyTheme: function(theme) {
            return new Promise((resolve, reject) => {
                try {
                    this.log('应用主题:', theme.Name);
                    
                    // 生成 CSS
                    const css = this.generateThemeCSS(theme);
                    
                    // 注入样式
                    this.injectStyle('theme', css, { priority: 'high' });
                    
                    // 应用自定义属性
                    if (this.compatibility.supportsCustomProperties) {
                        this.applyCustomProperties(theme);
                    }
                    
                    // 触发主题应用事件
                    this.dispatchEvent('themeApplied', { theme: theme });
                    
                    resolve();
                } catch (error) {
                    reject(error);
                }
            });
        },

        /**
         * 生成主题 CSS
         */
        generateThemeCSS: function(theme) {
            let css = '';
            
            // 基础样式重置
            css += this.generateBaseStyles();
            
            // 颜色样式
            if (theme.Colors) {
                css += this.generateColorStyles(theme.Colors);
            }
            
            // 字体样式
            if (theme.Typography) {
                css += this.generateTypographyStyles(theme.Typography);
            }
            
            // 布局样式
            if (theme.Layout) {
                css += this.generateLayoutStyles(theme.Layout);
            }
            
            // 动画样式
            css += this.generateAnimationStyles();
            
            // 响应式样式
            css += this.generateResponsiveStyles();
            
            return css;
        },

        /**
         * 生成基础样式
         */
        generateBaseStyles: function() {
            return `
                /* Emby 美化插件 - 基础样式 */
                .emby-beautify-enhanced {
                    transition: all 0.3s ease;
                }
                
                .emby-beautify-card {
                    border-radius: 8px;
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                    transition: transform 0.3s ease, box-shadow 0.3s ease;
                }
                
                .emby-beautify-card:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
                }
                
                .emby-beautify-button {
                    border-radius: 6px;
                    transition: all 0.2s ease;
                }
                
                .emby-beautify-button:hover {
                    transform: translateY(-1px);
                }
            `;
        },

        /**
         * 生成颜色样式
         */
        generateColorStyles: function(colors) {
            let css = '';
            
            if (this.compatibility.supportsCustomProperties) {
                // 使用 CSS 自定义属性
                css += ':root {\n';
                Object.keys(colors).forEach(key => {
                    const value = colors[key];
                    if (value) {
                        css += `  --emby-beautify-${key.toLowerCase()}: ${value};\n`;
                    }
                });
                css += '}\n';
                
                // 应用自定义属性
                css += `
                    .page { background-color: var(--emby-beautify-background, #f5f5f5); }
                    .card { background-color: var(--emby-beautify-surface, #ffffff); }
                    .button-primary { background-color: var(--emby-beautify-primary, #007bff); }
                    .text-primary { color: var(--emby-beautify-text, #333333); }
                `;
            } else {
                // 直接应用颜色值（兼容旧浏览器）
                css += `
                    .page { background-color: ${colors.Background || '#f5f5f5'}; }
                    .card { background-color: ${colors.Surface || '#ffffff'}; }
                    .button-primary { background-color: ${colors.Primary || '#007bff'}; }
                    .text-primary { color: ${colors.Text || '#333333'}; }
                `;
            }
            
            return css;
        },

        /**
         * 生成字体样式
         */
        generateTypographyStyles: function(typography) {
            let css = '';
            
            if (typography.FontFamily) {
                css += `body, .page { font-family: ${typography.FontFamily}; }\n`;
            }
            
            if (typography.FontSize) {
                css += `body { font-size: ${typography.FontSize}; }\n`;
            }
            
            if (typography.LineHeight) {
                css += `body { line-height: ${typography.LineHeight}; }\n`;
            }
            
            return css;
        },

        /**
         * 生成布局样式
         */
        generateLayoutStyles: function(layout) {
            let css = '';
            
            if (layout.MaxWidth) {
                css += `.page-container { max-width: ${layout.MaxWidth}; }\n`;
            }
            
            if (layout.Padding) {
                css += `.page { padding: ${layout.Padding}; }\n`;
            }
            
            if (layout.Margin) {
                css += `.page { margin: ${layout.Margin}; }\n`;
            }
            
            return css;
        },

        /**
         * 生成动画样式
         */
        generateAnimationStyles: function() {
            return `
                @keyframes emby-beautify-fadeIn {
                    from { opacity: 0; transform: translateY(10px); }
                    to { opacity: 1; transform: translateY(0); }
                }
                
                @keyframes emby-beautify-slideIn {
                    from { transform: translateX(-20px); opacity: 0; }
                    to { transform: translateX(0); opacity: 1; }
                }
                
                .emby-beautify-animate-in {
                    animation: emby-beautify-fadeIn 0.3s ease-out;
                }
                
                .emby-beautify-slide-in {
                    animation: emby-beautify-slideIn 0.4s ease-out;
                }
            `;
        },

        /**
         * 生成响应式样式
         */
        generateResponsiveStyles: function() {
            return `
                @media (max-width: 768px) {
                    .emby-beautify-card {
                        margin: 8px;
                        border-radius: 6px;
                    }
                    
                    .emby-beautify-button {
                        padding: 8px 16px;
                        font-size: 14px;
                    }
                }
                
                @media (max-width: 480px) {
                    .emby-beautify-card {
                        margin: 4px;
                        border-radius: 4px;
                    }
                    
                    .page {
                        padding: 12px;
                    }
                }
            `;
        },

        /**
         * 应用自定义属性
         */
        applyCustomProperties: function(theme) {
            const root = document.documentElement;
            
            if (theme.Colors) {
                Object.keys(theme.Colors).forEach(key => {
                    const value = theme.Colors[key];
                    if (value) {
                        root.style.setProperty(`--emby-beautify-${key.toLowerCase()}`, value);
                    }
                });
            }
            
            this.log('自定义属性应用完成');
        },

        /**
         * 注入样式
         */
        injectStyle: function(id, css, options = {}) {
            const styleId = this.config.stylePrefix + id;
            let styleElement = document.getElementById(styleId);
            
            if (!styleElement) {
                styleElement = document.createElement('style');
                styleElement.id = styleId;
                styleElement.type = 'text/css';
                
                // 设置优先级
                if (options.priority === 'high') {
                    styleElement.setAttribute('data-priority', 'high');
                }
                
                const head = document.head || document.getElementsByTagName('head')[0];
                head.appendChild(styleElement);
                
                this.log(`创建样式元素: ${styleId}`);
            }
            
            // 更新样式内容
            if (styleElement.styleSheet) {
                // IE 兼容
                styleElement.styleSheet.cssText = css;
            } else {
                styleElement.textContent = css;
            }
            
            // 记录注入的样式
            this.state.injectedStyles.set(id, {
                element: styleElement,
                css: css,
                timestamp: Date.now(),
                options: options
            });
            
            this.log(`样式注入完成: ${id}`);
        },

        /**
         * 移除样式
         */
        removeStyle: function(id) {
            const styleInfo = this.state.injectedStyles.get(id);
            if (styleInfo && styleInfo.element) {
                styleInfo.element.remove();
                this.state.injectedStyles.delete(id);
                this.log(`样式移除完成: ${id}`);
            }
        },

        /**
         * 更新样式
         */
        updateStyles: function() {
            if (Date.now() - this.state.lastUpdateTime < 1000) {
                // 防止频繁更新
                return Promise.resolve();
            }
            
            this.state.lastUpdateTime = Date.now();
            this.log('开始更新样式...');
            
            return this.fetchCurrentTheme()
                .then(theme => {
                    if (theme) {
                        return this.applyTheme(theme);
                    }
                })
                .then(() => {
                    this.log('样式更新完成');
                    this.dispatchEvent('stylesUpdated');
                })
                .catch(error => {
                    this.error('样式更新失败:', error);
                    this.handleUpdateError(error);
                });
        },

        /**
         * 应用默认样式
         */
        applyDefaultStyles: function() {
            const defaultCSS = this.generateBaseStyles() + this.generateAnimationStyles();
            this.injectStyle('default', defaultCSS, { priority: 'low' });
            this.log('应用默认样式');
        },

        /**
         * 处理更新错误
         */
        handleUpdateError: function(error) {
            this.state.retryCount++;
            
            if (this.state.retryCount < this.config.maxRetries) {
                this.log(`样式更新失败，${this.config.retryDelay}ms 后重试 (${this.state.retryCount}/${this.config.maxRetries})`);
                
                setTimeout(() => {
                    this.updateStyles();
                }, this.config.retryDelay);
            } else {
                this.error('样式更新重试次数已达上限，应用默认样式');
                this.applyDefaultStyles();
                this.state.retryCount = 0;
            }
        },

        /**
         * 开始定期更新
         */
        startPeriodicUpdate: function() {
            if (this.state.updateInterval) {
                clearInterval(this.state.updateInterval);
            }
            
            this.state.updateInterval = setInterval(() => {
                this.updateStyles();
            }, this.config.updateInterval);
            
            this.log('定期更新已启动');
        },

        /**
         * 停止定期更新
         */
        stopPeriodicUpdate: function() {
            if (this.state.updateInterval) {
                clearInterval(this.state.updateInterval);
                this.state.updateInterval = null;
                this.log('定期更新已停止');
            }
        },

        /**
         * 发起网络请求
         */
        makeRequest: function(url, options = {}) {
            const defaultOptions = {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            };
            
            const finalOptions = Object.assign({}, defaultOptions, options);
            
            if (typeof fetch !== 'undefined') {
                return fetch(url, finalOptions);
            } else {
                // 回退到 XMLHttpRequest
                return this.makeXHRRequest(url, finalOptions);
            }
        },

        /**
         * XMLHttpRequest 回退方案
         */
        makeXHRRequest: function(url, options) {
            return new Promise((resolve, reject) => {
                const xhr = new XMLHttpRequest();
                xhr.open(options.method || 'GET', url);
                
                // 设置请求头
                if (options.headers) {
                    Object.keys(options.headers).forEach(key => {
                        xhr.setRequestHeader(key, options.headers[key]);
                    });
                }
                
                xhr.onload = function() {
                    if (xhr.status >= 200 && xhr.status < 300) {
                        resolve({
                            json: () => Promise.resolve(JSON.parse(xhr.responseText)),
                            text: () => Promise.resolve(xhr.responseText)
                        });
                    } else {
                        reject(new Error(`HTTP ${xhr.status}: ${xhr.statusText}`));
                    }
                };
                
                xhr.onerror = function() {
                    reject(new Error('网络请求失败'));
                };
                
                xhr.send(options.body);
            });
        },

        /**
         * 防抖函数
         */
        debounce: function(key, func, delay) {
            if (this.state.debounceTimers) {
                this.state.debounceTimers = {};
            }
            
            if (this.state.debounceTimers[key]) {
                clearTimeout(this.state.debounceTimers[key]);
            }
            
            this.state.debounceTimers[key] = setTimeout(() => {
                func();
                delete this.state.debounceTimers[key];
            }, delay);
        },

        /**
         * 派发自定义事件
         */
        dispatchEvent: function(eventName, detail = {}) {
            const event = new CustomEvent('emby-beautify-' + eventName, {
                detail: detail,
                bubbles: true,
                cancelable: true
            });
            
            document.dispatchEvent(event);
            this.log(`事件派发: ${eventName}`, detail);
        },

        /**
         * 日志记录
         */
        log: function(...args) {
            if (this.config.debugMode) {
                console.log('[EmbyBeautify StyleInjector]', ...args);
            }
        },

        /**
         * 错误记录
         */
        error: function(...args) {
            console.error('[EmbyBeautify StyleInjector]', ...args);
        },

        /**
         * 销毁实例
         */
        destroy: function() {
            this.log('销毁样式注入器...');
            
            // 停止定期更新
            this.stopPeriodicUpdate();
            
            // 移除所有注入的样式
            this.state.injectedStyles.forEach((styleInfo, id) => {
                this.removeStyle(id);
            });
            
            // 断开观察器
            this.state.observers.forEach(observer => {
                observer.disconnect();
            });
            
            // 重置状态
            this.state.isInitialized = false;
            this.state.injectedStyles.clear();
            this.state.observers.clear();
            
            this.log('样式注入器已销毁');
        }
    };

    // 导出到全局命名空间
    window.EmbyBeautifyStyleInjector = StyleInjector;

    // 自动初始化（如果页面已加载）
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            StyleInjector.init().catch(error => {
                console.error('样式注入器自动初始化失败:', error);
            });
        });
    } else {
        // 页面已加载，立即初始化
        setTimeout(() => {
            StyleInjector.init().catch(error => {
                console.error('样式注入器自动初始化失败:', error);
            });
        }, 100);
    }

})(window, document);