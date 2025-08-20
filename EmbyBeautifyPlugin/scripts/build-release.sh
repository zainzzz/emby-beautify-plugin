#!/bin/bash

# 完整的发布构建流程
# 包括测试、构建、打包和验证

set -e

PLUGIN_NAME="EmbyBeautifyPlugin"
VERSION=$(grep -o '<Version>[^<]*' plugin.xml | sed 's/<Version>//')

echo "🚀 开始 $PLUGIN_NAME v$VERSION 发布构建流程..."
echo ""

# 检查必要的工具
echo "1. 检查构建环境..."
if ! command -v dotnet &> /dev/null; then
    echo "错误: .NET SDK 未安装"
    exit 1
fi

if ! command -v zip &> /dev/null; then
    echo "错误: zip 工具未安装"
    exit 1
fi

echo "✅ 构建环境检查通过"
echo ""

# 清理之前的构建
echo "2. 清理构建环境..."
dotnet clean
rm -rf build/ dist/ bin/ obj/
echo "✅ 构建环境清理完成"
echo ""

# 还原依赖
echo "3. 还原 NuGet 包..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ NuGet 包还原失败"
    exit 1
fi
echo "✅ NuGet 包还原完成"
echo ""

# 运行测试
echo "4. 运行单元测试..."
cd ../EmbyBeautifyPlugin.Tests
dotnet test --verbosity normal --logger "console;verbosity=detailed"
TEST_RESULT=$?
cd ../EmbyBeautifyPlugin

if [ $TEST_RESULT -ne 0 ]; then
    echo "❌ 测试失败，停止构建"
    exit 1
fi
echo "✅ 所有测试通过"
echo ""

# 构建项目
echo "5. 构建项目..."
dotnet build -c Release --no-restore
if [ $? -ne 0 ]; then
    echo "❌ 项目构建失败"
    exit 1
fi
echo "✅ 项目构建完成"
echo ""

# 验证构建输出
echo "6. 验证构建输出..."
DLL_PATH="./bin/Release/net6.0/EmbyBeautifyPlugin.dll"
if [ ! -f "$DLL_PATH" ]; then
    echo "❌ 找不到构建的 DLL 文件: $DLL_PATH"
    exit 1
fi

# 检查 DLL 文件信息
echo "DLL 文件信息:"
ls -lh "$DLL_PATH"
echo "✅ 构建输出验证通过"
echo ""

# 创建插件包
echo "7. 创建插件包..."
./scripts/build-package.sh
if [ $? -ne 0 ]; then
    echo "❌ 插件包创建失败"
    exit 1
fi
echo "✅ 插件包创建完成"
echo ""

# 验证插件包
echo "8. 验证插件包..."
PACKAGE_ZIP="dist/${PLUGIN_NAME}-v${VERSION}.zip"
PACKAGE_TAR="dist/${PLUGIN_NAME}-v${VERSION}.tar.gz"

if [ ! -f "$PACKAGE_ZIP" ]; then
    echo "❌ ZIP 包不存在: $PACKAGE_ZIP"
    exit 1
fi

if [ ! -f "$PACKAGE_TAR" ]; then
    echo "❌ TAR.GZ 包不存在: $PACKAGE_TAR"
    exit 1
fi

# 检查包内容
echo "验证 ZIP 包内容:"
unzip -l "$PACKAGE_ZIP"

echo ""
echo "验证校验和文件:"
if [ -f "$PACKAGE_ZIP.sha256" ]; then
    echo "ZIP SHA256: $(cat "$PACKAGE_ZIP.sha256")"
else
    echo "❌ ZIP 校验和文件不存在"
    exit 1
fi

if [ -f "$PACKAGE_TAR.sha256" ]; then
    echo "TAR.GZ SHA256: $(cat "$PACKAGE_TAR.sha256")"
else
    echo "❌ TAR.GZ 校验和文件不存在"
    exit 1
fi

echo "✅ 插件包验证通过"
echo ""

# 生成发布报告
echo "9. 生成发布报告..."
REPORT_FILE="dist/release-report-v${VERSION}.txt"

cat > "$REPORT_FILE" << EOF
Emby Beautify Plugin 发布报告
============================

构建信息:
- 插件名称: $PLUGIN_NAME
- 版本: $VERSION
- 构建时间: $(date)
- 构建环境: $(uname -a)
- .NET 版本: $(dotnet --version)

构建文件:
- 主程序集: EmbyBeautifyPlugin.dll ($(stat -f%z "$DLL_PATH" 2>/dev/null || stat -c%s "$DLL_PATH") bytes)
- 插件清单: plugin.xml
- ZIP 包: ${PLUGIN_NAME}-v${VERSION}.zip ($(stat -f%z "$PACKAGE_ZIP" 2>/dev/null || stat -c%s "$PACKAGE_ZIP") bytes)
- TAR.GZ 包: ${PLUGIN_NAME}-v${VERSION}.tar.gz ($(stat -f%z "$PACKAGE_TAR" 2>/dev/null || stat -c%s "$PACKAGE_TAR") bytes)

校验和:
- ZIP SHA256: $(cat "$PACKAGE_ZIP.sha256" | cut -d' ' -f1)
- TAR.GZ SHA256: $(cat "$PACKAGE_TAR.sha256" | cut -d' ' -f1)

测试结果:
- 单元测试: 通过
- 构建测试: 通过
- 包完整性: 通过

兼容性:
- 目标框架: .NET 6.0
- 最低 Emby 版本: 4.7.0.0
- 最高 Emby 版本: 4.8.0.0

安装说明:
1. 下载插件包 (ZIP 或 TAR.GZ)
2. 解压到 Emby 插件目录
3. 重启 Emby Server
4. 在管理界面中启用插件

发布检查清单:
☑ 代码构建成功
☑ 单元测试通过
☑ 插件包创建完成
☑ 校验和生成完成
☑ 包完整性验证通过
☐ 集成测试验证 (手动)
☐ 文档更新 (手动)
☐ 发布说明准备 (手动)
EOF

echo "✅ 发布报告已生成: $REPORT_FILE"
echo ""

# 显示最终结果
echo "🎉 发布构建完成！"
echo ""
echo "📦 生成的文件:"
ls -lh dist/
echo ""
echo "📋 下一步操作:"
echo "1. 查看发布报告: cat $REPORT_FILE"
echo "2. 进行集成测试: ./scripts/deploy-to-existing-emby.sh"
echo "3. 更新文档和发布说明"
echo "4. 创建 Git 标签: git tag v$VERSION"
echo "5. 发布到分发渠道"
echo ""
echo "🔗 快速测试命令:"
echo "   ./scripts/deploy-to-existing-emby.sh"