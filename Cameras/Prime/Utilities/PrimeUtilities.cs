using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using LabDrivers.Cameras.Prime.DllImports;

namespace LabDrivers.Cameras.Prime.Utilities
{
    internal static class PrimeUtilities
    {
        public static bool IsParamAvailable(this Int16 cameraHandle, UInt32 paramId)
        {
            IntPtr unmngIsAvaiable = Marshal.AllocHGlobal(sizeof(UInt16));
            Boolean retValue = false;
            if (Pvcam.pl_get_param(cameraHandle, paramId, (Int16) PvTypes.AttributeIDs.ATTR_AVAIL, unmngIsAvaiable))
            {
                retValue = Convert.ToBoolean(Marshal.ReadInt16(unmngIsAvaiable));
            }

            //free unmanaged memory
            Marshal.FreeHGlobal(unmngIsAvaiable);
            unmngIsAvaiable = IntPtr.Zero;
            return retValue;
        }

        public static bool ReadEnumeration(this Int16 cameraHandle,
            out List<CameraAttributeEnumeratePairValue> nvpList, UInt32 paramId)
        {
            nvpList = new List<CameraAttributeEnumeratePairValue>();

            IntPtr unmngIsAvaiable = Marshal.AllocHGlobal(sizeof(UInt16));
            bool result = Pvcam.pl_get_param(cameraHandle, paramId, (Int16)PvTypes.AttributeIDs.ATTR_AVAIL,
                              unmngIsAvaiable) && Marshal.ReadInt16(unmngIsAvaiable) == 1;
            Marshal.FreeHGlobal(unmngIsAvaiable);
            unmngIsAvaiable = IntPtr.Zero;
            if (!result)
            {
                return false;
            }


            IntPtr unmngCount = Marshal.AllocHGlobal(sizeof(UInt32));
            result = Pvcam.pl_get_param(cameraHandle, paramId, (Int16)PvTypes.AttributeIDs.ATTR_COUNT, unmngCount);
            var count = result? Convert.ToUInt32(Marshal.ReadInt32(unmngCount)):0;
            Marshal.FreeHGlobal(unmngCount);
            unmngCount = IntPtr.Zero;
            if (!result)
            {
                return false;
            }

            StringBuilder paramName = new StringBuilder();
            //Now get the value and name associated
            for (UInt32 i = 0; i < count; i++)
            {
                if (!Pvcam.pl_enum_str_length(cameraHandle, paramId, i, out UInt32 strLength))
                {
                    return false;
                }

                if (!Pvcam.pl_get_enum_param(cameraHandle, paramId, i, out Int32 paramValue, paramName, strLength))
                {
                    return false;
                }
                nvpList.Add(new CameraAttributeEnumeratePairValue(paramValue, paramName.ToString()));
            }
            return true;
        }
        public static UInt32 GetEstReadoutTime(this Int16 cameraHandle)
        {
            UInt32 time = 0;
            if (cameraHandle.IsParamAvailable(PvTypes.PARAM_READOUT_TIME))
            {
                IntPtr unmgReadoutTime = Marshal.AllocHGlobal(sizeof(Int32));

                if (Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_READOUT_TIME,
                    (Int16) PvTypes.AttributeIDs.ATTR_CURRENT, unmgReadoutTime))
                {
                    time = (UInt32) Marshal.ReadInt32(unmgReadoutTime);
                }

                Marshal.FreeHGlobal(unmgReadoutTime);
                unmgReadoutTime = IntPtr.Zero;
            }

            return time;
        }

        public static void ReadCameraParametersToInfos(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {

            //build speed tabls

            cameraHandle.BuildSpeedTable(cameraInfo);

            //get serial (X) and parallel (Y) size of the CCD
            IntPtr unmngCcdSize = Marshal.AllocHGlobal(sizeof(Int16));
            Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_PAR_SIZE, (Int16) PvTypes.AttributeIDs.ATTR_CURRENT,
                unmngCcdSize);
            cameraInfo.CameraParas.SizeX = Marshal.ReadInt16(unmngCcdSize);

            Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_SER_SIZE, (Int16) PvTypes.AttributeIDs.ATTR_CURRENT,
                unmngCcdSize);
            cameraInfo.CameraParas.SizeY = Marshal.ReadInt16(unmngCcdSize);

            Marshal.FreeHGlobal(unmngCcdSize);
            unmngCcdSize = IntPtr.Zero;

            //read the camera chip name, currently used for main identification of the camera model
            IntPtr unmngChipName = Marshal.AllocHGlobal(PvTypes.CCD_NAME_LEN);
            Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_CHIP_NAME, (Int16) PvTypes.AttributeIDs.ATTR_CURRENT,
                unmngChipName);
            cameraInfo.CameraParas.ChipName =
                new StringBuilder(Marshal.PtrToStringAnsi(unmngChipName, PvTypes.CCD_NAME_LEN));
            Marshal.FreeHGlobal(unmngChipName);
            unmngChipName = IntPtr.Zero;

            //temperature always available
            if (ReadCoolingParameters(cameraHandle, out var min, out var max, out var setPoint))
            {
                cameraInfo.CameraParas.TemperatureMin = min;
                cameraInfo.CameraParas.TemperatureMax = max;
                cameraInfo.CameraSettings.Temperature = setPoint;
            }

            //regions, will always get at leat one ROI, must after sizex and sizey gettings
            ReadRegions(cameraHandle, cameraInfo);

            //set optional params, may not supported by camera
            GetCameraParas(cameraHandle, cameraInfo);

            //exposeTime
            cameraInfo.CameraSettings.ExposureTime = new CameraPrimitiveParameter("Explore Time", (int)10); //default 


            ////Read Trigger input Modes available on the camera
            if (ReadEnumeration(cameraHandle, out var list, PvTypes.PARAM_EXPOSURE_MODE))
            {
                cameraInfo.CameraParas.TriggerModes = list;
                cameraInfo.CameraSettings.TriggerMode = list[0].Value;
            }

            //Check if Expose Out Parameter is available on the camera
            cameraInfo.CameraParas.IsExposeOutModeSupported =
                IsParamAvailable(cameraHandle, PvTypes.PARAM_EXPOSE_OUT_MODE);

            if (cameraInfo.CameraParas.IsExposeOutModeSupported)
            {
                if(ReadEnumeration(cameraHandle, out var exposeOutList, PvTypes.PARAM_EXPOSE_OUT_MODE))
                {
                    cameraInfo.CameraParas.ExposeOutModes = exposeOutList;
                    cameraInfo.CameraSettings.ExposeOutMode = exposeOutList[0].Value;
                }
            }  
        }

        public static bool ReadCoolingParameters(this Int16 cameraHandle, out Int16 min, out Int16 max,
            out Int16 setPoint)
        {
            min = 0;
            max = 0;
            setPoint = 0;
            //Get Temperature setpoint - range and current CCD temperature
            IntPtr unmgTempSetpoint = Marshal.AllocHGlobal(sizeof(Int16));
            var result = Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_TEMP_SETPOINT,
                (Int16) PvTypes.AttributeIDs.ATTR_CURRENT, unmgTempSetpoint);
            setPoint = Marshal.ReadInt16(unmgTempSetpoint);
            Marshal.FreeHGlobal(unmgTempSetpoint);
            unmgTempSetpoint = IntPtr.Zero;


            
            IntPtr unmgTempSetpointMin = Marshal.AllocHGlobal(sizeof(Int16));
            result &= Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_TEMP_SETPOINT,
                (Int16) PvTypes.AttributeIDs.ATTR_MIN,
                unmgTempSetpointMin);
            min = Marshal.ReadInt16(unmgTempSetpointMin);
            Marshal.FreeHGlobal(unmgTempSetpointMin);
            unmgTempSetpointMin = IntPtr.Zero;


            IntPtr unmgTempSetpointMax = Marshal.AllocHGlobal(sizeof(Int16));
            result &= Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_TEMP_SETPOINT,
                (Int16) PvTypes.AttributeIDs.ATTR_MAX,
                unmgTempSetpointMax);
            max = Marshal.ReadInt16(unmgTempSetpointMax);
            Marshal.FreeHGlobal(unmgTempSetpointMax);
            unmgTempSetpointMax = IntPtr.Zero;

            return result;
        }

        public static bool ReadRegions(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {
            int maxRoiCount = 1;
            //Set Max Roi count to 1, if multiroi is not available 
            if (IsParamAvailable(cameraHandle,PvTypes.PARAM_ROI_COUNT))
            {
                cameraInfo.CameraParas.MultiRoiAvailable = true;
                // read Max ROI available
                IntPtr unmgRoiCount = Marshal.AllocHGlobal(sizeof(UInt16));
                if (Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_ROI_COUNT, (Int16)PvTypes.AttributeIDs.ATTR_MAX, unmgRoiCount))
                {
                    maxRoiCount = Marshal.ReadInt16(unmgRoiCount);              
                }
                Marshal.FreeHGlobal(unmgRoiCount);
                unmgRoiCount = IntPtr.Zero;
            }
            else
            {
                cameraInfo.CameraParas.MultiRoiAvailable = false;
            }

            //define Region type array 
            var regions = new PvTypes.RegionType[maxRoiCount];

            //set current roi count to 1 and set default CCD region to full size 
            regions[0].s1 = 0;
            regions[0].s2 = (UInt16)(cameraInfo.CameraParas.SizeX - 1);
            regions[0].p1 = 0;
            regions[0].p2 = (UInt16)(cameraInfo.CameraParas.SizeY - 1);
            regions[0].sbin = 1;
            regions[0].pbin = 1;

            cameraInfo.CameraSettings.Regions = regions;
            cameraInfo.CameraSettings.RegionRoiCount = 1;
            return true;
        }


        public static void GetCameraParas(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {
            cameraInfo.CameraParas.IsMetadataAvailable = IsParamAvailable(cameraHandle, PvTypes.PARAM_METADATA_ENABLED);
            cameraInfo.CameraSettings.IsMetadataEnabled = false; //default
            cameraInfo.CameraParas.IsCentroidAvailable =
                IsParamAvailable(cameraHandle, PvTypes.PARAM_CENTROIDS_ENABLED);
            cameraInfo.CameraSettings.IsCentroidEnabled = false; //default

            //even mutliple region is supported, but we only support one and it is the deafult setting, 
            //so no bothering to any reading and setting
        }


        //set 

        public static void SetInfosToCameraParameters(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {

            ////set readout
             cameraHandle.SetReadoutSpeed(cameraInfo);

            SetDefualt(cameraHandle, cameraInfo);

            // exposuretime and bins used in the camera setup

            //// set temp
            // SetTemperature(cameraHandle, cameraInfo, cameraInfo.CameraSettings.Temperature);

        }

        public static bool SetDefualt(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {
            var result = true;
            if (cameraInfo.CameraParas.IsMetadataAvailable)
            {
                result &= SetMetadata(cameraHandle, false);
            }

            if (cameraInfo.CameraParas.IsCentroidAvailable)
            {
                result &= SetCentroiding(cameraHandle, false, 0, 0);
            }
            return result;
        }

        //current always set it to false;
        public static bool SetCentroiding(this Int16 cameraHandle, bool enableState, UInt16 centroidCnt,
            UInt16 centroidRadius)
        {
            bool result;
            IntPtr unmgCentroidState = Marshal.AllocHGlobal(sizeof(Int16));
            if (enableState)
            {
                Marshal.WriteInt16(unmgCentroidState, 1);
                result = Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_CENTROIDS_ENABLED, unmgCentroidState);

                IntPtr unmgCentroidRadius = Marshal.AllocHGlobal(sizeof(Int16));
                Marshal.WriteInt16(unmgCentroidRadius, (Int16) centroidRadius);
                result &= Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_CENTROIDS_RADIUS, unmgCentroidRadius);
                Marshal.FreeHGlobal(unmgCentroidRadius);
                unmgCentroidRadius = IntPtr.Zero;


                IntPtr unmgCentroidCount = Marshal.AllocHGlobal(sizeof(Int16));
                Marshal.WriteInt16(unmgCentroidCount, (Int16) centroidCnt);
                result &= Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_CENTROIDS_COUNT, unmgCentroidCount);
                Marshal.FreeHGlobal(unmgCentroidCount);
                unmgCentroidCount = IntPtr.Zero;
            }
            else
            {
                Marshal.WriteInt16(unmgCentroidState, 0);
                result = Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_CENTROIDS_ENABLED, unmgCentroidState);
            }

            Marshal.FreeHGlobal(unmgCentroidState);
            unmgCentroidState = IntPtr.Zero;

            return result;
        }

        //current always set it to false;
        public static bool SetMetadata(this Int16 cameraHandle, bool enableState)
        {
            var unmgEnableMd = Marshal.AllocHGlobal(sizeof(Int16));
            Marshal.WriteInt16(unmgEnableMd, enableState ? (Int16) 1 : (Int16) 0);

            var result = Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_METADATA_ENABLED, unmgEnableMd);

            Marshal.FreeHGlobal(unmgEnableMd);
            unmgEnableMd = IntPtr.Zero;

            return result;
        }

        public static bool SetTemperature(this Int16 cameraHandle, ICameraInfo cameraInfo, Int16 setPoint)
        {
            //Check if value is in range
            if (setPoint < cameraInfo.CameraParas.TemperatureMin || setPoint > cameraInfo.CameraParas.TemperatureMax)
            {
                return false;
            }

            var unmngSetpoint = Marshal.AllocHGlobal(sizeof(Int16));
            Marshal.WriteInt16(unmngSetpoint, setPoint);

            var result = Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_TEMP_SETPOINT, unmngSetpoint);
            Marshal.FreeHGlobal(unmngSetpoint);
            unmngSetpoint = IntPtr.Zero;

            return result;
        }


       
    }
}
