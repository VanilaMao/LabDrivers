using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LabImage.Test
{
    [TestClass]
    public class UnitTest1
    {
        private ILabImage _labImage;

        [TestInitialize]
        public void Setup()
        {
            
        }

        [TestMethod]
        public void TestMethod1()
        {
            _labImage = new LabImage(2, 2, new ushort[] { 10000, 20000, 30000, 50000 });
            _labImage.SaveImage("C://test//test.png");
        }

        [TestMethod]
        public void Read16bitsImage()
        {
            var file = "C://test//test.tiff";
            var image = file.ReadFromFile();
            double x, y;
            image.GetMinMax(out x, out y);

        }
    }
}
