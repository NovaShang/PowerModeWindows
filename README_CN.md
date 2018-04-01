![LOGO](https://github.com/Elenovar/PowerModeWindows/raw/master/images/logo.png)

# POWER MODE WINDOWS

释放你键盘上的洪荒之力吧！本程序让你的每一次键盘敲击变得激情四射，不断累计的连击数让最无聊的文字录入工作也不再无聊！

![Preview](https://github.com/Elenovar/PowerModeWindows/raw/master/images/preview.gif)

本程序的效果类似Atom、Visual Studio Code等流行代码编辑器中的Power Mode插件。但是它可以作用于Windows全局，在大多数软件中使用。

## 安装与使用说明

下载地址：

- [V1.0.0 X64](https://github.com/Elenovar/PowerModeWindows/releases/download/v1.0.0/PowerModeWindows_1.0.0_x64_portable.zip)
- [V1.0.0 X86](https://github.com/Elenovar/PowerModeWindows/releases/download/v1.0.0/PowerModeWindows_1.0.0_x86_portable.zip)

本程序无需安装，只需下载后运行可执行程序即可。如需临时禁用或彻底退出，可在任务栏右侧的通知区域中找到Power Mode Windows的图标，右键点击，在弹出的菜单中进行相应的操作。

## 目前的一些问题

- 本程序的光标特效在部分软件中无法正确显示，若影响使用，可暂时禁用本程序。
- 暂无开机自启动功能，如需自启动可手工添加至Windows启动项。
- 本程序的特效使用GPU渲染，快速输入时会占用大量显卡计算资源，建议玩游戏前禁用本程序。

## 技术与安全说明

本程序灵感来自于VSCode和Atom插件Active-Power-Mode插件，使用C#在.Net平台开发，绘图部分使用WPF的原生图形库绘制，支持GPU加速。

本程序使用 Windows Hook API 创建了键盘钩子来监听全局的键盘输入事件，一些木马程序会使用同样的技术手段监视你的键盘输入，所以某些反病毒软件会有报警信息。本程序无任何键盘记录和网络通讯的行为，若不放心可审查源代码后重新编译使用。

## 作者

Nova & Eleanor

有任何问题请在Github上提交Issue，原因一起改进本程序可直接提交Push Request。