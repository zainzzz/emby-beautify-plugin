# PowerShell 版本管理脚本
# 用于更新插件版本号

param(
    [Parameter(Mandatory=$true)]
    [string]$NewVersion,
    
    [switch]$DryRun = $false,
    [switch]$AutoCommit = $false,
    [switch]$AutoTag = $false
)

# 获取当前版本
$currentVersionMatch = Select-String -Path "plugin.xml" -Pattern "<Version>([^<]+)</Version>"
if (-not $currentVersionMatch) {
    Write-Error "无法从 plugin.xml 中读取当前版本"
    exit 1
}

$currentVersion = $currentVersionMatch.Matches[0].Groups[1].Value
Write-Host "当前版本: $currentVersion" -ForegroundColor Cyan
Write-Host ""

# 解析当前版本
$versionParts = $currentVersion.Split('.')
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$patch = [int]$versionParts[2]

# 计算新版本
switch ($NewVersion.ToLower()) {
    "major" {
        $NewVersion = "$($major + 1).0.0.0"
    }
    "minor" {
        $NewVersion = "$major.$($minor + 1).0.0"
    }
    "patch" {
        $NewVersion = "$major.$minor.$($patch + 1).0"
    }
    default {
        # 验证版本格式
        if ($NewVersion -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
            Write-Error "错误: 版本格式无效。请使用 x.y.z 或 x.y.z.w 格式"
            exit 1
        }
        
        # 如果只有三位数字，添加第四位
        if ($NewVersion -notmatch '\.\d+$') {
            $NewVersion = "$NewVersion.0"
        }
    }
}

Write-Host "新版本: $NewVersion" -ForegroundColor Green
Write-Host ""

if ($DryRun) {
    Write-Host "🔍 预览模式 - 将要进行的更改:" -ForegroundColor Yellow
    Write-Host ""
}

# 更新文件函数
function Update-File {
    param(
        [string]$FilePath,
        [string]$Pattern,
        [string]$Replacement,
        [string]$Description
    )
    
    if ($DryRun) {
        Write-Host "  📝 $Description" -ForegroundColor Cyan
        Write-Host "     文件: $FilePath"
        Write-Host "     查找: $Pattern"
        Write-Host "     替换: $Replacement"
        Write-Host ""
    } else {
        if (Test-Path $FilePath) {
            Write-Host "更新 $Description..." -ForegroundColor Cyan
            (Get-Content $FilePath) -replace [regex]::Escape($Pattern), $Replacement | Set-Content $FilePath -Encoding UTF8
        }
    }
}

# 更新 plugin.xml
Update-File -FilePath "plugin.xml" `
    -Pattern "<Version>$currentVersion</Version>" `
    -Replacement "<Version>$NewVersion</Version>" `
    -Description "plugin.xml 中的版本号"

# 更新 .csproj 文件
if (Test-Path "EmbyBeautifyPlugin.csproj") {
    Update-File -FilePath "EmbyBeautifyPlugin.csproj" `
        -Pattern "<AssemblyVersion>$currentVersion</AssemblyVersion>" `
        -Replacement "<AssemblyVersion>$NewVersion</AssemblyVersion>" `
        -Description "项目文件中的程序集版本"
    
    Update-File -FilePath "EmbyBeautifyPlugin.csproj" `
        -Pattern "<FileVersion>$currentVersion</FileVersion>" `
        -Replacement "<FileVersion>$NewVersion</FileVersion>" `
        -Description "项目文件中的文件版本"
}

# 更新 AssemblyInfo.cs
if (Test-Path "Properties\AssemblyInfo.cs") {
    Update-File -FilePath "Properties\AssemblyInfo.cs" `
        -Pattern "AssemblyVersion(`"$currentVersion`")" `
        -Replacement "AssemblyVersion(`"$NewVersion`")" `
        -Description "AssemblyInfo.cs 中的程序集版本"
    
    Update-File -FilePath "Properties\AssemblyInfo.cs" `
        -Pattern "AssemblyFileVersion(`"$currentVersion`")" `
        -Replacement "AssemblyFileVersion(`"$NewVersion`")" `
        -Description "AssemblyInfo.cs 中的文件版本"
}

# 更新 README.md 中的版本引用
if (Test-Path "README.md") {
    Update-File -FilePath "README.md" `
        -Pattern "Version: $currentVersion" `
        -Replacement "Version: $NewVersion" `
        -Description "README.md 中的版本信息"
}

if ($DryRun) {
    Write-Host "🔍 预览完成。使用不带 -DryRun 参数运行以实际更新版本。" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "✅ 版本更新完成: $currentVersion -> $NewVersion" -ForegroundColor Green

# Git 操作
if ($AutoCommit -or $AutoTag) {
    $gitAvailable = Get-Command git -ErrorAction SilentlyContinue
    if (-not $gitAvailable) {
        Write-Warning "Git 未安装，跳过 Git 操作"
    } elseif (-not (Test-Path ".git")) {
        Write-Warning "不在 Git 仓库中，跳过 Git 操作"
    } else {
        if ($AutoCommit) {
            Write-Host ""
            Write-Host "创建 Git commit..." -ForegroundColor Cyan
            try {
                git add plugin.xml EmbyBeautifyPlugin.csproj Properties/AssemblyInfo.cs README.md 2>$null
                git commit -m "chore: bump version to $NewVersion"
            } catch {
                Write-Warning "Git commit 失败: $_"
            }
        }
        
        if ($AutoTag) {
            Write-Host "创建 Git tag..." -ForegroundColor Cyan
            try {
                git tag -a "v$NewVersion" -m "Release version $NewVersion"
                Write-Host "提示: 使用 'git push origin v$NewVersion' 推送标签到远程仓库" -ForegroundColor Yellow
            } catch {
                Write-Warning "Git tag 创建失败: $_"
            }
        }
    }
}

Write-Host ""
Write-Host "下一步建议:" -ForegroundColor Cyan
Write-Host "1. 运行测试: dotnet test"
Write-Host "2. 构建插件: .\scripts\build-package.ps1"
Write-Host "3. 测试部署: .\scripts\deploy-to-existing-emby.ps1"

if ($AutoTag) {
    Write-Host "4. 推送标签: git push origin v$NewVersion"
}