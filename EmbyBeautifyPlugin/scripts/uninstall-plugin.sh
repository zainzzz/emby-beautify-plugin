#!/bin/bash

# Emby Beautify Plugin 卸载脚本
# 用于从 Emby Server 中完全移除插件

set -e

PLUGIN_NAME="EmbyBeautifyPlugin"
EMBY_CONFIG_DIR="${EMBY_CONFIG_DIR:-/config}"
PLUGIN_DIR="$EMBY_CONFIG_DIR/plugins/$PLUGIN_NAME"

echo "开始卸载 $PLUGIN_NAME..."

# 检查插件是否已安装
if [ ! -d "$PLUGIN_DIR" ]; then
    echo "插件未安装或已被移除"
    echo "插件目录不存在: $PLUGIN_DIR"
    exit 0
fi

echo "找到插件安装目录: $PLUGIN_DIR"

# 显示将要删除的文件
echo ""
echo "将要删除的文件和目录:"
ls -la "$PLUGIN_DIR"

# 确认删除
if [ "${FORCE_UNINSTALL:-}" != "true" ]; then
    echo ""
    read -p "确定要卸载插件吗？这将删除所有插件文件和配置。(y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "取消卸载"
        exit 0
    fi
fi

# 备份配置（如果存在）
CONFIG_BACKUP_DIR="/tmp/${PLUGIN_NAME}_backup_$(date +%Y%m%d_%H%M%S)"
if [ -f "$PLUGIN_DIR/config.json" ] || [ -f "$EMBY_CONFIG_DIR/config/plugins/$PLUGIN_NAME.xml" ]; then
    echo ""
    echo "备份配置文件到: $CONFIG_BACKUP_DIR"
    mkdir -p "$CONFIG_BACKUP_DIR"
    
    # 备份插件目录中的配置
    if [ -f "$PLUGIN_DIR/config.json" ]; then
        cp "$PLUGIN_DIR/config.json" "$CONFIG_BACKUP_DIR/"
    fi
    
    # 备份 Emby 配置目录中的插件配置
    if [ -f "$EMBY_CONFIG_DIR/config/plugins/$PLUGIN_NAME.xml" ]; then
        cp "$EMBY_CONFIG_DIR/config/plugins/$PLUGIN_NAME.xml" "$CONFIG_BACKUP_DIR/"
    fi
    
    echo "配置文件已备份，如需恢复可从备份目录复制"
fi

# 删除插件目录
echo ""
echo "删除插件目录..."
rm -rf "$PLUGIN_DIR"

# 删除 Emby 配置中的插件设置
EMBY_PLUGIN_CONFIG="$EMBY_CONFIG_DIR/config/plugins/$PLUGIN_NAME.xml"
if [ -f "$EMBY_PLUGIN_CONFIG" ]; then
    echo "删除 Emby 插件配置..."
    rm -f "$EMBY_PLUGIN_CONFIG"
fi

# 清理可能的缓存文件
CACHE_DIRS=(
    "$EMBY_CONFIG_DIR/cache/plugins/$PLUGIN_NAME"
    "$EMBY_CONFIG_DIR/logs/plugins/$PLUGIN_NAME"
    "$EMBY_CONFIG_DIR/temp/$PLUGIN_NAME"
)

for cache_dir in "${CACHE_DIRS[@]}"; do
    if [ -d "$cache_dir" ]; then
        echo "清理缓存目录: $cache_dir"
        rm -rf "$cache_dir"
    fi
done

# 检查 Emby 是否在运行
echo ""
if pgrep -f "emby-server" > /dev/null; then
    echo "⚠️  检测到 Emby Server 正在运行"
    echo "建议重启 Emby Server 以完全移除插件:"
    
    # 如果在 Docker 环境中
    if [ -f "/.dockerenv" ] || [ -n "${DOCKER_CONTAINER:-}" ]; then
        echo "  docker-compose restart emby"
        echo "  或"
        echo "  docker restart <emby-container-name>"
    else
        echo "  sudo systemctl restart emby-server"
        echo "  或通过 Emby 管理界面重启服务器"
    fi
else
    echo "Emby Server 未运行，插件已完全移除"
fi

echo ""
echo "✅ $PLUGIN_NAME 卸载完成！"
echo ""
echo "卸载摘要:"
echo "  - 已删除插件目录: $PLUGIN_DIR"
echo "  - 已删除插件配置: $EMBY_PLUGIN_CONFIG"
echo "  - 已清理相关缓存文件"

if [ -d "$CONFIG_BACKUP_DIR" ]; then
    echo "  - 配置备份位置: $CONFIG_BACKUP_DIR"
fi

echo ""
echo "注意事项:"
echo "  - 所有自定义主题设置已被移除"
echo "  - Emby 界面将恢复为默认样式"
echo "  - 如需重新安装，请使用安装脚本"

# 提供重新安装的提示
echo ""
echo "如需重新安装插件:"
echo "  1. 下载最新版本的插件包"
echo "  2. 运行安装脚本: ./install-plugin.sh"
echo "  3. 重启 Emby Server"