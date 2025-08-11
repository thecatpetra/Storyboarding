namespace FastDrawing.Sliders;

public struct Point(float x, float y)
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;

    public static Point operator +(Point self, Point other)
    {
        return new Point(self.X + other.X, self.Y + other.Y);
    }

    public static Point operator -(Point self, Point other)
    {
        return new Point(self.X - other.X, self.Y - other.Y);
    }

    public static Point operator *(float num, Point other)
    {
        return new Point(num * other.X, num * other.Y);
    }

    public static Point operator *(Point other, float num)
    {
        return new Point(num * other.X, num * other.Y);
    }
    // let inline lengthSquared (vx, vy) (wx, wy) =
    // (vx - wx) * (vx - wx) + (vy - wy) * (vy - wy)
    //
    private static float LengthSquared(Point self, Point other)
    {
        return (self.X - other.X) * (self.X - other.X) + (self.Y - other.Y) * (self.Y - other.Y);
    }
    // let inline dot (vx, vy) (wx, wy) =
    // vx * wx + vy * wy
    private static float DotProduct(Point self, Point other)
    {
        return (self.X * other.X) + (self.Y * other.Y);
    }

    // let inline distancePP (vx, vy) (wx, wy) =
    // lengthSquared (vx, vy) (wx, wy) |> sqrt
    private static float Distance(Point self, Point other)
    {
        return MathF.Sqrt(LengthSquared(self, other));
    }

    public override string ToString() => $"({X}, {Y})";
}