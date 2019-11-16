// Thanks to NAudio.Lame project (by Corey Murtagh) for inspiration
// https://github.com/Corey-M/NAudio.Lame

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if !NET35
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
#if NETSTANDARD2_0
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
#endif
using SharpAvi;
using SharpAvi.Codecs;

namespace SharpAvi.Codecs
{
    /// <summary>
    /// Mpeg Layer 3 (MP3) audio encoder using the LAME codec in external DLL.
    /// </summary>
    /// <remarks>
    /// The class is designed for using only a single instance at a time.
    /// Find information about and downloads of the LAME project at http://lame.sourceforge.net/
    /// </remarks>
    public partial class Mp3AudioEncoderLame : IAudioEncoder, IDisposable
    {
        /// <summary>
        /// Supported output bit rates (in kilobits per second).
        /// </summary>
        /// <remarks>
        /// Currently supported are 64, 96, 128, 160, 192 and 320 kbps.
        /// </remarks>
        public static readonly int[] SupportedBitRates = new[] { 64, 96, 128, 160, 192, 320 };


        #region Loading LAME DLL

        private static Type lameFacadeType;

        /// <summary>
        /// Sets the location of LAME DLL for using by this class.
        /// </summary>
        /// <remarks>
        /// This method may be called before creating any instances of this class.
        /// The LAME DLL should have the appropriate bitness (32/64), depending on the current process.
        /// If it is not already loaded into the process, the method loads it automatically.
        /// </remarks>
        public static void SetLameDllLocation(string lameDllPath)
        {
#if !NET35
            Contract.Requires(!string.IsNullOrEmpty(lameDllPath));
#endif

            var libraryName = Path.GetFileName(lameDllPath);
            if (!IsLibraryLoaded(libraryName))
            {
                var loadResult = LoadLibrary(lameDllPath);
                if (loadResult == IntPtr.Zero)
                {
                    throw new DllNotFoundException(string.Format("Library '{0}' could not be loaded.", lameDllPath));
                }
            }

            var facadeAsm = GenerateLameFacadeAssembly(libraryName);
            lameFacadeType = facadeAsm.GetType(typeof(Mp3AudioEncoderLame).Namespace + ".Runtime.LameFacadeImpl");
        }

        private static Assembly GenerateLameFacadeAssembly(string lameDllName)
        {
            var thisAsm = typeof(Mp3AudioEncoderLame).Assembly;
            var source = GetLameFacadeAssemblySource(lameDllName, thisAsm);

#if NETSTANDARD2_0
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            string assemblyName = Path.GetRandomFileName();

            string[] refPaths = new[] {
                typeof(System.Object).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"),
                thisAsm.Location
            };

            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    var sb = new StringBuilder();

                    foreach (Diagnostic diagnostic in failures)
                    {
                        sb.AppendFormat("{0}: {1}\n", diagnostic.Id, diagnostic.GetMessage());
                    }

                    throw new Exception("Could not generate LAME facade assembly.\n" + sb.ToString());
                }
                else
                {
                    ms.Position = 0;
                    return Assembly.Load(ms.ToArray());
                }
            }
#else
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();
            var compilerOptions = new System.CodeDom.Compiler.CompilerParameters()
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                IncludeDebugInformation = false,
                CompilerOptions = "/optimize",
                ReferencedAssemblies = { "mscorlib.dll", thisAsm.Location }
            };
            var compilerResult = compiler.CompileAssemblyFromSource(compilerOptions, source);
            if (compilerResult.Errors.HasErrors)
            {
                throw new Exception("Could not generate LAME facade assembly.");
            }
            return compilerResult.CompiledAssembly;
#endif
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

        private static bool IsLibraryLoaded(string libraryName)
        {
            var process = Process.GetCurrentProcess();
            return process.Modules.Cast<ProcessModule>().
                Any(m => string.Compare(m.ModuleName, libraryName, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string fileName);

#endregion


        private const int SAMPLE_BYTE_SIZE = 2;

        private readonly ILameFacade lame;
        private readonly byte[] formatData;

        /// <summary>
        /// Creates a new instance of <see cref="Mp3AudioEncoderLame"/>.
        /// </summary>
        /// <param name="channelCount">Channel count.</param>
        /// <param name="sampleRate">Sample rate (in samples per second).</param>
        /// <param name="outputBitRateKbps">Output bit rate (in kilobits per second).</param>
        /// <remarks>
        /// Encoder expects audio data in 16-bit samples.
        /// Stereo data should be interleaved: left sample first, right sample second.
        /// </remarks>
        public Mp3AudioEncoderLame(int channelCount, int sampleRate, int outputBitRateKbps)
        {
#if !NET35
            Contract.Requires(channelCount == 1 || channelCount == 2);
            Contract.Requires(sampleRate > 0);
            Contract.Requires(SupportedBitRates.Contains(outputBitRateKbps));
#endif

            if (lameFacadeType == null)
            {
                throw new InvalidOperationException("LAME DLL is not loaded. Call SetLameDllLocation first.");
            }

            lame = (ILameFacade)Activator.CreateInstance(lameFacadeType);
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
            return lame.Encode(source, sourceOffset, sourceCount / SAMPLE_BYTE_SIZE, destination, destinationOffset);
        }

        /// <summary>
        /// Flushes internal encoder's buffers.
        /// </summary>
        public int Flush(byte[] destination, int destinationOffset)
        {
            return lame.FinishEncoding(destination, destinationOffset);
        }

        /// <summary>
        /// Gets maximum length of encoded data.
        /// </summary>
        public int GetMaxEncodedLength(int sourceCount)
        {
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
