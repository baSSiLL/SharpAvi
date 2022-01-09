﻿// Thanks to NAudio.Lame project (by Corey Murtagh) for inspiration
// https://github.com/Corey-M/NAudio.Lame

using SharpAvi.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Mpeg Layer 3 (MP3) audio encoder using the LAME codec in external DLL.
    /// </summary>
    /// <remarks>
    /// The class is designed for using only a single instance at a time.
    /// Find information about and downloads of the LAME project at http://lame.sourceforge.net/
    /// </remarks>
    public partial class Mp3LameAudioEncoder : IAudioEncoder, IDisposable
    {
        /// <summary>
        /// Supported output bit rates (in kilobits per second).
        /// </summary>
        /// <remarks>
        /// Currently supported are 64, 96, 128, 160, 192 and 320 kbps.
        /// </remarks>
        public static readonly int[] SupportedBitRates = new[] { 64, 96, 128, 160, 192, 320 };


        #region Loading LAME DLL

        private static readonly object lameFacadeSync = new object();
        private static Type lameFacadeType;
        private static string lastLameLibraryName;

        /// <summary>
        /// Sets the location of the LAME library for using by this class.
        /// </summary>
        /// <remarks>
        /// This method may be called before creating any instances of this class.
        /// The LAME library should have the appropriate bitness (32/64), depending on the current process.
        /// If it is not already loaded into the process, the method loads it automatically.
        /// </remarks>
        public static void SetLameDllLocation(string lameLibraryPath)
        {
            Argument.IsNotNullOrEmpty(lameLibraryPath, nameof(lameLibraryPath));

            lock (lameFacadeSync)
            {
                var libraryName = Path.GetFileName(lameLibraryPath);
                if (!IsLibraryLoaded(libraryName))
                {
#if NET5_0_OR_GREATER
                    NativeLibrary.Load(lameLibraryPath);
#else
                    LoadLameLibrary45(lameLibraryPath);
#endif
                }
#if NET5_0_OR_GREATER
                ResolveFacadeImpl50(libraryName);
#else
                ResolveFacadeImpl45(libraryName);
#endif
                lastLameLibraryName = libraryName;
            }
        }

#if NET5_0_OR_GREATER
        private static void ResolveFacadeImpl50(string libraryName)
        {
            RedirectDllResolver.SetRedirect(LameFacadeImpl.DLL_NAME, libraryName);
            lameFacadeType = typeof(LameFacadeImpl);
        }
#else
        private static void LoadLameLibrary45(string libraryPath)
        {
            var loadResult = LoadLibrary(libraryPath);
            if (loadResult == IntPtr.Zero)
            {
                throw new DllNotFoundException(string.Format("Library '{0}' could not be loaded.", libraryPath));
            }
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string fileName);

        private static void ResolveFacadeImpl45(string libraryName)
        {
            if (lameFacadeType is null || lastLameLibraryName != libraryName)
            {
                var facadeAsm = GenerateLameFacadeAssembly(libraryName);
                lameFacadeType = facadeAsm.GetType(typeof(Mp3LameAudioEncoder).Namespace + ".Runtime.LameFacadeImpl");
            }
        }

        private static Assembly GenerateLameFacadeAssembly(string lameDllName)
        {
            var thisAsm = typeof(Mp3LameAudioEncoder).Assembly;
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();
            var compilerOptions = new System.CodeDom.Compiler.CompilerParameters()
            {
                 GenerateInMemory = true,
                 GenerateExecutable = false,
                 IncludeDebugInformation = false,
                 CompilerOptions = "/optimize",
                 ReferencedAssemblies = {"mscorlib.dll", thisAsm.Location}
            };
            var source = GetLameFacadeAssemblySource(lameDllName, thisAsm);
            var compilerResult = compiler.CompileAssemblyFromSource(compilerOptions, source);
            if (compilerResult.Errors.HasErrors)
            {
                throw new Exception("Could not generate LAME facade assembly.");
            }
            return compilerResult.CompiledAssembly;
        }

        private static string GetLameFacadeAssemblySource(string lameDllName, Assembly resourceAsm)
        {
            string source;
            using (var sourceStream = resourceAsm.GetManifestResourceStream("SharpAvi.Codecs.LameFacadeImpl.cs"))
            using (var sourceReader = new StreamReader(sourceStream))
            {
                source = sourceReader.ReadToEnd();
                sourceReader.Close();
            }

            var lameDllNameLiteral = string.Format("\"{0}\"", lameDllName);
            source = source.Replace("\"lame_enc.dll\"", lameDllNameLiteral);

            return source;
        }
#endif

        private static bool IsLibraryLoaded(string libraryName)
        {
            var process = Process.GetCurrentProcess();
            return process.Modules.Cast<ProcessModule>().
                Any(m => string.Compare(m.ModuleName, libraryName, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

#endregion


        private const int SAMPLE_BYTE_SIZE = 2;

        private readonly ILameFacade lame;
        private readonly byte[] formatData;

        /// <summary>
        /// Creates a new instance of <see cref="Mp3LameAudioEncoder"/>.
        /// </summary>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="sampleRate">Sample rate (in samples per second).</param>
        /// <param name="outputBitRateKbps">Output bit rate (in kilobits per second).</param>
        /// <remarks>
        /// Encoder expects audio data in 16-bit samples.
        /// Stereo data should be interleaved: left sample first, right sample second.
        /// </remarks>
        public Mp3LameAudioEncoder(int channelCount, int sampleRate, int outputBitRateKbps)
        {
            Argument.IsInRange(channelCount, 1, 2, nameof(channelCount));
            Argument.IsPositive(sampleRate, nameof(sampleRate));
            Argument.Meets(SupportedBitRates.Contains(outputBitRateKbps), nameof(outputBitRateKbps));

            lock (lameFacadeSync)
            {
                if (lameFacadeType is null)
                {
                    throw new InvalidOperationException("LAME DLL is not loaded. Call SetLameDllLocation first.");
                }

                lame = (ILameFacade)Activator.CreateInstance(lameFacadeType);
            }

            lame.ChannelCount = channelCount;
            lame.InputSampleRate = sampleRate;
            lame.OutputBitRate = outputBitRateKbps;

            lame.PrepareEncoding();

            formatData = FillFormatData();
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            var lameDisposable = lame as IDisposable;
            if (lameDisposable != null)
            {
                lameDisposable.Dispose();
            }
        }

        /// <summary>
        /// Encodes block of audio data.
        /// </summary>
        public int EncodeBlock(byte[] source, int sourceOffset, int sourceCount, byte[] destination, int destinationOffset)
        {
            Argument.IsNotNull(source, nameof(source));
            Argument.IsNotNegative(sourceOffset, nameof(sourceOffset));
            Argument.IsPositive(sourceCount, nameof(sourceCount));
            Argument.ConditionIsMet(sourceOffset + sourceCount <= source.Length,
                "Source end offset exceeds the source length.");
            Argument.IsNotNull(destination, nameof(destination));
            Argument.IsNotNegative(destinationOffset, nameof(destinationOffset));

            return lame.Encode(source, sourceOffset, sourceCount / SAMPLE_BYTE_SIZE, destination, destinationOffset);
        }

        /// <summary>
        /// Flushes internal encoder's buffers.
        /// </summary>
        public int Flush(byte[] destination, int destinationOffset)
        {
            Argument.IsNotNull(destination, nameof(destination));
            Argument.IsNotNegative(destinationOffset, nameof(destinationOffset));

            return lame.FinishEncoding(destination, destinationOffset);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Encodes block of audio data.
        /// </summary>
        public int EncodeBlock(ReadOnlySpan<byte> source, Span<byte> destination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Flushes internal encoder's buffers.
        /// </summary>
        public int Flush(Span<byte> destination)
        {
            throw new NotImplementedException();
        }
#endif

        /// <summary>
        /// Gets maximum length of encoded data.
        /// </summary>
        public int GetMaxEncodedLength(int sourceCount)
        {
            Argument.IsNotNegative(sourceCount, nameof(sourceCount));

            // Estimate taken from the description of 'lame_encode_buffer' method in 'lame.h'
            var numberOfSamples = sourceCount / SAMPLE_BYTE_SIZE;
            return (int)Math.Ceiling(1.25 * numberOfSamples + 7200);
        }


        /// <summary>
        /// Number of audio channels.
        /// </summary>
        public int ChannelCount
        {
            get { return lame.ChannelCount; }
        }

        /// <summary>
        /// Sample rate.
        /// </summary>
        public int SamplesPerSecond
        {
            get { return lame.OutputSampleRate; }
        }

        /// <summary>
        /// Bits per sample per single channel.
        /// </summary>
        public int BitsPerSample
        {
            get { return SAMPLE_BYTE_SIZE * 8; }
        }

        /// <summary>
        /// Audio format.
        /// </summary>
        public short Format
        {
            get { return AudioFormats.Mp3; }
        }

        /// <summary>
        /// Byte rate of the stream.
        /// </summary>
        public int BytesPerSecond
        {
            get { return lame.OutputBitRate * 1000 / 8; }
        }

        /// <summary>
        /// Minimum amount of data.
        /// </summary>
        public int Granularity
        {
            get { return 1; }
        }

        /// <summary>
        /// Format-specific data.
        /// </summary>
        public byte[] FormatSpecificData
        {
            get { return formatData; }
        }


        private byte[] FillFormatData()
        {
            // See MPEGLAYER3WAVEFORMAT structure
            var mp3Data = new MemoryStream(4 * sizeof(ushort) + sizeof(uint));
            using (var writer = new BinaryWriter(mp3Data))
            {
                writer.Write((ushort)1); // MPEGLAYER3_ID_MPEG
                writer.Write(0x00000002U); // MPEGLAYER3_FLAG_PADDING_OFF
                writer.Write((ushort)lame.FrameSize); // nBlockSize
                writer.Write((ushort)1); // nFramesPerBlock
                writer.Write((ushort)lame.EncoderDelay);
            }
            return mp3Data.ToArray();
        }
    }
}