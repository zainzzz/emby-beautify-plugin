# PowerShell 版本的完整发布构建流程
# 包括测试、构建、打包和验证

param(
    [switch]$SkipTests = $false
)

$PLUGIN_NAME = "EmbyBeautifyPlugin"

# 确保在正确的目录中运行
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptDir
Push-Location $projectDir

$VERSION = (Select-Xml -Path "plugin.xml" -XPath "//Version").Node.InnerText

Write-Host "🚀 开始 $PLUGIN_NAME v$VERSION 发布构建流程..." -ForegroundColor Green
Write-Host ""

# 检查必要的工具
Write-Host "1. 检查构建环境..." -ForegroundColor Cyan
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ .NET SDK 未安装"
    exit 1
}

Write-Host "✅ 构建环境检查通过" -ForegroundColor Green
Write-Host ""

# 清理之前的构建
Write-Host "2. 清理构建环境..." -ForegroundColor Cyan
dotnet clean | Out-Null
Remove-Item -Path "build", "dist", "bin", "obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ 构建环境清理完成" -ForegroundColor Green
Write-Host ""

# 还原依赖
Write-Host "3. 还原 NuGet 包..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ NuGet 包还原失败"
    exit 1
}
Write-Host "✅ NuGet 包还原完成" -ForegroundColor Green
Write-Host ""

# 运行测试
if (-not $SkipTests) {
    Write-Host "4. 运行单元测试..." -ForegroundColor Cyan
    Push-Location "..\EmbyBeautifyPlugin.Tests"
    try {
        dotnet test --verbosity normal --logger "console;verbosity=detailed"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "❌ 测试失败，停止构建"
            exit 1
        }
        Write-Host "✅ 所有测试通过" -ForegroundColor Green
    } finally {
        Pop-Location
    }
    Write-Host ""
} else {
    Write-Host "4. 跳过单元测试 (使用了 -SkipTests 参数)" -ForegroundColor Yellow
    Write-Host ""
}

# 构建项目
Write-Host "5. 构建项目..." -ForegroundColor Cyan
dotnet build -c Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ 项目构建失败"
    exit 1
}
Write-Host "✅ 项目构建完成" -ForegroundColor Green
Write-Host ""

# 验证构建输出
Write-Host "6. 验证构建输出..." -ForegroundColor Cyan
$dllPath = ".\bin\Release\net6.0\EmbyBeautifyPlugin.dll"
if (-not (Test-Path $dllPath)) {
    Write-Error "❌ 找不到构建的 DLL 文件: $dllPath"
    exit 1
}

# 检查 DLL 文件信息
Write-Host "DLL 文件信息:"
Get-Item $dllPath | Format-Table Name, Length, LastWriteTime
Write-Host "✅ 构建输出验证通过" -ForegroundColor Green
Write-Host ""

# 创建插件包
Write-Host "7. 创建插件包..." -ForegroundColor Cyan
try {
    & ".\scripts\build-package.ps1"
    if ($LASTEXITCODE -ne 0) {
        throw "插件包创建失败"
    }
} catch {
    Write-Error "❌ 插件包创建失败: $_"
    exit 1
}
Write-Host "✅ 插件包创建完成" -ForegroundColor Green
Write-Host ""

# 验证插件包
Write-Host "8. 验证插件包..." -ForegroundColor Cyan
$packageZip = "dist\${PLUGIN_NAME}-v${VERSION}.zip"

if (-not (Test-Path $packageZip)) {
    Write-Error "❌ ZIP 包不存在: $packageZip"
    exit 1
}

# 检查包内容
Write-Host "验证 ZIP 包内容:"
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $packageZip).Path)
$zip.Entries | Format-Table FullName, Length, LastWriteTime
$zip.Dispose()

Write-Host ""
Write-Host "验证校验和文件:"
$checksumFile = "$packageZip.sha256"
if (Test-Path $checksumFile) {
    $checksum = Get-Content $checksumFile
    Write-Host "ZIP SHA256: $checksum" -ForegroundColor Green
} else {
    Write-Error "❌ ZIP 校验和文件不存在"
    exit 1
}

Write-Host "✅ 插件包验证通过" -ForegroundColor Green
Write-Host ""

# 生成发布报告
Write-Host "9. 生成发布报告..." -ForegroundColor Cyan
$reportFile = "dist\release-report-v${VERSION}.txt"

$dllSize = (Get-Item $dllPath).Length
$zipSize = (Get-Item $packageZip).Length
$checksum = (Get-Content $checksumFile).Split(' ')[0]

$reportContent = @"
Emby Beautify Plugin 发布报告
============================

构建信息:
- 插件名称: $PLUGIN_NAME
- 版本: $VERSION
- 构建时间: $(Get-Date)
- 构建环境: $env:COMPUTERNAME ($env:OS)
- .NET 版本: $dotnetVersion

构建文件:
- 主程序集: EmbyBeautifyPlugin.dll ($dllSize bytes)
- 插件清单: plugin.xml
- ZIP 包: ${PLUGIN_NAME}-v${VERSION}.zip ($zipSize bytes)

校验和:
- ZIP SHA256: $checksum

测试结果:
- 单元测试: $(if ($SkipTests) { "跳过" } else { "通过" })
- 构建测试: 通过
- 包完整性: 通过

兼容性:
- 目标框架: .NET 6.0
- 最低 Emby 版本: 4.7.0.0
- 最高 Emby 版本: 4.8.0.0

安装说明:
1. 下载插件包 (ZIP)
2. 解压到 Emby 插件目录
3. 重启 Emby Server
4. 在管理界面中启用插件

发布检查清单:
☑ 代码构建成功
☑ $(if ($SkipTests) { "单元测试跳过" } else { "单元测试通过" })
☑ 插件包创建完成
☑ 校验和生成完成
☑ 包完整性验证通过
☐ 集成测试验证 (手动)
☐ 文档更新 (手动)
☐ 发布说明准备 (手动)
"@

Set-Content -Path $reportFile -Value $reportContent -Encoding UTF8
Write-Host "✅ 发布报告已生成: $reportFile" -ForegroundColor Green
Write-Host ""

# 显示最终结果
Write-Host "🎉 发布构建完成！" -ForegroundColor Green
Write-Host ""
Write-Host "📦 生成的文件:" -ForegroundColor Cyan
Get-ChildItem "dist" | Format-Table Name, Length, LastWriteTime
Write-Host ""
Write-Host "📋 下一步操作:" -ForegroundColor Cyan
Write-Host "1. 查看发布报告: Get-Content $reportFile"
Write-Host "2. 进行集成测试: .\scripts\deploy-to-existing-emby.ps1"
Write-Host "3. 更新文档和发布说明"
Write-Host "4. 创建 Git 标签: git tag v$VERSION"
Write-Host "5. 发布到分发渠道"
Write-Host ""
Write-Host "🔗 快速测试命令:" -ForegroundColor Yellow
Write-Host "   .\scripts\deploy-to-existing-emby.ps1"

# 恢复原始目录
Pop-Location