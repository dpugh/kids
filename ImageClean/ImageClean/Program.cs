using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageClean
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 3)
            {
                var pass = args[0];
                var fileIn = args[1];
                var fileOut = args[2];

                var image = Image.FromFile(fileIn);
                var bitmap = (Bitmap)image;

                if (pass == "c")
                {
                    for (int x = 0; (x < bitmap.Width); ++x)
                    {
                        for (int y = 0; (y < bitmap.Height); y++)
                        {
                            var pixel = bitmap.GetPixel(x, y);

                            if (pixel.R + pixel.G + pixel.B < 0xff + 0xff)
                            {
                                pixel = Color.Black;
                            }
                            else
                            {
                                pixel = Color.White;
                            }

                            bitmap.SetPixel(x, y, pixel);

                        }
                    }
                }
                else if (pass == "t")
                {
                    for (int x = 0; (x < bitmap.Width); ++x)
                    {
                        for (int y = 0; (y < bitmap.Height); y++)
                        {
                            var pixel = bitmap.GetPixel(x, y);

                            if (ColorsEqual(pixel, Color.White))
                            {
                                if (Neighbors(bitmap, x, y, 1).Any(c => ColorsEqual(c, Color.Black)))
                                {
                                    bitmap.SetPixel(x, y, Color.Red);
                                }
                            }
                        }
                    }

                    for (int x = 0; (x < bitmap.Width); ++x)
                    {
                        for (int y = 0; (y < bitmap.Height); y++)
                        {
                            var pixel = bitmap.GetPixel(x, y);

                            if (ColorsEqual(pixel, Color.Red))
                            {
                                bitmap.SetPixel(x, y, Color.Black);
                            }
                        }
                    }
                }

                image.Save(fileOut, ImageFormat.Bmp);
            }
        }

        static bool ColorsEqual(Color left, Color right)
        {
            return (left.R == right.R) && (left.G == right.G) && (left.B == right.B);
        }

        static IEnumerable<Color> Neighbors(Bitmap bitmap, int x, int y, int r)
        {
            for (int dx = -r; (dx <= r); ++dx)
            {
                if ((x + dx >= 0) && (x + dx < bitmap.Width))
                {
                    int ry = (int)(Math.Sqrt((double)(r * r - dx * dx)));
                    for (int dy = -ry; (dy <= ry); ++dy)
                    {
                        if (((dx != 0) || (dy != 0)) && (y + dy >= 0) && (y + dy < bitmap.Height))
                        {
                            yield return bitmap.GetPixel(x + dx, y + dy);
                        }
                    }
                }
            }
        }
    }
}
