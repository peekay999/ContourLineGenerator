using Godot;
using System;

// Represents a custom 2D contour line, extending the functionality of Line2D.
public partial class ContourLine : Line2D
{
    private float Height = 0.0f;
    private float Area = 0.0f;

    // Gets the height of the contour line.
    public float GetHeight()
    {
        return Height;
    }

    // Sets the height of the contour line.
    public void SetHeight(float height)
    {
        this.Height = height;
    }

    // Calculates and returns the area of the contour line, which is the accumulated distance to the first point.
    public float GetArea()
    {
        for (int i = 0; i < GetPointCount(); i++)
        {
            Area += GetPointPosition(i).DistanceSquaredTo(GetPointPosition(0));
        }
        Area = Area / GetPointCount();
        return Area;
    }

    // Applies quadratic Bezier interpolation to the contour line, smoothing the segments.
    public void QuadraticBezier()
    {
        for (int i = 0; i < GetPointCount() - 3; i++)
        {
            float t = 0.5f;
            Vector2 p0 = GetPointPosition(i);
            Vector2 p1 = GetPointPosition(i + 1);
            Vector2 p2 = GetPointPosition(i + 2);
            Vector2 q0 = p0.Lerp(p1, t);
            Vector2 q1 = p1.Lerp(p2, t);
            Vector2 r = q0.Lerp(q1, t);

            RemovePoint(i + 1);
            AddPoint(r, i + 1);
        }
    }

    // Applies cubic Bezier interpolation to the contour line, smoothing the segments more aggressively.
    public void CubicBezier()
    {
        for (int i = 0; i < GetPointCount() - 4; i++)
        {
            float t = 0.5f;
            Vector2 p0 = GetPointPosition(i);
            Vector2 p1 = GetPointPosition(i + 1);
            Vector2 p2 = GetPointPosition(i + 2);
            Vector2 p3 = GetPointPosition(i + 3);
            Vector2 q0 = p0.Lerp(p1, t);
            Vector2 q1 = p1.Lerp(p2, t);
            Vector2 q2 = p2.Lerp(p3, t);
            Vector2 r0 = q0.Lerp(q1, t);
            Vector2 r1 = q1.Lerp(q2, t);
            Vector2 s = r0.Lerp(r1, t);

            RemovePoint(i + 1);
            RemovePoint(i + 2);
            AddPoint(s, i + 1);
        }
    }

    // Placeholder method for removing knots from the contour line.
    public void RemoveKnots()
    {
        // The implementation to remove knots should be added here.
        // As it stands, the method doesn't perform any action on the contour line points.
    }
}