**SharpAvi** is a simple .NET library for creating video files in AVI format.

If you want to render some video sequence, and do not want to touch DirectShow or depend on command-line utilities - **SharpAvi** may be what you need.
Writing uncompressed AVI does not require any external dependencies, it's pure .NET code. Files are produced in compliance with the [OpenDML extensions](http://www.jmcgowan.com/avitech.html#OpenDML) which allow (nearly) unlimited file size (no 2GB limit).

Video is created by supplying individual in-memory bitmaps (byte arrays) and audio samples. Included are implementations of encoders for Motion JPEG (requires WPF), MPEG-4 (requires external VfW codecs) and MP3 (requires LAME binaries). Output format is always AVI, regardless of a specific codec used. Asynchronous writing/encoding is supported.

To get started, jump to the [project's wiki](https://github.com/baSSiLL/SharpAvi/wiki/Home).

Project binaries can also be downloaded as [NuGet package](https://www.nuget.org/packages/SharpAvi/).

***

A bit of history. This project started on [CodePlex](https://sharpavi.codeplex.com/) in 2013. Then the repository was mirrored on GitHub for some time, until the project moved on GitHub completely in the middle of 2017.
