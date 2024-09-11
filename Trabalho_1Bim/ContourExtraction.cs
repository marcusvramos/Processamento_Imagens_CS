using System.Drawing.Imaging;
using System.Drawing;

namespace Trabalho_1Bim {
    public class ContourExtraction
    {
        public unsafe void Countour(Bitmap imageBitmapSrc, Bitmap imageBitmapDest)
        {
            ConvertPretoBranco(imageBitmapSrc);
            int width = imageBitmapSrc.Width;
            int height = imageBitmapSrc.Height;
            int pixelSize = 3;

            BitmapData bitmapDataSrc = imageBitmapSrc.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData bitmapDataDest = imageBitmapDest.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = bitmapDataSrc.Stride;
            byte* src = (byte*)bitmapDataSrc.Scan0;
            byte* dst = (byte*)bitmapDataDest.Scan0;

            // iniciando img destino como branca
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = y * stride + x * pixelSize;
                    dst[pos] = dst[pos + 1] = dst[pos + 2] = 255;
                }
            }

            // percorrendo a imagem
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsBlackPixel(src, width, height, stride, x, y))
                    {
                        // verificando os 8 pixels adjacentes
                        PaintIfWhite(dst, src, width, height, stride, x + 1, y); // Direita
                        PaintIfWhite(dst, src, width, height, stride, x - 1, y); // Esquerda
                        PaintIfWhite(dst, src, width, height, stride, x, y + 1); // Abaixo
                        PaintIfWhite(dst, src, width, height, stride, x, y - 1); // Acima
                        PaintIfWhite(dst, src, width, height, stride, x + 1, y + 1); // Diagonal baixo-direita
                        PaintIfWhite(dst, src, width, height, stride, x - 1, y + 1); // Diagonal baixo-esquerda
                        PaintIfWhite(dst, src, width, height, stride, x + 1, y - 1); // Diagonal cima-direita
                        PaintIfWhite(dst, src, width, height, stride, x - 1, y - 1); // Diagonal cima-esquerda
                    }
                }
            }

            imageBitmapSrc.UnlockBits(bitmapDataSrc);
            imageBitmapDest.UnlockBits(bitmapDataDest);
        }

        private unsafe bool IsBlackPixel(byte* src, int width, int height, int stride, int x, int y)
        {
            int pixelSize = 3;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            int pos = y * stride + x * pixelSize;
            return (src[pos] == 0);
        }

        private unsafe void PaintIfWhite(byte* dst, byte* src, int width, int height, int stride, int x, int y)
        {
            int pixelSize = 3;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;

            int pos = y * stride + x * pixelSize;
            if (src[pos] == 255)
            {
                dst[pos] = dst[pos + 1] = dst[pos + 2] = 0; // preto
            }
        }

        private unsafe void ConvertPretoBranco(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            int pixelSize = 3;

            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* src = (byte*)bitmapData.Scan0;
            int stride = bitmapData.Stride;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = y * stride + x * pixelSize;
                    byte r = src[pos + 2];
                    byte g = src[pos + 1];
                    byte b = src[pos];

                    byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);
                    byte binary = gray > 220 ? (byte)255 : (byte)0;

                    src[pos + 2] = src[pos + 1] = src[pos] = binary;
                }
            }

            image.UnlockBits(bitmapData);
        }
    }
}
