using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using SixLabors.ImageSharp;
using System.Linq;

namespace test_carwheel_detector
{
    public static class MagPlacement
    {
        public static System.Drawing.PointF[] ConvertToDrawingPoints(SixLabors.ImageSharp.PointF[] points)
        {
            return points.Select(p => new System.Drawing.PointF(p.X, p.Y)).ToArray();
        }
    

        public static VectorOfPoint ToVectorOfPoint(SixLabors.ImageSharp.PointF[] points)
        {
            var vector = new VectorOfPoint();
            foreach (var p in points)
                vector.Push(new[] { new System.Drawing.Point((int)Math.Round(p.X), (int)Math.Round(p.Y)) });
            return vector;
        }

        public static void ReplaceCarMag(string carImageFile, Rectangle carWheelBoundingBox,
           string magImageFile)
        {
            // Load base car image and mag image (PNG with transparent background)
            //string carImagePath = "car.jpg";
            //string magImagePath = "mag.png";

            Mat carImage = CvInvoke.Imread(carImageFile, ImreadModes.Unchanged);
            
            PointF[] wheelCorners  = RectangleExtensions.ToPointFArray(carWheelBoundingBox);
            var emguPoints = ToVectorOfPoint(wheelCorners);

            // Create a binary mask of the wheel area
            Mat mask = new Mat(carImage.Size, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(0));
            CvInvoke.FillConvexPoly(mask, emguPoints, new MCvScalar(255));

            // Inpaint the wheel area
            Mat inpainted = new Mat();
            CvInvoke.Inpaint(carImage, mask, inpainted, 3, Emgu.CV.CvEnum.InpaintType.Telea);

            // Use inpainted as the new base image
            carImage = inpainted.Clone();


            Mat magImage = CvInvoke.Imread(magImageFile, ImreadModes.Unchanged); // keep alpha

            // Define 4 points on the car image where the mag should go
            // This would come from Azure Custom Vision or YOLOv8
            // Example: front-left wheel rectangle (manually defined or from detection)

            
            // PointF[] wheelCorners = new PointF[]
            // {
            //     new PointF(300, 600), // top-left
            //     new PointF(400, 590), // top-right
            //     new PointF(410, 700), // bottom-right
            //     new PointF(290, 710)  // bottom-left
            // };

            // Define corners of the mag image
            float w = magImage.Width;
            float h = magImage.Height;
            PointF[] magCorners = new PointF[]
            {
                new PointF(0, 0),
                new PointF(w, 0),
                new PointF(w, h),
                new PointF(0, h)
            };

            // Get perspective transform matrix
            Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(ConvertToDrawingPoints(magCorners), ConvertToDrawingPoints(wheelCorners));

            // Warp mag image to fit the wheel area
            Mat warpedMag = new Mat(carImage.Size, DepthType.Cv8U, 4); // use 4 channels for transparency
            CvInvoke.WarpPerspective(magImage, warpedMag, perspectiveMatrix, carImage.Size, Inter.Linear, Warp.Default, BorderType.Transparent, new MCvScalar());

            // Separate the alpha channel to use as mask
            VectorOfMat channels = new VectorOfMat();
            CvInvoke.Split(warpedMag, channels);
            Mat alphaMask = channels[3]; // alpha channel

            // Convert car and mag images to Image<Bgra, Byte> for pixel access
            Image<Bgra, byte> carImageRGBA = carImage.ToImage<Bgra, byte>();
            Image<Bgra, byte> warpedMagImage = warpedMag.ToImage<Bgra, byte>();

            int width = carImage.Width;
            int height = carImage.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte alpha = warpedMagImage.Data[y, x, 3];
                    if (alpha > 0)
                    {
                        float a = alpha / 255f;

                        for (int c = 0; c < 3; c++) // RGB channels
                        {
                            byte magVal = warpedMagImage.Data[y, x, c];
                            byte carVal = carImageRGBA.Data[y, x, c];

                            carImageRGBA.Data[y, x, c] = (byte)(magVal * a + carVal * (1 - a));
                        }
                    }
                }
            }


            // Save the blended result
            carImageRGBA.ToBitmap().Save("car_with_mag.png");
            Console.WriteLine("Image saved as 'car_with_mag.png'");
        }
    }

}

