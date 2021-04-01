Primary repository on [GitHub](https://github.com/CyberBotX/fsb5_split).

# fsb5_split

This is a tool to split a multi-stream FSB5 into multiple single-stream FSB5s. The tool was inspired in part by a
similar tool by hcs that was designed for FSB3 and FSB4.

If you have an FSB3 or FSB4 that needs splitting, you can get *fsbii* from [hcs64.com](http://hcs64.com/vgm_ripping.html).

## Compiling

You will need to compile the tool with Visual Studio, at least version 2019 (the earliest with .NET 5 support) or on
the command line via .NET 5's `dotnet` command.

Additionally, you can include support for outputting an unlooped .wav file for each new FSB5 through use of the
[FMOD Studio API](http://www.fmod.org/download/#StudioAPI) by including `FMOD` in your *Conditional compiler symbols*
under the Build settings of the project's properties. (Alternatively, just use the *Debug with FMOD* or
*Release with FMOD* build configurations.)

## Running

The tool is a C# console tool. You will need to run it from the command line. Syntax is:

```
fsb5_split.exe <fsb file> [<output directory>]
```

If the output directory is not given, the new FSB files will be written to the same directory as the given FSB file.

## Contact

If you have any questions, comments or concerns about fsb5_split:
* Contact me by email: cyberbotx@cyberbotx.com (please include fsb5_split in the subject line)
* Contact me on IRC: On the server jenna.cyberbotx.com (I will usually be under CyberBotX)
* Contact me on Discord: CyberBotX#8477
* Submit an issue via [GitHub's issue tracker](https://github.com/CyberBotX/fsb5_split/issues)


## License

fsb5_split is licensed as follows:

```
The MIT License (MIT)

Copyright (c) 2015-2021 Naram Qashat

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
