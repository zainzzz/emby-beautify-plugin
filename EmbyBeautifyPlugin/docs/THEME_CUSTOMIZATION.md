# 主题自定义教程

本教程将详细介绍如何使用 Emby Beautify Plugin 创建和自定义您的专属主题。

## 目录
- [主题基础概念](#主题基础概念)
- [创建自定义主题](#创建自定义主题)
- [颜色系统](#颜色系统)
- [字体和排版](#字体和排版)
- [布局和间距](#布局和间距)
- [组件样式](#组件样式)
- [响应式设计](#响应式设计)
- [高级技巧](#高级技巧)
- [主题分享](#主题分享)

## 主题基础概念

### 什么是主题
主题是一套完整的视觉设计方案，包括：
- **颜色方案**: 主色调、辅助色、背景色等
- **字体设置**: 字体族、大小、粗细、行高
- **布局参数**: 间距、圆角、阴影等
- **组件样式**: 按钮、卡片、导航栏等的外观

### 主题结构
```json
{
  "name": "我的自定义主题",
  "version": "1.0.0",
  "colors": {
    "primary": "#667eea",
    "secondary": "#764ba2",
    "background": "#ffffff",
    "surface": "#f8f9fa",
    "text": "#333333"
  },
  "typography": {
    "fontFamily": "Inter, sans-serif",
    "fontSize": "14px",
    "fontWeight": "400",
    "lineHeight": "1.5"
  },
  "layout": {
    "borderRadius": "8px",
    "spacing": "16px",
    "shadow": "0 2px 8px rgba(0,0,0,0.1)"
  }
}
```

## 创建自定义主题

### 方法一：基于预设主题修改

#### 1. 选择基础主题
1. 进入插件设置页面
2. 选择一个接近您需求的预设主题
3. 点击"复制为自定义主题"按钮

#### 2. 修改主题属性
1. 在"自定义主题"区域进行修改
2. 实时预览更改效果
3. 满意后保存主题

### 方法二：从零开始创建

#### 1. 创建新主题
1. 点击"创建新主题"按钮
2. 输入主题名称和描述
3. 选择基础模板（浅色/深色）

#### 2. 配置主题属性
按照以下步骤逐步配置主题的各个方面。

## 颜色系统

### 主要颜色

#### 主色调 (Primary Color)
- **用途**: 按钮、链接、重要元素
- **选择建议**: 选择品牌色或您喜欢的主色
- **示例**: `#667eea` (蓝紫色)

```css
/* 主色调的应用 */
.button-primary {
    background-color: var(--primary-color);
}

.link {
    color: var(--primary-color);
}
```

#### 辅助色 (Secondary Color)
- **用途**: 辅助按钮、标签、装饰元素
- **选择建议**: 与主色调形成良好对比
- **示例**: `#764ba2` (紫色)

#### 背景色系统
```json
{
  "background": {
    "primary": "#ffffff",    // 主背景色
    "secondary": "#f8f9fa",  // 次要背景色
    "tertiary": "#e9ecef"    // 第三级背景色
  }
}
```

#### 文字色系统
```json
{
  "text": {
    "primary": "#212529",    // 主要文字
    "secondary": "#6c757d",  // 次要文字
    "disabled": "#adb5bd",   // 禁用文字
    "inverse": "#ffffff"     // 反色文字
  }
}
```

### 颜色选择技巧

#### 1. 使用颜色理论
- **单色方案**: 使用同一色相的不同明度和饱和度
- **互补色方案**: 使用色轮上相对的颜色
- **三角色方案**: 使用色轮上等距的三种颜色

#### 2. 确保可访问性
- **对比度**: 文字与背景的对比度至少为 4.5:1
- **色盲友好**: 避免仅依赖颜色传达信息
- **测试工具**: 使用在线对比度检查工具

#### 3. 颜色生成工具
推荐使用以下工具生成配色方案：
- Adobe Color
- Coolors.co
- Material Design Color Tool

### 深色主题特殊考虑

#### 背景色选择
```json
{
  "dark": {
    "background": {
      "primary": "#121212",    // 主背景 - 纯黑会太刺眼
      "secondary": "#1e1e1e",  // 卡片背景
      "tertiary": "#2d2d2d"    // 悬停背景
    }
  }
}
```

#### 文字色调整
```json
{
  "dark": {
    "text": {
      "primary": "#ffffff",    // 主要文字
      "secondary": "#b3b3b3",  // 次要文字 - 降低不透明度
      "disabled": "#666666"    // 禁用文字
    }
  }
}
```

## 字体和排版

### 字体选择

#### 系统字体栈
```css
/* 推荐的字体栈 */
font-family: 
  -apple-system,           /* macOS */
  BlinkMacSystemFont,      /* macOS */
  "Segoe UI",              /* Windows */
  Roboto,                  /* Android */
  "Helvetica Neue",        /* macOS */
  Arial,                   /* 通用 */
  sans-serif;              /* 后备 */
```

#### Web 字体
```css
/* Google Fonts 示例 */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');

font-family: 'Inter', sans-serif;
```

### 字体大小系统

#### 基础字体大小
```json
{
  "typography": {
    "fontSize": {
      "xs": "12px",      // 小号文字
      "sm": "14px",      // 默认文字
      "base": "16px",    // 基础文字
      "lg": "18px",      // 大号文字
      "xl": "20px",      // 标题文字
      "2xl": "24px",     // 大标题
      "3xl": "30px",     // 主标题
      "4xl": "36px"      // 超大标题
    }
  }
}
```

#### 字体粗细
```json
{
  "fontWeight": {
    "light": "300",      // 细体
    "normal": "400",     // 正常
    "medium": "500",     // 中等
    "semibold": "600",   // 半粗
    "bold": "700"        // 粗体
  }
}
```

### 行高和间距

#### 行高设置
```css
/* 不同内容的行高建议 */
.text-body {
    line-height: 1.6;    /* 正文 - 提高可读性 */
}

.text-heading {
    line-height: 1.2;    /* 标题 - 更紧凑 */
}

.text-caption {
    line-height: 1.4;    /* 说明文字 */
}
```

#### 字符间距
```css
.text-heading {
    letter-spacing: -0.025em;  /* 标题稍微紧凑 */
}

.text-button {
    letter-spacing: 0.025em;   /* 按钮文字稍微宽松 */
}
```

## 布局和间距

### 间距系统

#### 基础间距单位
```json
{
  "spacing": {
    "xs": "4px",       // 0.25rem
    "sm": "8px",       // 0.5rem
    "base": "16px",    // 1rem
    "lg": "24px",      // 1.5rem
    "xl": "32px",      // 2rem
    "2xl": "48px",     // 3rem
    "3xl": "64px"      // 4rem
  }
}
```

#### 应用示例
```css
/* 卡片内边距 */
.card {
    padding: var(--spacing-lg);
}

/* 元素间距 */
.element + .element {
    margin-top: var(--spacing-base);
}

/* 容器边距 */
.container {
    margin: 0 var(--spacing-xl);
}
```

### 圆角系统

#### 圆角大小
```json
{
  "borderRadius": {
    "none": "0",
    "sm": "4px",
    "base": "8px",
    "lg": "12px",
    "xl": "16px",
    "full": "9999px"    // 完全圆形
  }
}
```

#### 应用场景
```css
.card {
    border-radius: var(--border-radius-lg);
}

.button {
    border-radius: var(--border-radius-base);
}

.avatar {
    border-radius: var(--border-radius-full);
}
```

### 阴影系统

#### 阴影层级
```json
{
  "shadow": {
    "sm": "0 1px 2px rgba(0,0,0,0.05)",
    "base": "0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06)",
    "md": "0 4px 6px rgba(0,0,0,0.07), 0 2px 4px rgba(0,0,0,0.06)",
    "lg": "0 10px 15px rgba(0,0,0,0.1), 0 4px 6px rgba(0,0,0,0.05)",
    "xl": "0 20px 25px rgba(0,0,0,0.1), 0 10px 10px rgba(0,0,0,0.04)"
  }
}
```

## 组件样式

### 按钮样式

#### 基础按钮
```css
.button {
    /* 基础样式 */
    padding: var(--spacing-sm) var(--spacing-base);
    border-radius: var(--border-radius-base);
    font-weight: var(--font-weight-medium);
    transition: all 0.2s ease;
    
    /* 主要按钮 */
    &.button-primary {
        background-color: var(--primary-color);
        color: white;
        
        &:hover {
            background-color: var(--primary-color-dark);
            transform: translateY(-1px);
            box-shadow: var(--shadow-md);
        }
    }
    
    /* 次要按钮 */
    &.button-secondary {
        background-color: transparent;
        color: var(--primary-color);
        border: 1px solid var(--primary-color);
        
        &:hover {
            background-color: var(--primary-color);
            color: white;
        }
    }
}
```

#### 按钮大小变体
```css
.button {
    &.button-sm {
        padding: var(--spacing-xs) var(--spacing-sm);
        font-size: var(--font-size-sm);
    }
    
    &.button-lg {
        padding: var(--spacing-base) var(--spacing-lg);
        font-size: var(--font-size-lg);
    }
}
```

### 卡片样式

#### 基础卡片
```css
.card {
    background-color: var(--background-secondary);
    border-radius: var(--border-radius-lg);
    box-shadow: var(--shadow-base);
    padding: var(--spacing-lg);
    transition: all 0.3s ease;
    
    &:hover {
        transform: translateY(-2px);
        box-shadow: var(--shadow-lg);
    }
}
```

#### 媒体卡片
```css
.media-card {
    position: relative;
    overflow: hidden;
    border-radius: var(--border-radius-lg);
    
    .media-image {
        width: 100%;
        height: auto;
        transition: transform 0.3s ease;
    }
    
    .media-overlay {
        position: absolute;
        bottom: 0;
        left: 0;
        right: 0;
        background: linear-gradient(transparent, rgba(0,0,0,0.7));
        color: white;
        padding: var(--spacing-base);
    }
    
    &:hover .media-image {
        transform: scale(1.05);
    }
}
```

### 导航栏样式

#### 基础导航栏
```css
.navbar {
    background-color: var(--background-primary);
    backdrop-filter: blur(10px);
    border-bottom: 1px solid var(--border-color);
    padding: var(--spacing-base) var(--spacing-xl);
    
    .navbar-brand {
        font-size: var(--font-size-xl);
        font-weight: var(--font-weight-bold);
        color: var(--primary-color);
    }
    
    .navbar-nav {
        display: flex;
        gap: var(--spacing-lg);
        
        .nav-link {
            color: var(--text-secondary);
            text-decoration: none;
            padding: var(--spacing-sm) var(--spacing-base);
            border-radius: var(--border-radius-base);
            transition: all 0.2s ease;
            
            &:hover,
            &.active {
                color: var(--primary-color);
                background-color: var(--primary-color-light);
            }
        }
    }
}
```

## 响应式设计

### 断点系统

#### 标准断点
```css
/* 移动端优先的断点 */
:root {
    --breakpoint-sm: 576px;    /* 小屏幕 */
    --breakpoint-md: 768px;    /* 平板 */
    --breakpoint-lg: 992px;    /* 桌面 */
    --breakpoint-xl: 1200px;   /* 大屏幕 */
}
```

#### 媒体查询
```css
/* 移动端样式 */
.container {
    padding: var(--spacing-base);
}

/* 平板端样式 */
@media (min-width: 768px) {
    .container {
        padding: var(--spacing-lg);
    }
}

/* 桌面端样式 */
@media (min-width: 992px) {
    .container {
        padding: var(--spacing-xl);
        max-width: 1200px;
        margin: 0 auto;
    }
}
```

### 响应式组件

#### 响应式网格
```css
.grid {
    display: grid;
    gap: var(--spacing-base);
    
    /* 移动端：单列 */
    grid-template-columns: 1fr;
    
    /* 平板端：两列 */
    @media (min-width: 768px) {
        grid-template-columns: repeat(2, 1fr);
    }
    
    /* 桌面端：多列 */
    @media (min-width: 992px) {
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    }
}
```

#### 响应式字体
```css
.heading {
    /* 移动端 */
    font-size: var(--font-size-xl);
    
    /* 平板端 */
    @media (min-width: 768px) {
        font-size: var(--font-size-2xl);
    }
    
    /* 桌面端 */
    @media (min-width: 992px) {
        font-size: var(--font-size-3xl);
    }
}
```

## 高级技巧

### CSS 变量的使用

#### 动态主题切换
```css
:root {
    --primary-color: #667eea;
    --background-color: #ffffff;
    --text-color: #333333;
}

[data-theme="dark"] {
    --primary-color: #8b9cf7;
    --background-color: #121212;
    --text-color: #ffffff;
}
```

#### 计算属性
```css
:root {
    --base-size: 16px;
    --scale-ratio: 1.25;
    
    --font-size-sm: calc(var(--base-size) / var(--scale-ratio));
    --font-size-lg: calc(var(--base-size) * var(--scale-ratio));
    --font-size-xl: calc(var(--font-size-lg) * var(--scale-ratio));
}
```

### 动画和过渡

#### 缓动函数
```css
:root {
    --ease-out-cubic: cubic-bezier(0.33, 1, 0.68, 1);
    --ease-in-out-cubic: cubic-bezier(0.65, 0, 0.35, 1);
}

.element {
    transition: all 0.3s var(--ease-out-cubic);
}
```

#### 复杂动画
```css
@keyframes slideInUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.card {
    animation: slideInUp 0.6s var(--ease-out-cubic);
}
```

### 性能优化

#### 硬件加速
```css
.animated-element {
    will-change: transform;
    transform: translateZ(0); /* 触发硬件加速 */
}
```

#### 减少重绘
```css
/* 使用 transform 而不是改变 top/left */
.element {
    transform: translateX(100px);
    /* 而不是 left: 100px; */
}
```

## 主题分享

### 导出主题

#### 1. 完成主题设计
确保您的主题在各种场景下都表现良好。

#### 2. 导出主题文件
1. 进入插件设置页面
2. 点击"导出主题"按钮
3. 保存 `.json` 文件

#### 3. 添加主题信息
```json
{
  "name": "我的精美主题",
  "description": "一个现代化的深色主题，适合夜间使用",
  "author": "您的名字",
  "version": "1.0.0",
  "tags": ["dark", "modern", "minimal"],
  "preview": "theme-preview.png",
  "colors": {
    // 颜色配置
  },
  "typography": {
    // 字体配置
  },
  "layout": {
    // 布局配置
  }
}
```

### 分享主题

#### 1. 创建预览图
- 截取主题应用后的界面截图
- 尺寸建议：1200x800px
- 展示主要界面元素

#### 2. 编写说明文档
```markdown
# 主题名称

## 描述
简要描述主题的特点和适用场景。

## 特性
- 特性1
- 特性2
- 特性3

## 安装方法
1. 下载主题文件
2. 在插件设置中导入
3. 应用主题

## 预览
![主题预览](preview.png)
```

#### 3. 发布渠道
- GitHub 仓库
- 社区论坛
- 主题分享网站

### 主题质量检查清单

#### 设计质量
- [ ] 颜色搭配和谐
- [ ] 对比度符合可访问性标准
- [ ] 字体清晰易读
- [ ] 布局合理美观

#### 功能完整性
- [ ] 所有界面元素都有样式
- [ ] 响应式设计正常工作
- [ ] 动画效果流畅
- [ ] 深色/浅色模式支持

#### 兼容性
- [ ] 主流浏览器兼容
- [ ] 移动端显示正常
- [ ] 不同屏幕尺寸适配
- [ ] 性能表现良好

#### 文档完整性
- [ ] 主题说明清晰
- [ ] 安装步骤详细
- [ ] 预览图片清晰
- [ ] 版本信息完整

---

通过本教程，您应该能够创建出专业、美观且实用的自定义主题。记住，好的主题不仅要美观，还要注重用户体验和可访问性。