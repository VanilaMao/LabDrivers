

using System.Dynamic;

namespace LabDrivers.Cameras.Prime
{
    public class PrimeCameraInfo: ICameraInfo
    {
        public PrimeCameraInfo(string name, int index)
        {
            Name = name;
            Index = index;
            CameraSettings = new ExpandoObject();
            CameraParas = new ExpandoObject();
            CameraType = CameraType.Prime; 
        }

        public string Name { get; }
        public int Index { get; }
        public CameraType CameraType { get; }
        public dynamic CameraSettings { get; }
        public dynamic CameraParas { get; }
    }
}
