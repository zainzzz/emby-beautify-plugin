# Emby Beautify Plugin API 文档

本文档描述了 Emby Beautify Plugin 提供的 Web API 接口。

## 主题管理 API

### 获取所有可用主题
- **端点**: `GET /emby-beautify/themes`
- **描述**: 获取所有可用主题的列表
- **响应**: 
```json
{
  "themes": [
    {
      "id": "default-light",
      "name": "默认浅色主题",
      "description": "默认的浅色主题",
      "version": "1.0.0",
      "author": "Emby Beautify Plugin",
      "colors": { ... },
      "typography": { ... },
      "layout": { ... }
    }
  ],
  "count": 1
}
```

### 获取指定主题
- **端点**: `GET /emby-beautify/themes/{themeId}`
- **描述**: 获取指定ID的主题详情
- **参数**: 
  - `themeId`: 主题ID
- **响应**: 主题对象

### 获取当前活动主题
- **端点**: `GET /emby-beautify/themes/active`
- **描述**: 获取当前活动的主题
- **响应**: 当前活动主题对象

### 设置活动主题
- **端点**: `POST /emby-beautify/themes/active`
- **描述**: 设置指定主题为活动主题
- **请求体**:
```json
{
  "themeId": "default-dark"
}
```
- **响应**:
```json
{
  "success": true,
  "message": "主题已成功切换到 '默认深色主题'",
  "activeTheme": { ... }
}
```

### 获取主题CSS
- **端点**: `GET /emby-beautify/themes/{themeId}/css`
- **描述**: 获取指定主题生成的CSS样式
- **参数**: 
  - `themeId`: 主题ID
- **响应**:
```json
{
  "themeId": "default-light",
  "css": "body { background-color: #ffffff; ... }",
  "generatedAt": "2024-01-01T12:00:00Z"
}
```

## 配置管理 API

### 获取插件配置
- **端点**: `GET /emby-beautify/configuration`
- **描述**: 获取当前插件配置
- **响应**:
```json
{
  "configuration": {
    "activeThemeId": "default-light",
    "enableAnimations": true,
    "enableCustomFonts": true,
    "animationDuration": 300,
    "responsiveSettings": { ... },
    "customSettings": { ... }
  },
  "loadedAt": "2024-01-01T12:00:00Z"
}
```

### 更新插件配置
- **端点**: `POST /emby-beautify/configuration`
- **描述**: 更新插件配置
- **请求体**:
```json
{
  "configuration": {
    "activeThemeId": "default-dark",
    "enableAnimations": true,
    "animationDuration": 500,
    ...
  }
}
```
- **响应**:
```json
{
  "success": true,
  "message": "配置已成功更新",
  "configuration": { ... },
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

### 验证插件配置
- **端点**: `POST /emby-beautify/configuration/validate`
- **描述**: 验证插件配置是否有效
- **请求体**:
```json
{
  "configuration": { ... }
}
```
- **响应**:
```json
{
  "isValid": true,
  "message": "配置验证通过",
  "validationErrors": [],
  "validatedAt": "2024-01-01T12:00:00Z"
}
```

## 错误处理

所有API端点都遵循统一的错误处理模式：

- **成功响应**: HTTP 200 OK
- **客户端错误**: HTTP 400 Bad Request
- **服务器错误**: HTTP 500 Internal Server Error

错误响应格式：
```json
{
  "success": false,
  "message": "错误描述",
  "error": "详细错误信息"
}
```

## 使用示例

### JavaScript 示例

```javascript
// 获取所有主题
fetch('/emby-beautify/themes')
  .then(response => response.json())
  .then(data => console.log('可用主题:', data.themes));

// 切换主题
fetch('/emby-beautify/themes/active', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    themeId: 'default-dark'
  })
})
.then(response => response.json())
.then(data => console.log('主题切换结果:', data));

// 更新配置
fetch('/emby-beautify/configuration', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    configuration: {
      activeThemeId: 'custom-theme',
      enableAnimations: false,
      animationDuration: 200
    }
  })
})
.then(response => response.json())
.then(data => console.log('配置更新结果:', data));
```

### cURL 示例

```bash
# 获取所有主题
curl -X GET "http://localhost:8096/emby-beautify/themes"

# 设置活动主题
curl -X POST "http://localhost:8096/emby-beautify/themes/active" \
  -H "Content-Type: application/json" \
  -d '{"themeId": "default-dark"}'

# 获取配置
curl -X GET "http://localhost:8096/emby-beautify/configuration"

# 更新配置
curl -X POST "http://localhost:8096/emby-beautify/configuration" \
  -H "Content-Type: application/json" \
  -d '{"configuration": {"activeThemeId": "custom-theme", "enableAnimations": true}}'
```

## 注意事项

1. 所有API端点都需要适当的Emby服务器权限
2. 主题ID必须是已注册的有效主题
3. 配置更新会立即生效，但可能需要刷新页面才能看到视觉变化
4. API响应时间取决于主题复杂度和服务器性能
5. 建议在生产环境中启用适当的缓存机制