# Emby美化插件部署指南

## 部署步骤

### 1. 构建插件
```bash
cd EmbyBeautifyPlugin
dotnet build --configuration Release
```

### 2. 复制插件文件
将以下文件复制到Emby服务器的插件目录：
- `bin/Release/net6.0/EmbyBeautifyPlugin.dll`
- `bin/Release/net6.0/EmbyBeautifyPlugin.dll.config`

**Emby插件目录位置：**
- Windows: `%ProgramData%\Emby-Server\plugins`
- Linux: `/var/lib/emby/plugins`
- Docker: `/config/plugins`

### 3. 重启Emby服务器
重启Emby服务器以加载新插件。

### 4. 验证插件安装
1. 登录Emby管理界面
2. 导航到 **设置** > **插件**
3. 确认"Emby美化插件"出现在已安装插件列表中
4. 检查插件状态为"已启用"

### 5. 配置插件
1. 在插件列表中点击"Emby美化插件"
2. 配置主题设置、样式缓存等选项
3. 保存配置

## 故障排除

### 插件未显示
- 检查文件权限
- 确认.NET 6.0运行时已安装
- 查看Emby日志文件中的错误信息

### 功能异常
- 检查浏览器控制台是否有JavaScript错误
- 验证CSS样式是否正确加载
- 查看插件日志输出

## 卸载插件
1. 在Emby管理界面中禁用插件
2. 删除插件文件
3. 重启Emby服务器

## 技术支持
如遇到问题，请检查：
1. Emby服务器日志
2. 插件配置文件
3. 浏览器开发者工具