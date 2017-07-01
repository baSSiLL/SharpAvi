# Asynchronous Writing

Sometimes you may get a performance gain by implementing asynchronous operations. For example, you can prepare data for the next frame, while the previous frame is being written. **SharpAvi** streams support one of the standard asynchronous patterns for the **WriteFrame**/**WriteBlock** methods, depending on the target platform.

Implementation of asynchronous writes can properly handle multiple simultaneous operations, serializing them in one queue internally. However, for avoiding errors, it is recommended to always wait for completion of previous write operation before starting a new one on the same stream. Apparently, you should never modify the contents of input buffer until an asynchronous operation which it was passed to ends.

When using asynchronous writing for encoding threads, remember about thread-safety issues with some of the encoders, which may require you to use [SingleThreadedVideoEncoderWrapper](Using-Video-Encoders#single-threaded).

## .NET 4.5 - Write...Async

Stream interfaces implement _Task-based Asynchronous Pattern_ for the writing operations. Method **IAviVideoStream.WriteFrame** has asynchronous counterpart **WriteFrameAsync**. And there is method **IAviAudioStream.WriteBlockAsync**.

Asynchronous variants of methods start writing operation and return the corresponding **Task** object. You can do common task operations with it - use with **await** keyword, wait for it synchronously or add a continuation task.
{code:c#}
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
{code:c#}

## .NET 3.5 - BeginWrite..., EndWrite...

Stream interfaces implement _Asynchronous Programming Model_ (aka _IAsyncResult Pattern_) for the writing operations. Method **IAviVideoStream.WriteFrame** has corresponding methods **BeginWriteFrame** and **EndWriteFrame** for asynchronous operations. Likewise, there are methods **BeginWriteBlock** and **EndWriteBlock** in the **IAviAudioStream** interface. 

Methods **BeginWrite...** start asynchronous writing. They have additional parameters for user callback (invoked after operation ends) and user state object. They return **IAsyncResult** object representing the asynchronous operation. Methods **EndWrite...** takes this **IAsyncResult** object and waits until this operation ends.
{code:c#}
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
{code:c#}