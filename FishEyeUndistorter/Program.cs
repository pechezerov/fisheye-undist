using OpenCvSharp;
using System;

namespace FishEyeUndistorter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("using: inputFilePath outputFilePath");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            Mat originalImage = Cv2.ImRead(inputFile);
            if (originalImage.Empty())
            {
                Console.WriteLine("Empty image");
                return;
            }

            Mat outImage = new Mat(originalImage.Rows, originalImage.Cols, originalImage.Type());

            for (int i = 0; i < outImage.Cols; i++)
            {
                for (int j = 0; j < outImage.Rows; j++)
                {
                    Point2d inP = GetInputPoint(i, j, originalImage.Cols, originalImage.Rows);
                    Point inP2 = new Point((int)inP.X, (int)inP.Y);

                    if (inP2.X >= originalImage.Cols || inP2.Y >= originalImage.Rows)
                        continue;

                    if (inP2.X < 0 || inP2.Y < 0)
                        continue;

                    Vec3b color = originalImage.Get<Vec3b>(inP2.Y, inP2.X);
                    Vec3b target = outImage.Get<Vec3b>(new int[] { j, i });
                    outImage.Set<Vec3b>(j, i, color);
                }
            }

            Cv2.ImWrite("result.png", outImage);
        }

        static Point2d GetInputPoint(int x, int y, int srcwidth, int srcheight)
        {
            Point2d pfish;
            double theta, phi, r, r2;
            Point3d psph;
            double FOV = Math.PI / 180 * 180;
            double FOV2 = Math.PI / 180 * 180;
            double width = srcwidth;
            double height = srcheight;

            // Polar angles
            theta = Math.PI * (x / width - 0.5); // -pi/2 to pi/2
            phi = Math.PI * (y / height - 0.5);  // -pi/2 to pi/2

            // Vector in 3D space
            psph.X = Math.Cos(phi) * Math.Sin(theta);
            psph.Y = Math.Cos(phi) * Math.Cos(theta);
            psph.Z = Math.Sin(phi) * Math.Cos(theta);

            // Calculate fisheye angle and radius
            theta = Math.Atan2(psph.Z, psph.X);
            phi = Math.Atan2(Math.Sqrt(psph.X * psph.X + psph.Z * psph.Z), psph.Y);

            r = width * phi / FOV;
            r2 = height * phi / FOV2;

            // Pixel in fisheye space
            pfish.X = 0.5 * width + r * Math.Cos(theta);
            pfish.Y = 0.5 * height + r2 * Math.Sin(theta);
            return pfish;
        }
    }
}
