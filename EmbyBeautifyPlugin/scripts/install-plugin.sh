#!/bin/bash

# Emby Beautify Plugin 安装脚本
# 用于在Docker容器中安装插件

set -e

PLUGIN_NAME="EmbyBeautifyPlugin"
EMBY_CONFIG_DIR="${EMBY_CONFIG_DIR:-/config}"
PLUGIN_DIR="$EMBY_CONFIG_DIR/plugins/$PLUGIN_NAME"

echo "开始安装 $PLUGIN_NAME..."

# 创建插件目录
echo "创建插件目录: $PLUGIN_DIR"
mkdir -p "$PLUGIN_DIR"

# 检查是否有构建好的文件
if [ ! -f "./bin/Release/EmbyBeautifyPlugin.dll" ]; then
    echo "错误: 找不到构建好的插件文件"
    echo "请先运行: dotnet build -c Release"
    exit 1
fi

# 复制插件文件
echo "复制插件文件..."
cp "./bin/Release/EmbyBeautifyPlugin.dll" "$PLUGIN_DIR/"
cp "./plugin.xml" "$PLUGIN_DIR/"

# 如果存在PDB文件，也复制过去（用于调试）
if [ -f "./bin/Release/EmbyBeautifyPlugin.pdb" ]; then
    cp "./bin/Release/EmbyBeautifyPlugin.pdb" "$PLUGIN_DIR/"
fi

# 设置权限
echo "设置文件权限..."
chown -R emby:emby "$PLUGIN_DIR" 2>/dev/null || true
chmod -R 755 "$PLUGIN_DIR"

echo "插件安装完成！"
echo "插件位置: $PLUGIN_DIR"
echo "请重启Emby Server以加载插件"

# 检查Emby是否在运行
if pgrep -f "emby-server" > /dev/null; then
    echo ""
    echo "检测到Emby Server正在运行"
    echo "建议重启容器以加载新插件:"
    echo "  docker-compose restart emby-with-beautify"
fi