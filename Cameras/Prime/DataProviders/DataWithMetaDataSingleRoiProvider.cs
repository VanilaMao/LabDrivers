
using System.Runtime.InteropServices;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataWithMetaDataSingleRoiProvider : DataWithMetaDataProvider
    {
        public DataWithMetaDataSingleRoiProvider(DataProviderWithMetaDataContext context) : base(context)
        {
        }

        protected override short[] CopyFromRioFrameToBuffer()
        {
            var size = (int) (FrameRois[0].dataSize / 2);
            var frameDataSigned = new short[size];
            Marshal.Copy(FrameRois[0].data, frameDataSigned, 0, size);
            return frameDataSigned;
        }
    }
}
