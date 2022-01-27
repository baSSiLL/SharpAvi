# Working with Audio Streams

Dealing with audio streams is very similar to video streams but differs in some details.

## Basics

An uncompressed format for audio data is the _Pulse Code Modulation_ (aka _PCM_), which can be obtained from any sound capture device. Data consists of samples of a fixed size (16 bits is the most common). If there are more than 1 audio channels (for example, left and right), then data for channels are interleaved. That is, data goes in chunks each containing samples for all channels with the same timestamp. The number of samples per 1 second of audio data  (per single channel) is called a _sample rate_.

Audio data can be stored in an AVI stream in blocks of an arbitrary length. However, in practice, it is recommended to choose the block size so that the duration of contained audio data is equal to the duration of a single video frame. Video and audio data should be interleaved: write video frame, write an audio block(s) for this frame, write the next video frame, write an audio block(s) for the next frame, etc.

Audio streams are represented by the `IAviAudioStream` interface. To store uncompressed audio data in an AVI file, you only need to specify three discussed properties of the data. And then supply it with data using the `IAviAudioStream.WriteBlock` method.
```cs
var writer = new AviWriter("test.avi") { FramesPerSecond = 30 };
var videoStream = ...
var audioStream = writer.AddAudioStream(channelCount: 2, samplesPerSecond: 44100, bitsPerSample: 16);
// The recommended length of an audio block (the duration of a video frame)
// may be computed for PCM as
var audioByteRate = (audioStream.BitsPerSample / 8) * audioStream.ChannelCount * audioStream.SamplesPerSecond;
var audioBlockSize = (int)(audioByteRate / writer.FramesPerSecond);
var audioBuffer = new byte[audioBlockSize];

while (/* not done */)
{
    // Get the data

    videoStream.WriteFrame(...);
    audioStream.WriteBlock(audioBuffer, 0, audioBuffer.Length);
}
```

`IAviAudioStream` contains many other properties, most of which are only useful when you feed the stream with pre-compressed audio data and have to set corresponding values for the stream headers in an AVI file.

## Encoding

[Just like video streams](using-video-encoders.md), audio streams can process data with audio _encoders_. That is, objects implementing the `IAudioEncoder` interface. To create an encoding stream, use the `AviWriter.AddEncodingAudioStream` method. Then all data passed to this stream will be encoded with the specified encoder.
```cs
// Create encoder
var encoder = ...
// Create stream
var encodingStream = writer.AddEncodingAudioStream(encoder);
// Encode and write data
encodingStream.WriteBlock(audioBuffer, 0, audioBuffer.Length);
```

**SharpAvi** includes only one audio encoder from the box - `Mp3LameAudioEncoder`. This is an _MPEG Layer 3_ (aka _MP3_) encoder employing the [LAME](http://lame.sourceforge.net/links.php#Binaries) library. It expects data with 16 bits per sample and 1 or 2 channels. When creating an instance, you specify a format of input data and a desired output bitrate:
```cs
var encoder = new Mp3LameAudioEncoder(
    /* channelCount: */ 2,
    /* samplesPerSecond: */ 44100, 
    /* outputBitRateKbps: */ 192);
```

Alternatively, you can use an extension method `AddMp3LameAudioStream` (defined in the `EncodingStreamFactory`) to create an MP3 encoding stream without creating an encoder instance explicitly:
```cs
var mp3Stream = writer.AddMp3LameAudioStream(2, 44100, 192);
```

The LAME binaries are not included with **SharpAvi**. You should get them on your own if planning to use `Mp3AudioEncoderLame`. Before creating instances of this class, you should tell it where the LAME DLL of the appropriate bitness is located:
```cs
// Get path where LAME DLL is stored, something like
var dllPath = Path.Combine(dllDir, Environment.Is64BitProcess ? "x64" : "x86", "lame_enc.dll");
// Set the location to the encoder
Mp3AudioEncoderLame.SetLameDllLocation(dllPath);
```

> :memo: **Note.** `Mp3LameAudioEncoder` is not available for the .NET Standard 2.0 target because I found no way to customize the filename of a native DLL.

> :warning: **Warning.** I have not yet tested `Mp3LameAudioEncoder` on non-Windows platforms. Please let me know of any issues.