# PowerShell 版本的插件卸载脚本
# 用于从 Emby Server 中完全移除插件

param(
    [string]$EmbyConfigDir = "",
    [switch]$Force = $false
)

$PLUGIN_NAME = "EmbyBeautifyPlugin"

# 确定 Emby 配置目录
if ([string]::IsNullOrEmpty($EmbyConfigDir)) {
    # 尝试常见的 Emby 配置目录位置
    $possiblePaths = @(
        "$env:ProgramData\Emby-Server",
        "$env:APPDATA\Emby-Server",
        "C:\ProgramData\Emby-Server",
        "/config"  # Docker 环境
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $EmbyConfigDir = $path
            break
        }
    }
    
    if ([string]::IsNullOrEmpty($EmbyConfigDir)) {
        Write-Error "无法找到 Emby 配置目录。请使用 -EmbyConfigDir 参数指定路径。"
        Write-Host ""
        Write-Host "使用方法: .\uninstall-plugin.ps1 -EmbyConfigDir 'C:\ProgramData\Emby-Server'"
        exit 1
    }
}

$PLUGIN_DIR = Join-Path $EmbyConfigDir "plugins\$PLUGIN_NAME"

Write-Host "开始卸载 $PLUGIN_NAME..." -ForegroundColor Yellow

# 检查插件是否已安装
if (-not (Test-Path $PLUGIN_DIR)) {
    Write-Host "插件未安装或已被移除" -ForegroundColor Green
    Write-Host "插件目录不存在: $PLUGIN_DIR"
    exit 0
}

Write-Host "找到插件安装目录: $PLUGIN_DIR" -ForegroundColor Cyan

# 显示将要删除的文件
Write-Host ""
Write-Host "将要删除的文件和目录:" -ForegroundColor Cyan
Get-ChildItem $PLUGIN_DIR | Format-Table Name, Length, LastWriteTime

# 确认删除
if (-not $Force) {
    Write-Host ""
    $confirmation = Read-Host "确定要卸载插件吗？这将删除所有插件文件和配置。(y/N)"
    if ($confirmation -notmatch '^[Yy]$') {
        Write-Host "取消卸载" -ForegroundColor Yellow
        exit 0
    }
}

# 备份配置（如果存在）
$configBackupDir = "$env:TEMP\${PLUGIN_NAME}_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
$pluginConfigFile = Join-Path $PLUGIN_DIR "config.json"
$embyPluginConfigFile = Join-Path $EmbyConfigDir "config\plugins\$PLUGIN_NAME.xml"

if ((Test-Path $pluginConfigFile) -or (Test-Path $embyPluginConfigFile)) {
    Write-Host ""
    Write-Host "备份配置文件到: $configBackupDir" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $configBackupDir -Force | Out-Null
    
    # 备份插件目录中的配置
    if (Test-Path $pluginConfigFile) {
        Copy-Item $pluginConfigFile $configBackupDir
    }
    
    # 备份 Emby 配置目录中的插件配置
    if (Test-Path $embyPluginConfigFile) {
        Copy-Item $embyPluginConfigFile $configBackupDir
    }
    
    Write-Host "配置文件已备份，如需恢复可从备份目录复制" -ForegroundColor Green
}

# 删除插件目录
Write-Host ""
Write-Host "删除插件目录..." -ForegroundColor Cyan
try {
    Remove-Item -Path $PLUGIN_DIR -Recurse -Force
    Write-Host "插件目录删除成功" -ForegroundColor Green
} catch {
    Write-Error "删除插件目录失败: $_"
    exit 1
}

# 删除 Emby 配置中的插件设置
if (Test-Path $embyPluginConfigFile) {
    Write-Host "删除 Emby 插件配置..." -ForegroundColor Cyan
    try {
        Remove-Item $embyPluginConfigFile -Force
        Write-Host "Emby 插件配置删除成功" -ForegroundColor Green
    } catch {
        Write-Warning "删除 Emby 插件配置失败: $_"
    }
}

# 清理可能的缓存文件
$cacheDirs = @(
    (Join-Path $EmbyConfigDir "cache\plugins\$PLUGIN_NAME"),
    (Join-Path $EmbyConfigDir "logs\plugins\$PLUGIN_NAME"),
    (Join-Path $EmbyConfigDir "temp\$PLUGIN_NAME")
)

foreach ($cacheDir in $cacheDirs) {
    if (Test-Path $cacheDir) {
        Write-Host "清理缓存目录: $cacheDir" -ForegroundColor Cyan
        try {
            Remove-Item -Path $cacheDir -Recurse -Force
        } catch {
            Write-Warning "清理缓存目录失败: $_"
        }
    }
}

# 检查 Emby 是否在运行
Write-Host ""
$embyProcess = Get-Process -Name "*emby*" -ErrorAction SilentlyContinue
if ($embyProcess) {
    Write-Host "⚠️  检测到 Emby Server 正在运行" -ForegroundColor Yellow
    Write-Host "建议重启 Emby Server 以完全移除插件:" -ForegroundColor Yellow
    
    # 检查是否为 Windows 服务
    $embyService = Get-Service -Name "*Emby*" -ErrorAction SilentlyContinue
    if ($embyService) {
        Write-Host "  Restart-Service -Name '$($embyService.Name)'" -ForegroundColor Cyan
    } else {
        Write-Host "  通过 Emby 管理界面重启服务器" -ForegroundColor Cyan
        Write-Host "  或手动重启 Emby 进程" -ForegroundColor Cyan
    }
} else {
    Write-Host "Emby Server 未运行，插件已完全移除" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ $PLUGIN_NAME 卸载完成！" -ForegroundColor Green
Write-Host ""
Write-Host "卸载摘要:" -ForegroundColor Cyan
Write-Host "  - 已删除插件目录: $PLUGIN_DIR"
Write-Host "  - 已删除插件配置: $embyPluginConfigFile"
Write-Host "  - 已清理相关缓存文件"

if (Test-Path $configBackupDir) {
    Write-Host "  - 配置备份位置: $configBackupDir"
}

Write-Host ""
Write-Host "注意事项:" -ForegroundColor Yellow
Write-Host "  - 所有自定义主题设置已被移除"
Write-Host "  - Emby 界面将恢复为默认样式"
Write-Host "  - 如需重新安装，请使用安装脚本"

# 提供重新安装的提示
Write-Host ""
Write-Host "如需重新安装插件:" -ForegroundColor Cyan
Write-Host "  1. 下载最新版本的插件包"
Write-Host "  2. 运行安装脚本: .\install-plugin.ps1"
Write-Host "  3. 重启 Emby Server"