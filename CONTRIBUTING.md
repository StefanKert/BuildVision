# Contributing to BuildVision

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

* Try to stick to the existing coding style (some styles are defined in StyleCop [settings](https://github.com/nagits/BuildVision/blob/master/Settings.StyleCop)).
* Give a short description in the pull request what you're doing and why.
* When you send a pull request, please send it to the ```devel``` branch.

## Building and debugging

1. Install Visual Studio SDK ([download SDK for VS2013](http://www.microsoft.com/en-us/download/details.aspx?id=40758))
2. Open Solution → Project Properties → Debug:
    * In "Start Action" choose "Start external program" and specify a path to the devenv.exe (eg for VS2013: ```C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.exe```).
    * In "Start Options" specify the following command line arguments: ```/rootsuffix Exp /log```.
        * ```/rootsuffix Exp``` to start experimental instance of Visual Studio.
        * ```/log``` to enable BuildVision logging into [ActivityLog.xml](https://msdn.microsoft.com/en-us/library/ms241272.aspx).

## Logging

Most of the errors in BuildVision, and also some warnings and messages are logged.

* For DEBUG mode:
    * See [ActivityLog.xml](https://msdn.microsoft.com/en-us/library/ms241272.aspx)
    * See ```%appdata%\BuildVision\log\*.svclog```
* For RELEASE mode:
    * Start Visual Studio with ```/log``` switch. See [ActivityLog.xml](https://msdn.microsoft.com/en-us/library/ms241272.aspx)

If Visual Studio has been crashed, you can also see the Windows Event Log (Win+R, ```eventvwr /c:Application```).
