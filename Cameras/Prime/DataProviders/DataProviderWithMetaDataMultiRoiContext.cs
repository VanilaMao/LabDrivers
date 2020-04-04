

using System;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataProviderWithMetaDataMultiRoiContext :DataProviderWithMetaDataContext
    {
        public DataProviderWithMetaDataMultiRoiContext(IntPtr sourceIntPtr, 
            uint frameSize,
            int roiCount, 
            short imageSizeX,
            bool isCentroidEnabled1, 
            short imageSizeY) : base(sourceIntPtr, frameSize, roiCount)
        {
            ImageSizeX = imageSizeX;
            IsCentroidEnabled = isCentroidEnabled1;
            ImageSizeY = imageSizeY;
        }

        public Int16 ImageSizeX { get; }

        public bool IsCentroidEnabled { get; }

        public Int16 ImageSizeY { get; }
    }
}
