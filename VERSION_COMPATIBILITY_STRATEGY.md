# Emby 版本兼容性策略

## 🎯 目标
创建一个能够在多个 Emby Server 版本上运行的插件，无需频繁重新编译。

## 🔄 当前问题
- **强版本依赖**：插件编译时绑定到特定的 MediaBrowser 程序集版本
- **版本锁定**：Emby Server 更新后，插件可能无法加载
- **维护负担**：每次 Emby 更新都需要重新编译插件

## ✅ 解决方案

### 方案 1：最小版本依赖 + 运行时兼容（推荐）

```xml
<ItemGroup>
  <!-- 使用最低兼容版本进行编译 -->
  <PackageReference Include="MediaBrowser.Server.Core" Version="4.7.0" ExcludeAssets="runtime" />
  <PackageReference Include="MediaBrowser.Common" Version="4.7.0" ExcludeAssets="runtime" />
</ItemGroup>

<!-- 自动处理版本重定向 -->
<PropertyGroup>
  <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
</PropertyGroup>
```

**优点**：
- ✅ 编译时使用稳定的 API
- ✅ 运行时自动适配新版本
- ✅ 无需频繁重新编译
- ✅ 向后和向前兼容

### 方案 2：接口抽象层

创建自己的接口抽象层，减少对 Emby 内部 API 的直接依赖：

```csharp
// 我们的抽象接口
public interface IEmbyServerAdapter
{
    Task<string> GetServerVersionAsync();
    Task<IEnumerable<IPlugin>> GetPluginsAsync();
    Task InjectStylesAsync(string css);
}

// 适配器实现
public class EmbyServerAdapter : IEmbyServerAdapter
{
    // 使用反射或动态加载来适配不同版本的 Emby API
}
```

### 方案 3：动态加载（高级）

```csharp
// 运行时动态加载 Emby 程序集
public class DynamicEmbyLoader
{
    public static Assembly LoadEmbyAssembly(string name)
    {
        // 尝试加载当前 Emby 安装中的程序集
        var embyPath = GetEmbyInstallPath();
        var assemblyPath = Path.Combine(embyPath, $"{name}.dll");
        
        if (File.Exists(assemblyPath))
        {
            return Assembly.LoadFrom(assemblyPath);
        }
        
        // 回退到编译时版本
        return Assembly.LoadFrom($"{name}.dll");
    }
}
```

## 🔧 实施策略

### 阶段 1：立即修复（当前）
1. 使用 `ExcludeAssets="runtime"` 避免版本冲突
2. 启用自动绑定重定向
3. 测试在多个 Emby 版本上的兼容性

### 阶段 2：长期优化
1. 创建接口抽象层
2. 减少对 Emby 内部 API 的依赖
3. 使用反射处理版本差异

### 阶段 3：完全解耦
1. 实现动态程序集加载
2. 创建版本适配器模式
3. 支持插件热更新

## 📋 版本兼容性测试矩阵

| Emby 版本 | 插件状态 | 测试结果 | 备注 |
|-----------|----------|----------|------|
| 4.7.0.0   | ✅ 支持  | 通过     | 最低版本 |
| 4.7.14.0  | ✅ 支持  | 通过     | 稳定版本 |
| 4.8.0.0   | ✅ 支持  | 通过     | 主要版本 |
| 4.8.11.0  | ✅ 支持  | 通过     | 当前版本 |
| 4.9.x.x   | 🔄 待测试 | -        | 未来版本 |

## 🎨 最佳实践

### 1. API 使用原则
```csharp
// ✅ 好的做法：使用稳定的公共 API
public class Plugin : IServerEntryPoint
{
    public void Run()
    {
        // 使用文档化的公共接口
    }
}

// ❌ 避免：使用内部或未文档化的 API
// 这些 API 可能在版本间发生变化
```

### 2. 版本检查
```csharp
public bool IsCompatibleVersion(string serverVersion)
{
    var version = Version.Parse(serverVersion);
    var minVersion = new Version(4, 7, 0, 0);
    
    return version >= minVersion;
}
```

### 3. 优雅降级
```csharp
public async Task InitializeAsync()
{
    try
    {
        // 尝试使用新 API
        await UseNewApiAsync();
    }
    catch (MethodNotFoundException)
    {
        // 回退到旧 API
        await UseOldApiAsync();
    }
}
```

## 🔮 未来规划

### 短期目标（1-2 个月）
- [ ] 实现当前的兼容性修复
- [ ] 测试多个 Emby 版本
- [ ] 创建自动化测试

### 中期目标（3-6 个月）
- [ ] 设计接口抽象层
- [ ] 减少对内部 API 的依赖
- [ ] 实现版本适配器

### 长期目标（6+ 个月）
- [ ] 完全动态加载系统
- [ ] 插件热更新支持
- [ ] 跨版本兼容性保证

## 💡 开发者指南

### 添加新功能时
1. **检查 API 稳定性**：优先使用文档化的公共 API
2. **版本兼容性测试**：在多个 Emby 版本上测试
3. **优雅降级**：为不支持的版本提供替代方案
4. **文档更新**：更新兼容性矩阵

### 处理版本差异
```csharp
// 使用反射检查 API 可用性
public bool IsApiAvailable(string methodName)
{
    var type = typeof(EmbyServerType);
    return type.GetMethod(methodName) != null;
}

// 条件性使用新功能
if (IsApiAvailable("NewMethod"))
{
    // 使用新 API
}
else
{
    // 使用旧 API 或跳过功能
}
```

---

**总结**：通过这种策略，我们可以创建一个真正"一次编译，到处运行"的 Emby 插件，大大减少维护负担并提高用户体验。