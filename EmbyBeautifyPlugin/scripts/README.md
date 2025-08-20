# 构建和部署脚本

这个目录包含了用于构建、打包、部署和管理 Emby Beautify Plugin 的脚本。

## 脚本概览

### 构建脚本
- `build-package.sh` / `build-package.ps1` - 创建可分发的插件包
- `build-release.sh` / `build-release.ps1` - 完整的发布构建流程
- `build-and-deploy.sh` - 构建并部署到开发环境

### 部署脚本
- `install-plugin.sh` - 在Docker环境中安装插件
- `deploy-to-existing-emby.sh` / `deploy-to-existing-emby.ps1` - 部署到现有Emby容器
- `uninstall-plugin.sh` / `uninstall-plugin.ps1` - 完全卸载插件

### 版本管理
- `update-version.sh` / `update-version.ps1` - 更新插件版本号

### 测试脚本
- `run-tests.sh` / `run-tests.ps1` - 运行单元测试

## 使用指南

### Windows 用户

#### 1. 完整发布构建
```powershell
# 运行完整的构建流程（包括测试）
.\scripts\build-release.ps1

# 跳过测试的快速构建
.\scripts\build-release.ps1 -SkipTests
```

#### 2. 创建插件包
```powershell
# 创建分发包
.\scripts\build-package.ps1

# 清理后重新构建
.\scripts\build-package.ps1 -Clean
```

#### 3. 版本管理
```powershell
# 更新到指定版本
.\scripts\update-version.ps1 -NewVersion "1.0.1"

# 自动递增版本
.\scripts\update-version.ps1 -NewVersion "patch"  # 1.0.0 -> 1.0.1
.\scripts\update-version.ps1 -NewVersion "minor"  # 1.0.0 -> 1.1.0
.\scripts\update-version.ps1 -NewVersion "major"  # 1.0.0 -> 2.0.0

# 预览更改（不实际修改）
.\scripts\update-version.ps1 -NewVersion "1.0.1" -DryRun

# 自动创建Git提交和标签
.\scripts\update-version.ps1 -NewVersion "1.0.1" -AutoCommit -AutoTag
```

#### 4. 部署到现有Emby容器
```powershell
# 部署到默认容器名 "emby"
.\scripts\deploy-to-existing-emby.ps1

# 部署到指定容器
.\scripts\deploy-to-existing-emby.ps1 -ContainerName "my-emby-container"
```

#### 5. 卸载插件
```powershell
# 交互式卸载
.\scripts\uninstall-plugin.ps1

# 指定Emby配置目录
.\scripts\uninstall-plugin.ps1 -EmbyConfigDir "C:\ProgramData\Emby-Server"

# 强制卸载（无确认提示）
.\scripts\uninstall-plugin.ps1 -Force
```

#### 6. 运行测试
```powershell
# 运行所有测试
.\scripts\run-tests.ps1

# 运行测试并生成覆盖率报告
.\scripts\run-tests.ps1 -Coverage

# 详细输出
.\scripts\run-tests.ps1 -Verbose
```

### Linux/macOS 用户

#### 1. 设置脚本权限
```bash
# 首次使用前设置执行权限
chmod +x scripts/*.sh
```

#### 2. 完整发布构建
```bash
# 运行完整的构建流程
./scripts/build-release.sh
```

#### 3. 创建插件包
```bash
# 创建分发包
./scripts/build-package.sh
```

#### 4. 版本管理
```bash
# 更新到指定版本
./scripts/update-version.sh 1.0.1

# 自动递增版本
./scripts/update-version.sh patch   # 1.0.0 -> 1.0.1
./scripts/update-version.sh minor   # 1.0.0 -> 1.1.0
./scripts/update-version.sh major   # 1.0.0 -> 2.0.0

# 预览更改
./scripts/update-version.sh 1.0.1 --dry-run

# 自动创建Git提交和标签
./scripts/update-version.sh 1.0.1 --commit --tag
```

#### 5. 部署到现有Emby容器
```bash
# 部署到默认容器名 "emby"
./scripts/deploy-to-existing-emby.sh

# 部署到指定容器
./scripts/deploy-to-existing-emby.sh my-emby-container
```

#### 6. 卸载插件
```bash
# 交互式卸载
./scripts/uninstall-plugin.sh

# 强制卸载（无确认提示）
FORCE_UNINSTALL=true ./scripts/uninstall-plugin.sh
```

#### 7. 运行测试
```bash
# 运行所有测试
./scripts/run-tests.sh

# 运行测试并生成覆盖率报告
./scripts/run-tests.sh --coverage

# 详细输出
./scripts/run-tests.sh --verbose
```

## 开发工作流程

### 日常开发
1. 修改代码
2. 运行测试: `./scripts/run-tests.sh` 或 `.\scripts\run-tests.ps1`
3. 测试部署: `./scripts/deploy-to-existing-emby.sh` 或 `.\scripts\deploy-to-existing-emby.ps1`

### 发布新版本
1. 更新版本号: `./scripts/update-version.sh 1.0.1 --commit --tag`
2. 运行完整构建: `./scripts/build-release.sh`
3. 验证插件包: 检查 `dist/` 目录中的文件
4. 推送到仓库: `git push origin main && git push origin v1.0.1`

### 紧急修复
1. 创建修复分支
2. 修改代码并测试
3. 更新补丁版本: `./scripts/update-version.sh patch`
4. 快速构建: `./scripts/build-package.sh`
5. 部署测试: `./scripts/deploy-to-existing-emby.sh`

## 故障排除

### 常见问题

#### 1. 权限错误 (Linux/macOS)
```bash
# 设置脚本执行权限
chmod +x scripts/*.sh
```

#### 2. .NET SDK 未找到
- 确保安装了 .NET 6.0 SDK
- 检查 PATH 环境变量

#### 3. Docker 容器未找到
```bash
# 检查容器是否运行
docker ps

# 检查所有容器（包括停止的）
docker ps -a
```

#### 4. 插件加载失败
- 检查 Emby Server 版本兼容性 (4.7.0 - 4.8.0)
- 查看 Emby 日志文件
- 确认插件文件权限正确

#### 5. 构建失败
```bash
# 清理并重新构建
dotnet clean
dotnet restore
dotnet build -c Release
```

### 日志和调试

#### 查看构建日志
- Windows: 检查 PowerShell 输出
- Linux/macOS: 检查终端输出

#### 查看Emby日志
- Docker: `docker logs <emby-container-name>`
- 本地安装: 查看 Emby 配置目录中的日志文件

#### 插件调试
- 使用 `EmbyBeautifyPlugin.pdb` 文件进行调试
- 检查 Emby 插件管理界面中的错误信息

## 环境要求

### 开发环境
- .NET 6.0 SDK
- Git (可选，用于版本管理)
- Docker (用于容器部署)

### 运行环境
- Emby Server 4.7.0 - 4.8.0
- .NET 6.0 Runtime

### 支持的操作系统
- Windows 10/11
- Linux (Ubuntu, CentOS, etc.)
- macOS
- Docker 容器环境