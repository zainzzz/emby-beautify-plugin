# Emby Beautify Plugin

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/6.0)
[![Emby](https://img.shields.io/badge/Emby-4.7.0+-green.svg)](https://emby.media/)

A powerful Emby Server plugin that enhances the web interface with custom themes, modern UI improvements, and responsive design.

## ✨ Features

### 🎨 Theme System
- **Multiple Built-in Themes**: Light, Dark, and Modern themes
- **Full Customization**: Colors, fonts, layouts, and animations
- **Real-time Preview**: See changes instantly as you customize
- **Import/Export**: Share themes with the community

### 📱 Responsive Design
- **Mobile Optimized**: Perfect experience on phones and tablets
- **Adaptive Layouts**: Automatically adjusts to screen size
- **Touch Friendly**: Large buttons and intuitive gestures

### ⚡ Performance
- **Smart Caching**: Intelligent style caching for fast loading
- **Lazy Loading**: Load styles only when needed
- **Hardware Acceleration**: Smooth animations with GPU support
- **Memory Optimization**: Efficient resource management

### 🔧 Advanced Features
- **CSS Injection**: Custom CSS support for power users
- **Animation Control**: Configurable transitions and effects
- **Debug Tools**: Built-in debugging and performance monitoring
- **Error Recovery**: Graceful fallback mechanisms

## 🚀 Quick Start

### System Requirements
- **Emby Server**: 4.7.0 - 4.8.0
- **.NET Runtime**: 6.0 or higher
- **Browsers**: Chrome, Firefox, Safari, Edge

### Installation

#### Option 1: Automatic Installation (Recommended)

**Windows:**
```powershell
# Download the latest release
Invoke-WebRequest -Uri "https://github.com/zainzzz/emby-beautify-plugin/releases/latest/download/EmbyBeautifyPlugin-v1.0.0.0.zip" -OutFile "EmbyBeautifyPlugin.zip"

# Extract and install
Expand-Archive -Path "EmbyBeautifyPlugin.zip" -DestinationPath "EmbyBeautifyPlugin"
cd EmbyBeautifyPlugin
.\install-plugin.ps1
```

**Linux/macOS:**
```bash
# Download the latest release
wget https://github.com/zainzzz/emby-beautify-plugin/releases/latest/download/EmbyBeautifyPlugin-v1.0.0.0.tar.gz

# Extract and install
tar -xzf EmbyBeautifyPlugin-v1.0.0.0.tar.gz
cd package
sudo ./install-plugin.sh
```

**Docker:**
```bash
# For existing Emby containers
./deploy-to-existing-emby.sh your-emby-container-name
```

#### Option 2: Manual Installation

1. Download the plugin package from [Releases](https://github.com/zainzzz/emby-beautify-plugin/releases)
2. Extract to your Emby plugins directory:
   - **Windows**: `%ProgramData%\Emby-Server\plugins\EmbyBeautifyPlugin\`
   - **Linux**: `/var/lib/emby/plugins/EmbyBeautifyPlugin/`
   - **Docker**: `/config/plugins/EmbyBeautifyPlugin/`
3. Restart Emby Server
4. Enable the plugin in Emby's plugin management page

### First Use

1. Open Emby Web Interface
2. Go to **Dashboard** → **Plugins** → **Emby Beautify Plugin**
3. Click **Settings** to configure
4. Choose a theme and customize to your liking
5. Save and enjoy your beautiful new interface!

## 📖 Documentation

- **[Installation Guide](EmbyBeautifyPlugin/docs/INSTALLATION.md)** - Detailed installation instructions
- **[User Manual](EmbyBeautifyPlugin/docs/USER_GUIDE.md)** - Complete feature guide
- **[Theme Customization](EmbyBeautifyPlugin/docs/THEME_CUSTOMIZATION.md)** - Create custom themes
- **[Troubleshooting](EmbyBeautifyPlugin/docs/TROUBLESHOOTING.md)** - Common issues and solutions
- **[Build Scripts](EmbyBeautifyPlugin/scripts/README.md)** - Development and deployment tools

## 🎨 Screenshots

### Light Theme
![Light Theme](screenshots/light-theme.png)

### Dark Theme
![Dark Theme](screenshots/dark-theme.png)

### Mobile View
![Mobile View](screenshots/mobile-view.png)

### Theme Customization
![Theme Customization](screenshots/customization.png)

## 🛠️ Development

### Prerequisites
- .NET 6.0 SDK
- Git
- Docker (optional, for testing)

### Building from Source

```bash
# Clone the repository
git clone https://github.com/zainzzz/emby-beautify-plugin.git
cd emby-beautify-plugin

# Build the plugin
cd EmbyBeautifyPlugin
dotnet restore
dotnet build -c Release

# Run tests
cd ../EmbyBeautifyPlugin.Tests
dotnet test

# Create distribution package
cd ../EmbyBeautifyPlugin
./scripts/build-package.sh  # Linux/macOS
# or
.\scripts\build-package.ps1  # Windows
```

### Development Workflow

1. **Make Changes**: Edit source code
2. **Run Tests**: `dotnet test`
3. **Build Plugin**: `dotnet build -c Release`
4. **Test Deploy**: `./scripts/deploy-to-existing-emby.sh`
5. **Create Package**: `./scripts/build-package.sh`

### Project Structure

```
EmbyBeautifyPlugin/
├── Abstracts/           # Abstract base classes
├── Controllers/         # Web API controllers
├── Exceptions/          # Custom exceptions
├── Extensions/          # Extension methods
├── Interfaces/          # Core interfaces
├── Models/              # Data models
├── Services/            # Business logic
├── Views/               # HTML templates and JS
├── docs/                # Documentation
├── scripts/             # Build and deployment scripts
├── Plugin.cs            # Main plugin entry point
└── plugin.xml           # Plugin manifest

EmbyBeautifyPlugin.Tests/
├── *Tests.cs            # Unit tests
└── TestConfiguration.cs # Test helpers
```

## 🤝 Contributing

We welcome contributions! Here's how you can help:

### Ways to Contribute
- 🐛 **Report Bugs**: [Create an issue](https://github.com/zainzzz/emby-beautify-plugin/issues/new)
- 💡 **Suggest Features**: [Start a discussion](https://github.com/zainzzz/emby-beautify-plugin/discussions)
- 🎨 **Share Themes**: Submit your custom themes
- 📝 **Improve Docs**: Help make documentation better
- 💻 **Code**: Submit pull requests

### Development Guidelines
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes and add tests
4. Ensure all tests pass: `dotnet test`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

### Code Style
- Follow C# coding conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Use meaningful commit messages

## 📋 Roadmap

### v1.1.0 (Next Release)
- [ ] Theme Store integration
- [ ] Visual theme editor
- [ ] More built-in themes
- [ ] Performance improvements

### v1.2.0 (Future)
- [ ] Plugin ecosystem support
- [ ] Multi-language support
- [ ] Advanced animation controls
- [ ] Usage analytics

### v2.0.0 (Long-term)
- [ ] Complete UI overhaul
- [ ] Mobile app integration
- [ ] Cloud theme sync
- [ ] AI-powered theme suggestions

## 🆘 Support

### Getting Help
1. **Documentation**: Check our [comprehensive docs](EmbyBeautifyPlugin/docs/)
2. **Issues**: [Search existing issues](https://github.com/zainzzz/emby-beautify-plugin/issues)
3. **Discussions**: [Community discussions](https://github.com/zainzzz/emby-beautify-plugin/discussions)
4. **Emby Forum**: [Official Emby community](https://emby.media/community/)

### Reporting Issues
When reporting issues, please include:
- Emby Server version
- Operating system and browser
- Plugin version
- Steps to reproduce
- Error logs (if any)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Emby Team**: For creating an amazing media server
- **Contributors**: Everyone who has contributed to this project
- **Community**: Users who provide feedback and support
- **Open Source**: Built with love using open source technologies

## 📊 Stats

![GitHub stars](https://img.shields.io/github/stars/zainzzz/emby-beautify-plugin?style=social)
![GitHub forks](https://img.shields.io/github/forks/zainzzz/emby-beautify-plugin?style=social)
![GitHub issues](https://img.shields.io/github/issues/zainzzz/emby-beautify-plugin)
![GitHub pull requests](https://img.shields.io/github/issues-pr/zainzzz/emby-beautify-plugin)

---

**Made with ❤️ for the Emby community**

If you find this plugin useful, please consider giving it a ⭐ star and sharing it with other Emby users!