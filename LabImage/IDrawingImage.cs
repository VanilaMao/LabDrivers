
using System;
using System.Drawing;
namespace LabImage
{
    public interface IDrawingImage : IDisposable
    {
        Bitmap ToBitmap(ImageType type);
    }
}
