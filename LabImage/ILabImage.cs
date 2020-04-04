using System.Collections.Generic;

namespace LabImage
{
    // 1 channel image with 16 gray color
    public interface ILabImage : IDrawingImage
    {
        void CastTo(ImageType type, int castBits = 8);

        IEnumerable<Roi> Rois { get; }

        int BlobCounts { get; }
        void Histogram(out int min, out int max);

        bool SaveImage(string fileName);

        void GetMinMax(out double min, out double max);

        bool DetectBlobs(ProcessOptions options);

        void DrawImageRectangle(Roi roi, ImageType type, MarkerColor color = MarkerColor.Default, int? suggestedColor = null);
        void DrawBinaryImageAllRectangles(MarkerColor color = MarkerColor.Default);
    }
}
