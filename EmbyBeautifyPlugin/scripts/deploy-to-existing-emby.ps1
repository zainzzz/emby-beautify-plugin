# PowerShell 版本的部署脚本
# 部署插件到现有的 linuxserver/emby 容器

param(
    [string]$ContainerName = "emby"
)

$PLUGIN_NAME = "EmbyBeautifyPlugin"
$EMBY_CONTAINER_NAME = $ContainerName

Write-Host "部署 $PLUGIN_NAME 到现有的 linuxserver/emby 容器: $EMBY_CONTAINER_NAME" -ForegroundColor Green

# 检查 Docker 是否可用
try {
    docker --version | Out-Null
} catch {
    Write-Error "错误: Docker 未安装或不可用"
    exit 1
}

# 检查容器是否存在
$containerExists = docker ps -a --format "table {{.Names}}" | Select-String "^$EMBY_CONTAINER_NAME$"
if (-not $containerExists) {
    Write-Error "错误: 找不到名为 '$EMBY_CONTAINER_NAME' 的容器"
    Write-Host "请检查容器名称或先启动 Emby 容器" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "使用方法: .\deploy-to-existing-emby.ps1 [容器名称]"
    Write-Host "示例: .\deploy-to-existing-emby.ps1 my-emby-container"
    exit 1
}

# 构建插件
Write-Host "1. 构建插件..." -ForegroundColor Cyan
$dotnetAvailable = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnetAvailable) {
    dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "插件构建失败"
        exit 1
    }
} else {
    Write-Host "本地没有 .NET SDK，使用 Docker 构建..." -ForegroundColor Yellow
    docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:6.0 sh -c "dotnet restore && dotnet build -c Release"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "插件构建失败"
        exit 1
    }
}

if (-not (Test-Path "./bin/Release/EmbyBeautifyPlugin.dll")) {
    Write-Error "错误: 插件构建失败，找不到 DLL 文件"
    exit 1
}

Write-Host "插件构建成功！" -ForegroundColor Green

# 获取容器的配置目录挂载点
Write-Host "2. 检查容器配置..." -ForegroundColor Cyan
$inspectResult = docker inspect $EMBY_CONTAINER_NAME | ConvertFrom-Json
$configMount = $inspectResult.Mounts | Where-Object { $_.Destination -eq "/config" } | Select-Object -First 1

if (-not $configMount) {
    Write-Host "警告: 无法找到 /config 目录的挂载点" -ForegroundColor Yellow
    Write-Host "将直接复制文件到容器内部（重启后会丢失）" -ForegroundColor Yellow
    
    # 直接复制到容器内部
    Write-Host "3. 创建插件目录..." -ForegroundColor Cyan
    docker exec $EMBY_CONTAINER_NAME mkdir -p "/config/plugins/$PLUGIN_NAME"
    
    Write-Host "4. 复制插件文件到容器..." -ForegroundColor Cyan
    docker cp "./bin/Release/EmbyBeautifyPlugin.dll" "${EMBY_CONTAINER_NAME}:/config/plugins/$PLUGIN_NAME/"
    docker cp "./plugin.xml" "${EMBY_CONTAINER_NAME}:/config/plugins/$PLUGIN_NAME/"
    
    if (Test-Path "./bin/Release/EmbyBeautifyPlugin.pdb") {
        docker cp "./bin/Release/EmbyBeautifyPlugin.pdb" "${EMBY_CONTAINER_NAME}:/config/plugins/$PLUGIN_NAME/"
    }
    
    Write-Host "5. 设置文件权限..." -ForegroundColor Cyan
    docker exec $EMBY_CONTAINER_NAME chown -R abc:abc "/config/plugins/$PLUGIN_NAME" 2>$null
    docker exec $EMBY_CONTAINER_NAME chmod -R 755 "/config/plugins/$PLUGIN_NAME"
    
} else {
    $configMountSource = $configMount.Source
    Write-Host "找到配置目录挂载点: $configMountSource" -ForegroundColor Green
    
    # 复制到挂载的主机目录
    $pluginHostDir = Join-Path $configMountSource "plugins\$PLUGIN_NAME"
    Write-Host "3. 创建插件目录: $pluginHostDir" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $pluginHostDir -Force | Out-Null
    
    Write-Host "4. 复制插件文件..." -ForegroundColor Cyan
    Copy-Item "./bin/Release/EmbyBeautifyPlugin.dll" $pluginHostDir
    Copy-Item "./plugin.xml" $pluginHostDir
    
    if (Test-Path "./bin/Release/EmbyBeautifyPlugin.pdb") {
        Copy-Item "./bin/Release/EmbyBeautifyPlugin.pdb" $pluginHostDir
    }
    
    Write-Host "5. 文件权限已设置（Windows 环境）" -ForegroundColor Cyan
}

# 重启容器
Write-Host "6. 重启 Emby 容器以加载插件..." -ForegroundColor Cyan
docker restart $EMBY_CONTAINER_NAME

Write-Host "等待容器重启完成..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "✅ 插件部署成功！" -ForegroundColor Green
Write-Host ""
Write-Host "插件信息:" -ForegroundColor Cyan
Write-Host "  - 名称: $PLUGIN_NAME"
Write-Host "  - 容器: $EMBY_CONTAINER_NAME"
if ($configMount) {
    Write-Host "  - 主机路径: $($configMount.Source)\plugins\$PLUGIN_NAME"
}
Write-Host "  - 容器路径: /config/plugins/$PLUGIN_NAME"
Write-Host ""
Write-Host "请访问 Emby 管理界面检查插件是否加载成功:" -ForegroundColor Yellow
Write-Host "  http://localhost:8096 -> 控制台 -> 插件"