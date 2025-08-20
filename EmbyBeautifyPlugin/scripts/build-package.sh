#!/bin/bash

# Emby Beautify Plugin 打包脚本
# 创建可分发的插件包

set -e

PLUGIN_NAME="EmbyBeautifyPlugin"
VERSION=$(grep -o '<Version>[^<]*' plugin.xml | sed 's/<Version>//')
BUILD_DIR="build"
PACKAGE_DIR="$BUILD_DIR/package"
DIST_DIR="dist"

echo "开始构建 $PLUGIN_NAME v$VERSION 插件包..."

# 清理之前的构建
echo "1. 清理构建目录..."
rm -rf "$BUILD_DIR" "$DIST_DIR"
mkdir -p "$PACKAGE_DIR" "$DIST_DIR"

# 构建插件
echo "2. 构建插件..."
dotnet clean
dotnet restore
dotnet build -c Release --no-restore

if [ ! -f "./bin/Release/net6.0/EmbyBeautifyPlugin.dll" ]; then
    echo "错误: 插件构建失败"
    exit 1
fi

echo "插件构建成功！"

# 复制插件文件到包目录
echo "3. 准备插件文件..."
cp "./bin/Release/net6.0/EmbyBeautifyPlugin.dll" "$PACKAGE_DIR/"
cp "./plugin.xml" "$PACKAGE_DIR/"

# 复制依赖项（如果有的话）
if [ -f "./bin/Release/net6.0/Newtonsoft.Json.dll" ]; then
    cp "./bin/Release/net6.0/Newtonsoft.Json.dll" "$PACKAGE_DIR/"
fi

# 创建安装说明
cat > "$PACKAGE_DIR/INSTALL.txt" << EOF
Emby Beautify Plugin v$VERSION 安装说明
========================================

安装步骤:
1. 停止 Emby Server
2. 将此文件夹中的所有文件复制到 Emby 插件目录:
   - Windows: %ProgramData%\Emby-Server\plugins\EmbyBeautifyPlugin\
   - Linux: /var/lib/emby/plugins/EmbyBeautifyPlugin/
   - Docker: /config/plugins/EmbyBeautifyPlugin/
3. 启动 Emby Server
4. 在管理界面中启用插件

文件说明:
- EmbyBeautifyPlugin.dll: 主插件文件
- plugin.xml: 插件清单文件
- Newtonsoft.Json.dll: JSON 处理依赖项（如果存在）

支持的 Emby Server 版本: 4.7.0 - 4.8.0
目标框架: .NET 6.0

更多信息请访问: https://github.com/emby-beautify/plugin
EOF

# 创建卸载说明
cat > "$PACKAGE_DIR/UNINSTALL.txt" << EOF
Emby Beautify Plugin 卸载说明
============================

卸载步骤:
1. 停止 Emby Server
2. 删除插件目录:
   - Windows: %ProgramData%\Emby-Server\plugins\EmbyBeautifyPlugin\
   - Linux: /var/lib/emby/plugins/EmbyBeautifyPlugin/
   - Docker: /config/plugins/EmbyBeautifyPlugin/
3. 启动 Emby Server

注意: 卸载插件后，所有自定义主题设置将被重置为默认值。
EOF

# 创建版本信息文件
cat > "$PACKAGE_DIR/VERSION.json" << EOF
{
  "name": "$PLUGIN_NAME",
  "version": "$VERSION",
  "buildDate": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "targetFramework": "net6.0",
  "minEmbyVersion": "4.7.0.0",
  "maxEmbyVersion": "4.8.0.0",
  "files": [
    "EmbyBeautifyPlugin.dll",
    "plugin.xml"
  ],
  "dependencies": [
    "Newtonsoft.Json"
  ],
  "checksum": {
    "algorithm": "SHA256",
    "dll": "$(sha256sum ./bin/Release/net6.0/EmbyBeautifyPlugin.dll | cut -d' ' -f1)",
    "xml": "$(sha256sum ./plugin.xml | cut -d' ' -f1)"
  }
}
EOF

# 创建ZIP包
echo "4. 创建分发包..."
cd "$BUILD_DIR"
zip -r "../$DIST_DIR/${PLUGIN_NAME}-v${VERSION}.zip" package/
cd ..

# 创建TAR.GZ包（Linux用户友好）
cd "$BUILD_DIR"
tar -czf "../$DIST_DIR/${PLUGIN_NAME}-v${VERSION}.tar.gz" package/
cd ..

# 生成校验和文件
echo "5. 生成校验和..."
cd "$DIST_DIR"
sha256sum "${PLUGIN_NAME}-v${VERSION}.zip" > "${PLUGIN_NAME}-v${VERSION}.zip.sha256"
sha256sum "${PLUGIN_NAME}-v${VERSION}.tar.gz" > "${PLUGIN_NAME}-v${VERSION}.tar.gz.sha256"
cd ..

# 显示构建结果
echo ""
echo "✅ 插件包构建完成！"
echo ""
echo "构建信息:"
echo "  - 插件名称: $PLUGIN_NAME"
echo "  - 版本: $VERSION"
echo "  - 构建时间: $(date)"
echo ""
echo "生成的文件:"
echo "  - $DIST_DIR/${PLUGIN_NAME}-v${VERSION}.zip"
echo "  - $DIST_DIR/${PLUGIN_NAME}-v${VERSION}.tar.gz"
echo "  - $DIST_DIR/${PLUGIN_NAME}-v${VERSION}.zip.sha256"
echo "  - $DIST_DIR/${PLUGIN_NAME}-v${VERSION}.tar.gz.sha256"
echo ""
echo "包内容:"
ls -la "$PACKAGE_DIR"
echo ""
echo "分发包大小:"
ls -lh "$DIST_DIR"