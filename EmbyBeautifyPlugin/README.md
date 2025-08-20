# Emby Beautify Plugin

A plugin to beautify Emby Server interface with custom themes and enhanced UI.

## Project Structure

```
EmbyBeautifyPlugin/
├── Interfaces/                 # Core interfaces
│   ├── IEmbyBeautifyPlugin.cs  # Main plugin interface
│   ├── IThemeManager.cs        # Theme management interface
│   ├── IStyleInjector.cs       # Style injection interface
│   └── IConfigurationManager.cs # Configuration management interface
├── Models/                     # Data models
│   ├── Theme.cs               # Theme model
│   ├── ThemeColors.cs         # Theme colors model
│   ├── ThemeTypography.cs     # Typography settings
│   ├── ThemeLayout.cs         # Layout settings
│   ├── BeautifyConfig.cs      # Main configuration model
│   ├── ResponsiveSettings.cs  # Responsive configuration
│   └── BreakpointSettings.cs  # Breakpoint settings
├── Abstracts/                 # Abstract base classes
│   ├── BaseThemeManager.cs    # Base theme manager
│   ├── BaseStyleInjector.cs   # Base style injector
│   └── BaseConfigurationManager.cs # Base configuration manager
├── Properties/                # Assembly properties
│   └── AssemblyInfo.cs       # Assembly information
├── Plugin.cs                 # Main plugin entry point
├── EmbyBeautifyPlugin.csproj # Project file
├── plugin.xml               # Plugin manifest
└── README.md               # This file
```

## Features

- Custom theme system with support for colors, typography, and layout
- Dynamic style injection into Emby web client
- Configuration management with validation
- Responsive design support for different screen sizes
- Modular architecture with dependency injection support

## Requirements

- .NET 6.0 or later
- Emby Server 4.7.0 or later
- Compatible with Emby plugin framework

## Installation

1. Build the plugin using `dotnet build`
2. Copy the built DLL and plugin.xml to your Emby plugins directory
3. Restart Emby Server
4. Configure the plugin through the Emby admin interface

## Development

This project follows the interface-based architecture pattern with abstract base classes for extensibility. Each component is designed to be testable and maintainable.

### Key Components

- **Plugin.cs**: Main entry point implementing IEmbyBeautifyPlugin and IServerEntryPoint
- **Theme System**: Manages theme loading, switching, and CSS generation
- **Style Injection**: Handles dynamic CSS injection into web client
- **Configuration**: Manages plugin settings and user preferences

### Testing

The project includes comprehensive unit tests and integration tests:

```bash
# Run tests using .NET CLI
cd EmbyBeautifyPlugin.Tests
dotnet test --verbosity normal

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Or use the provided scripts
./scripts/run-tests.sh          # Linux/macOS
./scripts/run-tests.ps1         # Windows PowerShell

# Run tests with Docker (if .NET SDK not installed locally)
docker run --rm -v "$(pwd):/src" -w /src mcr.microsoft.com/dotnet/sdk:6.0 \
  sh -c "cd EmbyBeautifyPlugin.Tests && dotnet test --verbosity normal"
```

### Test Structure

- **PluginTests.cs**: Unit tests for the main Plugin class
- **PluginIntegrationTests.cs**: Integration tests with mocked dependencies
- **TestConfiguration.cs**: Test data and helper methods

### Key Features Tested

- ✅ Plugin initialization and lifecycle management
- ✅ Configuration loading and validation
- ✅ Theme system integration
- ✅ Error handling and graceful degradation
- ✅ Dependency injection compatibility
- ✅ Emby Server integration points