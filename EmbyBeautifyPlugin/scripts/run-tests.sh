#!/bin/bash

# Emby Beautify Plugin 测试运行脚本
# 运行所有单元测试和集成测试

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
TEST_PROJECT="$PROJECT_ROOT/../EmbyBeautifyPlugin.Tests"

echo "开始运行 Emby Beautify Plugin 测试套件..."
echo "项目根目录: $PROJECT_ROOT"
echo "测试项目目录: $TEST_PROJECT"

# 检查测试项目是否存在
if [ ! -d "$TEST_PROJECT" ]; then
    echo "错误: 测试项目目录不存在: $TEST_PROJECT"
    exit 1
fi

# 检查是否有 .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "错误: 未找到 .NET SDK"
    echo "请安装 .NET 6.0 SDK 或使用 Docker 运行测试"
    echo ""
    echo "使用 Docker 运行测试:"
    echo "  docker run --rm -v \"\$(pwd):/src\" -w /src mcr.microsoft.com/dotnet/sdk:6.0 \\"
    echo "    sh -c \"cd EmbyBeautifyPlugin.Tests && dotnet test --verbosity normal\""
    exit 1
fi

echo ""
echo "1. 恢复测试项目依赖..."
cd "$TEST_PROJECT"
dotnet restore

if [ $? -ne 0 ]; then
    echo "错误: 依赖恢复失败"
    exit 1
fi

echo ""
echo "2. 构建测试项目..."
dotnet build --no-restore

if [ $? -ne 0 ]; then
    echo "错误: 测试项目构建失败"
    exit 1
fi

echo ""
echo "3. 运行单元测试..."
dotnet test --no-build --verbosity normal --logger "console;verbosity=detailed"

TEST_RESULT=$?

echo ""
if [ $TEST_RESULT -eq 0 ]; then
    echo "✅ 所有测试通过！"
    echo ""
    echo "测试统计:"
    dotnet test --no-build --verbosity quiet --logger "console;verbosity=minimal"
else
    echo "❌ 测试失败！"
    echo "请检查测试输出中的错误信息"
    exit 1
fi

echo ""
echo "4. 生成测试覆盖率报告（如果可用）..."
if command -v reportgenerator &> /dev/null; then
    dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory ./TestResults
    
    if [ -d "./TestResults" ]; then
        echo "测试覆盖率文件已生成在 ./TestResults 目录"
    fi
else
    echo "跳过覆盖率报告生成（未安装 reportgenerator）"
fi

echo ""
echo "测试运行完成！"