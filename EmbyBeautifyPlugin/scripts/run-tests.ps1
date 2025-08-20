# PowerShell 版本的测试运行脚本
# 运行所有单元测试和集成测试

param(
    [switch]$Coverage = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$TestProject = Join-Path (Split-Path -Parent $ProjectRoot) "EmbyBeautifyPlugin.Tests"

Write-Host "开始运行 Emby Beautify Plugin 测试套件..." -ForegroundColor Green
Write-Host "项目根目录: $ProjectRoot"
Write-Host "测试项目目录: $TestProject"

# 检查测试项目是否存在
if (-not (Test-Path $TestProject)) {
    Write-Error "错误: 测试项目目录不存在: $TestProject"
    exit 1
}

# 检查是否有 .NET SDK
try {
    dotnet --version | Out-Null
} catch {
    Write-Error "错误: 未找到 .NET SDK"
    Write-Host "请安装 .NET 6.0 SDK 或使用 Docker 运行测试" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "使用 Docker 运行测试:" -ForegroundColor Cyan
    Write-Host "  docker run --rm -v `"`$(pwd):/src`" -w /src mcr.microsoft.com/dotnet/sdk:6.0 ``"
    Write-Host "    sh -c `"cd EmbyBeautifyPlugin.Tests && dotnet test --verbosity normal`""
    exit 1
}

try {
    Write-Host ""
    Write-Host "1. 恢复测试项目依赖..." -ForegroundColor Cyan
    Push-Location $TestProject
    dotnet restore
    
    if ($LASTEXITCODE -ne 0) {
        throw "依赖恢复失败"
    }

    Write-Host ""
    Write-Host "2. 构建测试项目..." -ForegroundColor Cyan
    dotnet build --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        throw "测试项目构建失败"
    }

    Write-Host ""
    Write-Host "3. 运行单元测试..." -ForegroundColor Cyan
    
    $testArgs = @("test", "--no-build")
    
    if ($Verbose) {
        $testArgs += "--verbosity", "detailed"
        $testArgs += "--logger", "console;verbosity=detailed"
    } else {
        $testArgs += "--verbosity", "normal"
        $testArgs += "--logger", "console;verbosity=normal"
    }
    
    if ($Coverage) {
        $testArgs += "--collect:XPlat Code Coverage"
        $testArgs += "--results-directory", "./TestResults"
    }
    
    & dotnet @testArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "测试失败"
    }

    Write-Host ""
    Write-Host "✅ 所有测试通过！" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "测试统计:" -ForegroundColor Cyan
    dotnet test --no-build --verbosity quiet --logger "console;verbosity=minimal"

    if ($Coverage -and (Test-Path "./TestResults")) {
        Write-Host ""
        Write-Host "4. 测试覆盖率报告已生成..." -ForegroundColor Cyan
        Write-Host "测试覆盖率文件位于: ./TestResults" -ForegroundColor Yellow
        
        # 如果安装了 reportgenerator，生成 HTML 报告
        try {
            dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool" | Out-Null
            if ($?) {
                Write-Host "生成 HTML 覆盖率报告..." -ForegroundColor Cyan
                $coverageFiles = Get-ChildItem -Path "./TestResults" -Filter "coverage.cobertura.xml" -Recurse
                if ($coverageFiles.Count -gt 0) {
                    dotnet reportgenerator "-reports:./TestResults/**/coverage.cobertura.xml" "-targetdir:./TestResults/CoverageReport" "-reporttypes:Html"
                    Write-Host "HTML 覆盖率报告已生成: ./TestResults/CoverageReport/index.html" -ForegroundColor Green
                }
            }
        } catch {
            Write-Host "跳过 HTML 报告生成（未安装 reportgenerator）" -ForegroundColor Yellow
        }
    }

} catch {
    Write-Host ""
    Write-Host "❌ 测试失败！" -ForegroundColor Red
    Write-Host "错误: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "测试运行完成！" -ForegroundColor Green