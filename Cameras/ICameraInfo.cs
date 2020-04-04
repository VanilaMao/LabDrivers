using System.Dynamic;

namespace LabDrivers.Cameras
{
    public interface ICameraInfo
    {
        string Name { get; }
        int Index { get; }

        CameraType CameraType { get; }
        dynamic CameraSettings { get; }

        // CameraParas cannot be modified since coming with the camrea features
        dynamic CameraParas { get; }
    }
}
