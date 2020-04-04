using System;
using System.Text;
using System.Threading;
using LabDrivers.Cameras.Events;
using LabDrivers.Cameras.Prime.DataProviders;
using LabDrivers.Cameras.Prime.DllImports;
using LabDrivers.Cameras.Prime.Utilities;
using LabDrivers.Exceptions;

namespace LabDrivers.Cameras.Prime
{
    public abstract class Prime95Camera<T> : ICamera where T : struct, IConvertible
    {
        private Int16 _cameraHandleId;
        // ReSharper disable once InconsistentNaming
        protected IDataProviderFactory DataProviderFactory { get; }
        protected Prime95Camera(ICameraInfo cameraInfo)
        {
            CameraInfo = cameraInfo;
            DataProviderFactory = new DataProviderFactory();
            Callback = new PvTypes.PMCallBack_Ex3(EOFHandler);
        }

        public Int16 ImageSizeX { get; protected set; }

        public Int16 ImageSizeY { get; protected set; }



        protected Int16 CameraHandleId
        {
            get => _cameraHandleId;
            set => _cameraHandleId = value;
        }

        public bool IsAcquisition { get; protected set; }
        public ICameraInfo CameraInfo { get; private set; }
        public uint ReadOutTime { get; protected set; }

        public bool IsOpened { get; protected set; }

        protected PvTypes.PMCallBack_Ex3 Callback { get; }
        private bool IsAcquiring { get; set; }
        public abstract T Type { get; }

        protected abstract bool SetupCamera();

        protected abstract bool StartAcquisition();

        protected abstract void AllocateBuffers();

        protected abstract void UnAllocateBuffers();

        protected abstract bool StopAcqusition();

        protected abstract short[] HandleFrame(AcqParamsContext para);

        public event EventHandler<AcquistionCompletedEventArgs> AcquistionCompleted;


        public bool Open()
        {
            if (!Pvcam.pl_cam_open(new StringBuilder(CameraInfo.Name), out _cameraHandleId,
                PvTypes.CameraOpenMode.OPEN_EXCLUSIVE))
            {
                throw new PrimeCamerException($"cannot open the {CameraInfo.Name} camera");
            };
            CameraInfo = CameraInfo;
            CameraHandleId.ReadCameraParametersToInfos(CameraInfo);
            IsOpened = true;
            return true;
        }

        // set up camera parameters
        public bool Setup()
        {
            CameraHandleId.SetInfosToCameraParameters(CameraInfo);
            return true;
        }


        //start a new thread to check status? No
        //while (!m_abortAcquisition
        //       && PVCAM.pl_exp_check_status(m_hCam, out status, out byte_cnt)
        //       && status != (Int16)PvTypes.ReadoutStatuses.READOUT_COMPLETE
        //       && status != (Int16)PvTypes.ReadoutStatuses.READOUT_FAILED)
        //{
        //    Thread.Sleep(10);

        //}
        private bool StartInternal()
        {
            Setup();
            Initialize();

            ReadOutTime = CameraHandleId.GetEstReadoutTime();

            if (!SetupCamera())
            {
                throw new PrimeCamerException($"cannot set up camera for {CameraInfo.Name}");
            }

            if (!RegisterCallBackForAcquistionCompleted())
            {
                return false;
            }


            IsAcquiring = false;
            if (!Acquisition()) // need to start twice, no idea what is going on
            {

                return false;
            }
            return true;
        }

        public bool Start()
        {

            IsAcquiring = true;
            if (!StartInternal() && !IsAcquiring)
            {
                StartInternal();
            }
            IsAcquisition = true;

            return true;
        }

        public bool Acquisition()
        {
            return StartAcquisition();
        }


        // some property settings
        private void Initialize()
        {
            //please see the pcam sdk example to see how the image size to be calculated
            // if multiple ROI, Metadataenabled must be on
            //if (CameraInfo.CameraSettings.Metadataenabled) 
            //{
            //

            //}

            //need something to fix

            //  ImageSize size means the  pixel array to the end user

            if (CameraInfo.CameraSettings.IsMetadataEnabled && CameraInfo.CameraSettings.RegionRoiCount > 1)
            {
                ImageSizeX = (Int16)(CameraInfo.CameraParas.SzieX / CameraInfo.CameraSettings.Regions[0].sbin);
                ImageSizeY = (Int16)(CameraInfo.CameraParas.SzieY / CameraInfo.CameraSettings.Regions[0].pbin);
            }
            ImageSizeX = (Int16)((CameraInfo.CameraSettings.Regions[0].s2 - CameraInfo.CameraSettings.Regions[0].s1 + 1)
                                 / CameraInfo.CameraSettings.Regions[0].sbin);
            ImageSizeY = (Int16)((CameraInfo.CameraSettings.Regions[0].p2 - CameraInfo.CameraSettings.Regions[0].p1 + 1)
                                   / CameraInfo.CameraSettings.Regions[0].pbin);

            // PixelStreamSize is not always the same as the framsize becuase framsize could inlcude metadata
            AllocateBuffers();

            ////Metadata should be enabled for MultiROI or centroiding. Enable it if it is not already done
            //if (((CurrentROICount > 1) || (IsCentroidEnabled)) && (m_metadataEnabled == false))
            //{
            //    //Enable metadata
            //    ConfigureMetadata(true);
            //}
        }


        public bool Close()
        {
            Stop();
            if (Pvcam.pl_cam_close(CameraHandleId))
            {
                IsOpened = false;
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            UnAllocateBuffers();
            if (Pvcam.pl_cam_deregister_callback(CameraHandleId, PvTypes.PL_CALLBACK_EVENT.PL_CALLBACK_EOF)
                & StopAcqusition())
            {
                IsAcquisition = false;
                return true;
            }
            return false;
        }



        private bool RegisterCallBackForAcquistionCompleted()
        {
            //register a callback function (pointed to by the m_EOFHandlerDelegate). The PVCAM will be calling this function
            //each time EOF event occurs (EOF = end of frame readout) and pass the address of the structure that should be
            //pushed back to callback handler function once the EOF event occurs
            //callback functions are the preferred notification methods to polling thanks to its lower impact on the CPU and acquisition
            //UInt16 sbin = (UInt16)CameraInfo.CameraSettings.Regions[0].sbin;
            //var paramsHandle = GCHandle.Alloc(new AcqParamsContext(sbin), GCHandleType.Pinned);
            return Pvcam.pl_cam_register_callback_ex3(CameraHandleId,
                PvTypes.PL_CALLBACK_EVENT.PL_CALLBACK_EOF,
                Callback, IntPtr.Zero);
        }

        private void EOFHandler(ref PvTypes.FRAME_INFO pFrameInfo, IntPtr context)
        {
            //for demonstration purposes you can inspect the acqParams values
            AcqParamsContext acqParams = new AcqParamsContext(); // (AcqParamsContext)Marshal.PtrToStructure(context, typeof(AcqParamsContext));

            var frame = HandleFrame(acqParams);

            // free memory
            //Marshal.FreeHGlobal(context);
            //context = IntPtr.Zero;

            var unsignedFrame = new ushort[frame.Length];
            Buffer.BlockCopy(frame, 0, unsignedFrame, 0, frame.Length * 2);



            AcquistionCompleted?.Invoke(null, new AcquistionCompletedEventArgs(unsignedFrame, ImageSizeX, ImageSizeY));

            //set the EOF event notifying the acquisition thread that a new frame has arrived
        }


    }
}
