
using System;
using System.Runtime.InteropServices;

namespace LabDrivers.Cameras.Prime.DllImports
{
    public static class WindowsApi
    {
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr MemSet(IntPtr dest, int c, int byteCount);
    }
}
