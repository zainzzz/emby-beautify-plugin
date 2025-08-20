#!/bin/bash

# 版本管理脚本
# 用于更新插件版本号

set -e

CURRENT_VERSION=$(grep -o '<Version>[^<]*' plugin.xml | sed 's/<Version>//')

echo "当前版本: $CURRENT_VERSION"
echo ""

if [ $# -eq 0 ]; then
    echo "使用方法: $0 <新版本号> [选项]"
    echo ""
    echo "示例:"
    echo "  $0 1.0.1          # 更新到指定版本"
    echo "  $0 patch          # 自动递增补丁版本 (1.0.0 -> 1.0.1)"
    echo "  $0 minor          # 自动递增次版本 (1.0.0 -> 1.1.0)"
    echo "  $0 major          # 自动递增主版本 (1.0.0 -> 2.0.0)"
    echo ""
    echo "选项:"
    echo "  --dry-run         # 仅显示将要更改的内容，不实际修改"
    echo "  --commit          # 自动创建 git commit"
    echo "  --tag             # 自动创建 git tag"
    exit 1
fi

NEW_VERSION="$1"
DRY_RUN=false
AUTO_COMMIT=false
AUTO_TAG=false

# 解析选项
shift
while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --commit)
            AUTO_COMMIT=true
            shift
            ;;
        --tag)
            AUTO_TAG=true
            shift
            ;;
        *)
            echo "未知选项: $1"
            exit 1
            ;;
    esac
done

# 解析当前版本
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION"
MAJOR=${VERSION_PARTS[0]}
MINOR=${VERSION_PARTS[1]}
PATCH=${VERSION_PARTS[2]%.*}  # 移除可能的第四位数字

# 计算新版本
case $NEW_VERSION in
    "major")
        NEW_VERSION="$((MAJOR + 1)).0.0.0"
        ;;
    "minor")
        NEW_VERSION="$MAJOR.$((MINOR + 1)).0.0"
        ;;
    "patch")
        NEW_VERSION="$MAJOR.$MINOR.$((PATCH + 1)).0"
        ;;
    *)
        # 验证版本格式
        if [[ ! $NEW_VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(\.[0-9]+)?$ ]]; then
            echo "错误: 版本格式无效。请使用 x.y.z 或 x.y.z.w 格式"
            exit 1
        fi
        
        # 如果只有三位数字，添加第四位
        if [[ ! $NEW_VERSION =~ \.[0-9]+$ ]]; then
            NEW_VERSION="$NEW_VERSION.0"
        fi
        ;;
esac

echo "新版本: $NEW_VERSION"
echo ""

if [ "$DRY_RUN" = true ]; then
    echo "🔍 预览模式 - 将要进行的更改:"
    echo ""
fi

# 更新文件函数
update_file() {
    local file="$1"
    local pattern="$2"
    local replacement="$3"
    local description="$4"
    
    if [ "$DRY_RUN" = true ]; then
        echo "  📝 $description"
        echo "     文件: $file"
        echo "     查找: $pattern"
        echo "     替换: $replacement"
        echo ""
    else
        echo "更新 $description..."
        sed -i "s|$pattern|$replacement|g" "$file"
    fi
}

# 更新 plugin.xml
update_file "plugin.xml" \
    "<Version>$CURRENT_VERSION</Version>" \
    "<Version>$NEW_VERSION</Version>" \
    "plugin.xml 中的版本号"

# 更新 .csproj 文件
if [ -f "EmbyBeautifyPlugin.csproj" ]; then
    update_file "EmbyBeautifyPlugin.csproj" \
        "<AssemblyVersion>$CURRENT_VERSION</AssemblyVersion>" \
        "<AssemblyVersion>$NEW_VERSION</AssemblyVersion>" \
        "项目文件中的程序集版本"
    
    update_file "EmbyBeautifyPlugin.csproj" \
        "<FileVersion>$CURRENT_VERSION</FileVersion>" \
        "<FileVersion>$NEW_VERSION</FileVersion>" \
        "项目文件中的文件版本"
fi

# 更新 AssemblyInfo.cs
if [ -f "Properties/AssemblyInfo.cs" ]; then
    update_file "Properties/AssemblyInfo.cs" \
        "AssemblyVersion(\"$CURRENT_VERSION\")" \
        "AssemblyVersion(\"$NEW_VERSION\")" \
        "AssemblyInfo.cs 中的程序集版本"
    
    update_file "Properties/AssemblyInfo.cs" \
        "AssemblyFileVersion(\"$CURRENT_VERSION\")" \
        "AssemblyFileVersion(\"$NEW_VERSION\")" \
        "AssemblyInfo.cs 中的文件版本"
fi

# 更新 README.md 中的版本引用
if [ -f "README.md" ]; then
    update_file "README.md" \
        "Version: $CURRENT_VERSION" \
        "Version: $NEW_VERSION" \
        "README.md 中的版本信息"
fi

if [ "$DRY_RUN" = true ]; then
    echo "🔍 预览完成。使用不带 --dry-run 参数运行以实际更新版本。"
    exit 0
fi

echo ""
echo "✅ 版本更新完成: $CURRENT_VERSION -> $NEW_VERSION"

# Git 操作
if [ "$AUTO_COMMIT" = true ] || [ "$AUTO_TAG" = true ]; then
    if ! command -v git &> /dev/null; then
        echo "警告: Git 未安装，跳过 Git 操作"
    elif [ ! -d ".git" ]; then
        echo "警告: 不在 Git 仓库中，跳过 Git 操作"
    else
        if [ "$AUTO_COMMIT" = true ]; then
            echo ""
            echo "创建 Git commit..."
            git add plugin.xml EmbyBeautifyPlugin.csproj Properties/AssemblyInfo.cs README.md 2>/dev/null || true
            git commit -m "chore: bump version to $NEW_VERSION" || echo "警告: Git commit 失败"
        fi
        
        if [ "$AUTO_TAG" = true ]; then
            echo "创建 Git tag..."
            git tag -a "v$NEW_VERSION" -m "Release version $NEW_VERSION" || echo "警告: Git tag 创建失败"
            echo "提示: 使用 'git push origin v$NEW_VERSION' 推送标签到远程仓库"
        fi
    fi
fi

echo ""
echo "下一步建议:"
echo "1. 运行测试: dotnet test"
echo "2. 构建插件: ./scripts/build-package.sh"
echo "3. 测试部署: ./scripts/deploy-to-existing-emby.sh"

if [ "$AUTO_TAG" = true ]; then
    echo "4. 推送标签: git push origin v$NEW_VERSION"
fi