using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace LabImage
{
    public static class LabImageTool
    {
        public static ILabImage ReadFromFile(this string fileName)
        {
            return new LabImage(CvInvoke.Imread(fileName, ImreadModes.AnyDepth|ImreadModes.Grayscale));
        }

        public static ILabImage ReadFromTiff(this string fileName)
        {
            using (Stream imageStreamSource = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                TiffBitmapDecoder decoder = new TiffBitmapDecoder(imageStreamSource,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapSource bitmapSource = decoder.Frames[0];

                int width = bitmapSource.PixelWidth;
                int height = bitmapSource.PixelHeight;
                ushort[] data = new ushort[width * height];

                bitmapSource.CopyPixels(data, 2 * width, 0);


                //andor has 10bits, need to shift bits
                for (int i = 0; i < width * height; i++)
                    data[i] *= 64;
                return new LabImage(width, height, data);
            } 
        }

        public static Bitmap CovertDataArrayToBitmap(int width, int height, short[] dataArray)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppGrayScale);
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(dataArray, 0, ptr, dataArray.Length);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public static void SaveToTiff(this Stream stream, int width, int height, ushort[] dataArray)
        {
            var image = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Gray16,null);
            image.WritePixels(new Int32Rect(0, 0, width, height), dataArray, image.BackBufferStride, 0);
            image.Freeze();
            var frame = BitmapFrame.Create(image);
            frame.Freeze();
            var encoder = new TiffBitmapEncoder();
            encoder.Frames.Add(frame);
            encoder.Save(stream);
        }

        public static void GdiSaveToTiff(this FileStream stream, int width, int height, ushort[] dataArray)
        {
            var array = Array.ConvertAll(dataArray, x => (short) x);
            using (var bitmap = CovertDataArrayToBitmap(width, height, array))
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Tiff);
                byte[] bytes = memory.ToArray();
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
} 
