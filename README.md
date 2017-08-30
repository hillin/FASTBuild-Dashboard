# FASTBuild Dashboard

FASTBuild ([website](http://www.fastbuild.org/) or [GitHub repository](https://github.com/fastbuild/fastbuild)) is an amazing distributed building system. It can drastically shorten your build time by utilizing its distributed and cached building mechanisms.

FASTBuild Dashboard (FBD) is a GUI program for FASTBuild. It can watch and report FASTBuild's build progress in a friendly timeline interface; track your local worker's activities; and provide a simple setting interface to configure how FASTBuild works.

![Screenshot of FBD 0.93.1](https://github.com/hillin/FASTBuilder/blob/master/Documentations/Screenshots/FASTBuild-Dashboard.0.93.1.png)

## Get FASTBuild Dashboard
You can get the latest release of FBD at the [Release Page](https://github.com/hillin/FASTBuild-Dashboard/releases). You'll need [.NET Framework 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=48130) or newer version installed on your Windows system. 

Please note that FBD is still in its early development stage (although already suffice daily use), so everything is prone to change.

## Development
FBD is developed with .NET and WPF technology.

Third-party libraries:
- [Caliburn.Micro](http://caliburnmicro.com/)
- [Caliburn.Micro.Validation](https://github.com/AIexandr/Caliburn.Micro.Validation)
- [Fody Costura](https://github.com/Fody/Costura)
- [Material Design in Xaml Toolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)

This project is partially based on [FASTBuild Monitor](https://github.com/yass007/FASTBuildMonitor); especially their work on [defining and implementing the log protocol](https://github.com/fastbuild/fastbuild/issues/127) should really be thanked.
