# fsb5_split

This is a tool to split a multi-stream FSB5 into multiple single-stream FSB5s. The tool was inspired in part by a similar tool by hcs that was designed for FSB3 and FSB4.

If you have an FSB3 or FSB4 that needs splitting, you can get *fsbii* from [hcs64.com](http://hcs64.com/vgm_ripping.html).

## Compiling

You will need to compile the tool with Visual Studio, at least version 2010 (the earliest with .NET 4 support as well as the earlier that supports the code's Solution/Project files).

Additionally, you can include support for outputting an unlooped .wav file for each new FSB5 through use of the [FMOD Studio API](http://www.fmod.org/download/#StudioAPI) by including *FMOD* in your *Conditional compiler symbols* under the Build settings of the project's properties.

**NOTE**: Due to limitations in the MSBuild system, I could not include a way to set the directory in the same way. If you decide to use the FMOD support, you must manually change the 2 Compile paths and the PostBuildEvent in the .csproj file to point to the proper location where you installed the FMDO Studio API.

## Running

The tool is a C# console tool. You will need to run it from the command line. Syntax is:

```
fsb5_split.exe <fsb file> [<output directory>]
```

If the output directory is not given, the new FSB files will be written to the same directory as the given FSB file.

## Contact

If you have any questions, comments, or concerns, you may email me at cyberbotx@cyberbotx.com. Additionally, if you spot any issues, please file an issue on GitHub.