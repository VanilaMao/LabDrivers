

using System.Collections.Generic;

namespace LabDrivers.Cameras
{
    public interface ICameraService
    {
        IEnumerable<ICameraInfo> FetchCameras();

        // the calling app should know which camera is open 
        // and it has the responsibility to shut it down before call this function
        bool ShutDown();

        ICamera OpenCamera(ICameraInfo cameraInfo);
    }
}
