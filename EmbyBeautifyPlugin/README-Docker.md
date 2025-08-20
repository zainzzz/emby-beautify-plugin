# Emby Beautify Plugin - Docker 部署指南

本指南介绍如何在 Docker 环境中部署 Emby Beautify Plugin，特别是针对 `linuxserver/emby` 镜像。

## 部署方式

### 方式一：使用 docker-compose（推荐）

1. **准备环境**
   ```bash
   # 克隆或下载插件代码
   git clone <your-repo> EmbyBeautifyPlugin
   cd EmbyBeautifyPlugin
   ```

2. **构建插件**
   ```bash
   # 使用 Docker 构建插件（无需本地安装 .NET SDK）
   docker-compose --profile build run --rm plugin-builder
   ```

3. **启动 Emby 服务**
   ```bash
   # 启动 linuxserver/emby 容器
   docker-compose up -d emby
   ```

4. **访问 Emby**
   - 打开浏览器访问: http://localhost:8096
   - 进入 控制台 -> 插件，查看是否加载了 "Emby Beautify Plugin"

### 方式二：部署到现有容器

如果你已经有运行中的 `linuxserver/emby` 容器：

**Linux/macOS:**
```bash
# 给脚本执行权限
chmod +x scripts/deploy-to-existing-emby.sh

# 部署到默认名称的容器（emby）
./scripts/deploy-to-existing-emby.sh

# 或指定容器名称
./scripts/deploy-to-existing-emby.sh my-emby-container
```

**Windows (PowerShell):**
```powershell
# 直接运行脚本
bash scripts/deploy-to-existing-emby.sh

# 或指定容器名称
bash scripts/deploy-to-existing-emby.sh my-emby-container
```

### 方式三：手动部署

1. **构建插件**
   ```bash
   # 如果有 .NET SDK
   dotnet build -c Release
   
   # 或使用 Docker
   docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:6.0 \
     sh -c "dotnet restore && dotnet build -c Release"
   ```

2. **找到 Emby 配置目录**
   ```bash
   # 查看容器的卷挂载
   docker inspect your-emby-container | grep -A 5 "Mounts"
   ```

3. **复制插件文件**
   ```bash
   # 假设配置目录挂载在 ./emby-config
   mkdir -p ./emby-config/plugins/EmbyBeautifyPlugin
   cp ./bin/Release/EmbyBeautifyPlugin.dll ./emby-config/plugins/EmbyBeautifyPlugin/
   cp ./plugin.xml ./emby-config/plugins/EmbyBeautifyPlugin/
   ```

4. **重启容器**
   ```bash
   docker restart your-emby-container
   ```

## 配置说明

### docker-compose.yml 配置

```yaml
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    container_name: emby-server
    environment:
      - PUID=1000          # 用户ID
      - PGID=1000          # 组ID
      - TZ=Asia/Shanghai   # 时区
    volumes:
      - ./emby-config:/config                                    # 配置目录
      - ./media:/data/movies                                     # 电影目录
      - ./bin/Release:/config/plugins/EmbyBeautifyPlugin:ro      # 插件目录（只读）
      - ./plugin.xml:/config/plugins/EmbyBeautifyPlugin/plugin.xml:ro
    ports:
      - "8096:8096"        # HTTP端口
      - "8920:8920"        # HTTPS端口
```

### 重要说明

1. **权限设置**: 确保插件文件的所有者与容器内的用户一致（通常是 PUID/PGID）
2. **文件路径**: 插件必须放在 `/config/plugins/EmbyBeautifyPlugin/` 目录下
3. **重启需求**: 安装或更新插件后需要重启 Emby 容器

## 故障排除

### 插件未显示

1. **检查文件权限**
   ```bash
   ls -la ./emby-config/plugins/EmbyBeautifyPlugin/
   ```

2. **检查容器日志**
   ```bash
   docker logs emby-server
   ```

3. **验证插件文件**
   ```bash
   # 检查 DLL 文件是否存在且不为空
   ls -la ./bin/Release/EmbyBeautifyPlugin.dll
   
   # 检查 plugin.xml 格式
   cat plugin.xml
   ```

### 权限问题

```bash
# 设置正确的文件所有者（替换为你的 PUID/PGID）
sudo chown -R 1000:1000 ./emby-config/plugins/EmbyBeautifyPlugin/
chmod -R 755 ./emby-config/plugins/EmbyBeautifyPlugin/
```

### 插件加载失败

1. 检查 Emby 版本兼容性
2. 查看 Emby 日志中的错误信息
3. 确认 .NET 版本兼容性

## 开发模式

开发时可以使用卷挂载实现热重载：

```yaml
volumes:
  # 开发模式：直接挂载构建输出目录
  - ./bin/Release:/config/plugins/EmbyBeautifyPlugin:ro
```

每次修改代码后：
```bash
# 重新构建
docker-compose --profile build run --rm plugin-builder

# 重启 Emby
docker-compose restart emby
```

## 生产部署建议

1. 使用固定版本的镜像标签而不是 `latest`
2. 定期备份配置目录
3. 监控容器日志
4. 设置适当的资源限制

```yaml
services:
  emby:
    image: lscr.io/linuxserver/emby:4.7.14  # 使用固定版本
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'
```