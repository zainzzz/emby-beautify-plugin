# Emby Beautify Plugin v1.0.0 - Initial Release

🎉 **首次发布！** 欢迎使用 Emby Beautify Plugin，一个强大的 Emby Server 界面美化插件。

## ✨ 主要特性

### 🎨 主题系统
- **多种内置主题**: 浅色、深色和现代化主题
- **完全自定义**: 颜色、字体、布局和动画全面可定制
- **实时预览**: 修改时即时查看效果
- **导入导出**: 与社区分享主题配置

### 📱 响应式设计
- **移动端优化**: 完美适配手机和平板设备
- **自适应布局**: 根据屏幕尺寸自动调整
- **触摸友好**: 大按钮和直观手势操作

### ⚡ 性能优化
- **智能缓存**: 快速加载的样式缓存机制
- **懒加载**: 按需加载样式资源
- **硬件加速**: GPU 加速的流畅动画
- **内存优化**: 高效的资源管理

### 🔧 高级功能
- **CSS 注入**: 支持高级用户自定义样式
- **动画控制**: 可配置的过渡效果和动画
- **调试工具**: 内置调试和性能监控
- **错误恢复**: 优雅的降级和回退机制

## 📋 系统要求

- **Emby Server**: 4.7.0 - 4.8.0
- **.NET Runtime**: 6.0 或更高版本
- **支持的浏览器**: Chrome, Firefox, Safari, Edge

## 📦 下载文件

### 主要下载
- **EmbyBeautifyPlugin-v1.0.0.0.zip** (138 KB) - 主要插件包
  - SHA256: `53d5349b6c9b1ceb0e48b40dfbb269fe971e450d89bcea1e2d4777bb5ddab34f`

### 包含文件
- `EmbyBeautifyPlugin.dll` - 主插件程序集 (457 KB)
- `plugin.xml` - 插件清单文件
- `INSTALL.txt` - 安装说明
- `UNINSTALL.txt` - 卸载说明
- `VERSION.json` - 版本信息和校验和

## 🚀 快速安装

### Windows 用户
```powershell
# 下载并解压插件包
Expand-Archive -Path "EmbyBeautifyPlugin-v1.0.0.0.zip" -DestinationPath "EmbyBeautifyPlugin"

# 复制到 Emby 插件目录
Copy-Item "EmbyBeautifyPlugin\*" -Destination "$env:ProgramData\Emby-Server\plugins\EmbyBeautifyPlugin\" -Recurse

# 重启 Emby Server
```

### Linux 用户
```bash
# 解压插件包
unzip EmbyBeautifyPlugin-v1.0.0.0.zip

# 复制到 Emby 插件目录
sudo cp -r package/* /var/lib/emby/plugins/EmbyBeautifyPlugin/

# 设置权限
sudo chown -R emby:emby /var/lib/emby/plugins/EmbyBeautifyPlugin/
sudo chmod -R 755 /var/lib/emby/plugins/EmbyBeautifyPlugin/

# 重启 Emby Server
sudo systemctl restart emby-server
```

### Docker 用户
```bash
# 解压到挂载的配置目录
unzip EmbyBeautifyPlugin-v1.0.0.0.zip -d /path/to/emby/config/plugins/EmbyBeautifyPlugin/

# 重启容器
docker restart emby-container
```

## 📖 文档

- **[安装指南](https://github.com/zainzzz/emby-beautify-plugin/blob/main/EmbyBeautifyPlugin/docs/INSTALLATION.md)** - 详细安装步骤
- **[用户手册](https://github.com/zainzzz/emby-beautify-plugin/blob/main/EmbyBeautifyPlugin/docs/USER_GUIDE.md)** - 完整功能指南
- **[主题自定义](https://github.com/zainzzz/emby-beautify-plugin/blob/main/EmbyBeautifyPlugin/docs/THEME_CUSTOMIZATION.md)** - 创建自定义主题
- **[故障排除](https://github.com/zainzzz/emby-beautify-plugin/blob/main/EmbyBeautifyPlugin/docs/TROUBLESHOOTING.md)** - 常见问题解决

## 🐛 已知问题

- 某些旧版本浏览器可能不支持所有动画效果
- 在极低配置设备上可能需要禁用动画以获得最佳性能

## 🔄 升级说明

这是首次发布，无需升级操作。

## 🤝 贡献

欢迎贡献代码、报告问题或分享主题！

- **报告问题**: [GitHub Issues](https://github.com/zainzzz/emby-beautify-plugin/issues)
- **功能建议**: [GitHub Discussions](https://github.com/zainzzz/emby-beautify-plugin/discussions)
- **贡献代码**: [Pull Requests](https://github.com/zainzzz/emby-beautify-plugin/pulls)

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](https://github.com/zainzzz/emby-beautify-plugin/blob/main/LICENSE) 文件了解详情。

## 🙏 致谢

感谢 Emby 团队创造了优秀的媒体服务器，以及所有测试和反馈的用户！

---

**享受您美化后的 Emby 界面！** 🎨✨