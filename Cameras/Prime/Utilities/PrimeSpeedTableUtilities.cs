using LabDrivers.Cameras.Prime.DllImports;
using System;
using System.Runtime.InteropServices;
using System.Text;
using LabDrivers.Cameras.Prime.Models;
using System.Linq;

namespace LabDrivers.Cameras.Prime.Utilities
{

    // tech debt here 
    public static class PrimeSpeedTableUtilities
    {

        public static bool BuildSpeedTable(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {
            StringBuilder desc = new StringBuilder(100);
            IntPtr unmngRdPortCount = Marshal.AllocHGlobal(sizeof(UInt32));
            IntPtr unmngRdPortSet = Marshal.AllocHGlobal(sizeof(UInt32));
            IntPtr unmngSpdTabIndexCount = Marshal.AllocHGlobal(sizeof(UInt32));
            IntPtr unmngBitDepth = Marshal.AllocHGlobal(sizeof(Int16));
            IntPtr unmngGainCount = Marshal.AllocHGlobal(sizeof(Int32));
            IntPtr unmngSpdTabIdx = Marshal.AllocHGlobal(sizeof(Int16));
            IntPtr unmngPixTime = Marshal.AllocHGlobal(sizeof(UInt16));
            IntPtr unmngGainMax = Marshal.AllocHGlobal(sizeof(UInt16));

            var speedTable = new SpeedTable();

            speedTable.ReadoutOptions.Clear();

            //read number of available ports
            Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_READOUT_PORT, (Int16)PvTypes.AttributeIDs.ATTR_COUNT, unmngRdPortCount);

            speedTable.ReadoutPorts = Convert.ToUInt32(Marshal.ReadInt32(unmngRdPortCount));
            Marshal.FreeHGlobal(unmngRdPortCount);
            unmngRdPortCount = IntPtr.Zero;

            //set each port and find number of readout speeds on that port
            for (Int16 i = 0; i < speedTable.ReadoutPorts; i++)
            {
                Marshal.WriteInt32(unmngRdPortSet, i);

                //set readout port
                Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_READOUT_PORT, unmngRdPortSet);
          

                //get port description
                //NOTE: for Interline and sCMOS cameras this often returns "Mutliplication gain" even though the port does
                //not have any multiplication gain capability
                Pvcam.pl_get_enum_param(cameraHandle, PvTypes.PARAM_READOUT_PORT, (UInt32)i, out _, desc, 100);

                //get number of readout speeds on each readout port
                Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_SPDTAB_INDEX, (Int16)PvTypes.AttributeIDs.ATTR_COUNT, unmngSpdTabIndexCount);
                speedTable.ReadoutSpeeds = Convert.ToUInt32(Marshal.ReadInt32(unmngSpdTabIndexCount));

                //for all readout speeds
                for (Int16 j = 0; j < speedTable.ReadoutSpeeds; j++)
                {
                    Marshal.WriteInt16(unmngSpdTabIdx, j);
                    Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_SPDTAB_INDEX, unmngSpdTabIdx);
                  
                    //Get Number of gains available on this speed, note that use ATTR_MAX to find max gains available as ATTR_COUNT
                    //has issues in certain version of PVCAM
                    Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_GAIN_INDEX, (Int16)PvTypes.AttributeIDs.ATTR_COUNT, unmngGainMax);
                    var gainMax = (UInt16)Marshal.ReadInt16(unmngGainMax);

                    //get bit depth of the speed
                    Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_BIT_DEPTH, (Int16)PvTypes.AttributeIDs.ATTR_CURRENT, unmngBitDepth);
                    Int16 bidDepth = Marshal.ReadInt16(unmngBitDepth);

                    //get readout frequency (pixel time) of the speed
                    Pvcam.pl_get_param(cameraHandle, PvTypes.PARAM_PIX_TIME, (Int16)PvTypes.AttributeIDs.ATTR_CURRENT, unmngPixTime);
                    double pixTime = Convert.ToDouble(Marshal.ReadInt16(unmngPixTime));
                    

                    //add new readout option to our list of options
                    speedTable.ReadoutOptions.Add(new ReadoutOption(i, j, bidDepth, gainMax, $"{1000/pixTime}MZ "));
                }
            }



            cameraInfo.CameraParas.SpeedTable = speedTable;
            cameraInfo.CameraSettings.ReadOutOptions = new CameraOptionParameter
                ("ReadOutSpeed",speedTable.ReadoutOptions.Select(x => x.ToString()).ToList(),1);
            Marshal.FreeHGlobal(unmngRdPortSet);
            unmngRdPortSet = IntPtr.Zero;

            Marshal.FreeHGlobal(unmngSpdTabIndexCount);
            unmngSpdTabIndexCount = IntPtr.Zero;

            Marshal.FreeHGlobal(unmngBitDepth);
            unmngBitDepth = IntPtr.Zero;

            Marshal.FreeHGlobal(unmngGainCount);
            unmngGainCount = IntPtr.Zero;

            Marshal.FreeHGlobal(unmngSpdTabIdx);
            unmngSpdTabIdx = IntPtr.Zero;

            Marshal.FreeHGlobal(unmngPixTime);
            unmngPixTime = IntPtr.Zero;

            return true;
        }

        //set the camera readout speed
        public static bool SetReadoutSpeed(this Int16 cameraHandle, ICameraInfo cameraInfo)
        {
            //select the port and speed index according to the speed table index selected by the user

            var options = cameraInfo.CameraSettings.ReadOutOptions as CameraOptionParameter;

            var index = (int)cameraInfo.CameraSettings.ReadOutOptions.Value;
    

            if (!(cameraInfo.CameraParas.SpeedTable is SpeedTable speedTable)) return false;

            var option = speedTable.ReadoutOptions.FirstOrDefault(x => x.ToString() == options.Options[index]);
            

            Int32 port = option.Port;
            Int16 speedIndex = option.Speed;
            var unmngValue = Marshal.AllocHGlobal(sizeof(Int32));
            Marshal.WriteInt32(unmngValue, port);
            Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_READOUT_PORT, unmngValue);
            Marshal.FreeHGlobal(unmngValue);
            unmngValue = IntPtr.Zero;

            unmngValue = Marshal.AllocHGlobal(sizeof(Int16));
            Marshal.WriteInt32(unmngValue, speedIndex);
            Pvcam.pl_set_param(cameraHandle, PvTypes.PARAM_SPDTAB_INDEX, unmngValue);
            Marshal.FreeHGlobal(unmngValue);
            unmngValue = IntPtr.Zero;

            return true;
        }
    }
}
