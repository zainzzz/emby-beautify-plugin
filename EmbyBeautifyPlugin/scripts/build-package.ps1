# PowerShell 版本的插件打包脚本
# 创建可分发的插件包

param(
    [switch]$Clean = $false
)

$PLUGIN_NAME = "EmbyBeautifyPlugin"

# 确保在正确的目录中运行
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptDir
Push-Location $projectDir

$VERSION = (Select-Xml -Path "plugin.xml" -XPath "//Version").Node.InnerText
$BUILD_DIR = "build"
$PACKAGE_DIR = "$BUILD_DIR\package"
$DIST_DIR = "dist"

Write-Host "开始构建 $PLUGIN_NAME v$VERSION 插件包..." -ForegroundColor Green

# 清理之前的构建
if ($Clean -or (Test-Path $BUILD_DIR)) {
    Write-Host "1. 清理构建目录..." -ForegroundColor Cyan
    Remove-Item -Path $BUILD_DIR -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $DIST_DIR -Recurse -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Path $PACKAGE_DIR -Force | Out-Null
New-Item -ItemType Directory -Path $DIST_DIR -Force | Out-Null

# 构建插件
Write-Host "2. 构建插件..." -ForegroundColor Cyan
dotnet clean
if ($LASTEXITCODE -ne 0) {
    Write-Error "清理失败"
    exit 1
}

dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "还原包失败"
    exit 1
}

dotnet build -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "构建失败"
    exit 1
}

$dllPath = ".\bin\Release\net6.0\EmbyBeautifyPlugin.dll"
if (-not (Test-Path $dllPath)) {
    Write-Error "错误: 插件构建失败，找不到 DLL 文件"
    exit 1
}

Write-Host "插件构建成功！" -ForegroundColor Green

# 复制插件文件到包目录
Write-Host "3. 准备插件文件..." -ForegroundColor Cyan
Copy-Item $dllPath $PACKAGE_DIR
Copy-Item ".\plugin.xml" $PACKAGE_DIR

# 复制依赖项（如果有的话）
$newtonsoftPath = ".\bin\Release\net6.0\Newtonsoft.Json.dll"
if (Test-Path $newtonsoftPath) {
    Copy-Item $newtonsoftPath $PACKAGE_DIR
}

# 创建安装说明
$installContent = "Emby Beautify Plugin v$VERSION 安装说明`n" +
"========================================`n`n" +
"安装步骤:`n" +
"1. 停止 Emby Server`n" +
"2. 将此文件夹中的所有文件复制到 Emby 插件目录:`n" +
"   - Windows: %ProgramData%\Emby-Server\plugins\EmbyBeautifyPlugin\`n" +
"   - Linux: /var/lib/emby/plugins/EmbyBeautifyPlugin/`n" +
"   - Docker: /config/plugins/EmbyBeautifyPlugin/`n" +
"3. 启动 Emby Server`n" +
"4. 在管理界面中启用插件`n`n" +
"文件说明:`n" +
"- EmbyBeautifyPlugin.dll: 主插件文件`n" +
"- plugin.xml: 插件清单文件`n" +
"- Newtonsoft.Json.dll: JSON 处理依赖项（如果存在）`n`n" +
"支持的 Emby Server 版本: 4.7.0 - 4.8.0`n" +
"目标框架: .NET 6.0`n`n" +
"更多信息请访问: https://github.com/emby-beautify/plugin"

Set-Content -Path "$PACKAGE_DIR\INSTALL.txt" -Value $installContent -Encoding UTF8

# 创建卸载说明
$uninstallContent = "Emby Beautify Plugin 卸载说明`n" +
"===========================`n`n" +
"卸载步骤:`n" +
"1. 停止 Emby Server`n" +
"2. 删除插件目录:`n" +
"   - Windows: %ProgramData%\Emby-Server\plugins\EmbyBeautifyPlugin\`n" +
"   - Linux: /var/lib/emby/plugins/EmbyBeautifyPlugin/`n" +
"   - Docker: /config/plugins/EmbyBeautifyPlugin/`n" +
"3. 启动 Emby Server`n`n" +
"注意: 卸载插件后，所有自定义主题设置将被重置为默认值。"

Set-Content -Path "$PACKAGE_DIR\UNINSTALL.txt" -Value $uninstallContent -Encoding UTF8

# 计算文件校验和
$dllHash = (Get-FileHash $dllPath -Algorithm SHA256).Hash.ToLower()
$xmlHash = (Get-FileHash ".\plugin.xml" -Algorithm SHA256).Hash.ToLower()

# 创建版本信息文件
$versionInfo = @{
    name = $PLUGIN_NAME
    version = $VERSION
    buildDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    targetFramework = "net6.0"
    minEmbyVersion = "4.7.0.0"
    maxEmbyVersion = "4.8.0.0"
    files = @("EmbyBeautifyPlugin.dll", "plugin.xml")
    dependencies = @("Newtonsoft.Json")
    checksum = @{
        algorithm = "SHA256"
        dll = $dllHash
        xml = $xmlHash
    }
}

$versionInfo | ConvertTo-Json -Depth 3 | Set-Content -Path "$PACKAGE_DIR\VERSION.json" -Encoding UTF8

# 创建ZIP包
Write-Host "4. 创建分发包..." -ForegroundColor Cyan
$zipPath = "$DIST_DIR\${PLUGIN_NAME}-v${VERSION}.zip"
Compress-Archive -Path "$PACKAGE_DIR\*" -DestinationPath $zipPath -Force

# 生成校验和文件
Write-Host "5. 生成校验和..." -ForegroundColor Cyan
$zipHash = (Get-FileHash $zipPath -Algorithm SHA256).Hash.ToLower()
Set-Content -Path "$zipPath.sha256" -Value "$zipHash  ${PLUGIN_NAME}-v${VERSION}.zip" -Encoding ASCII

# 显示构建结果
Write-Host "" 
Write-Host "✅ 插件包构建完成！" -ForegroundColor Green
Write-Host ""
Write-Host "构建信息:" -ForegroundColor Cyan
Write-Host "  - 插件名称: $PLUGIN_NAME"
Write-Host "  - 版本: $VERSION"
Write-Host "  - 构建时间: $(Get-Date)"
Write-Host ""
Write-Host "生成的文件:" -ForegroundColor Cyan
Write-Host "  - $zipPath"
Write-Host "  - $zipPath.sha256"
Write-Host ""
Write-Host "包内容:" -ForegroundColor Cyan
Get-ChildItem $PACKAGE_DIR | Format-Table Name, Length, LastWriteTime
Write-Host ""
Write-Host "分发包大小:" -ForegroundColor Cyan
Get-ChildItem $DIST_DIR | Format-Table Name, Length, LastWriteTime

# 恢复原始目录
Pop-Location