# 故障排除指南

本指南帮助您解决使用 Emby Beautify Plugin 时可能遇到的常见问题。

## 目录
- [安装问题](#安装问题)
- [主题问题](#主题问题)
- [性能问题](#性能问题)
- [兼容性问题](#兼容性问题)
- [界面显示问题](#界面显示问题)
- [配置问题](#配置问题)
- [调试工具](#调试工具)
- [获取帮助](#获取帮助)

## 安装问题

### 问题1：插件未出现在插件列表中

**症状**：
- 安装后在 Emby 插件管理页面看不到插件
- 插件状态显示为"未安装"

**可能原因**：
1. 文件复制不完整
2. 权限设置错误
3. Emby Server 版本不兼容
4. 插件文件损坏

**解决方案**：

#### 步骤1：检查文件完整性
```bash
# Linux/macOS
ls -la /var/lib/emby/plugins/EmbyBeautifyPlugin/
# 应该看到：
# EmbyBeautifyPlugin.dll
# plugin.xml

# Windows
dir "C:\ProgramData\Emby-Server\plugins\EmbyBeautifyPlugin"
```

#### 步骤2：检查文件权限
```bash
# Linux
sudo chown -R emby:emby /var/lib/emby/plugins/EmbyBeautifyPlugin/
sudo chmod -R 755 /var/lib/emby/plugins/EmbyBeautifyPlugin/

# Docker
docker exec emby-container chown -R abc:abc /config/plugins/EmbyBeautifyPlugin
docker exec emby-container chmod -R 755 /config/plugins/EmbyBeautifyPlugin
```

#### 步骤3：验证 Emby Server 版本
```bash
# 检查 Emby Server 版本
emby-server --version
# 或在 Web 界面查看：控制台 → 关于
```

支持的版本：4.7.0.0 - 4.8.0.0

#### 步骤4：重新安装插件
```bash
# 删除现有文件
rm -rf /var/lib/emby/plugins/EmbyBeautifyPlugin/

# 重新解压和安装
tar -xzf EmbyBeautifyPlugin-v1.0.0.0.tar.gz
sudo ./install-plugin.sh
```

### 问题2：插件显示为"不兼容"

**症状**：
- 插件出现在列表中但标记为"不兼容"
- 无法启用插件

**解决方案**：

#### 检查依赖项
```bash
# 检查 .NET Runtime 版本
dotnet --list-runtimes
# 应该包含 Microsoft.NETCore.App 6.0.x
```

#### 更新 Emby Server
1. 备份 Emby 配置
2. 升级到支持的版本
3. 重启服务器
4. 重新安装插件

### 问题3：Docker 环境安装失败

**症状**：
- Docker 容器中插件安装失败
- 权限被拒绝错误

**解决方案**：

#### 检查容器配置
```bash
# 检查容器挂载点
docker inspect emby-container | grep -A 10 "Mounts"

# 检查容器用户
docker exec emby-container id
```

#### 修复权限问题
```bash
# 获取容器的 PUID 和 PGID
PUID=$(docker exec emby-container id -u abc)
PGID=$(docker exec emby-container id -g abc)

# 在主机上设置正确权限
sudo chown -R $PUID:$PGID /path/to/emby/config/plugins/EmbyBeautifyPlugin
```

## 主题问题

### 问题1：主题没有生效

**症状**：
- 应用主题后界面没有变化
- 样式没有更新

**解决方案**：

#### 步骤1：清除浏览器缓存
```javascript
// 在浏览器控制台执行
location.reload(true); // 强制刷新

// 或使用快捷键
// Windows/Linux: Ctrl + F5
// macOS: Cmd + Shift + R
```

#### 步骤2：检查插件状态
1. 进入 Emby 控制台 → 插件
2. 确认插件状态为"活动"
3. 检查插件设置是否正确保存

#### 步骤3：检查浏览器控制台
1. 按 F12 打开开发者工具
2. 查看 Console 标签页
3. 寻找相关错误信息

常见错误信息：
```
Failed to load resource: net::ERR_BLOCKED_BY_CLIENT
Content Security Policy directive violated
Uncaught TypeError: Cannot read property 'style' of null
```

#### 步骤4：禁用浏览器扩展
某些浏览器扩展可能阻止样式加载：
- 广告拦截器
- 隐私保护扩展
- 脚本拦截器

### 问题2：主题部分生效

**症状**：
- 某些元素应用了主题样式
- 其他元素保持默认样式

**解决方案**：

#### 检查 CSS 选择器优先级
```css
/* 可能需要增加选择器权重 */
.emby-container .card {
    background-color: var(--card-background) !important;
}
```

#### 检查样式加载顺序
1. 确保插件样式在默认样式之后加载
2. 检查是否有其他插件冲突

### 问题3：自定义 CSS 不工作

**症状**：
- 自定义 CSS 代码没有效果
- 语法错误提示

**解决方案**：

#### 验证 CSS 语法
```css
/* 错误示例 */
.card {
    background-color: #ffffff
    border-radius: 8px;  /* 缺少分号 */
}

/* 正确示例 */
.card {
    background-color: #ffffff;
    border-radius: 8px;
}
```

#### 使用浏览器开发工具测试
1. 在 Elements 标签页中找到目标元素
2. 在 Styles 面板中添加样式
3. 确认样式生效后复制到插件设置

## 性能问题

### 问题1：界面响应缓慢

**症状**：
- 页面加载时间长
- 动画卡顿
- 滚动不流畅

**解决方案**：

#### 步骤1：检查硬件加速
```css
/* 启用硬件加速 */
.animated-element {
    will-change: transform;
    transform: translateZ(0);
}
```

#### 步骤2：优化动画
```css
/* 使用 transform 而不是改变布局属性 */
.element {
    /* 好的做法 */
    transform: translateX(100px);
    
    /* 避免的做法 */
    /* left: 100px; */
}
```

#### 步骤3：减少复杂选择器
```css
/* 避免过于复杂的选择器 */
/* .container .card .header .title .text { } */

/* 使用更简单的选择器 */
.card-title { }
```

### 问题2：内存使用过高

**症状**：
- 浏览器内存占用持续增长
- 页面变得不响应

**解决方案**：

#### 检查内存泄漏
1. 打开浏览器任务管理器 (Shift + Esc)
2. 监控内存使用情况
3. 刷新页面观察内存是否释放

#### 优化图片和资源
```css
/* 使用 CSS 而不是大图片 */
.gradient-background {
    background: linear-gradient(45deg, #667eea, #764ba2);
    /* 而不是使用大的渐变图片 */
}
```

## 兼容性问题

### 问题1：移动端显示异常

**症状**：
- 移动设备上布局错乱
- 按钮太小难以点击
- 文字过小难以阅读

**解决方案**：

#### 检查视口设置
```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```

#### 优化触摸目标
```css
.button {
    min-height: 44px;  /* iOS 推荐的最小触摸目标 */
    min-width: 44px;
    padding: 12px 16px;
}
```

#### 使用响应式断点
```css
@media (max-width: 768px) {
    .card {
        padding: 12px;
        margin: 8px;
    }
    
    .text {
        font-size: 16px;  /* 移动端最小推荐字体大小 */
    }
}
```

### 问题2：特定浏览器问题

**症状**：
- 在某些浏览器中显示异常
- 功能在特定浏览器中不工作

**解决方案**：

#### Safari 兼容性
```css
/* Safari 需要 -webkit- 前缀 */
.element {
    -webkit-backdrop-filter: blur(10px);
    backdrop-filter: blur(10px);
}
```

#### Internet Explorer 兼容性
```css
/* IE 不支持 CSS 变量，需要提供后备值 */
.element {
    background-color: #667eea;  /* 后备值 */
    background-color: var(--primary-color);
}
```

## 界面显示问题

### 问题1：元素重叠或错位

**症状**：
- 界面元素相互重叠
- 布局错乱

**解决方案**：

#### 检查 z-index 层级
```css
.modal {
    z-index: 1000;
}

.dropdown {
    z-index: 999;
}

.navbar {
    z-index: 100;
}
```

#### 检查定位属性
```css
/* 确保定位元素有明确的位置 */
.positioned-element {
    position: absolute;
    top: 0;
    left: 0;
    /* 或使用 transform 居中 */
    transform: translate(-50%, -50%);
}
```

### 问题2：文字显示异常

**症状**：
- 文字被截断
- 字体无法加载
- 文字颜色对比度不足

**解决方案**：

#### 修复文字截断
```css
.text-container {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    
    /* 或允许换行 */
    word-wrap: break-word;
    overflow-wrap: break-word;
}
```

#### 字体加载失败处理
```css
.text {
    font-family: 
        'CustomFont',           /* 自定义字体 */
        -apple-system,          /* 系统字体后备 */
        BlinkMacSystemFont,
        sans-serif;             /* 通用后备 */
}
```

## 配置问题

### 问题1：设置无法保存

**症状**：
- 点击保存后设置没有生效
- 刷新页面后设置丢失

**解决方案**：

#### 检查权限
```bash
# 检查配置文件权限
ls -la /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml

# 修复权限
sudo chown emby:emby /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml
sudo chmod 644 /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml
```

#### 检查磁盘空间
```bash
# 检查可用空间
df -h /var/lib/emby/

# 清理日志文件（如果需要）
sudo find /var/lib/emby/logs/ -name "*.log" -mtime +7 -delete
```

### 问题2：配置文件损坏

**症状**：
- 插件无法启动
- 设置页面显示错误

**解决方案**：

#### 重置配置
```bash
# 备份现有配置
cp /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml /tmp/backup.xml

# 删除配置文件（将使用默认配置）
rm /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml

# 重启 Emby Server
sudo systemctl restart emby-server
```

## 调试工具

### 浏览器开发者工具

#### Console 标签页
```javascript
// 检查插件是否加载
console.log('EmbyBeautifyPlugin loaded:', window.EmbyBeautifyPlugin);

// 检查 CSS 变量
getComputedStyle(document.documentElement).getPropertyValue('--primary-color');

// 检查元素样式
const element = document.querySelector('.card');
console.log(getComputedStyle(element));
```

#### Network 标签页
- 检查 CSS 文件是否成功加载
- 查看加载时间和文件大小
- 确认没有 404 错误

#### Elements 标签页
- 检查 HTML 结构
- 查看应用的 CSS 样式
- 实时修改样式进行测试

### 插件调试模式

#### 启用调试模式
1. 进入插件设置页面
2. 勾选"启用调试模式"
3. 保存设置并刷新页面

#### 查看调试信息
```javascript
// 在浏览器控制台查看调试日志
EmbyBeautifyPlugin.debug.getLogs();

// 查看性能信息
EmbyBeautifyPlugin.debug.getPerformanceInfo();

// 查看当前配置
EmbyBeautifyPlugin.debug.getCurrentConfig();
```

### 日志分析

#### Emby Server 日志
```bash
# 实时查看日志
tail -f /var/lib/emby/logs/embyserver.txt

# 搜索插件相关日志
grep -i "beautify" /var/lib/emby/logs/embyserver.txt

# 查看错误日志
grep -i "error" /var/lib/emby/logs/embyserver.txt | grep -i "beautify"
```

#### 常见日志信息
```
[INFO] EmbyBeautifyPlugin: Plugin initialized successfully
[WARN] EmbyBeautifyPlugin: Theme file not found, using default
[ERROR] EmbyBeautifyPlugin: Failed to load configuration
```

## 获取帮助

### 自助诊断

#### 系统信息收集
```bash
#!/bin/bash
echo "=== Emby Beautify Plugin 诊断信息 ==="
echo "日期: $(date)"
echo "系统: $(uname -a)"
echo "Emby 版本: $(emby-server --version 2>/dev/null || echo '未知')"
echo "插件版本: $(cat /var/lib/emby/plugins/EmbyBeautifyPlugin/VERSION.json 2>/dev/null | grep version || echo '未知')"
echo ""
echo "=== 文件检查 ==="
ls -la /var/lib/emby/plugins/EmbyBeautifyPlugin/
echo ""
echo "=== 权限检查 ==="
ls -la /var/lib/emby/config/plugins/EmbyBeautifyPlugin.xml
echo ""
echo "=== 最近日志 ==="
tail -n 20 /var/lib/emby/logs/embyserver.txt | grep -i beautify
```

#### 浏览器信息收集
```javascript
// 在浏览器控制台执行
const diagnostics = {
    userAgent: navigator.userAgent,
    viewport: {
        width: window.innerWidth,
        height: window.innerHeight
    },
    screen: {
        width: screen.width,
        height: screen.height
    },
    pluginLoaded: !!window.EmbyBeautifyPlugin,
    cssVariables: {
        primaryColor: getComputedStyle(document.documentElement).getPropertyValue('--primary-color'),
        backgroundColor: getComputedStyle(document.documentElement).getPropertyValue('--background-color')
    },
    errors: console.errors || []
};

console.log('诊断信息:', JSON.stringify(diagnostics, null, 2));
```

### 社区支持

#### GitHub Issues
提交问题时请包含：
1. 详细的问题描述
2. 重现步骤
3. 系统信息
4. 错误日志
5. 截图（如果适用）

#### 论坛求助
- [Emby 官方论坛](https://emby.media/community/)
- [Reddit r/emby](https://reddit.com/r/emby)
- 插件专用讨论区

#### 实时聊天
- Discord 服务器
- Telegram 群组
- QQ 群

### 专业支持

#### 付费支持
如果您需要专业的技术支持，可以考虑：
- 一对一技术咨询
- 自定义主题开发
- 企业级部署支持

#### 开发者联系
- Email: support@emby-beautify.com
- GitHub: @emby-beautify
- Twitter: @EmbyBeautify

---

如果本指南没有解决您的问题，请不要犹豫联系我们的支持团队。我们致力于为每位用户提供最佳的使用体验。