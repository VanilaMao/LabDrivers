
using System.Dynamic;

namespace LabDrivers.Cameras.Andor
{
    public class AndorCameraInfo : ICameraInfo
    {
        public AndorCameraInfo(string name)
        {
            Name = name;
            Index = 0;
            CameraSettings = new ExpandoObject();
            CameraParas = new ExpandoObject();
            CameraType = CameraType.Andor;
        }

        public string Name { get; }
        public int Index { get; }
        public CameraType CameraType { get; }
        public dynamic CameraSettings { get; }
        public dynamic CameraParas { get; }
    }
}
