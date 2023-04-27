# PIGNUMBERS NGS Damage Parser
A complete overhaul of OverParse, based on Remon-7L's design (forked from Tyrone's parser), and Lapig's compatibility fixes and closed-source plugin. 
This is a standalone log reader and overlay tool. This tool shows damage statistics for you, the user, and your MPA in realtime, and (hopefully soon) restores detailed damage breakdowns for later analysis.

## Credits
- [TyroneSama](https://github.com/TyroneSama/OverParse) for making the original version of OverParse.
- [Lapig](https://github.com/Lapig/PIGNUMBERS) for adding compatibility to NGS

This fork works with the alpha version of the parser plugin, but compatibility with updates is currently unknown.

## MIT License

**Copyright (c)**

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

---

## Developers Section
### Requirements
This project is written in `C#` and is made possible and built with these requirements: 
* Microsoft's [Visual Studio 2022](https://www.visualstudio.com/vs/whatsnew/) IDE.
* Microsoft's [.NET Framework 4.8.0](https://www.microsoft.com/net/download/dotnet-framework-runtime) Runtime.
* The [NHotkey Library](https://github.com/thomaslevesque/NHotkey), for hot key managing.
* The [Fody Costura](https://github.com/Fody/Costura) addon, for compiling DLLs into executable.

###### Files Explained:
1. `MainWindow.xaml` UI is in .xaml format.

2. `MainWindow.xaml.cs` On startup, the settings is loaded from here, and is responsible for starting the iteration.

3. `Log.cs` Connects the installation and process logs, and .csv file reading.

4. `Click.cs` After MainWindow.xaml.cs is ran, this is relevant for processing and partitioning objects into partial classes.

5. `WindowsServices.cs` After HideIfInactive is ran, it calls on Visual Studios’s generated window title.

6. `Details.xaml.cs` Window for when they double-click on options from ListViewItem.

7. `FontDialogEx.xaml.cs` Font Selection Window.


###### Process Flow:
1. `MainWindow.xaml.cs/MainWindow.MainWindow()` loads on startup

2. Calls on `Log.cs / Log.Log() - MainWindow()` and installation connects with PSO2

3. `UpdateForm()` New info updated every 200ms, the screen is updated through looping.

4. `HideIfInactive()` For every second, the active window’s title is obtained through iterations.

5. `CheckForNewLog()` For every second, Confirms and loops if there is no new .csv file.

6. Event handlers are covered by `MainWindow.xaml.cs`, and Click.cs splits it up for easier use. However, in foresight, there’s still room for improvement, as it’s still pretty bad, at least bad in my opinion.
 
