# Using Video Encoders

## Encoding a video stream

If you want a stream to perform encoding on supplied frame data, you should use the `AviWriter.AddEncodingVideoStream` method to create the stream. It takes an encoder object as the first parameter, which is used to encode incoming frames. The return value is the same `IAviVideoStream` interface, so basic usage of the stream does not change.
```cs
var encoder = ...
var stream = writer.AddEncodingVideoStream(encoder, width: 640, height: 480);
```

How to get an encoder instance? More on this [later...](#creating-a-video-encoder) And now look how to use an encoding stream.

First, you should not set a codec and bits per pixel for an encoding stream directly, as these are determined by the encoder. Also, a value of the `isKeyFrame` parameter for the `WriteFrame` method is ignored, because the encoder itself defines which frames are keys.

Next, all encoders expect input image data in the specific format. It is the BGR32 _top-down_ - 32 bits per pixel, blue byte first, alpha byte not used, top line goes first. This is the format you can often get from existing images. For example, when locking `System.Drawing.Bitmap` instances. Thus, the size of the data is fixed and is determined by the frame dimensions - `Width * Height * 4` bytes. For this reason, encoding streams also ignore a value of the `length` parameter for the `WriteFrame` method.

So, you simply pass an uncompressed top-down BGR32 frame to an encoding stream, and it cares about encoding:
```cs
// Say, you have a System.Drawing.Bitmap
System.Drawing.Bitmap bitmap;

// Assuming you are in the unsafe context, you can write the bitmap contents right to the stream
var bits = bitmap.LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
encodingStream.WriteFrame(true, new ReadOnlySpan<byte>(bits.Scan0.ToPointer(), bits.Stride * stream.Height));
bitmap.UnlockBits(bits);
```

## Creating a video encoder

Now about a video encoder. This is an object which implements the `IVideoEncoder` interface. This interface provides properties to determine the data format for the stream, and a method for encoding. **SharpAvi** includes several implementations which may be sufficient for many cases. If your case is not included, you are encouraged to write your own implementation of the interface for a preferred codec :)

### Uncompressed

The simplest is the `UncompressedVideoEncoder`. It does no real encoding, just flips image vertically and converts BGR32 data to BGR24 data to reduce the size.
```cs
var encoder = new UncompressedVideoEncoder(stream.Width, stream.Height);
```

### Motion JPEG

Next is the `MJpegWpfVideoEncoder` which does Motion JPEG encoding. It uses `System.Windows.Media.Imaging.JpegBitmapEncoder` under the hood. Hence it's only available in Windows targets. Besides dimensions, you provide a desired quality level to its constructor, ranging from 1 (low quality, small size) to 100 (high quality, large size).
```cs
var encoder = new MJpegWpfVideoEncoder(stream.Width, stream.Height, 70);
```

There is an alternative cross-platform Motion JPEG encoder `MJpegImageSharpVideoEncoder` in a separate package **SharpAvi.ImageSharp**. As you might have guessed, it is based on the [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) library. So it is a good choice if you already use **ImageSharp** in your code.

> :memo: **Note.** From my tests, `MJpegWpfVideoEncoder` works about twice faster than `MJpegImageSharpVideoEncoder` so it makes sense to use the former when it's available.

### MPEG-4

Finally, the `Mpeg4VcmVideoEncoder` does MPEG-4 encoding using a _Video for Windows_ (aka _VfW_) or _Video Compression Manager_ (aka _VCM_) compatible codec installed on the system. Apparently, this encoder works on Windows only (but is available for any target).

Currently tested codecs include **Microsoft MPEG-4 V2** and **V3**, [Xvid](https://www.xvid.com/download/), [DivX](http://www.divx.com/en/software/divx) and [x264vfw](http://sourceforge.net/projects/x264vfw/files/). Unfortunately, some of them have only 32-bit versions, others produce errors in 64 bits. The only codec which looks to work reliably in 64 bits is **x264vfw64**.

> :bulb: **Tip.** For **x264vfw** (both 32- and 64-bit), it is recommended to check the option **Zero Latency** in its configuration utility to prevent picture freezes. This configuration utility is installed along with the codec.

You can get the list of available codecs (of mentioned above) with the `Mpeg4VcmVideoEncoder.GetAvailableCodecs` method. When creating an instance of the encoder, you can specify a list of codecs that should be used in order of preference. Otherwise, the default preference list will be used.

> :bulb: **Tip.** You can actually specify other codec IDs (not mentioned above) in the list of preference. And the encoder will try to use that codec if it is found in the system. However, this may not work as you expect.

```cs
var codecs = Mpeg4VcmVideoEncoder.GetAvailableCodecs();
// Present available codecs to user or select programmatically
// ...
FourCC selectedCodec = CodecIds.Xvid;
var encoder = new Mpeg4VcmVideoEncoder(stream.Width, stream.Height, 
                                    30, // frame rate
                                    0, // number of frames, if known beforehand, or zero
                                    70, // quality, though usually ignored :(
                                    selectedCodec // codecs preference
                                    );
```

> :memo: **Note.** Regardless of an encoder used, SharpAVI always produces an AVI file. The encoder is only used to compress the stream data, and does not change the format of a video file.

## Threading issues

As you may notice in the XML docs for the `Mpeg4VcmVideoEncoder` class, there is a talk about calling on a single thread. That is, the instances of this class should be used carefully in multi-threaded scenarios.
To help with this, the `SingleThreadedVideoEncoderWrapper` class is at your service. You can wrap an encoder instance, and this wrapper will guarantee that an underlying encoder is always accessed from the same thread, including its instantiation.
```cs
var threadSafeEncoder = new SingleThreadedVideoEncoderWrapper(() => new Mpeg4VcmVideoEncoder(...));
```
This is particularly useful for [asynchronous writing](asynchronous-writing.md) scenarios.

## Even more

The `EncodingStreamFactory` class contains extension methods for `AviWriter` which make the creation of encoding streams even simpler:
```cs
// Stream with UncompressedVideoEncoder
var uncompressedStream = writer.AddUncompressedVideoStream(640, 480);

// Stream with MJpegWpfVideoEncoder
var mjpegStream1 = writer.AddMJpegWpfVideoStream(640, 480, quality: 70);

// Stream with MJpegImageSharpVideoEncoder
// This extension method is included in the SharpAvi.ImageSharp package
var mjpegStream2 = writer.AddMJpegImageSharpVideoStream(640, 480, quality: 70);

// Stream with Mpeg4VcmVideoEncoder
// Parameter forceSingleThreadedAccess controls the creation of 
// SingleThreadedVideoEncoderWrapper
var mpeg4Stream = writer.AddMpeg4VideoStream(640, 480, 30, 
    quality: 70, codec: CodecIds.X264, forceSingleThreadedAccess: true);
```
