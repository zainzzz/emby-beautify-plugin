# 多阶段构建 - 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# 复制项目文件
COPY EmbyBeautifyPlugin.csproj .
RUN dotnet restore

# 复制源代码并构建
COPY . .
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# 运行时阶段 - 基于Emby官方镜像
FROM emby/embyserver:latest

# 创建插件目录
RUN mkdir -p /config/plugins/EmbyBeautifyPlugin

# 复制插件文件
COPY --from=build /app/publish/EmbyBeautifyPlugin.dll /config/plugins/EmbyBeautifyPlugin/
COPY --from=build /app/publish/EmbyBeautifyPlugin.pdb /config/plugins/EmbyBeautifyPlugin/
COPY plugin.xml /config/plugins/EmbyBeautifyPlugin/

# 设置权限
RUN chown -R emby:emby /config/plugins/EmbyBeautifyPlugin
RUN chmod -R 755 /config/plugins/EmbyBeautifyPlugin

# 暴露Emby端口
EXPOSE 8096 8920

# 使用Emby的默认启动命令
ENTRYPOINT ["/usr/bin/emby-server"]