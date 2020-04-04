using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace LabImage
{
    public static class LabImageFactory
    {
        // or create a herit class: BinSplitLabImage, now over kill
        public static ILabImage From(int width, int height, ushort[] data, bool binSplit = false,
            bool binSplitHandleLeft = false)
        {
            if (!binSplit)
            {
                return new LabImage(width, height, data);
            }

            using (var mat = new Mat(width, height, DepthType.Cv16U, 1))
            {
                mat.SetTo(data);
                var rect = binSplitHandleLeft
                    ? new Rectangle(0, 0, width / 2, height)
                    : new Rectangle(width / 2, 0, width / 2, height);
                return new LabImage(mat, width / 2, height, rect);
            }
        }
    }
}
