using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace LabImage
{
    public static class LabImageUtilities
    {
        public static IDrawingImage Merge(int color, ILabImage left, ILabImage right)
        {
            // in the future, move merge to ILabImage interface to solve anti-pattern
            if (!(left is LabImage leftImage) || !(right is LabImage rightImage) || color < 0 || color > 255)
            {
                return null;
            }

            var sizeX = leftImage.SizeX;
            var sizeY = leftImage.SizeY;
            if (sizeX != rightImage.SizeX || sizeY != rightImage.SizeY)
            {
                return null;
            }
            using (var mat = Mat.Zeros(sizeY, sizeX, DepthType.Cv8U, 1))
            using (var outMat = new Mat(sizeX, sizeY,DepthType.Cv8U,3))
            {
                var baseMat = mat;
                if (color > 0)
                {
                    baseMat = mat + color;
                }

                //bgr
                CvInvoke.Merge(new VectorOfMat(baseMat, leftImage.Bit8Mat, rightImage.Bit8Mat), outMat);
                baseMat?.Dispose();
                return new DrawImage(outMat);
            }
        }
    }
}
