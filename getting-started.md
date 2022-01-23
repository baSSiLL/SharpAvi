# Getting Started

There are a few namespaces in the library:
```cs
// Contains common types for AVI format like FourCC
using SharpAvi;
// Contains types used for writing like AviWriter
using SharpAvi.Output;
// Contains types related to encoding like Mpeg4VideoEncoderVcm
using SharpAvi.Codecs;
```

Writing an AVI file starts with creating an instance of the `AviWriter` class, specifying the path of the target file. This effectively creates the file. Usually, you will also specify a frame rate value:
```cs
var writer = new AviWriter("test.avi")
{
    FramesPerSecond = 30,
    // Emitting the AVI v1 index in addition to the OpenDML index (AVI v2)
    // improves compatibility with some software, including 
    // standard Windows programs like Media Player and File Explorer
    EmitIndex1 = true
};
```

Next, you define the streams of the file. Often, all you need is a single video stream:
```cs
// returns IAviVideoStream
var stream = writer.AddVideoStream();
```

You get an object implementing the `IAviVideoStream` interface. The interface contains several properties, the most important of which are `Width` and `Height`, defining the dimensions of a frame, and `Codec`, a FOURCC value defining the format of the stream's data. Let's use uncompressed stream with 32 bits per pixel.
```cs
// set standard VGA resolution
stream.Width = 640;
stream.Height = 480;
// class SharpAvi.KnownFourCCs.Codecs contains FOURCCs for several well-known codecs
// Uncompressed is the default value, just set it for clarity
stream.Codec = KnownFourCCs.Codecs.Uncompressed;
// Uncompressed format requires to also specify bits per pixel
stream.BitsPerPixel = BitsPerPixel.Bpp32;
```

Now you are ready to write the frames. AVI expects uncompressed data in format of standard Windows DIB, that is bottom-up bitmap of the specified bit-depth. For each frame, put its data in byte array and call `IAviVideoStream.WriteFrame()`.
```cs
var frameData = new byte[stream.Width * stream.Height * 4];
while (/* !finished */)
{
    // fill frameData with image
    ...

    // write data to a frame
    stream.WriteFrame(true, // is a key frame? (many codecs use the concept of key frames, for others - all frames are keys)
                      frameData, // an array with frame data
                      0, // a starting index in the array
                      frameData.Length // a length of the data
    );
}
```

You don't need to specify the number of frames beforehand, just write as many frames as you need. When done, just call the `AviWriter.Close()`, and file is ready to be played. That's it!
```cs
writer.Close();
```

You can easily add one or more audio streams into the mix. See [Working with Audio Streams](working-with-audio-streams.md).

Writing uncompressed data is simple, yet leads to enormous file sizes. Though **SharpAvi** makes big AVI files readable by the most players (supporting OpenDML), it's often worth compressing the data.
If you already have pre-compressed frames (for example, when copying from other video file) just set the properties of the stream properly to describe the compression used. Then you can simply write your compressed data to the stream.

However, usually you have uncompressed data which need to be compressed somehow. **SharpAvi** provides this ability through the concept of _encoders_. Follow to [Using Video Encoders](using-video-encoders.md).
