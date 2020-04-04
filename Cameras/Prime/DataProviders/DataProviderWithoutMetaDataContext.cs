using System;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataProviderWithoutMetaDataContext :DataProviderContext
    {
        public DataProviderWithoutMetaDataContext(IntPtr sourceIntPtr, uint frameSize)
        {
            SourceIntPtr = sourceIntPtr;
            FrameSize = frameSize;
        }

        public IntPtr SourceIntPtr { get; }

        public uint FrameSize { get; }
    }
}
