using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmbyBeautifyPlugin.Services
{
    /// <summary>
    /// CSS构建器，用于构建结构化的CSS内容
    /// </summary>
    public class CssBuilder
    {
        private readonly StringBuilder _css;
        private readonly Stack<string> _ruleStack;
        private int _indentLevel;
        private bool _minify;

        public CssBuilder(bool minify = false)
        {
            _css = new StringBuilder();
            _ruleStack = new Stack<string>();
            _indentLevel = 0;
            _minify = minify;
        }

        /// <summary>
        /// 添加注释
        /// </summary>
        /// <param name="comment">注释内容</param>
        public CssBuilder AddComment(string comment)
        {
            if (!_minify && !string.IsNullOrEmpty(comment))
            {
                AppendIndented($"/* {comment} */");
                AppendNewLine();
            }
            return this;
        }

        /// <summary>
        /// 开始CSS规则
        /// </summary>
        /// <param name="selector">选择器</param>
        public CssBuilder StartRule(string selector)
        {
            if (string.IsNullOrEmpty(selector))
                throw new ArgumentException("选择器不能为空", nameof(selector));

            _ruleStack.Push(selector);
            AppendIndented($"{selector} {{");
            AppendNewLine();
            _indentLevel++;
            return this;
        }

        /// <summary>
        /// 结束当前CSS规则
        /// </summary>
        public CssBuilder EndRule()
        {
            if (_ruleStack.Count == 0)
                throw new InvalidOperationException("没有活动的CSS规则可以结束");

            _indentLevel--;
            _ruleStack.Pop();
            AppendIndented("}");
            AppendNewLine();
            return this;
        }

        /// <summary>
        /// 添加CSS属性
        /// </summary>
        /// <param name="property">属性名</param>
        /// <param name="value">属性值</param>
        public CssBuilder AddProperty(string property, string value)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentException("属性名不能为空", nameof(property));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("属性值不能为空", nameof(value));

            var separator = _minify ? ":" : ": ";
            var terminator = _minify ? ";" : ";";
            AppendIndented($"{property}{separator}{value}{terminator}");
            AppendNewLine();
            return this;
        }

        /// <summary>
        /// 开始媒体查询
        /// </summary>
        /// <param name="mediaQuery">媒体查询条件</param>
        public CssBuilder StartMediaQuery(string mediaQuery)
        {
            if (string.IsNullOrEmpty(mediaQuery))
                throw new ArgumentException("媒体查询不能为空", nameof(mediaQuery));

            var query = mediaQuery.StartsWith("@media") ? mediaQuery : $"@media {mediaQuery}";
            _ruleStack.Push(query);
            AppendIndented($"{query} {{");
            AppendNewLine();
            _indentLevel++;
            return this;
        }

        /// <summary>
        /// 结束媒体查询
        /// </summary>
        public CssBuilder EndMediaQuery()
        {
            if (_ruleStack.Count == 0 || !_ruleStack.Peek().StartsWith("@media"))
                throw new InvalidOperationException("没有活动的媒体查询可以结束");

            return EndRule();
        }

        /// <summary>
        /// 添加关键帧动画
        /// </summary>
        /// <param name="name">动画名称</param>
        /// <param name="keyframes">关键帧定义</param>
        public CssBuilder AddKeyframes(string name, Dictionary<string, Dictionary<string, string>> keyframes)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("动画名称不能为空", nameof(name));
            if (keyframes == null || !keyframes.Any())
                throw new ArgumentException("关键帧不能为空", nameof(keyframes));

            AppendIndented($"@keyframes {name} {{");
            AppendNewLine();
            _indentLevel++;

            foreach (var keyframe in keyframes)
            {
                AppendIndented($"{keyframe.Key} {{");
                AppendNewLine();
                _indentLevel++;

                foreach (var property in keyframe.Value)
                {
                    AddProperty(property.Key, property.Value);
                }

                _indentLevel--;
                AppendIndented("}");
                AppendNewLine();
            }

            _indentLevel--;
            AppendIndented("}");
            AppendNewLine();
            return this;
        }

        /// <summary>
        /// 添加原始CSS内容
        /// </summary>
        /// <param name="css">CSS内容</param>
        public CssBuilder AddRawCss(string css)
        {
            if (!string.IsNullOrEmpty(css))
            {
                _css.Append(css);
                if (!css.EndsWith("\n") && !_minify)
                {
                    AppendNewLine();
                }
            }
            return this;
        }

        /// <summary>
        /// 添加空行
        /// </summary>
        public CssBuilder AddNewLine()
        {
            if (!_minify)
            {
                _css.AppendLine();
            }
            return this;
        }

        /// <summary>
        /// 添加多个空行
        /// </summary>
        /// <param name="count">空行数量</param>
        public CssBuilder AddNewLines(int count)
        {
            if (!_minify)
            {
                for (int i = 0; i < count; i++)
                {
                    _css.AppendLine();
                }
            }
            return this;
        }

        /// <summary>
        /// 构建最终的CSS字符串
        /// </summary>
        /// <returns>CSS字符串</returns>
        public string Build()
        {
            if (_ruleStack.Count > 0)
            {
                throw new InvalidOperationException($"存在未关闭的CSS规则: {string.Join(", ", _ruleStack)}");
            }

            return _css.ToString();
        }

        /// <summary>
        /// 清空构建器内容
        /// </summary>
        public CssBuilder Clear()
        {
            _css.Clear();
            _ruleStack.Clear();
            _indentLevel = 0;
            return this;
        }

        /// <summary>
        /// 获取当前CSS内容长度
        /// </summary>
        public int Length => _css.Length;

        /// <summary>
        /// 检查是否有活动的规则
        /// </summary>
        public bool HasActiveRules => _ruleStack.Count > 0;

        /// <summary>
        /// 获取当前缩进级别
        /// </summary>
        public int IndentLevel => _indentLevel;

        /// <summary>
        /// 添加带缩进的内容
        /// </summary>
        /// <param name="content">内容</param>
        private void AppendIndented(string content)
        {
            if (!_minify && _indentLevel > 0)
            {
                _css.Append(new string(' ', _indentLevel * 2));
            }
            _css.Append(content);
        }

        /// <summary>
        /// 添加换行符
        /// </summary>
        private void AppendNewLine()
        {
            if (!_minify)
            {
                _css.AppendLine();
            }
        }

        /// <summary>
        /// 设置压缩模式
        /// </summary>
        /// <param name="minify">是否压缩</param>
        public CssBuilder SetMinify(bool minify)
        {
            _minify = minify;
            return this;
        }

        /// <summary>
        /// 添加CSS变量定义
        /// </summary>
        /// <param name="name">变量名（不包含--前缀）</param>
        /// <param name="value">变量值</param>
        public CssBuilder AddVariable(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("变量名不能为空", nameof(name));

            var variableName = name.StartsWith("--") ? name : $"--{name}";
            return AddProperty(variableName, value);
        }

        /// <summary>
        /// 添加CSS变量引用
        /// </summary>
        /// <param name="name">变量名（不包含--前缀）</param>
        /// <param name="fallback">回退值</param>
        /// <returns>CSS变量引用字符串</returns>
        public static string VarReference(string name, string fallback = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("变量名不能为空", nameof(name));

            var variableName = name.StartsWith("--") ? name : $"--{name}";
            return string.IsNullOrEmpty(fallback) 
                ? $"var({variableName})" 
                : $"var({variableName}, {fallback})";
        }

        /// <summary>
        /// 添加渐变背景
        /// </summary>
        /// <param name="type">渐变类型（linear-gradient, radial-gradient等）</param>
        /// <param name="direction">渐变方向或位置</param>
        /// <param name="colorStops">颜色停止点</param>
        public CssBuilder AddGradient(string property, string type, string direction, params string[] colorStops)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentException("属性名不能为空", nameof(property));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("渐变类型不能为空", nameof(type));
            if (colorStops == null || colorStops.Length == 0)
                throw new ArgumentException("颜色停止点不能为空", nameof(colorStops));

            var gradient = string.IsNullOrEmpty(direction)
                ? $"{type}({string.Join(", ", colorStops)})"
                : $"{type}({direction}, {string.Join(", ", colorStops)})";

            return AddProperty(property, gradient);
        }

        /// <summary>
        /// 添加过渡效果
        /// </summary>
        /// <param name="properties">要过渡的属性</param>
        /// <param name="duration">持续时间</param>
        /// <param name="timingFunction">时间函数</param>
        /// <param name="delay">延迟时间</param>
        public CssBuilder AddTransition(string properties, string duration = "0.3s", 
            string timingFunction = "ease", string delay = "0s")
        {
            if (string.IsNullOrEmpty(properties))
                throw new ArgumentException("过渡属性不能为空", nameof(properties));

            var transition = $"{properties} {duration} {timingFunction} {delay}";
            return AddProperty("transition", transition);
        }

        /// <summary>
        /// 添加阴影效果
        /// </summary>
        /// <param name="type">阴影类型（box-shadow或text-shadow）</param>
        /// <param name="shadows">阴影定义</param>
        public CssBuilder AddShadow(string type, params string[] shadows)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("阴影类型不能为空", nameof(type));
            if (shadows == null || shadows.Length == 0)
                throw new ArgumentException("阴影定义不能为空", nameof(shadows));

            return AddProperty(type, string.Join(", ", shadows));
        }
    }
}