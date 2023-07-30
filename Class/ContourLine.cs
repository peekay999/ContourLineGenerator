using Godot;
using System;
using System.Collections.Generic;

// Represents a custom 2D contour line, extending the functionality of Line2D.
public partial class ContourLine : Line2D
{
    private float height = 0.0f;
    private float area = 0.0f;
    List<Label> heightLabels = new List<Label>();
    Theme theme = new Theme();
    
    // Gets the height of the contour line.
    public float GetHeight()
    {
        return height;
    }

    // Sets the height of the contour line.
    public void SetHeight(float height)
    {
        this.height = height;
        JointMode = LineJointMode.Round;
    }

    // Sets the theme for the contour line. Used for the test labels for height indication.
    public void SetTheme(Theme theme)
    {
        this.theme = theme;
    }

    // Calculates and returns the area of the contour line, which is the accumulated distance to the first point.
    public float GetArea()
    {
        for (int i = 0; i < GetPointCount(); i++)
        {
            area += GetPointPosition(i).DistanceSquaredTo(GetPointPosition(0));
        }
        area = area / GetPointCount();
        return area;
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

    public void AddHeightLabels()
    {
        heightLabels.Clear();
        Label heightLabel = new Label();
        heightLabel.ZIndex = 2;
        heightLabel.Theme = theme;
        int indexPosition = GetPointCount()/2;
        Vector2 position = GetPointPosition(indexPosition);
        heightLabel.SetGlobalPosition(position);
        heightLabel.Text = height.ToString();
        AddChild(heightLabel);
    }

    // Removes knots in the contour line. A knot is a section of the line that loops back on itself.
    public void RemoveKnots()
    {
        for (int i = 0; i < GetPointCount(); i++)
        {
            for (int j = i + 1; j < GetPointCount(); j++)
            {
                if (GetPointPosition(i) == GetPointPosition(j) && i > 4)
                {
                    // A knot is found, find the range of points that form the loop
                    int loopStart = i;
                    int loopEnd = j;

                    // Remove the range of points that form the loop
                    for (int k = loopStart + 1; k <= loopEnd; k++)
                    {
                        RemovePoint(loopStart + 1);
                    }

                    // Continue checking for more knots from the next point after the loop
                    break;
                }
            }
        }
    }
}