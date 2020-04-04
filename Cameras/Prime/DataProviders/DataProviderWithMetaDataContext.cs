using System;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataProviderWithMetaDataContext : DataProviderWithoutMetaDataContext
    {
        public DataProviderWithMetaDataContext(IntPtr sourceIntPtr, uint frameSize, int roiCount) : base(sourceIntPtr, frameSize)
        {
            RoiCount = roiCount;
        }

        public int RoiCount { get; }
    }
}
