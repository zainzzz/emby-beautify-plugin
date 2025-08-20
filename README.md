# EmbyBeautifyPlugin

一个功能强大的Emby主题美化插件，提供丰富的界面定制选项和现代化的用户体验。

## 功能特性

### 🎨 主题定制
- **多主题支持**: 内置多种精美主题，支持深色/浅色模式
- **颜色定制**: 完全自定义主色调、背景色、文字色等
- **布局优化**: 响应式设计，适配各种屏幕尺寸
- **字体美化**: 支持自定义字体系列和大小

### ✨ 动画效果
- **流畅过渡**: 页面切换和元素变化的平滑动画
- **交互反馈**: 按钮点击、悬停等交互的视觉反馈
- **性能优化**: GPU加速的CSS动画，确保流畅体验

### 📱 响应式设计
- **移动端优化**: 完美适配手机和平板设备
- **断点管理**: 智能响应不同屏幕尺寸
- **触摸友好**: 优化的触摸交互体验

### ⚡ 性能优化
- **样式缓存**: 智能缓存机制，减少重复计算
- **懒加载**: 按需加载样式资源
- **内存管理**: 优化的内存使用策略

## 安装说明

### 方法一：手动安装
1. 下载最新版本的插件包 `EmbyBeautifyPlugin-v1.0.0.zip`
2. 解压到Emby服务器的插件目录
3. 重启Emby服务器
4. 在管理面板中启用插件

### 方法二：通过Emby插件目录
1. 打开Emby管理面板
2. 进入插件 > 目录
3. 搜索"EmbyBeautifyPlugin"
4. 点击安装并重启服务器

## 使用指南

### 基础配置
1. 安装插件后，进入 设置 > 插件 > EmbyBeautifyPlugin
2. 选择您喜欢的主题
3. 调整颜色和布局设置
4. 保存配置并刷新页面

### 高级定制
- **主题编辑器**: 使用内置编辑器创建自定义主题
- **CSS注入**: 添加自定义CSS代码
- **动画控制**: 调整动画速度和效果
- **响应式设置**: 配置不同设备的显示效果

## 兼容性

- **Emby版本**: 4.7.0.0 及以上
- **浏览器支持**: Chrome 80+, Firefox 75+, Safari 13+, Edge 80+
- **操作系统**: Windows, Linux, macOS, Docker

## 开发说明

### 技术栈
- **.NET Standard 2.0**: 核心框架
- **C#**: 主要开发语言
- **HTML/CSS/JavaScript**: 前端界面
- **Emby Plugin API**: 插件接口

### 项目结构
```
EmbyBeautifyPlugin/
├── Controllers/          # API控制器
├── Services/            # 核心服务
├── Models/              # 数据模型
├── Views/               # 前端页面
├── Abstracts/           # 抽象基类
├── Interfaces/          # 接口定义
└── Extensions/          # 扩展方法
```

### 构建说明
```bash
# 克隆项目
git clone https://github.com/zainzzz/emby-beautify-plugin.git

# 进入项目目录
cd emby-beautify-plugin

# 构建项目
dotnet build -c Release

# 运行测试
dotnet test

# 打包插件
.\scripts\build-package.ps1
```

## 贡献指南

我们欢迎社区贡献！请遵循以下步骤：

1. Fork 本项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 问题反馈

如果您遇到问题或有建议，请：

1. 查看 [故障排除指南](docs/TROUBLESHOOTING.md)
2. 搜索现有的 [Issues](https://github.com/zainzzz/emby-beautify-plugin/issues)
3. 创建新的 Issue 并提供详细信息

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 更新日志

### v1.0.0 (2024-01-20)
- 🎉 首次发布
- ✨ 完整的主题定制功能
- 🚀 性能优化和缓存机制
- 📱 响应式设计支持
- 🎨 多种内置主题
- 🔧 完善的配置界面

## 致谢

感谢所有为本项目做出贡献的开发者和用户！

---

**EmbyBeautifyPlugin** - 让您的Emby更加美观和易用！