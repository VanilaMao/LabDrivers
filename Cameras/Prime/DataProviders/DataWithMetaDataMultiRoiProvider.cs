using System;
using System.Runtime.InteropServices;
using LabDrivers.Cameras.Prime.DllImports;

namespace LabDrivers.Cameras.Prime.DataProviders
{
    public class DataWithMetaDataMultiRoiProvider : DataWithMetaDataProvider
    {
        public DataWithMetaDataMultiRoiProvider(DataProviderWithMetaDataMultiRoiContext context) : base(context)
        {
            ImageSizeX = context.ImageSizeX;
            ImageSizeY = context.ImageSizeY;
            IsCentroidEnabled = context.IsCentroidEnabled;
            PixelStreamSize = context.ImageSizeX * context.ImageSizeY * sizeof(UInt16);
        }
        public int PixelStreamSize { get; }
        public Int16 ImageSizeX { get; }

        public bool IsCentroidEnabled { get; }

        public Int16 ImageSizeY { get; }


        protected override short[] CopyFromRioFrameToBuffer()
        {
            //if centroiding Zero out the frame before recomposing as ROI are dynamically changing
            var pixelStream = Marshal.AllocHGlobal(PixelStreamSize);
            short[] frameDataSigned = null; 
            if (IsCentroidEnabled)
                WindowsApi.MemSet(pixelStream, 0, (int)PixelStreamSize);
            if (Pvcam.pl_md_frame_recompose(pixelStream, MdFrame.impliedRoi.s1, MdFrame.impliedRoi.p1,
                (UInt16)ImageSizeX, (UInt16)ImageSizeY, ref _mdFrame))
            {
                frameDataSigned = new short[PixelStreamSize/2];
                //Copy recomposed frame to managed array
                Marshal.Copy(pixelStream, frameDataSigned, 0, frameDataSigned.Length);
            }

            Marshal.FreeHGlobal(pixelStream);
            pixelStream = IntPtr.Zero;
            return frameDataSigned;
        }
    }
}
