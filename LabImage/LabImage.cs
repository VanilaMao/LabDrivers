using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace LabImage
{
    public class LabImage :ILabImage
    {
        private readonly Mat _mat;
        private readonly Mat _bit8Mat;
        private readonly Mat _binaryMat;
        private readonly Mat _rgbMat;
        private readonly List<Roi> _rois;

        //tech debt
        public LabImage()
        {
            Chanel = 1;
            _rois = new List<Roi>();
            _bit8Mat = new Mat(SizeX, SizeY, DepthType.Cv8U, 1);
            _binaryMat = new Mat(SizeX, SizeY, DepthType.Cv8U, 1);
            _rgbMat = new Mat(SizeX, SizeY,DepthType.Cv8U,3);
        }

        internal LabImage(Mat mat, int sizeX, int sizeY, Rectangle rectangle) :this()
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _mat= new Mat(mat, rectangle);
        }
        public LabImage( int sizeX, int sizeY, ushort[] pixels):this()
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _mat = new Mat(sizeX, sizeY,DepthType.Cv16U,Chanel);
            _mat.SetTo(pixels);
        }

        public LabImage(int sizeX, int sizeY, int[] pixels) : this()
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _mat = new Mat(sizeX, sizeY, DepthType.Cv32F, Chanel);
            _mat.SetTo(pixels);
        }

        public LabImage(int sizeX, int sizeY, byte[] pixels) : this()
        {
            SizeX = sizeX;
            SizeY = sizeY;
            _mat = new Mat(sizeX, sizeY, DepthType.Cv8U, Chanel);
            _mat.SetTo(pixels);
        }

        internal LabImage(Mat mat) :this()
        {
            _mat = mat;
            SizeX = mat.Width;
            SizeY = mat.Height;
        }

        internal Mat Bit8Mat => _bit8Mat;

        public int SizeX { get; }

        public int SizeY { get; }

        public int Chanel { get; }

        public int BlobCounts { get; private set; }

        public IEnumerable<Roi> Rois => _rois;


        public Bitmap ToBitmap(ImageType type)
        {
            return GetImage(type)?.Bitmap;
        }

        public void CastTo(ImageType type, int castBits = 8)
        {
            if (type == ImageType.Bit8Gray)
            {
                _mat.ConvertTo(_bit8Mat, DepthType.Cv8U, 1.0 / Math.Pow(2, castBits));
            }
        }

        public bool DetectBlobs(ProcessOptions options)
        {
            var cast = options.CastBits;

            _mat.ConvertTo(_bit8Mat, DepthType.Cv8U, 1.0 / Math.Pow(2, cast));
            if (options.AdaptiveThreshHold)
            {
                ThreshholdAdaptive(options);
            }
            else
            {
                Threshhold(options, cast);
            }
            Threshhold(options, cast);
            _rois.Clear();
            var rioCounts = 0;
            switch (options.MethodType)
            {
                case ProcessBlobMethod.ConnectedComponent:
                    rioCounts=ConnectedComponent(options);
                    break;
                case ProcessBlobMethod.FindContour:
                    rioCounts=FindCountours(options);
                    break;
                case ProcessBlobMethod.SimpleBlob:
                    rioCounts=SimpleBlobDector(options);
                    break;
            }

            if (options.IsRgbImageAvailable)
            {
                CvInvoke.CvtColor(_bit8Mat, _rgbMat, ColorConversion.Gray2Rgb);
            }

            BlobCounts = rioCounts;
            return rioCounts > 0;
        }

        private void ThreshholdAdaptive(ProcessOptions options)
        {
            CvInvoke.AdaptiveThreshold(_bit8Mat, _binaryMat, 255, AdaptiveThresholdType.MeanC,
                options.PickDark ? ThresholdType.BinaryInv : ThresholdType.Binary, 13, 5);
        }

        private void Threshhold(ProcessOptions options, int cast)
        {
            MCvScalar mean = new MCvScalar();
            MCvScalar stdDev = new MCvScalar();
            CvInvoke.MeanStdDev(_mat, ref mean, ref stdDev);
            Point point = new Point();
            double minValue = 0, maxValue = 0;
            CvInvoke.MinMaxLoc(_mat, ref minValue, ref maxValue, ref point, ref point);
            var castValue = Math.Pow(2, cast);
            if (options.PickDark)
            {
                int iRegionMin = (int)((mean.V0 - minValue) * options.ThreshHoldFactor);
                double threshMin = (minValue + iRegionMin) / castValue;
                CvInvoke.Threshold(_bit8Mat, _binaryMat, threshMin, 255
                    , ThresholdType.BinaryInv);
            }
            else
            {
                int iRegionMax = (int)((maxValue - mean.V0) * options.ThreshHoldFactor);

                double threshMax = (mean.V0 + iRegionMax) / castValue;
                CvInvoke.Threshold(_bit8Mat, _binaryMat, threshMax, 255
                    , ThresholdType.Binary);
            }               
        }

        private int ConnectedComponent(ProcessOptions options)
        {
            int num = 0;
            using (var labels = new Mat())
            using (var stats = new Mat())
            using (var centroids = new Mat())
            {
                
                int count = CvInvoke.ConnectedComponentsWithStats(_binaryMat, labels, stats, centroids, LineType.FourConnected);
                for (int i = 0; i < count; i++)
                {
                    var statsAr = stats.GetData();
                    var sizeX =(int) statsAr.GetValue(i,(int)ConnectedComponentsTypes.Width);
                    var sizeY = (int)statsAr.GetValue(i, (int)ConnectedComponentsTypes.Height);
                    var left = (int)statsAr.GetValue(i, (int)ConnectedComponentsTypes.Left);
                    var top = (int)statsAr.GetValue(i, (int)ConnectedComponentsTypes.Top);
                    var length = sizeX > SizeY ? sizeX : sizeY;
                    var area = sizeX * sizeY;
                    if (options.MinLength > length || options.MaxLength < length || options.MinArea > area ||
                        options.MaxArea < area)
                    {
                        continue;
                    }
                    var centroidAr = centroids.GetData();
                    var x = Convert.ToInt32(centroidAr.GetValue(i, 0));
                    var y = Convert.ToInt32(centroidAr.GetValue(i, 1));
                    if (IsCenterPointInCutRange(x, y, options))
                    {
                        continue;
                    }
                    _rois.Add(new Roi()
                    {
                        Left = left,
                        Top = top,
                        Width = sizeX,
                        Height = sizeY,
                        CenterX = x,
                        CenterY = y,
                        Area = area
                    });
                    num++;
                }            
            }
            return num;
        }
      
        private int FindCountours(ProcessOptions options)
        {
            var num = 0;
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            using(var hirarchy = new Mat())
            {
                CvInvoke.FindContours(_binaryMat, contours, hirarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
               
                for (int i = 0; i < contours.Size; i++)
                {
                    var vectorOfPoint = contours[i];
                    double area = CvInvoke.ContourArea(vectorOfPoint);
                    if (options.AreaFilter == AreaFilterType.Area && (options.MinArea > area ||
                        options.MaxArea < area))
                    {
                        continue;
                    }
                    using (var moment = CvInvoke.Moments(vectorOfPoint))
                    {
                        var centroid = new Point((int)(moment.M10 / moment.M00), (int)(moment.M01 / moment.M00));
                        var rect = CvInvoke.BoundingRectangle(vectorOfPoint);
                        if(options.AreaFilter == AreaFilterType.WidthTimesHeight)
                        {
                            area = rect.Width * rect.Height;
                            if (options.MinArea > area || options.MaxArea < area)
                            {
                                continue;
                            }
                        }
                       
                        var length = rect.Width > rect.Height ? rect.Width : rect.Height;
                        if (IsCenterPointInCutRange(centroid.X, centroid.Y, options))
                        {
                            continue;
                        }
                        if (options.MinLength <= length && options.MaxLength >= length)
                        {
                            _rois.Add(new Roi()
                            {
                                Left = rect.Left,
                                Top = rect.Top,
                                Width = rect.Width,
                                Height = rect.Height,
                                CenterX = centroid.X,
                                CenterY = centroid.Y,
                                Area = Convert.ToInt32(area)
                            });
                           num++;
                        }
                        
                    }                 
                }   
            }
            return num;
        }


        //https://www.learnopencv.com/blob-detection-using-opencv-python-c/
        //slow
        private int SimpleBlobDector(ProcessOptions options)
        {
            var num = 0;
            using (var para = new SimpleBlobDetectorParams())
            {
                para.FilterByInertia = false;
                para.FilterByConvexity = false;
                para.FilterByColor = false;
                para.FilterByCircularity = false;
                para.FilterByArea = true;
                para.MinArea = options.MinArea;
                para.MaxArea = options.MaxArea;
                using (var simpleBlobDector = new SimpleBlobDetector(para))
                {
                    var k = simpleBlobDector.Detect(_binaryMat);
                    for (var i = 0; i < k.Length; i++)
                    {
                        var area = (int)k[i].Size;
                        var centerX = (int) k[i].Point.X;
                        var centerY = (int) k[i].Point.Y;
                        if (IsCenterPointInCutRange(centerX, centerY, options))
                        {
                            continue;
                        }
                        _rois.Add(new Roi()
                        {
                            Left = centerX - area/2,
                            Top = centerY-area/2,
                            Width = area,
                            Height = area,
                            CenterX = centerX,
                            CenterY = centerY,
                            Area = area*area
                        });
                        num++;
                    }

                }
            }
           
            return num;
        }

        private bool IsCenterPointInCutRange(int x, int y,ProcessOptions options)
        {
            if (options.CutEdege <= 0)
            {
                return false;
            }

            return SizeX - options.CutEdege <= x || x <= options.CutEdege || SizeY - options.CutEdege <= y || y <= options.CutEdege;
        }

        public bool SaveImage(string fileName)
        {
            return CvInvoke.Imwrite(fileName, _mat);

        }

        public void GetMinMax(out double min, out double max)
        {
            Point point = new Point();
            double minValue = 0, maxValue =0 ;
            CvInvoke.MinMaxLoc(_mat, ref minValue, ref maxValue, ref point, ref point);
            min = minValue;
            max = maxValue;
        }

        public void Histogram(out int min, out int max)
        {
            using (Mat hist = new Mat())
            using (VectorOfMat vm = new VectorOfMat())
            {
                _mat.ConvertTo(_bit8Mat, DepthType.Cv8U, 1.0 / Math.Pow(2, 0));
                vm.Push(_bit8Mat);

                CvInvoke.CalcHist(vm, new [] { 0 }, null, hist, new[] {256}, new float[] { 0,256 }, false);
                double maxVal = 0;
                double minVal = 0;
                Point point = new Point();
                
                CvInvoke.MinMaxLoc(_mat, ref minVal, ref maxVal, ref point, ref point);
                min = (int)minVal;
                max = (int)maxVal;
            }
            
        }

        // color will be ignored in gray image
        public void DrawImageRectangle(Roi roi, ImageType type, MarkerColor color, int? suggestedColor)
        {
            var image = GetImage(type);
            if (image == null)
            {
                return;              
            }
            CvInvoke.Rectangle(image, new Rectangle(roi.Left, roi.Top, roi.Width, roi.Height), GetColor(color, suggestedColor));
        }

        public void DrawBinaryImageAllRectangles(MarkerColor color)
        {
            if (!_binaryMat.IsEmpty)
            {
                foreach(var roi in Rois)
                {
                    CvInvoke.Rectangle(_binaryMat, new Rectangle(roi.Left, roi.Top, roi.Width, roi.Height), GetColor(color));
                }
            }        
        }

        private Mat GetImage(ImageType type)
        {
            if (type == ImageType.Bit8Gray)
                return _bit8Mat;
            if (type == ImageType.Binary)
            {
                return _binaryMat;
            }
            if (type == ImageType.Rgb)
            {
                return _rgbMat;
            }

            if (type == ImageType.Bit16Gray)
            {
                return _mat;
            }

            return null;
        }

        //Note that Emgu CV's Image class use BGR color space instead of RGB.
        private MCvScalar GetColor(MarkerColor color, int? suggestedColor = null)
        {
            switch (color)
            {
                case MarkerColor.Blue:
                    return new MCvScalar(255, 0, 0, 0);
                case MarkerColor.Green:
                    return new MCvScalar(0, 255, 0, 0);
                case MarkerColor.Red:
                    return new MCvScalar(0, 0,255, 0);
            }

            if (suggestedColor.HasValue)
            {
                return new MCvScalar(suggestedColor.Value, suggestedColor.Value, suggestedColor.Value,
                    suggestedColor.Value);
            }
            return new MCvScalar(192, 192, 192, 192);
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _mat?.Dispose();
                    _bit8Mat?.Dispose();
                    _binaryMat?.Dispose();
                    _rgbMat?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LabImage()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
