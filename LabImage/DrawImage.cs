using System.Drawing;
using Emgu.CV;

namespace LabImage
{
    public class DrawImage:IDrawingImage
    {
        private readonly Mat _mat;
        public void Dispose()
        {
            _mat?.Dispose();
        }

        // mat assuming rgb currently
        public DrawImage(Mat mat)
        {
            _mat = mat?.Clone();
        }

        public Bitmap ToBitmap(ImageType type)
        {
            if (type == ImageType.Rgb)
            {
                return _mat?.Bitmap;
            }

            return null;
        }
    }
}
