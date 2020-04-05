using System.IO;
using ATMCD64CS;

namespace LabDrivers.Cameras.Andor
{
    public static class AndorUtilities
    {
        public static int? GetHsSpeed(this AndorSDK andor)
        {
            if (andor.SetADChannel(0) != AndorSDK.DRV_SUCCESS) return null;
            var totalHssSpeedNumber = 0;
            if (andor.GetNumberHSSpeeds(0, 0, ref totalHssSpeedNumber) != AndorSDK.DRV_SUCCESS) return null;
            var hsNumber = 0;
            float fHsMaxSpeed = 0, fHsSpeed = 0;
            for (var i = 0; i < totalHssSpeedNumber; i++)
            {
                if (andor.GetHSSpeed(0, 0, i, ref fHsSpeed) != AndorSDK.DRV_SUCCESS) continue;
                if (!(fHsSpeed > fHsMaxSpeed)) continue;
                fHsMaxSpeed = fHsSpeed;
                hsNumber = i;
            }

            return hsNumber;
        }

        public static int? GetVsSpeed(this AndorSDK andor)
        {
            var totalVssSpeedNumber = 0;
            if (andor.GetNumberVSSpeeds(ref totalVssSpeedNumber) != AndorSDK.DRV_SUCCESS) return null;
            var vsNumber = 0;
            float fVsMaxSpeed = 0, fVsSpeed = 0;
            for (var i = 0; i < totalVssSpeedNumber; i++)
            {
                if (andor.GetVSSpeed(i, ref fVsSpeed) != AndorSDK.DRV_SUCCESS) continue;
                if (!(fVsSpeed > fVsMaxSpeed)) continue;
                fVsMaxSpeed = fVsSpeed;
                vsNumber = i;
            }
            return vsNumber;
        }

        public static ICameraInfo GetAndor(this AndorSDK andor)
        {
            var dir = Directory.GetCurrentDirectory();
            if (andor.Initialize(dir) != AndorSDK.DRV_SUCCESS)
            {
                return null;
            }
            return new AndorCameraInfo("Andor Camera");
        }
    }
}
