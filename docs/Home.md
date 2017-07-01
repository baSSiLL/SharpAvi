A simple .NET library for creating video files in AVI format.

If you want to render some video sequence, and do not want to touch DirectShow or depend on command-line utilities - **SharpAvi** may be what you need.
Writing uncompressed AVI does not require any external dependencies, it's pure .NET code. Files are produced in compliance with the [OpenDML extensions](http://www.jmcgowan.com/avitech.html#OpenDML) which allow (nearly) unlimited file size (no 2GB limit).

Video is created by supplying individual in-memory bitmaps (byte arrays) and audio samples. Included are implementations of encoders for Motion JPEG (requires WPF), MPEG-4 (requires external VfW codecs) and MP3 (requires LAME binaries). Output format is always AVI, regardless of a specific codec used. Asynchronous writing/encoding is supported.

To get started, jump to the [DOCUMENTATION](https://sharpavi.codeplex.com/documentation) tab.

There is a [mirror repository on GitHub](https://github.com/baSSiLL/SharpAvi), if you feel more comfortable there.

Project binaries can also be downloaded as [NuGet package](https://www.nuget.org/packages/SharpAvi/).

