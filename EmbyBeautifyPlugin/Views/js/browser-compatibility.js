/**
 * Emby 美化插件 - 浏览器兼容性处理模块
 * 处理不同浏览器的兼容性问题和功能回退
 */

(function(window, document) {
    'use strict';

    // 命名空间
    window.EmbyBeautifyCompatibility = window.EmbyBeautifyCompatibility || {};

    const BrowserCompatibility = {
        // 浏览器特性检测结果
        features: {
            customProperties: false,
            flexbox: false,
            grid: false,
            transforms: false,
            transitions: false,
            animations: false,
            mutationObserver: false,
            promises: false,
            fetch: false,
            classList: false,
            querySelector: false,
            addEventListener: false
        },

        // 浏览器信息
        browser: {
            name: '',
            version: 0,
            isModern: false,
            isMobile: false,
            isTouch: false,
            engine: ''
        },

        // 兼容性修复方法
        polyfills: {},

        /**
         * 初始化兼容性检测
         */
        init: function() {
            this.detectBrowser();
            this.detectFeatures();
            this.applyPolyfills();
            this.setupCompatibilityCSS();
            
            return {
                features: this.features,
                browser: this.browser,
                isSupported: this.isSupported()
            };
        },

        /**
         * 检测浏览器信息
         */
        detectBrowser: function() {
            const ua = navigator.userAgent;
            const browser = this.browser;

            // Chrome
            if (/Chrome/.test(ua) && /Google Inc/.test(navigator.vendor)) {
                browser.name = 'Chrome';
                const match = ua.match(/Chrome\/(\d+)/);
                browser.version = match ? parseInt(match[1]) : 0;
                browser.engine = 'Blink';
            }
            // Firefox
            else if (/Firefox/.test(ua)) {
                browser.name = 'Firefox';
                const match = ua.match(/Firefox\/(\d+)/);
                browser.version = match ? parseInt(match[1]) : 0;
                browser.engine = 'Gecko';
            }
            // Safari
            else if (/Safari/.test(ua) && /Apple Computer/.test(navigator.vendor)) {
                browser.name = 'Safari';
                const match = ua.match(/Version\/(\d+)/);
                browser.version = match ? parseInt(match[1]) : 0;
                browser.engine = 'WebKit';
            }
            // Edge
            else if (/Edge/.test(ua)) {
                browser.name = 'Edge';
                const match = ua.match(/Edge\/(\d+)/);
                browser.version = match ? parseInt(match[1]) : 0;
                browser.engine = 'EdgeHTML';
            }
            // IE
            else if (/Trident/.test(ua) || /MSIE/.test(ua)) {
                browser.name = 'IE';
                const match = ua.match(/(?:MSIE |rv:)(\d+)/);
                browser.version = match ? parseInt(match[1]) : 0;
                browser.engine = 'Trident';
            }

            // 移动设备检测
            browser.isMobile = /Mobile|Android|iPhone|iPad|iPod|BlackBerry|Windows Phone/.test(ua);
            
            // 触摸设备检测
            browser.isTouch = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

            // 现代浏览器判断
            browser.isModern = this.isModernBrowser();
        },

        /**
         * 判断是否为现代浏览器
         */
        isModernBrowser: function() {
            const browser = this.browser;
            
            switch (browser.name) {
                case 'Chrome':
                    return browser.version >= 60;
                case 'Firefox':
                    return browser.version >= 55;
                case 'Safari':
                    return browser.version >= 11;
                case 'Edge':
                    return browser.version >= 16;
                case 'IE':
                    return false; // IE 不被认为是现代浏览器
                default:
                    return false;
            }
        },

        /**
         * 检测浏览器特性支持
         */
        detectFeatures: function() {
            const features = this.features;

            // CSS 自定义属性
            features.customProperties = this.testCSSSupport('color', 'var(--test)');

            // Flexbox
            features.flexbox = this.testCSSSupport('display', 'flex');

            // CSS Grid
            features.grid = this.testCSSSupport('display', 'grid');

            // CSS Transforms
            features.transforms = this.testCSSSupport('transform', 'translateX(1px)');

            // CSS Transitions
            features.transitions = this.testCSSSupport('transition', 'all 1s');

            // CSS Animations
            features.animations = this.testCSSSupport('animation', 'test 1s');

            // MutationObserver
            features.mutationObserver = typeof MutationObserver !== 'undefined';

            // Promises
            features.promises = typeof Promise !== 'undefined';

            // Fetch API
            features.fetch = typeof fetch !== 'undefined';

            // classList
            features.classList = 'classList' in document.createElement('div');

            // querySelector
            features.querySelector = typeof document.querySelector !== 'undefined';

            // addEventListener
            features.addEventListener = typeof document.addEventListener !== 'undefined';
        },

        /**
         * 测试 CSS 特性支持
         */
        testCSSSupport: function(property, value) {
            if (window.CSS && window.CSS.supports) {
                return window.CSS.supports(property, value);
            }

            // 回退检测方法
            const element = document.createElement('div');
            element.style[property] = value;
            return element.style[property] === value;
        },

        /**
         * 应用 Polyfills
         */
        applyPolyfills: function() {
            // classList polyfill
            if (!this.features.classList) {
                this.polyfills.classList = this.createClassListPolyfill();
            }

            // addEventListener polyfill
            if (!this.features.addEventListener) {
                this.polyfills.addEventListener = this.createEventListenerPolyfill();
            }

            // Promise polyfill
            if (!this.features.promises) {
                this.polyfills.promise = this.createPromisePolyfill();
            }

            // querySelector polyfill
            if (!this.features.querySelector) {
                this.polyfills.querySelector = this.createQuerySelectorPolyfill();
            }

            // CSS 自定义属性 polyfill
            if (!this.features.customProperties) {
                this.polyfills.customProperties = this.createCustomPropertiesPolyfill();
            }
        },

        /**
         * classList polyfill
         */
        createClassListPolyfill: function() {
            if (!('classList' in document.createElement('_'))) {
                (function(view) {
                    if (!('Element' in view)) return;

                    var classListProp = 'classList',
                        protoProp = 'prototype',
                        elemCtrProto = view.Element[protoProp],
                        objCtr = Object,
                        strTrim = String[protoProp].trim || function() {
                            return this.replace(/^\s+|\s+$/g, '');
                        },
                        arrIndexOf = Array[protoProp].indexOf || function(item) {
                            var i = 0, len = this.length;
                            for (; i < len; i++) {
                                if (i in this && this[i] === item) {
                                    return i;
                                }
                            }
                            return -1;
                        },
                        DOMTokenList = function(el) {
                            this.el = el;
                            var classes = el.className.replace(/^\s+|\s+$/g, '').split(/\s+/);
                            for (var i = 0; i < classes.length; i++) {
                                this.push(classes[i]);
                            }
                            this._updateClassName = function() {
                                el.className = this.toString();
                            };
                        },
                        tokenListProto = DOMTokenList[protoProp] = [],
                        tokenListGetter = function() {
                            return new DOMTokenList(this);
                        };

                    tokenListProto.item = function(index) {
                        return this[index] || null;
                    };

                    tokenListProto.contains = function(token) {
                        token += '';
                        return arrIndexOf.call(this, token) !== -1;
                    };

                    tokenListProto.add = function() {
                        var tokens = arguments,
                            i = 0,
                            l = tokens.length,
                            token,
                            updated = false;
                        do {
                            token = tokens[i] + '';
                            if (arrIndexOf.call(this, token) === -1) {
                                this.push(token);
                                updated = true;
                            }
                        } while (++i < l);

                        if (updated) {
                            this._updateClassName();
                        }
                    };

                    tokenListProto.remove = function() {
                        var tokens = arguments,
                            i = 0,
                            l = tokens.length,
                            token,
                            updated = false,
                            index;
                        do {
                            token = tokens[i] + '';
                            index = arrIndexOf.call(this, token);
                            while (index !== -1) {
                                this.splice(index, 1);
                                updated = true;
                                index = arrIndexOf.call(this, token);
                            }
                        } while (++i < l);

                        if (updated) {
                            this._updateClassName();
                        }
                    };

                    tokenListProto.toggle = function(token, force) {
                        token += '';

                        var result = this.contains(token),
                            method = result ?
                                force !== true && 'remove' :
                                force !== false && 'add';

                        if (method) {
                            this[method](token);
                        }

                        if (force === true || force === false) {
                            return force;
                        } else {
                            return !result;
                        }
                    };

                    tokenListProto.toString = function() {
                        return this.join(' ');
                    };

                    if (objCtr.defineProperty) {
                        var defineProperty = {
                            get: tokenListGetter,
                            enumerable: true,
                            configurable: true
                        };
                        try {
                            objCtr.defineProperty(elemCtrProto, classListProp, defineProperty);
                        } catch (ex) {
                            if (ex.number === -0x7FF5EC54) {
                                defineProperty.enumerable = false;
                                objCtr.defineProperty(elemCtrProto, classListProp, defineProperty);
                            }
                        }
                    } else if (objCtr[protoProp].__defineGetter__) {
                        elemCtrProto.__defineGetter__(classListProp, tokenListGetter);
                    }
                }(window));
            }
        },

        /**
         * addEventListener polyfill
         */
        createEventListenerPolyfill: function() {
            if (!window.addEventListener) {
                window.addEventListener = function(type, listener, useCapture) {
                    this.attachEvent('on' + type, function(e) {
                        e.target = e.srcElement;
                        e.preventDefault = function() {
                            e.returnValue = false;
                        };
                        e.stopPropagation = function() {
                            e.cancelBubble = true;
                        };
                        listener.call(this, e);
                    });
                };

                window.removeEventListener = function(type, listener, useCapture) {
                    this.detachEvent('on' + type, listener);
                };
            }
        },

        /**
         * 简单的 Promise polyfill
         */
        createPromisePolyfill: function() {
            if (typeof Promise === 'undefined') {
                window.Promise = function(executor) {
                    var self = this;
                    self.state = 'pending';
                    self.value = undefined;
                    self.handlers = [];

                    function resolve(result) {
                        if (self.state === 'pending') {
                            self.state = 'fulfilled';
                            self.value = result;
                            self.handlers.forEach(handle);
                            self.handlers = null;
                        }
                    }

                    function reject(error) {
                        if (self.state === 'pending') {
                            self.state = 'rejected';
                            self.value = error;
                            self.handlers.forEach(handle);
                            self.handlers = null;
                        }
                    }

                    function handle(handler) {
                        if (self.state === 'pending') {
                            self.handlers.push(handler);
                        } else {
                            if (self.state === 'fulfilled' && typeof handler.onFulfilled === 'function') {
                                handler.onFulfilled(self.value);
                            }
                            if (self.state === 'rejected' && typeof handler.onRejected === 'function') {
                                handler.onRejected(self.value);
                            }
                        }
                    }

                    this.then = function(onFulfilled, onRejected) {
                        return new Promise(function(resolve, reject) {
                            handle({
                                onFulfilled: function(result) {
                                    try {
                                        resolve(onFulfilled ? onFulfilled(result) : result);
                                    } catch (ex) {
                                        reject(ex);
                                    }
                                },
                                onRejected: function(error) {
                                    try {
                                        resolve(onRejected ? onRejected(error) : error);
                                    } catch (ex) {
                                        reject(ex);
                                    }
                                }
                            });
                        });
                    };

                    this.catch = function(onRejected) {
                        return this.then(null, onRejected);
                    };

                    executor(resolve, reject);
                };

                Promise.resolve = function(value) {
                    return new Promise(function(resolve) {
                        resolve(value);
                    });
                };

                Promise.reject = function(reason) {
                    return new Promise(function(resolve, reject) {
                        reject(reason);
                    });
                };
            }
        },

        /**
         * querySelector polyfill
         */
        createQuerySelectorPolyfill: function() {
            if (!document.querySelector) {
                document.querySelector = function(selector) {
                    var elements = document.querySelectorAll(selector);
                    return elements.length ? elements[0] : null;
                };
            }

            if (!document.querySelectorAll) {
                document.querySelectorAll = function(selector) {
                    var style = document.createElement('style'),
                        elements = [],
                        element;
                    
                    document.documentElement.firstChild.appendChild(style);
                    document._qsa = [];

                    style.styleSheet.cssText = selector + '{x-qsa:expression(document._qsa && document._qsa.push(this))}';
                    window.scrollBy(0, 0);
                    style.parentNode.removeChild(style);

                    while (document._qsa.length) {
                        element = document._qsa.shift();
                        element.style.removeAttribute('x-qsa');
                        elements.push(element);
                    }
                    document._qsa = null;
                    return elements;
                };
            }
        },

        /**
         * CSS 自定义属性 polyfill
         */
        createCustomPropertiesPolyfill: function() {
            // 简单的 CSS 自定义属性支持
            return {
                setProperty: function(element, property, value) {
                    if (element && element.style) {
                        // 将自定义属性转换为数据属性
                        element.setAttribute('data-css-' + property.replace(/^--/, ''), value);
                    }
                },
                
                getProperty: function(element, property) {
                    if (element) {
                        return element.getAttribute('data-css-' + property.replace(/^--/, ''));
                    }
                    return null;
                },
                
                applyVariables: function(css) {
                    // 简单的变量替换
                    return css.replace(/var\(--([^)]+)\)/g, function(match, varName) {
                        var value = document.documentElement.getAttribute('data-css-' + varName);
                        return value || match;
                    });
                }
            };
        },

        /**
         * 设置兼容性 CSS
         */
        setupCompatibilityCSS: function() {
            var css = '';
            var browser = this.browser;

            // 为不同浏览器添加特定的 CSS 类
            document.documentElement.className += ' browser-' + browser.name.toLowerCase();
            document.documentElement.className += ' browser-version-' + browser.version;
            
            if (browser.isMobile) {
                document.documentElement.className += ' is-mobile';
            }
            
            if (browser.isTouch) {
                document.documentElement.className += ' is-touch';
            }

            // IE 特殊处理
            if (browser.name === 'IE') {
                css += `
                    .emby-beautify-card {
                        filter: progid:DXImageTransform.Microsoft.Shadow(color='#cccccc', Direction=135, Strength=3);
                    }
                    
                    .emby-beautify-button {
                        filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#4facfe', endColorstr='#00f2fe');
                    }
                `;
            }

            // 不支持 Flexbox 的回退
            if (!this.features.flexbox) {
                css += `
                    .emby-beautify-flex {
                        display: table;
                        width: 100%;
                    }
                    
                    .emby-beautify-flex-item {
                        display: table-cell;
                        vertical-align: middle;
                    }
                `;
            }

            // 不支持 CSS Grid 的回退
            if (!this.features.grid) {
                css += `
                    .emby-beautify-grid {
                        display: block;
                    }
                    
                    .emby-beautify-grid-item {
                        float: left;
                        width: 33.333%;
                        box-sizing: border-box;
                    }
                `;
            }

            // 不支持 Transforms 的回退
            if (!this.features.transforms) {
                css += `
                    .emby-beautify-card:hover {
                        margin-top: -2px;
                        margin-bottom: 2px;
                    }
                `;
            }

            // 应用兼容性 CSS
            if (css) {
                this.injectCSS('compatibility', css);
            }
        },

        /**
         * 注入 CSS
         */
        injectCSS: function(id, css) {
            var styleId = 'emby-beautify-compatibility-' + id;
            var styleElement = document.getElementById(styleId);
            
            if (!styleElement) {
                styleElement = document.createElement('style');
                styleElement.id = styleId;
                styleElement.type = 'text/css';
                
                var head = document.head || document.getElementsByTagName('head')[0];
                head.appendChild(styleElement);
            }
            
            if (styleElement.styleSheet) {
                // IE
                styleElement.styleSheet.cssText = css;
            } else {
                styleElement.textContent = css;
            }
        },

        /**
         * 检查是否支持插件功能
         */
        isSupported: function() {
            // 最低要求：支持基本的 DOM 操作和事件
            return this.features.querySelector && 
                   (this.features.addEventListener || window.attachEvent) &&
                   this.browser.name !== 'IE' || this.browser.version >= 9;
        },

        /**
         * 获取兼容性报告
         */
        getCompatibilityReport: function() {
            return {
                browser: this.browser,
                features: this.features,
                isSupported: this.isSupported(),
                recommendations: this.getRecommendations()
            };
        },

        /**
         * 获取兼容性建议
         */
        getRecommendations: function() {
            var recommendations = [];
            var browser = this.browser;

            if (browser.name === 'IE' && browser.version < 11) {
                recommendations.push('建议升级到 Internet Explorer 11 或更新的浏览器');
            }

            if (!this.features.customProperties) {
                recommendations.push('您的浏览器不支持 CSS 自定义属性，某些主题功能可能受限');
            }

            if (!this.features.flexbox) {
                recommendations.push('您的浏览器不支持 Flexbox，布局可能不够灵活');
            }

            if (!this.features.transitions) {
                recommendations.push('您的浏览器不支持 CSS 过渡效果，动画体验可能受影响');
            }

            if (browser.isMobile && !browser.isTouch) {
                recommendations.push('检测到移动设备但不支持触摸，某些交互功能可能不可用');
            }

            return recommendations;
        }
    };

    // 导出到全局命名空间
    window.EmbyBeautifyCompatibility = BrowserCompatibility;

    // 自动初始化
    document.addEventListener('DOMContentLoaded', function() {
        BrowserCompatibility.init();
    });

})(window, document);