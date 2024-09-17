using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Trabalho_1Bim
{
    internal class ZhangSuen
    {
        public unsafe Bitmap AfinamentoComDMA(Bitmap image, Bitmap result)
        {
            int width = image.Width;
            int height = image.Height;
            int pixelSize = 3;

            ConvertPretoBranco(image, result);

            BitmapData bitmapDataSrc = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bitmapDataDst = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bitmapDataSrc.Stride;
            byte* src = (byte*)bitmapDataSrc.Scan0;
            byte* dst = (byte*)bitmapDataDst.Scan0;

            bool pixelsWereRemoved;

            do
            {
                pixelsWereRemoved = false;

                List<(int x, int y)> pixelsToRemove = new List<(int x, int y)>();

                // Primeira sub-iteração
                for (int y = 0; y < height - 1; y++)
                {
                    for (int x = 0; x < width - 1; x++)
                    {
                        int pos = y * stride + x * pixelSize;
                        byte r = src[pos + 2];
                        byte g = src[pos + 1];
                        byte b = src[pos];
                        int intensity = (r + g + b) / 3;

                        if (intensity <= 220 && CanRemoveFirstSubIteration(src, stride, pixelSize, x, y))
                        {
                            pixelsToRemove.Add((x, y));
                            pixelsWereRemoved = true;
                        }
                    }
                }

                // aplicando mudanças
                foreach (var (x, y) in pixelsToRemove)
                {
                    int pos = y * stride + x * pixelSize;
                    dst[pos] = dst[pos + 1] = dst[pos + 2] = 255;
                }

                Buffer.MemoryCopy(dst, src, stride * height, stride * height);

                // Segunda sub-iteração
                pixelsToRemove.Clear();

                for (int y = 0; y < height - 1; y++)
                {
                    for (int x = 0; x < width - 1; x++)
                    {
                        int pos = y * stride + x * pixelSize;
                        byte r = src[pos + 2];
                        byte g = src[pos + 1];
                        byte b = src[pos];
                        int intensity = (r + g + b) / 3;

                        if (intensity <= 220 && CanRemoveSecondSubIteration(src, stride, pixelSize, x, y))
                        {
                            pixelsToRemove.Add((x, y));
                            pixelsWereRemoved = true;
                        }
                    }
                }

                // aplicando mudanças
                foreach (var (x, y) in pixelsToRemove)
                {
                    int pos = y * stride + x * pixelSize;
                    dst[pos] = dst[pos + 1] = dst[pos + 2] = 255;
                }

                Buffer.MemoryCopy(dst, src, stride * height, stride * height);

            } while (pixelsWereRemoved);

            image.UnlockBits(bitmapDataSrc);
            result.UnlockBits(bitmapDataDst);

            return result;
        }

        public static void ConvertPretoBranco(Bitmap imageBitmapSrc, Bitmap imageBitmapDest)
        {
            int width = imageBitmapSrc.Width;
            int height = imageBitmapSrc.Height;
            int pixelSize = 3;
            Int32 bw;

            BitmapData bitmapDataSrc = imageBitmapSrc.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bitmapDataDst = imageBitmapDest.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int padding = bitmapDataSrc.Stride - (width * pixelSize);

            unsafe
            {
                byte* src = (byte*)bitmapDataSrc.Scan0.ToPointer();
                byte* dst = (byte*)bitmapDataDst.Scan0.ToPointer();

                int r, g, b;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        b = *(src++);
                        g = *(src++);
                        r = *(src++);
                        byte gray = (byte)((r + b + g) / 3);

                        if (gray > 220)
                        {
                            bw = 255;
                        }
                        else
                        {
                            bw = 0;
                        }
                        *(dst++) = (byte)bw;
                        *(dst++) = (byte)bw;
                        *(dst++) = (byte)bw;
                    }
                    src += padding;
                    dst += padding;
                }
            }

            imageBitmapSrc.UnlockBits(bitmapDataSrc);
            imageBitmapDest.UnlockBits(bitmapDataDst);
        }

        private unsafe bool CanRemoveFirstSubIteration(byte* src, int stride, int pixelSize, int x, int y)
        {
            int[] neighbors = GetNeighbors(src, stride, pixelSize, x, y);
            int countBlackNeighbors = neighbors.Count(n => n == 1);
            int transitions = CountTransitions(neighbors);

            return countBlackNeighbors >= 2 && countBlackNeighbors <= 6 && transitions == 1 &&
               (neighbors[0] == 0 || neighbors[2] == 0 || neighbors[6] == 0) &&  // Verificando P2, P4, P8
               (neighbors[0] == 0 || neighbors[4] == 0 || neighbors[6] == 0);    // Verificando P2, P6, P8
        }

        private unsafe bool CanRemoveSecondSubIteration(byte* src, int stride, int pixelSize, int x, int y)
        {
            int[] neighbors = GetNeighbors(src, stride, pixelSize, x, y);
            int countBlackNeighbors = neighbors.Count(n => n == 1);
            int transitions = CountTransitions(neighbors);

            return countBlackNeighbors >= 2 && countBlackNeighbors <= 6 && transitions == 1 &&
               (neighbors[0] == 0 || neighbors[2] == 0 || neighbors[4] == 0) &&  // Verificando P2, P4, P6
               (neighbors[2] == 0 || neighbors[4] == 0 || neighbors[6] == 0);    // Verificando P4, P6, P8
        }

        private unsafe int[] GetNeighbors(byte* src, int stride, int pixelSize, int x, int y)
        {
            return new int[]
            {
                GetPixelIntensity(src, stride, pixelSize, x, y - 1),   // P2
                GetPixelIntensity(src, stride, pixelSize, x + 1, y - 1), // P3
                GetPixelIntensity(src, stride, pixelSize, x + 1, y),   // P4
                GetPixelIntensity(src, stride, pixelSize, x + 1, y + 1), // P5
                GetPixelIntensity(src, stride, pixelSize, x, y + 1),   // P6
                GetPixelIntensity(src, stride, pixelSize, x - 1, y + 1), // P7
                GetPixelIntensity(src, stride, pixelSize, x - 1, y),   // P8
                GetPixelIntensity(src, stride, pixelSize, x - 1, y - 1)  // P9
            };
        }

        private unsafe int GetPixelIntensity(byte* src, int stride, int pixelSize, int x, int y)
        {
            int pos = y * stride + x * pixelSize;
            int r = src[pos + 2];
            int g = src[pos + 1];
            int b = src[pos];
            return (r + g + b) / 3 <= 220 ? 1 : 0;
        }

        private int CountTransitions(int[] neighbors)
        {
            int count = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (neighbors[i] == 0 && neighbors[(i + 1) % neighbors.Length] == 1)
                {
                    count++;
                }
            }
            return count;
        }
    }
}