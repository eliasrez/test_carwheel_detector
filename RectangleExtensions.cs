using SixLabors.ImageSharp;

public static class RectangleExtensions
{
    public static PointF[] ToPointFArray(this Rectangle rect)
    {
        return new PointF[]
        {
            new PointF(rect.Left, rect.Top),              // Top-left
            new PointF(rect.Right, rect.Top),             // Top-right
            new PointF(rect.Right, rect.Bottom),          // Bottom-right
            new PointF(rect.Left, rect.Bottom)            // Bottom-left
        };
    }
}