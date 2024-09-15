using System.Drawing.Imaging;
using System.Drawing;

namespace Trabalho_1Bim
{
    public class ContourExtraction
    {
        public unsafe void Contour(Bitmap imageBitmapSrc, Bitmap imageBitmapDest)
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

            // Inicializando a imagem de destino como branca
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pos = y * stride + x * pixelSize;
                    dst[pos] = dst[pos + 1] = dst[pos + 2] = 255;
                }
            }

            // matriz para marcar os pixels visitados 
            bool[,] visited = new bool[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (src[y * stride + x * pixelSize] == 255 && !visited[y, x])
                    {
                        // Verifica se o pixel branco tem pelo menos um vizinho preto (é um pixel de contorno)
                        if (IsContourPixel(src, x, y, width, height, stride, pixelSize))
                        {
                            // Novo ponto inicial de contorno
                            FollowContour(src, dst, visited, x, y, width, height, stride, pixelSize);
                        }
                        else
                        {
                            visited[y, x] = true;
                        }
                    }
                }
            }

            imageBitmapSrc.UnlockBits(bitmapDataSrc);
            imageBitmapDest.UnlockBits(bitmapDataDest);
        }

        private unsafe void FollowContour(byte* src, byte* dst, bool[,] visited, int x, int y,
                                  int width, int height, int stride, int pixelSize)
        {
            int startX = x;
            int startY = y;
            int maxIterations = width * height;
            int iterations = 0;
            bool firstIteration = true;
            bool moved = true;

            while (moved && (firstIteration || !(x == startX && y == startY)) && iterations <= maxIterations)
            {
                firstIteration = false;

                int pos = y * stride + x * pixelSize;
                dst[pos] = dst[pos + 1] = dst[pos + 2] = 0; // Preto (marca o contorno)
                visited[y, x] = true;

                moved = false;

                // Verificar os vizinhos em sentido anti-horário
                // Direção 0: Direita
                if (!moved &&
                    x + 1 < width &&
                    src[y * stride + (x + 1) * pixelSize] == 255 &&
                    !visited[y, x + 1] &&
                    IsContourPixel(src, x + 1, y, width, height, stride, pixelSize))
                {
                    x = x + 1;
                    moved = true;
                }
                // Direção 1: Baixo-Direita
                else if (!moved &&
                    x + 1 < width &&
                    y + 1 < height &&
                    src[(y + 1) * stride + (x + 1) * pixelSize] == 255 &&
                    !visited[y + 1, x + 1] &&
                    IsContourPixel(src, x + 1, y + 1, width, height, stride, pixelSize))
                {
                    x = x + 1;
                    y = y + 1;
                    moved = true;
                }
                // Direção 2: Abaixo
                else if (!moved &&
                    y + 1 < height &&
                    src[(y + 1) * stride + x * pixelSize] == 255 &&
                    !visited[y + 1, x] &&
                    IsContourPixel(src, x, y + 1, width, height, stride, pixelSize))
                {
                    y = y + 1;
                    moved = true;
                }
                // Direção 3: Baixo-Esquerda
                else if (!moved &&
                    x - 1 >= 0 &&
                    y + 1 < height &&
                    src[(y + 1) * stride + (x - 1) * pixelSize] == 255 &&
                    !visited[y + 1, x - 1] &&
                    IsContourPixel(src, x - 1, y + 1, width, height, stride, pixelSize))
                {
                    x = x - 1;
                    y = y + 1;
                    moved = true;
                }
                // Direção 4: Esquerda
                else if (!moved &&
                    x - 1 >= 0 &&
                    src[y * stride + (x - 1) * pixelSize] == 255 &&
                    !visited[y, x - 1] &&
                    IsContourPixel(src, x - 1, y, width, height, stride, pixelSize))
                {
                    x = x - 1;
                    moved = true;
                }
                // Direção 5: Cima-Esquerda
                else if (!moved &&
                    x - 1 >= 0 &&
                    y - 1 >= 0 &&
                    src[(y - 1) * stride + (x - 1) * pixelSize] == 255 &&
                    !visited[y - 1, x - 1] &&
                    IsContourPixel(src, x - 1, y - 1, width, height, stride, pixelSize))
                {
                    x = x - 1;
                    y = y - 1;
                    moved = true;
                }
                // Direção 6: Acima
                else if (!moved &&
                    y - 1 >= 0 &&
                    src[(y - 1) * stride + x * pixelSize] == 255 &&
                    !visited[y - 1, x] &&
                    IsContourPixel(src, x, y - 1, width, height, stride, pixelSize))
                {
                    y = y - 1;
                    moved = true;
                }
                // Direção 7: Cima-Direita
                else if (!moved &&
                    x + 1 < width &&
                    y - 1 >= 0 &&
                    src[(y - 1) * stride + (x + 1) * pixelSize] == 255 &&
                    !visited[y - 1, x + 1] &&
                    IsContourPixel(src, x + 1, y - 1, width, height, stride, pixelSize))
                {
                    x = x + 1;
                    y = y - 1;
                    moved = true;
                }

                iterations++;
            }
        }

        private unsafe bool IsContourPixel(byte* src, int x, int y, int width, int height, int stride, int pixelSize)
        {
            int pos = y * stride + x * pixelSize;

            if (src[pos] != 255)
                return false;

            int[,] neighborOffsets = new int[,]
            {
                { 1, 0 },   // Direita
                { 1, 1 },   // Baixo-Direita
                { 0, 1 },   // Abaixo
                { -1, 1 },  // Baixo-Esquerda
                { -1, 0 },  // Esquerda
                { -1, -1 }, // Cima-Esquerda
                { 0, -1 },  // Acima
                { 1, -1 }   // Cima-Direita
            };

            bool isContour = false;
            for (int i = 0; i < 8 && !isContour; i++)
            {
                int nx = x + neighborOffsets[i, 0];
                int ny = y + neighborOffsets[i, 1];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    int nPos = ny * stride + nx * pixelSize;
                    if (src[nPos] == 0)
                    {
                        isContour = true;
                    }
                }
            }

            return isContour;
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