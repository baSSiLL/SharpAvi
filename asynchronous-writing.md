# Asynchronous Writing

Sometimes you can get a performance gain by implementing asynchronous operations. For example, you can prepare data for the next frame, while the previous frame is being written.

Implementation of asynchronous writes in **SharpAvi** can properly handle multiple simultaneous operations, serializing them into a single queue internally. However, for avoiding errors, it is recommended to always wait for completion of the previous write operation before starting a new one on the same stream. Apparently, you should never modify the contents of an input buffer which was passed to an asynchronous operation until that operation completes.

When using asynchronous writing for encoding threads, remember about thread-safety issues with some of the encoders, which may require you to use the [SingleThreadedVideoEncoderWrapper](using-video-encoders.md#threading-issues) class.

Method `IAviVideoStream.WriteFrame` has an asynchronous counterpart `WriteFrameAsync`. And there is the method `IAviAudioStream.WriteBlockAsync`. On .NET 5+ there are also overloads with a `ReadOnlyMemory<byte>` parameter instead of a byte array.

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
