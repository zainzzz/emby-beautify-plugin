# Emby Beautify Plugin 安装指南

本指南将帮助您在 Emby Server 上安装和配置 Emby Beautify Plugin。

## 系统要求

### Emby Server 要求
- **Emby Server 版本**: 4.7.0 - 4.8.0
- **.NET Runtime**: .NET 6.0 或更高版本
- **操作系统**: Windows, Linux, macOS, Docker

### 硬件要求
- **内存**: 至少 512MB 可用内存
- **存储**: 至少 50MB 可用磁盘空间
- **CPU**: 支持 .NET 6.0 的处理器

## 安装方法

### 方法一：自动安装（推荐）

#### Windows 用户
1. 下载最新版本的插件包 `EmbyBeautifyPlugin-v1.0.0.0.zip`
2. 解压到临时目录
3. 以管理员身份运行 PowerShell
4. 执行安装脚本：
   ```powershell
   .\install-plugin.ps1 -EmbyConfigDir "C:\ProgramData\Emby-Server"
   ```

#### Linux/macOS 用户
1. 下载最新版本的插件包 `EmbyBeautifyPlugin-v1.0.0.0.tar.gz`
2. 解压到临时目录：
   ```bash
   tar -xzf EmbyBeautifyPlugin-v1.0.0.0.tar.gz
   cd package
   ```
3. 运行安装脚本：
   ```bash
   chmod +x install-plugin.sh
   sudo ./install-plugin.sh
   ```

#### Docker 用户
1. 下载插件包并解压
2. 运行安装脚本：
   ```bash
   ./install-plugin.sh
   ```
   或者使用部署脚本：
   ```bash
   ./deploy-to-existing-emby.sh your-emby-container-name
   ```

### 方法二：手动安装

#### 1. 确定插件目录位置

**Windows:**
- 默认位置: `%ProgramData%\Emby-Server\plugins\EmbyBeautifyPlugin\`
- 用户安装: `%APPDATA%\Emby-Server\plugins\EmbyBeautifyPlugin\`

**Linux:**
- 系统安装: `/var/lib/emby/plugins/EmbyBeautifyPlugin/`
- 用户安装: `~/.config/emby-server/plugins/EmbyBeautifyPlugin/`

**macOS:**
- 用户安装: `~/Library/Application Support/Emby-Server/plugins/EmbyBeautifyPlugin/`

**Docker:**
- 容器内路径: `/config/plugins/EmbyBeautifyPlugin/`
- 主机挂载路径: `<your-config-path>/plugins/EmbyBeautifyPlugin/`

#### 2. 创建插件目录
```bash
# Linux/macOS
mkdir -p /var/lib/emby/plugins/EmbyBeautifyPlugin

# Windows (PowerShell)
New-Item -ItemType Directory -Path "$env:ProgramData\Emby-Server\plugins\EmbyBeautifyPlugin" -Force
```

#### 3. 复制插件文件
将以下文件复制到插件目录：
- `EmbyBeautifyPlugin.dll` - 主插件文件
- `plugin.xml` - 插件清单文件

#### 4. 设置文件权限（Linux/macOS）
```bash
# 设置正确的所有者和权限
sudo chown -R emby:emby /var/lib/emby/plugins/EmbyBeautifyPlugin
sudo chmod -R 755 /var/lib/emby/plugins/EmbyBeautifyPlugin
```

#### 5. 重启 Emby Server
```bash
# Linux (systemd)
sudo systemctl restart emby-server

# Docker
docker restart your-emby-container

# Windows
# 通过服务管理器重启 Emby Server 服务
```

## 验证安装

### 1. 检查插件状态
1. 打开 Emby Web 界面
2. 导航到 **控制台** → **插件**
3. 在已安装插件列表中查找 "Emby Beautify Plugin"
4. 确认状态显示为 "活动"

### 2. 检查插件版本
- 插件版本应显示为 `1.0.0.0`
- 状态应为 "已启用"

### 3. 访问插件设置
1. 在插件列表中点击 "Emby Beautify Plugin"
2. 点击 "设置" 按钮
3. 应该能看到插件配置界面

## 故障排除

### 常见问题

#### 1. 插件未出现在插件列表中

**可能原因:**
- 文件复制不完整
- 权限设置错误
- Emby Server 版本不兼容

**解决方案:**
```bash
# 检查文件是否存在
ls -la /var/lib/emby/plugins/EmbyBeautifyPlugin/

# 检查文件权限
ls -la /var/lib/emby/plugins/EmbyBeautifyPlugin/

# 检查 Emby 日志
tail -f /var/lib/emby/logs/embyserver.txt
```

#### 2. 插件显示为"不兼容"

**可能原因:**
- Emby Server 版本过低或过高
- .NET Runtime 版本不匹配

**解决方案:**
- 升级 Emby Server 到 4.7.0 或更高版本
- 确保安装了 .NET 6.0 Runtime

#### 3. 插件加载失败

**可能原因:**
- 依赖项缺失
- 文件损坏

**解决方案:**
```bash
# 重新下载并安装插件
rm -rf /var/lib/emby/plugins/EmbyBeautifyPlugin/
# 重新执行安装步骤
```

#### 4. Docker 环境中的权限问题

**解决方案:**
```bash
# 检查容器的用户ID
docker exec your-emby-container id

# 设置正确的权限
docker exec your-emby-container chown -R abc:abc /config/plugins/EmbyBeautifyPlugin
docker exec your-emby-container chmod -R 755 /config/plugins/EmbyBeautifyPlugin
```

### 日志分析

#### 查看 Emby 日志
```bash
# Linux
tail -f /var/lib/emby/logs/embyserver.txt

# Docker
docker logs -f your-emby-container

# Windows
# 查看 %ProgramData%\Emby-Server\logs\ 目录中的日志文件
```

#### 插件相关日志关键词
- `EmbyBeautifyPlugin`
- `Plugin loaded`
- `Plugin initialization`
- `BeautifyException`

## 升级插件

### 自动升级
1. 下载新版本的插件包
2. 运行安装脚本（会自动覆盖旧版本）
3. 重启 Emby Server

### 手动升级
1. 停止 Emby Server
2. 备份当前插件目录（可选）
3. 删除旧的插件文件
4. 复制新的插件文件
5. 重启 Emby Server

## 卸载插件

### 自动卸载
```bash
# Linux/macOS
./uninstall-plugin.sh

# Windows
.\uninstall-plugin.ps1
```

### 手动卸载
1. 停止 Emby Server
2. 删除插件目录：
   ```bash
   # Linux
   rm -rf /var/lib/emby/plugins/EmbyBeautifyPlugin/
   
   # Windows
   Remove-Item -Path "$env:ProgramData\Emby-Server\plugins\EmbyBeautifyPlugin" -Recurse -Force
   ```
3. 删除插件配置（可选）：
   ```bash
   # Linux
   rm -f /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml
   ```
4. 重启 Emby Server

## 技术支持

### 获取帮助
- **GitHub Issues**: [项目问题跟踪](https://github.com/emby-beautify/plugin/issues)
- **文档**: [完整文档](https://github.com/emby-beautify/plugin/wiki)
- **社区论坛**: [Emby 社区](https://emby.media/community/)

### 报告问题
提交问题时请包含以下信息：
1. Emby Server 版本
2. 操作系统和版本
3. 插件版本
4. 错误日志
5. 重现步骤

### 调试信息收集
```bash
# 收集系统信息
echo "Emby Server Version: $(emby-server --version)"
echo "OS: $(uname -a)"
echo "Plugin Version: $(cat /var/lib/emby/plugins/EmbyBeautifyPlugin/VERSION.json | grep version)"

# 收集日志
tail -n 100 /var/lib/emby/logs/embyserver.txt > emby-debug.log
```