#!/bin/bash

# Emby Beautify Plugin 构建和部署脚本
# 用于在 linuxserver/emby 容器中部署插件

set -e

PLUGIN_NAME="EmbyBeautifyPlugin"
CONTAINER_NAME="emby-server"

echo "开始构建和部署 $PLUGIN_NAME 到 linuxserver/emby..."

# 构建插件
echo "1. 构建插件..."
docker-compose --profile build run --rm plugin-builder

if [ $? -ne 0 ]; then
    echo "错误: 插件构建失败"
    exit 1
fi

echo "插件构建成功！"

# 检查构建文件是否存在
if [ ! -f "./bin/Release/EmbyBeautifyPlugin.dll" ]; then
    echo "错误: 找不到构建好的插件文件"
    exit 1
fi

# 检查容器状态
if docker ps -q -f name="$CONTAINER_NAME" | grep -q .; then
    echo "2. 检测到容器 $CONTAINER_NAME 正在运行"
    echo "3. 重启容器以加载新插件..."
    docker-compose restart emby
    
    echo "等待容器重启完成..."
    sleep 10
    
    echo "部署完成！插件已通过卷挂载自动加载"
    
elif docker ps -a -q -f name="$CONTAINER_NAME" | grep -q .; then
    echo "2. 容器 $CONTAINER_NAME 存在但未运行，启动容器..."
    docker-compose up -d emby
    
    echo "等待容器启动完成..."
    sleep 10
    
    echo "部署完成！"
    
else
    echo "2. 容器 $CONTAINER_NAME 不存在，创建并启动..."
    docker-compose up -d emby
    
    echo "等待容器启动完成..."
    sleep 15
    
    echo "部署完成！"
fi

echo ""
echo "插件部署成功！"
echo "访问 http://localhost:8096 查看Emby Server"
echo "在 控制台 -> 插件 页面应该能看到 '$PLUGIN_NAME'"
echo ""
echo "插件文件位置:"
echo "  - 主机: ./bin/Release/EmbyBeautifyPlugin.dll"
echo "  - 容器: /config/plugins/$PLUGIN_NAME/EmbyBeautifyPlugin.dll"