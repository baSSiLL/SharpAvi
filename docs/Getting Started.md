# Getting Started

There are a few namespaces in the library:
{code:c#}
// Contains common types for AVI format like FourCC
using SharpAvi;
// Contains types used for writing like AviWriter
using SharpAvi.Output;
// Contains types related to encoding like Mpeg4VideoEncoderVcm
using SharpAvi.Codecs;
{code:c#}

Writing an AVI file starts with creating an instance of the [AviWriter](https://sharpavi.codeplex.com/SourceControl/latest#SharpAvi/Output/AviWriter.cs) class, specifying the path of the target file. This effectively creates the file. Usually, you will also specify a frame rate value:
{code:c#}
var writer = new AviWriter("test.avi")
{
    FramesPerSecond = 30,
    // Emitting AVI v1 index in addition to OpenDML index (AVI v2)
    // improves compatibility with some software, including 
    // standard Windows programs like Media Player and File Explorer
    EmitIndex1 = true
};
{code:c#}

Next, you define the streams of the file. Often, all you need is a single video stream:
{code:c#}
// returns IAviVideoStream
var stream = writer.AddVideoStream();
{code:c#}

You get an object implementing the [IAviVideoStream](https://sharpavi.codeplex.com/SourceControl/latest#SharpAvi/Output/IAviVideoStream.cs) interface. The interface contains several properties, the most important of which are **Width** and **Height**, defining the dimensions of a frame, and **Codec**, a FOURCC value defining the format of the stream's data. Let's use uncompressed stream with 32 bits per pixel.
{code:c#}
// set standard VGA resolution
stream.Width = 640;
stream.Height = 480;
// class SharpAvi.KnownFourCCs.Codecs contains FOURCCs for several well-known codecs
// Uncompressed is the default value, just set it for clarity
stream.Codec = KnownFourCCs.Codecs.Uncompressed;
// Uncompressed format requires to also specify bits per pixel
stream.BitsPerPixel = BitsPerPixel.Bpp32;
{code:c#}

Now you are ready to write the frames. AVI expects uncompressed data in format of standard Windows DIB, that is bottom-up bitmap of the specified bit-depth. For each frame, put its data in byte array and call **IAviVideoStream.WriteFrame()**.
{code:c#}
var frameData = new byte[stream.Width * stream.Height * 4](stream.Width-_-stream.Height-_-4);
while (/* !finished */)
{
    // fill frameData with image
    ...

    // write data to a frame
    stream.WriteFrame(true, // is key frame? (many codecs use concept of key frames, for others - all frames are keys)
                      frameData, // array with frame data
                      0, // starting index in the array
                      frameData.Length // length of the data
    );
}
{code:c#}

You don't need to specify the number of frames beforehand, just write as many frames as you need. When done, just call the **AviWriter.Close()**, and file is ready to be played. That's it!
{code:c#}
writer.Close();
{code:c#}

You can easily add one or more audio streams into the mix. See [Working with Audio Streams](Working-with-Audio-Streams).

Writing uncompressed data is simple, yet leads to enormous file sizes. Though **SharpAvi** makes big AVI files readable by the most players (supporting OpenDML), it's often worth compressing the data.
If you already have pre-compressed frames (for example, when copying from other video file) just set the properties of the stream properly to describe the compression used. Then you can simply write your compressed data to the stream.

However, usually you have uncompressed data which need to be compressed somehow. **SharpAvi** provides this ability through the concept of _encoders_. Follow to [Using Video Encoders](Using-Video-Encoders).