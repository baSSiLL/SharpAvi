# Working with Audio Streams

Dealing with audio streams is very similar to video streams, differing in some details.

## Basics

Uncompressed format for audio data is _Pulse Code Modulation_ (aka _PCM_), which can be obtained from any sound capture device. Data consists of samples of fixed size (most common is 16 bits). If there are more than 1 audio channels (for example, left and right), then data for channels are interleaved (first samples for all channels, then second samples for all channel, and so on). The number of samples per 1 second of audio data  (per single channel) is called _sample rate_.

Audio data can be stored in AVI stream in blocks of arbitrary length. However, in practice, it is recommended to choose the block size so the duration of contained audio data is equal to the duration of a single video frame. Video and audio data should be interleaved: write video frame, write audio block(s) for this frame, write next video frame, write audio block(s) for next frame, etc.

Audio streams are represented by the [IAviAudioStream](https://sharpavi.codeplex.com/SourceControl/latest#SharpAvi/Output/IAviAudioStream.cs) interface. To store uncompressed audio data in AVI file, you only need to specify 3 discussed features of the data. And then fill it with data using the **WriteBlock** method.
{code:c#}
var writer = new AviWriter("test.avi") { FramesPerSecond = 30 };
var videoStream = ...
var audioStream = writer.AddAudioStream(channelCount: 2, samplesPerSecond: 44100, bitsPerSample: 16);
// The recommended length of audio block (duration of a video frame)
// may be computed for PCM as
var audioByteRate = (audioStream.BitsPerSample / 8) * audioStream.ChannelCount * audioStream.SamplesPerSecond;
var audioBlockSize = (int)(audioByteRate / writer.FramesPerSecond);
var audioBuffer = new byte[audioBlockSize](audioBlockSize);

while (/* not done */)
{
    // Get the data

    videoStream.WriteFrame(...);
    audioStream.WriteBlock(audioBuffer, 0, audioBuffer.Length);
}
{code:c#}

**IAviAudioStream** contains many other properties, most of which are only useful when you feed the stream with pre-compressed audio data, and need to set corresponding values for stream headers in AVI file.

## Encoding

[Just like video streams](Using-Video-Encoders), audio streams can process the data with audio _encoders_, objects implementing the [IAudioEncoder](https://sharpavi.codeplex.com/SourceControl/latest#SharpAvi/Codecs/IAudioEncoder.cs) interface. To create an encoding stream, use the **AviWriter.AddEncodingAudioStream** method. Then all data passed to this stream will be encoded with the specified encoder.
{code:c#}
// Create encoder
var encoder = ...
// Create stream
var encodingStream = writer.AddEncodingAudioStream(encoder);
// Encode and write data
encodingStream.WriteBlock(audioBuffer, 0, audioBuffer.Length);
{code:c#}

**SharpAvi** includes only one audio encoder from the box - [Mp3AudioEncoderLame](https://sharpavi.codeplex.com/SourceControl/latest#SharpAvi/Codecs/Mp3AudioEncoderLame.cs). This is an _MPEG Layer 3_ (aka _MP3_) encoder employing [LAME](http://lame.sourceforge.net/links.php#Binaries) library. It expects data with 16 bits per sample and 1 or 2 channels. When creating an instance, you specify format of input data and desired output bitrate:
{code:c#}
var encoder = new Mp3AudioEncoderLame(
    /* channelCount: */ 2,
    /* samplesPerSecond: */ 44100, 
    /* outputBitRateKbps: */ 192);
{code:c#}
Alternatively, you can use extension method in [EncodingStreamFactory](https://sharpavi.codeplex.com/SourceControl/latest#SharpAvi/Codecs/EncodingStreamFactory.cs) to create an MP3 encoding stream without explicitly creating the encoder:
{code:c#}
var mp3Stream = writer.AddMp3AudioStream(2, 44100, 192);
{code:c#}

The LAME binaries are not included with **SharpAvi**. You should get them on your own if planning to use **Mp3AudioEncoderLame**. Before creating instances of this class, you should tell it where the LAME DLL of appropriate bitness is located:
{code:c#}
// Get path where LAME DLL is stored, something like
var dllPath = Path.Combine(dllDir, Environment.Is64BitProcess ? "x64" : "x86", "lame_enc.dll");
// Set the location to the encoder
Mp3AudioEncoderLame.SetLameDllLocation(dllPath);
{code:c#}