using Godot;
using System;
using System.Collections.Generic;

// ContourDrawer is a custom Node2D that generates contour lines from a heightmap texture.
public partial class ContourDrawer : Node2D
{
    [Export]
    float heightMultiplier = 1.0f;
    [Export]
    private Texture2D HeightMap;
    private Image HeightMapData;
    [Export]
    private Color LineCollour;
    [Export(PropertyHint.Range, "0.01,3.0,")]
    private float LineWidth = 1.0f;
    [Export]
    private int ContourInterval = 4;
    [Export]
    private int StepSize = 3;
    [Export]
    private float SmallestAllowedRadius = 200.0f;
    private int Width;
    private int Height;
    List<ContourLine> ContourLines;

    public override void _Ready()
    {
        // Load the heightmap and initialize the width and height.
        if (HeightMap == null)
        {
            GD.PrintErr("Failed to load HeightMap image");
            return;
        }
        else
        {
            HeightMapData = HeightMap.GetImage();
            Width = HeightMapData.GetWidth();
            Height = HeightMapData.GetHeight();
            ContourLines = new List<ContourLine>();
            drawContours(); // Generate contour lines from the heightmap.
        }
    }

    // Called every frame during the game loop.
    public override void _Process(double delta)
    {
        base._Process(delta);

        // Update contour lines' properties like width and color.
        foreach (ContourLine contourLine in ContourLines)
        {
            contourLine.Width = LineWidth;
            contourLine.DefaultColor = LineCollour;
        }
    }

    // Generates contour lines from the heightmap.
    public void drawContours()
    {
        // Separate heightmap points into lists based on their heights.
        List<List<Line>> linesByHeight = new List<List<Line>>();
        float isoValue = 255;

        // Loop through height values to generate contour lines at different heights.
        for (float i = 255; isoValue > 0; i -= ContourInterval)
        {
            List<Line> linesAtHeight = new List<Line>();

            // Loop through heightmap points and create line segments for contour lines.
            for (int y = 0; y < Height - StepSize; y += StepSize)
            {
                for (int x = 0; x < Width - StepSize; x += StepSize)
                {
                    List<Line> lines = GetLineCase(x, y, isoValue);
                    if (lines.Count > 0)
                    {
                        // Remove redundant single-point lines.
                        foreach (Line line in lines)
                        {
                            if (line.GetStart() != line.GetEnd())
                            {
                                linesAtHeight.Add(line);
                            }
                        }
                    }
                }
            }

            // If there are lines at this height, add them to the list.
            if (linesAtHeight.Count != 0)
            {
                linesByHeight.Add(linesAtHeight);
            }

            isoValue = i;
        }

        // Merge continuous line segments to form contour lines.
        foreach (List<Line> lineList in linesByHeight)
        {
            MergeContinuousLines(lineList, isoValue);
        }

        // Add the generated contour lines as children of the node.
        foreach (ContourLine contourLine in ContourLines)
        {
            AddChild(contourLine);
        }
    }

    // Merges continuous line segments to form complete contour lines.
    public void MergeContinuousLines(List<Line> lines, float height)
    {
        ContourLine contourLine = new ContourLine();
        contourLine.AddPoint(lines[0].GetStart(), 0);
        float searchRadius = StepSize / 2;

        while (lines.Count > 0)
        {
            // Check forward - find lines close to the end of the contour line.
            int indexToRemove = -1;
            Vector2 endPoint = contourLine.GetPointPosition(contourLine.GetPointCount() - 1);
            Vector2 startPoint = contourLine.GetPointPosition(0);

            // Check all lines for any that are close to the front of the contour line.
            for (int i = 0; i < lines.Count; i++)
            {
                // Add the next point and remove the line from the list.
                if (lines[i].GetStart().DistanceTo(endPoint) < searchRadius)
                {
                    contourLine.AddPoint(lines[i].GetEnd());
                    indexToRemove = i;
                    break;
                }
                else if (lines[i].GetEnd().DistanceTo(endPoint) < searchRadius)
                {
                    contourLine.AddPoint(lines[i].GetStart());
                    indexToRemove = i;
                    break;
                }
            }

            // If no lines found forward, check backward for lines near the start of the contour line.
            if (indexToRemove == -1)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].GetEnd().DistanceTo(startPoint) < searchRadius)
                    {
                        contourLine.AddPoint(lines[i].GetStart(), 0);
                        indexToRemove = i;
                        break;
                    }
                    else if (lines[i].GetStart().DistanceTo(startPoint) < searchRadius)
                    {
                        contourLine.AddPoint(lines[i].GetEnd(), 0);
                        indexToRemove = i;
                        break;
                    }
                }
            }

            // Remove the processed line from the list, or start a new contour line if none found.
            if (indexToRemove != -1)
            {
                lines.RemoveAt(indexToRemove);
            }
            else
            {
                // No continuous line found, check if the contour line is valid and add it to the list.
                if (contourLine.GetPointCount() > 3 && (contourLine.GetArea() > SmallestAllowedRadius))
                {
                    contourLine.CubicBezier();
                    contourLine.QuadraticBezier();
                    ContourLines.Add(contourLine);
                }

                // Start a new contour line.
                contourLine = new ContourLine();
                contourLine.AddPoint(lines[0].GetStart(), 0);
            }
        }
    }

    // Determines the line segments based on the height values at four points in a square.
    private List<Line> GetLineCase(int x, int y, float isoValue)
    {

        List<Line> lines = new List<Line>();
        Vector2 p;
        Vector2 q; 

        // Define the four points of the square.
        Vector2 a = new Vector2(x, y);
        Vector2 b = new Vector2(x + StepSize, y);
        Vector2 c = new Vector2(x + StepSize, y + StepSize);
        Vector2 d = new Vector2(x, y + StepSize);

        // Get the height values at the four points.
        float a_f = HeightMapData.GetPixel(x, y).R8;
        float b_f = HeightMapData.GetPixel(x + StepSize, y).R8;
        float c_f = HeightMapData.GetPixel(x + StepSize, y + StepSize).R8;
        float d_f = HeightMapData.GetPixel(x, y + StepSize).R8;

        // Determine the case ID based on the number of points above the isoValue.
        LineShapes caseId = GetCaseId(a_f, b_f, c_f, d_f, isoValue);

        // Based on the case ID, create line segments for the contour lines.
        // Add the lines to the list.
        if (caseId == LineShapes.BottomLeft || caseId == LineShapes.AllButButtomLeft)
        {
            q = new Vector2(a.X, a.Y);
            p = new Vector2(c.X, c.Y);
            q.Y = a.Y + (d.Y - a.Y) * ((isoValue - a_f) / (d_f - a_f));
            p.X = c.X + (d.X - c.X) * ((isoValue - c_f) / (d_f - c_f));
            Line line = new Line(q, p);
            lines.Add(line);
        }
        if (caseId == LineShapes.BottomRight || caseId == LineShapes.AllButButtomRight)
        {
            q = new Vector2(b.X, b.Y);
            p = new Vector2(d.X, d.Y);
            q.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
            p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
            Line line = new Line(q, p);
            lines.Add(line);
        }
        if (caseId == LineShapes.Bottom || caseId == LineShapes.Top)
        {
            q = new Vector2(a.X, a.Y);
            p = new Vector2(b.X, b.Y);
            q.Y = a.Y + (d.Y - a.Y) * ((isoValue - a_f) / (d_f - a_f));
            p.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
            Line line = new Line(q, p);
            lines.Add(line);
        }
        if (caseId == LineShapes.TopRight || caseId == LineShapes.AllButTopRight)
        {
            q = new Vector2(c.X, c.Y);
            p = new Vector2(a.X, a.Y);
            q.Y = c.Y + (b.Y - c.Y) * ((isoValue - c_f) / (b_f - c_f));
            p.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
            Line line = new Line(q, p);
            lines.Add(line);
        }
        if (caseId == LineShapes.Right || caseId == LineShapes.Left)
        {
            q = new Vector2(a.X, a.Y);
            p = new Vector2(d.X, d.Y);
            q.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
            p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
            Line line = new Line(q, p);
            lines.Add(line);

        }
        if (caseId == LineShapes.TopLeft || caseId == LineShapes.AllButTopLeft)
        {
            q = new Vector2(d.X, d.Y);
            p = new Vector2(b.X, b.Y);
            q.Y = d.Y + (a.Y - d.Y) * ((isoValue - d_f) / (a_f - d_f));
            p.X = b.X + (a.X - b.X) * ((isoValue - b_f) / (a_f - b_f));
            Line line = new Line(q, p);
            lines.Add(line);
        }
        if ((caseId == LineShapes.TopRightBottomLeft))
        {
            float resolver = (a_f * c_f) - (b_f * d_f);
            if (resolver > 0)
            {
                q = new Vector2(a.X, a.Y);
                p = new Vector2(b.X, b.Y);
                q.Y = a.Y + (d.Y - a.Y) * ((isoValue - a_f) / (d_f - a_f));
                p.X = b.X + (a.X - b.X) * ((isoValue - b_f) / (a_f - b_f));
                Line line = new Line(q, p);
                lines.Add(line);
                q = new Vector2(c.X, c.Y);
                p = new Vector2(d.X, d.Y);
                q.Y = c.Y + (b.Y - c.Y) * ((isoValue - c_f) / (b_f - c_f));
                p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
                line = new Line(q, p);
                lines.Add(line);
            }
            else
            {
                q = new Vector2(b.X, b.Y);
                p = new Vector2(a.X, a.Y);
                q.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
                p.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
                Line line = new Line(q, p);
                lines.Add(line);
                q = new Vector2(d.X, d.Y);
                p = new Vector2(c.X, c.Y);
                q.Y = d.Y + (a.Y - d.Y) * ((isoValue - d_f) / (a_f - d_f));
                p.X = c.X + (d.X - c.X) * ((isoValue - c_f) / (d_f - c_f));
                line = new Line(q, p);
                lines.Add(line);
            }

        }
        if ((caseId == LineShapes.TopLeftBottomRight))
        {
            float resolver = (a_f * c_f) - (b_f * d_f);
            if (resolver > 0)
            {
                q = new Vector2(b.X, b.Y);
                p = new Vector2(a.X, a.Y);
                q.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
                p.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
                Line line = new Line(q, p);
                lines.Add(line);
                q = new Vector2(d.X, d.Y);
                p = new Vector2(c.X, c.Y);
                q.Y = d.Y + (a.Y - d.Y) * ((isoValue - d_f) / (a_f - d_f));
                p.X = c.X + (d.X - c.X) * ((isoValue - c_f) / (d_f - c_f));
                line = new Line(q, p);
                lines.Add(line);
            }
            else
            {
                q = new Vector2(a.X, a.Y);
                p = new Vector2(b.X, b.Y);
                q.Y = a.Y + (d.Y - a.Y) * ((isoValue - a_f) / (d_f - a_f));
                p.X = b.X + (a.X - b.X) * ((isoValue - b_f) / (a_f - b_f));
                Line line = new Line(q, p);
                lines.Add(line);
                q = new Vector2(c.X, c.Y);
                p = new Vector2(d.X, d.Y);
                q.Y = c.Y + (b.Y - c.Y) * ((isoValue - c_f) / (b_f - c_f));
                p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
                line = new Line(q, p);
                lines.Add(line);
            }

        }
        if ((caseId == LineShapes.Empty))
        {
        }
        if ((caseId == LineShapes.All))
        {
        }
        return lines;
    }

    // Determines the case ID based on the height values at four points in a square.
    // Used to identify the type of line segments for contour generation.
    private LineShapes GetCaseId(float p1, float p2, float p3, float p4, float isoValue)
    {
        int caseId = 0;
        if (p1 >= isoValue)
        {
            caseId |= 8;
        }

        if (p2 >= isoValue)
        {
            caseId |= 4;
        }

        if (p3 >= isoValue)
        {
            caseId |= 2;
        }

        if (p4 >= isoValue)
        {
            caseId |= 1;
        }
        return (LineShapes)caseId;
    }

    // Enum defining all the possible cases for line segments in a square.
    internal enum LineShapes
    {
        Empty = 0,
        BottomLeft = 1,
        BottomRight = 2,
        Bottom = 3,
        TopRight = 4,
        TopRightBottomLeft = 5,
        Right = 6,
        AllButTopLeft = 7,
        TopLeft = 8,
        Left = 9,
        TopLeftBottomRight = 10,
        AllButTopRight = 11,
        Top = 12,
        AllButButtomRight = 13,
        AllButButtomLeft = 14,
        All = 15,
    }


}
