# FASTBuilder

FASTBuild ([website](http://www.fastbuild.org/) or [GitHub repository](https://github.com/fastbuild/fastbuild)) is an amazing distributed building system. It can drastically shorten your build time by utilizing its distributed and cached building mechanisms.

While the name could be a bit confusing, FASTBuild**er** is a GUI program for FASTBuild. It can watch and report FASTBuild's build progress in a friendly timeline interface; track your local worker's activities; and provide a simple setting interface to configure how FASTBuild works.

![Screenshot of FASTBuilder 0.8.0](https://github.com/hillin/FASTBuilder/blob/master/Documentations/Screenshots/FASTBuilder.0.8.0.png)

## Get FASTBuilder
You can get the latest release of FASTBuilder at https://github.com/hillin/FASTBuilder/releases. You'll need [.NET Framework 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=48130) or newer version installed on your Windows system.

## Development
FASTBuilder is developed with .NET and WPF technology.

Third-party libraries:
- [Material Design in Xaml Toolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
- [Costura](https://github.com/Fody/Costura)
- [Caliburn.Micro](http://caliburnmicro.com/)

This project is partially based on [FASTBuild Monitor](https://github.com/yass007/FASTBuildMonitor); especially their work on [defining and implementing the log protocal](https://github.com/fastbuild/fastbuild/issues/127) should really be thanked.
