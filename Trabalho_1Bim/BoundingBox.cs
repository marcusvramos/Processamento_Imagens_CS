using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

public class ContourRectangle
{
    public void ProcessImageAndDrawRectangles(Bitmap imageSource, Bitmap imageDest)
    {
        bool[,] visited = new bool[imageSource.Width, imageSource.Height];
        List<List<(int x, int y)>> allContours = new List<List<(int x, int y)>>();

        BitmapData bitmapData = imageSource.LockBits(new Rectangle(0, 0, imageSource.Width, imageSource.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        int stride = bitmapData.Stride;
        unsafe
        {
            byte* ptr = (byte*)bitmapData.Scan0;
            int width = imageSource.Width;
            int height = imageSource.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsBlack(ptr, x, y, stride) && !visited[x, y])
                    {
                        List<(int x, int y)> contour = new List<(int x, int y)>();
                        FindContour(ptr, x, y, visited, contour, width, height, stride);

                        if (contour.Count > 0)
                        {
                            allContours.Add(contour);
                        }
                    }
                }
            }
        }

        imageSource.UnlockBits(bitmapData);

        // Para cada conjunto de contornos (cada letra), desenhar um retângulo ao redor
        foreach (var contour in allContours)
        {
            var (minX, minY, maxX, maxY) = CalculateBoundingBox(contour);
            DrawBoundingBox(imageDest, minX, minY, maxX, maxY);
        }
    }

    public (int minX, int minY, int maxX, int maxY) CalculateBoundingBox(List<(int x, int y)> contourPoints)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var point in contourPoints)
        {
            if (point.x < minX) minX = point.x;
            if (point.x > maxX) maxX = point.x;
            if (point.y < minY) minY = point.y;
            if (point.y > maxY) maxY = point.y;
        }

        return (minX, minY, maxX, maxY);
    }

    public void DrawBoundingBox(Bitmap image, int minX, int minY, int maxX, int maxY)
    {
        using (Graphics g = Graphics.FromImage(image))
        {
            Pen redPen = new Pen(Color.Red, 1);
            g.DrawRectangle(redPen, minX, minY, maxX - minX, maxY - minY);
        }
    }

    public unsafe void FindContour(byte* ptr, int startX, int startY, bool[,] visited, List<(int x, int y)> contour, int width, int height, int stride)
    {
        Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
        stack.Push((startX, startY));

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y])
                continue;

            if (IsBlack(ptr, x, y, stride))
            {
                visited[x, y] = true;
                contour.Add((x, y));

                // Adiciona os vizinhos
                stack.Push((x - 1, y));
                stack.Push((x + 1, y));
                stack.Push((x, y - 1));
                stack.Push((x, y + 1));
            }
        }
    }

    private unsafe bool IsBlack(byte* ptr, int x, int y, int stride, int tolerance = 50)
    {
        byte* pixel = ptr + (y * stride) + (x * 3);

        return (pixel[0] < tolerance && pixel[1] < tolerance && pixel[2] < tolerance);
    }
}
