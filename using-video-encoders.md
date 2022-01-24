# Using Video Encoders

## Encoding a video stream

If you want a stream to perform encoding on supplied frame data, you should use the `AviWriter.AddEncodingVideoStream` method to create the stream. It takes an encoder object as the first parameter, which is used to encode incoming frames. The return value is the same `IAviVideoStream` interface, so basic usage of the stream does not change.
```cs
var encoder = ...
var stream = writer.AddEncodingVideoStream(encoder, width: 640, height: 480);
```

How to get an encoder instance? More on this [later...](#creating-a-video-encoder) And now look how to use an encoding stream.

First, you don't need to set a codec and bits per pixel for an encoding stream directly, as these are determined by the encoder. Also, a value of the `isKeyFrame` parameter for the `WriteFrame` method is ignored, because the encoder itself defines which frames are keys.

Next, all encoders expect input image data in specific format. It is the BGR32 top-down - 32 bits per pixel, blue byte first, alpha byte not used, top line goes first. This is the format you can often get from existing images. For example, when locking `System.Drawing.Bitmap` instances. Thus, the size of the data is fixed and is determined by the frame dimensions - `Width * Height * 4` bytes. For this reason, encoding streams also ignore a value of the `length` parameter for the `WriteFrame` method.

So, you simply pass an uncompressed top-down BGR32 frame to an encoding stream, and it cares about encoding:
```cs
// Say, you have a System.Drawing.Bitmap
System.Drawing.Bitmap bitmap;
// and a buffer of appropriate size for storing its bits
var buffer = new byte[stream.Width * stream.Height * 4](stream.Width-_-stream.Height-_-4);

// Now copy bits from the bitmap to the buffer
var bits = bitmap.LockBits(new Rectangle(0, 0, stream.Width, stream.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
bitmap.UnlockBits(bits);

// and flush the buffer to an encoding stream
encodingStream.WriteFrame(true, buffer, 0, buffer.Length);
```

## Creating a video encoder

Now about a video encoder. This is an object which implements the `IVideoEncoder` interface. This interface provides properties to determine the data format for the stream, and a method for encoding. **SharpAvi** includes several implementations which may be sufficient for many cases. If your case is not included, you are encouraged to write your own implementation of the interface for a preferred codec :)

The simplest is the `UncompressedVideoEncoder`. It does no real encoding, just flips image vertically and converts BGR32 data to BGR24 data to reduce the size.
```cs
var encoder = new UncompressedVideoEncoder(stream.Width, stream.Height);
```

Next is the `MotionJpegVideoEncoderWpf` which does Motion JPEG encoding. It uses `System.Windows.Media.Imaging.JpegBitmapEncoder` under the hood. Besides dimensions, you provide a desired quality level to its constructor, ranging from 1 (low quality, small size) to 100 (high quality, large size).
```cs
var encoder = new MotionJpegVideoEncoderWpf(stream.Width, stream.Height, 70);
```

Finally, the `Mpeg4VideoEncoderVcm` does MPEG-4 encoding using a _Video for Windows_ (aka _VfW_) or _Video Compression Manager_ (aka _VCM_) compatible codec installed on the system.

Currently tested codecs include **Microsoft MPEG-4 V2** and **V3**, [Xvid](https://www.xvid.com/download/), [DivX](http://www.divx.com/en/software/divx) and [x264vfw](http://sourceforge.net/projects/x264vfw/files/). Unfortunately, some of them have only 32-bit versions, others produce errors in 64 bits. The only codec which looks to work reliably in 64 bits is **x264vfw64**. For **x264vfw** (both 32- and 64-bit), it is recommended to check option **Zero Latency** in its configuration utility (installed along with the codec) to prevent picture freezes.

You can get the list of available codecs with the `Mpeg4VideoEncoderVcm.GetAvailableCodecs` method. When creating an instance of the encoder, you can specify a list of codecs that should be used in order of preference. Otherwise, the default preference list will be used.
```cs
var codecs = Mpeg4VideoEncoder.GetAvailableCodecs();
// Present available codecs to user or select programmatically
// ...
FourCC selectedCodec = KnownFourCCs.Codecs.Xvid;
var encoder = new Mpeg4VideoEncoder(stream.Width, stream.Height, 
                                    30, // frame rate
                                    0, // number of frames, if known beforehand, or zero
                                    70, // quality, though usually ignored :(
                                    selectedCodec // codecs preference
                                    );
```

> :memo: **Note.** Regardless of an encoder used, SharpAVI always produces an AVI file. The encoder is only used to compress the stream data, and does not change the format of a video file.

## Threading issues

As you may notice in the XML docs for the `MotionJpegVideoEncoderWpf` and `Mpeg4VideoEncoderVcm` classes, there is a talk about calling on a single thread. That is, the instances of these classes should be used carefully in multi-threaded scenarios.
To help with this, the `SingleThreadedVideoEncoderWrapper` class is at your service. You can wrap an encoder instance, and this wrapper will guarantee that an underlying encoder is always accessed from the same thread, including its instantiation.
```cs
var threadSafeEncoder = new SingleThreadedVideoEncoderWrapper(() => new Mpeg4VideoEncoder(...));
```
This is particularly useful for [asynchronous writing](asynchronous-writing.md) scenarios.

## Even more

The `EncodingStreamFactory` class contains extension methods for `AviWriter` which make the creation of encoding streams even simpler:
```cs
// Stream with UncompressedVideoEncoder
var uncompressedStream = writer.AddUncompressedVideoStream(640, 480);

// Stream with MotionJpegVideoEncoderWpf
// Version for .NET 3.5 also includes forceSingleThreadedAccess parameter 
// as implementation of MotionJpegVideoEncoderWpf for that platform is not thread-safe
var mjpegStream = writer.AddMotionJpegVideoStream(640, 480, quality: 70);

// Stream with Mpeg4VideoEncoderVcm
// Parameter forceSingleThreadedAccess controls the creation of 
// SingleThreadedVideoEncoderWrapper
var mpeg4Stream = writer.AddMpeg4VideoStream(640, 480, 30, 
    quality: 70, codec: KnownFourCCs.Codecs.X264, forceSingleThreadedAccess: true);
```
