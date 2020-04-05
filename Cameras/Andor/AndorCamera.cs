using System;
using System.Threading;
using System.Threading.Tasks;
using ATMCD64CS;
using LabDrivers.Cameras.Events;

namespace LabDrivers.Cameras.Andor
{
    public class AndorCamera : ICamera
    {
        private readonly AndorSDK _andor;

        public AndorCamera(ICameraInfo cameraInfo)
        {
            CameraInfo = cameraInfo;
            _andor = AndorFactory.Current;
        }
        public bool IsAcquisition { get; private set; }

        public ICameraInfo CameraInfo { get; }

        public bool IsOpened { get; private set; }
        public bool IsContinous => true;

        private int ImageSizeX => 512;

        private int ImageSizeY => 512;

        private CancellationTokenSource TokenSource { get; set; }

        public event EventHandler<AcquistionCompletedEventArgs> AcquistionCompleted;

        public bool Acquisition()
        {
            var buffer = AcquisitionInternal();
            AcquistionCompleted?.Invoke(null, new AcquistionCompletedEventArgs(buffer, ImageSizeX, ImageSizeY));
            return true;
        }

        private ushort[] AcquisitionInternal()
        {
            _andor?.WaitForAcquisition();
            var buffer = new ushort[ImageSizeX * ImageSizeY];
            while (_andor?.GetMostRecentImage16(buffer, (uint)(ImageSizeX * ImageSizeY)) != AndorSDK.DRV_SUCCESS)
            {
            }
            return buffer;
        }

        public bool Close()
        {
            _andor?.CoolerOFF();
            IsOpened = false;
            return Stop() && _andor?.ShutDown() == AndorSDK.DRV_SUCCESS;
        }

        public bool Open()
        {
            var minTemp = 0;
            var maxTemp = 0;
            if (_andor?.GetTemperatureRange(ref minTemp, ref maxTemp) != AndorSDK.DRV_SUCCESS) return false;
            CameraInfo.CameraParas.TemperatureMin = minTemp;
            CameraInfo.CameraParas.TemperatureMax = maxTemp;


            // if want settings on UI, can make vaule as cameraparameter type
            CameraInfo.CameraSettings.Temperature = -75;
            CameraInfo.CameraSettings.Mode = 5;//acquire until abort
            CameraInfo.CameraSettings.ReadMode = 4;

            var vsSpeed = _andor?.GetVsSpeed();
            if (!vsSpeed.HasValue) return false;
            CameraInfo.CameraSettings.VsSpeed = vsSpeed;



            var hsSpeed = _andor?.GetHsSpeed();
            if (!hsSpeed.HasValue) return false;
            CameraInfo.CameraSettings.HsSpeed = hsSpeed.Value;

            CameraInfo.CameraSettings.BaselineClamp = true;

            CameraInfo.CameraSettings.TriggerMode = 0;

            var maxGain = 0;
            var minGain = 0;
            if (_andor.GetEMGainRange(ref minGain, ref maxGain) != AndorSDK.DRV_SUCCESS) return false;
            CameraInfo.CameraParas.MaxGain = maxGain;
            CameraInfo.CameraParas.MinGain = minGain;
            CameraInfo.CameraSettings.Gain = 90; //90% percent
            CameraInfo.CameraSettings.ExposureTime = 10;
            IsOpened = true;
            return Setup();
        }

        public bool Setup()
        {
            if (_andor?.SetTemperature(CameraInfo.CameraSettings.Temperature) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetAcquisitionMode(CameraInfo.CameraSettings.Mode) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetReadMode(CameraInfo.CameraSettings.ReadMode) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.CoolerON() != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetVSSpeed(CameraInfo.CameraSettings.VsSpeed) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetHSSpeed(0, CameraInfo.CameraSettings.HsSpeed) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetBaselineClamp(CameraInfo.CameraSettings.BaselineClamp) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetTriggerMode(CameraInfo.CameraSettings.TriggerMode) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetEMCCDGain((int)(CameraInfo.CameraSettings.Gain * CameraInfo.CameraParas.MaxGain / 100.0)) != AndorSDK.DRV_SUCCESS) return false;
            return true;
        }

        public bool Start()
        {
            if (_andor?.SetExposureTime((float)(CameraInfo.CameraSettings.Temperature / 1000.0)) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetImage(1, 1, 1, ImageSizeX, 1, ImageSizeY) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.SetShutter(1, 1, 0, 0) != AndorSDK.DRV_SUCCESS) return false;
            if (_andor?.PrepareAcquisition() != AndorSDK.DRV_SUCCESS) return false;

            var status = 0;
            if (_andor?.GetStatus(ref status) != AndorSDK.DRV_IDLE) return false;
            if (_andor?.StartAcquisition() == AndorSDK.DRV_SUCCESS)
            {
                TokenSource = new CancellationTokenSource();
                IsAcquisition = true;

                Task.Run(() =>
                {
                    try
                    {
                        while (true) //continous mode, till stop to cancel the aynsc thread
                        {
                            TokenSource.Token.ThrowIfCancellationRequested();
                            var buffer = AcquisitionInternal();
                            AcquistionCompleted?.Invoke(null, new AcquistionCompletedEventArgs(buffer, ImageSizeX, ImageSizeY));
                        }
                        
                    }
                    catch (OperationCanceledException)
                    {
                        IsAcquisition = false;
                        _andor?.AbortAcquisition();
                    }
                    finally
                    {
                        TokenSource.Dispose();
                    }
                }, TokenSource.Token);
                return true;
            }
            _andor?.AbortAcquisition();
            return false;
        }

        public bool Stop()
        {
            TokenSource?.Cancel();
            return _andor?.SetShutter(1, 2, 0, 0) == AndorSDK.DRV_SUCCESS;
        }
    }
}
