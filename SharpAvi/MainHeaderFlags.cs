using System;

namespace SharpAvi
{
    [Flags]
    internal enum MainHeaderFlags : uint
    {
        HasIndex = 0x00000010U,
        MustUseIndex = 0x00000020U,
        IsInterleaved = 0x00000100U,
        TrustChunkType = 0x00000800U,
        WasCaptureFile = 0x00010000U,
        Copyrighted = 0x000200000U,
    }
}
