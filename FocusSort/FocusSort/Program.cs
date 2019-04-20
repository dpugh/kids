using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusSort
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var a in args)
            {
                if (Directory.Exists(a))
                {
                    ScanDirectory(a);
                }
                else if (File.Exists(a))
                {
                    ScoreFile(a);
                }
            }
        }

        static void ScanDirectory(string path)
        {
            foreach (var f in Directory.EnumerateFiles(path))
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(f), ".jpg") ||
                    StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(f), ".jpeg"))
                {
                    ScoreFile(f);
                }
            }

            foreach (var d in Directory.EnumerateDirectories(path))
            {
                ScanDirectory(d);
            }
        }

        const int CompressionRatio = 6;

        static int ScoreFile(string path)
        {
            int bestScore = 0;

            using (var image = Image.FromFile(path))
            {
                var bitmap = (Bitmap)image;

                int compressedWidth = bitmap.Width / CompressionRatio;
                int compressedHeight = bitmap.Height / CompressionRatio;

                Color[,] compressedPixels = new Color[compressedWidth, compressedHeight];
                for (int x = 0; (x < compressedWidth); ++x)
                {
                    for (int y = 0; (y < compressedHeight); y++)
                    {
                        int r = 0;
                        int g = 0;
                        int b = 0;
                        for (int i = 0; (i < CompressionRatio); ++i)
                        {
                            for (int j = 0; (j < CompressionRatio); ++j)
                            {
                                var pixel = bitmap.GetPixel(x * CompressionRatio + i, y * CompressionRatio + j);

                                r += pixel.R;
                                g += pixel.G;
                                b += pixel.B;
                            }
                        }

                        compressedPixels[x, y] = Color.FromArgb(255, r / (CompressionRatio * CompressionRatio), g / (CompressionRatio * CompressionRatio), b / (CompressionRatio * CompressionRatio));
                    }
                }


                for (int x = 1; (x < compressedWidth - 1); ++x)
                {
                    for (int y = 1; (y < compressedHeight - 1); y++)
                    {
                        var pixel = compressedPixels[x, y];

                        int r = pixel.R * 4;
                        int g = pixel.G * 4;
                        int b = pixel.B * 4;
                        OffsetRGB(compressedPixels, x - 1, y, -1, ref r, ref g, ref b);
                        OffsetRGB(compressedPixels, x + 1, y, -1, ref r, ref g, ref b);
                        OffsetRGB(compressedPixels, x, y - 1, -1, ref r, ref g, ref b);
                        OffsetRGB(compressedPixels, x, y + 1, -1, ref r, ref g, ref b);

                        bestScore = Math.Max(bestScore, Math.Max(Math.Abs(r), Math.Max(Math.Abs(g), Math.Abs(b))));
                    }
                }

                Console.Write(bestScore);
                Console.Write('\t');

                Console.WriteLine(Path.GetFullPath(path));
            }

            if (bestScore < 70)
            {
                try
                {
                    var newPath = Path.Combine(@"d:\unfocused", Path.GetFileName(path));
                    int suffix = 1;
                    while (File.Exists(newPath))
                    {
                        newPath = Path.Combine(@"d:\unfocused",
                                               Path.GetFileNameWithoutExtension(path) + "-" + (++suffix).ToString() + Path.GetExtension(path));
                    }

                    File.Move(path, newPath);
                }
                catch (Exception)
                { }
            }

            return bestScore;
        }

        static int ScorePixels(Color left, Color Right)
        {
            return Math.Max(Math.Max(Math.Abs(left.R - Right.R), Math.Abs(left.G - Right.G)), Math.Abs(left.B - Right.B));
        }

        static void OffsetRGB(Color[,] bitmap, int x, int y, int m, ref int r, ref int g, ref int b)
        {
            var pixel = bitmap[x, y];
            r += m * pixel.R;
            g += m * pixel.G;
            b += m * pixel.B;
        }

        static void SetMinMax(int v, ref int min, ref int max)
        {
            if (v < min)
            {
                min = v;
            }

            if (v > max)
            {
                max = v;
            }
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
