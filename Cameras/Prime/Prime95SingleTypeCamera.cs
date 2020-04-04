using System;
using System.Runtime.InteropServices;
using LabDrivers.Cameras.Prime.DataProviders;
using LabDrivers.Cameras.Prime.DllImports;

namespace LabDrivers.Cameras.Prime
{
    public class Prime95SingleTypeCamera :Prime95Camera<AcquisitionType>
    { 
        private uint _frameSize;
        public Prime95SingleTypeCamera(ICameraInfo cameraInfo) : base(cameraInfo)
        {
        }

        public uint FrameSize
        {
            get => _frameSize;
            protected set => _frameSize = value;
        }
        public override AcquisitionType Type => AcquisitionType.Single;

        // not used without metadataenabled
        protected IntPtr FrameStream { get; set; }


        protected override bool StartAcquisition()
        {
            return Pvcam.pl_exp_start_seq(CameraHandleId, FrameStream);
        }

        protected override void UnAllocateBuffers()
        {
            if (FrameStream != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(FrameStream);
                FrameStream = IntPtr.Zero;
            }
        }

        protected override void AllocateBuffers()
        {
            //should use provider to seperate the metadata and without metedata

            //release pixel data buffer
            UnAllocateBuffers();

            //allocate the m_full_Frame_size amount of bytes in non-managed environment 
            FrameStream = Marshal.AllocHGlobal((Int32)FrameSize);

            ////if multiROI or Centroiding zero out the values of pixel buffer for recomposing the frame
            //if ((CurrentROICount > 1) || (IsCentroidEnabled))
            //{
            //    //Array.Clear(m_frameDataShorts, 0, m_frameDataSigned.Length - 1);
            //    //now zero out the pixel stream as well
            //    //Marshal.Copy(m_frameDataSigned, 0, m_pixel_stream, m_frameDataSigned.Length);
            //    MemSet(PixelStream, 0, (int) PixelStreamSize);
            //}
        }

        protected override bool StopAcqusition()
        {
            return Pvcam.pl_exp_abort(CameraHandleId, (Int16) PvTypes.AbortExposureFlags.CCS_CLEAR);
        }

        protected DataProviderContext DataProviderContext { get; private set; }

        // imagesizex and imagesizey does not apply to no metedata scenario
        protected override short[] HandleFrame(AcqParamsContext para)
        {
            // becuase ROI must have metadataenabled, so without metadata,
            // means single ROI, so the FrameStream include all the pixels data already
            if (!CameraInfo.CameraSettings.IsMetadataEnabled)
            {
                //verify(ImageSizeX*ImageSizeY *2 = FrameSize
                DataProviderContext = new DataProviderWithoutMetaDataContext(FrameStream,FrameSize);
            }
            else
            {
                if (CameraInfo.CameraSettings.RegionRoiCount > 1)
                {
                    DataProviderContext = new DataProviderWithMetaDataMultiRoiContext(FrameStream, FrameSize, 
                        CameraInfo.CameraSettings.RegionRoiCount, ImageSizeX, CameraInfo.CameraSettings.IsCentroidEnabled,ImageSizeY);
                }
                DataProviderContext = new DataProviderWithMetaDataContext
                    (FrameStream, FrameSize, CameraInfo.CameraSettings.RegionRoiCount);
            }
           var dataProvider =  DataProviderFactory.GetDataProvider(DataProviderContext);
           return dataProvider?.TransformData();
        }


        //private PvTypes.RegionType _region;
        //public PvTypes.RegionType Region { 
        //    get {return _region; }
        //    private set
        //    {
        //        _region = value;
        //    }
        //}

        protected override bool SetupCamera()
        {
            //if SMART streaming mode is On, then exposure time passed to
            //setup function must be non-zero value and all exposures are defined
            //by SMART streaming parameters (currently only supported
            //on Evolve-512 and Evolve-512 Delta
            //if (m_IsSmartStreamingOn)
            //    m_exposureTime = 10;

            //exposure mode is bitWsie of trigger in and expose out mode. Cameras that don't support
            //Expose out mode as we have set initial value of m_exposeOutMode=0, bitwise OR will have no effect
            //and expMode will be same as Trigger mode
            //trigger in mode
            Int16 expMode = (Int16)(CameraInfo.CameraSettings.TriggerMode | CameraInfo.CameraSettings.ExposeOutMode);
            var exposureTimePara = CameraInfo.CameraSettings.ExposureTime as CameraPrimitiveParameter;
            var exposureTime = exposureTimePara != null ? Convert.ChangeType(exposureTimePara.Value, exposureTimePara.Type) : 10;
            var regions = (PvTypes.RegionType[])CameraInfo.CameraSettings.Regions;
            if (!Pvcam.pl_exp_setup_seq(CameraHandleId, 1, (UInt16) 1, ref regions[0], expMode, Convert.ToUInt32(exposureTime),
                out _frameSize))
            {
                return false;
            }

            return true;
        }
    }
}
