using System.Collections.Generic;
using System.Linq;
using System.Text;
using LabDrivers.Cameras.Andor;
using LabDrivers.Cameras.Prime;
using LabDrivers.Cameras.Prime.DllImports;
using LabDrivers.Core;

namespace LabDrivers.Cameras
{
    public class CameraService : ICameraService
    {
        public static ICameraService Current { get; } = new CameraService();

        public CameraService()
        {
            Cameras = new List<ICamera>();
        }

        private List<ICamera> Cameras { get; }

        // always get new lists and app has the responsibilty to shut down old opened cameras
        // when starting opening a new camera
        public IEnumerable<ICameraInfo> FetchCameras()
        {
            var list = GetPrimeCameras();
            var andor = AndorFactory.Current.GetAndor();
            if (andor != null)
            {
                list.Add(andor);
            }

            return list;
        }

        public bool ShutDown()
        {
            Cameras.Where(x => x.IsOpened).Foreach(x =>
            {
                x.Stop();
                x.Close();
            });
            Cameras.Clear();
            return Pvcam.pl_pvcam_uninit();
        }

        public ICamera OpenCamera(ICameraInfo cameraInfo)
        {
            ICamera camera = null;
            switch (cameraInfo.CameraType)
            {
                case CameraType.Prime:
                    camera = new Prime95SingleTypeCamera(cameraInfo);
                    break;
                case CameraType.Andor:
                    camera = new AndorCamera(cameraInfo);
                    break;
            }

            if (camera == null) return null;
            Cameras.Add(camera);
            camera.Open();
            return camera;
        }

        private List<ICameraInfo> GetPrimeCameras()
        {
            //make sure PVCAM is initialized
            var list = new List<ICameraInfo>();
            //Pvcam.pl_pvcam_uninit();
            try
            {
                if (!Pvcam.pl_pvcam_init())
                {
                    return list;
                }

                if (Pvcam.pl_cam_get_total(out short totalCamera))
                {
                    StringBuilder cameraName = new StringBuilder(PvTypes.CAM_NAME_LEN);
                    for (short i = 0; i < totalCamera; i++)
                    {
                        if (Pvcam.pl_cam_get_name(i, cameraName))
                        {
                            list.Add(new PrimeCameraInfo(cameraName.ToString(), i));
                        }
                    }
                }

                return list;
            }
            catch
            {
                return list;
            }
        }
    }
}
