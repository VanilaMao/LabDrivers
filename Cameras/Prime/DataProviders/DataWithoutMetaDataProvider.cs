using System;
using System.Runtime.InteropServices;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataWithoutMetaDataProvider : IDataProvider
    {
        public DataWithoutMetaDataProvider(DataProviderWithoutMetaDataContext context)
        {
            SourceStream = context.SourceIntPtr;
            FrameSize = context.FrameSize;
        }

        public IntPtr SourceStream { get; }

        public uint FrameSize { get; }

        public virtual short[] TransformData()
        {
            var frameDataSigned = new short[FrameSize / 2];
            Marshal.Copy(SourceStream, frameDataSigned, 0, frameDataSigned.Length);
            return frameDataSigned;
        }
    }
}
