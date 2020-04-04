using System;
using LabDrivers.Cameras.Events;

namespace LabDrivers.Cameras
{
    public interface ICamera
    {
        bool Open();

        bool Start();

        event EventHandler<AcquistionCompletedEventArgs> AcquistionCompleted;
        bool Close();

        bool Stop();

        bool IsAcquisition { get; }

        ICameraInfo CameraInfo { get; }

        bool Setup();

        bool Acquisition();

        bool IsOpened { get; }
    }
}
