# Asynchronous Writing

Sometimes you can get a performance gain by implementing asynchronous operations. For example, you can prepare data for the next frame, while the previous frame is being written. **SharpAvi** streams support one of the standard asynchronous patterns for the `WriteFrame`/`WriteBlock` methods, depending on the target platform.

Implementation of asynchronous writes can properly handle multiple simultaneous operations, serializing them in one queue internally. However, for avoiding errors, it is recommended to always wait for completion of the previous write operation before starting a new one on the same stream. Apparently, you should never modify the contents of an input buffer which was passed to an asynchronous operation until that operation completes.

When using asynchronous writing for encoding threads, remember about thread-safety issues with some of the encoders, which may require you to use [SingleThreadedVideoEncoderWrapper](using-video-encoders.md#threading-issues).

## .NET 4.5 - Write...Async

Stream interfaces implement _Task-based Asynchronous Pattern_ for the writing operations. Method `IAviVideoStream.WriteFrame` has an asynchronous counterpart `WriteFrameAsync`. And there is the method `IAviAudioStream.WriteBlockAsync`.

Asynchronous variants of methods start writing operation and return the corresponding `Task` object. You can do common task operations with it - use with the `await` keyword, wait for it synchronously or add a continuation task.
```cs
var stream = writer.AddVideoStream(640, 480);

byte[]() data1 = ...
byte[]() data2 = ...
var tcs = new TaskCompletionSource<bool>();
tcs.SetResult(true);
// Initialize to completed task
Task writeResult = tcs.Task;
while (/* not done */)
{
    // Prepare next frame data
    PrepareFrame(data2);
    // Wait for previous frame written
    writeResult.Wait();
    // Switch buffers
    var data = data2;
    data2 = data1;
    data1 = data;
    // Start writing next frame
    writeResult = stream.WriteFrameAsync(true, data1, 0, data1.Length);
}
```

## .NET 3.5 - BeginWrite..., EndWrite...

The stream interfaces implement _Asynchronous Programming Model_ (aka _IAsyncResult Pattern_) for the writing operations. The method `IAviVideoStream.WriteFrame` has corresponding methods `BeginWriteFrame` and `EndWriteFrame` for asynchronous operations. Likewise, there are methods `BeginWriteBlock` and `EndWriteBlock` in the `IAviAudioStream` interface. 

The methods `BeginWrite...` start asynchronous writing. They have additional parameters for a user callback which is invoked after operation ends and a user state object. They return `IAsyncResult` object representing the asynchronous operation. Methods `EndWrite...` takes this `IAsyncResult` object and waits until this operation ends.
```cs
var stream = writer.AddVideoStream(640, 480);

byte[]() data1 = ...
byte[]() data2 = ...
IAsyncResult writeResult = null;
while (/* not done */)
{
    // Prepare next frame data
    PrepareFrame(data2);
    // Wait for previous frame written
    if (writeResult != null)
    {
        stream.EndWriteFrame(writeResult);
    }
    // Switch buffers
    var data = data2;
    data2 = data1;
    data1 = data;
    // Start writing next frame
    writeResult = stream.BeginWriteFrame(true, data1, 0, data1.Length, null, null);
}
```