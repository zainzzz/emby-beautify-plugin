#!/bin/bash

# 部署插件到现有的 linuxserver/emby 容器
# 适用于已经运行的 Emby 容器

set -e

PLUGIN_NAME="EmbyBeautifyPlugin"
EMBY_CONTAINER_NAME="${1:-emby}"  # 默认容器名为 emby，可通过参数指定

echo "部署 $PLUGIN_NAME 到现有的 linuxserver/emby 容器: $EMBY_CONTAINER_NAME"

# 检查容器是否存在
if ! docker ps -a --format "table {{.Names}}" | grep -q "^$EMBY_CONTAINER_NAME$"; then
    echo "错误: 找不到名为 '$EMBY_CONTAINER_NAME' 的容器"
    echo "请检查容器名称或先启动 Emby 容器"
    echo ""
    echo "使用方法: $0 [容器名称]"
    echo "示例: $0 my-emby-container"
    exit 1
fi

# 构建插件
echo "1. 构建插件..."
if command -v dotnet &> /dev/null; then
    dotnet build -c Release
else
    echo "本地没有 .NET SDK，使用 Docker 构建..."
    docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:6.0 \
        sh -c "dotnet restore && dotnet build -c Release"
fi

if [ ! -f "./bin/Release/EmbyBeautifyPlugin.dll" ]; then
    echo "错误: 插件构建失败"
    exit 1
fi

echo "插件构建成功！"

# 获取容器的配置目录挂载点
echo "2. 检查容器配置..."
CONFIG_MOUNT=$(docker inspect "$EMBY_CONTAINER_NAME" | grep -A 1 '"Destination": "/config"' | grep '"Source"' | cut -d'"' -f4)

if [ -z "$CONFIG_MOUNT" ]; then
    echo "警告: 无法找到 /config 目录的挂载点"
    echo "将直接复制文件到容器内部（重启后会丢失）"
    
    # 直接复制到容器内部
    echo "3. 创建插件目录..."
    docker exec "$EMBY_CONTAINER_NAME" mkdir -p "/config/plugins/$PLUGIN_NAME"
    
    echo "4. 复制插件文件到容器..."
    docker cp "./bin/Release/EmbyBeautifyPlugin.dll" "$EMBY_CONTAINER_NAME:/config/plugins/$PLUGIN_NAME/"
    docker cp "./plugin.xml" "$EMBY_CONTAINER_NAME:/config/plugins/$PLUGIN_NAME/"
    
    if [ -f "./bin/Release/EmbyBeautifyPlugin.pdb" ]; then
        docker cp "./bin/Release/EmbyBeautifyPlugin.pdb" "$EMBY_CONTAINER_NAME:/config/plugins/$PLUGIN_NAME/"
    fi
    
    echo "5. 设置文件权限..."
    docker exec "$EMBY_CONTAINER_NAME" chown -R abc:abc "/config/plugins/$PLUGIN_NAME" 2>/dev/null || true
    docker exec "$EMBY_CONTAINER_NAME" chmod -R 755 "/config/plugins/$PLUGIN_NAME"
    
else
    echo "找到配置目录挂载点: $CONFIG_MOUNT"
    
    # 复制到挂载的主机目录
    PLUGIN_HOST_DIR="$CONFIG_MOUNT/plugins/$PLUGIN_NAME"
    echo "3. 创建插件目录: $PLUGIN_HOST_DIR"
    mkdir -p "$PLUGIN_HOST_DIR"
    
    echo "4. 复制插件文件..."
    cp "./bin/Release/EmbyBeautifyPlugin.dll" "$PLUGIN_HOST_DIR/"
    cp "./plugin.xml" "$PLUGIN_HOST_DIR/"
    
    if [ -f "./bin/Release/EmbyBeautifyPlugin.pdb" ]; then
        cp "./bin/Release/EmbyBeautifyPlugin.pdb" "$PLUGIN_HOST_DIR/"
    fi
    
    echo "5. 设置文件权限..."
    # 获取容器的 PUID 和 PGID
    PUID=$(docker exec "$EMBY_CONTAINER_NAME" id -u abc 2>/dev/null || echo "1000")
    PGID=$(docker exec "$EMBY_CONTAINER_NAME" id -g abc 2>/dev/null || echo "1000")
    
    sudo chown -R "$PUID:$PGID" "$PLUGIN_HOST_DIR" 2>/dev/null || \
    chown -R "$PUID:$PGID" "$PLUGIN_HOST_DIR" 2>/dev/null || \
    echo "警告: 无法设置文件所有者，请手动设置权限"
    
    chmod -R 755 "$PLUGIN_HOST_DIR"
fi

# 重启容器
echo "6. 重启 Emby 容器以加载插件..."
docker restart "$EMBY_CONTAINER_NAME"

echo "等待容器重启完成..."
sleep 10

echo ""
echo "✅ 插件部署成功！"
echo ""
echo "插件信息:"
echo "  - 名称: $PLUGIN_NAME"
echo "  - 容器: $EMBY_CONTAINER_NAME"
if [ -n "$CONFIG_MOUNT" ]; then
    echo "  - 主机路径: $CONFIG_MOUNT/plugins/$PLUGIN_NAME"
fi
echo "  - 容器路径: /config/plugins/$PLUGIN_NAME"
echo ""
echo "请访问 Emby 管理界面检查插件是否加载成功:"
echo "  http://localhost:8096 -> 控制台 -> 插件"