using Godot;
using System;
using System.Collections.Generic;

// ContourDrawer is a custom Node2D that generates contour lines from a heightmap texture.
public partial class ContourDrawer : Node2D
{
    [Export]
    Theme theme;
    [Export]
    float heightMultiplier = 1.0f;
    [Export]
    private Texture2D heightMap;
    private Image heightMapData;
    [Export]
    private Color lineCollour;
    [Export(PropertyHint.Range, "0.01,3.0,")]
    private float lineWidth = 1.0f;
    [Export]
    private int contourInterval = 4;
    [Export]
    private int stepSize = 3;
    [Export]
    private float smallestAllowedRadius = 200.0f;
    private int width;
    private int height;
    List<ContourLine> contourLines;
    
    public override void _Ready()
    {
        // Load the heightmap and initialize the width and height.
        if (heightMap == null)
        {
            GD.PrintErr("Failed to load HeightMap image");
            return;
        }
        else
        {
            heightMapData = heightMap.GetImage();
            width = heightMapData.GetWidth();
            height = heightMapData.GetHeight();
            contourLines = new List<ContourLine>();
            DrawContours(); // Generate contour lines from the heightmap.
        }
    }

    // Called every frame during the game loop.
    public override void _Process(double delta)
    {
        base._Process(delta);
        
        int lineHeight = 250;
        // Update contour lines' properties like width and color.
        foreach (ContourLine contourLine in contourLines)
        {
            contourLine.Width = lineWidth;
            contourLine.DefaultColor = lineCollour;

            if (contourLine.GetHeight() == (lineHeight-(contourInterval*5)))
            {
                contourLine.Width = lineWidth*3;
                lineHeight = lineHeight-(contourInterval*5);
            }
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    // Generates contour lines from the heightmap.
    public void DrawContours()
    {
        // Separate heightmap points into lists based on their heights.
        List<List<Line>> linesByHeight = new List<List<Line>>();

        // Loop through height values to generate contour lines at different heights.
        for (float isoValue = 250; isoValue > 0; isoValue -= contourInterval)
        {
            List<Line> linesAtHeight = new List<Line>();

            // Loop through heightmap points and create line segments for contour lines.
            for (int y = 0; y < height - stepSize; y += stepSize)
            {
                for (int x = 0; x < width - stepSize; x += stepSize)
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
        }

        // Merge continuous line segments to form contour lines.
        foreach (List<Line> lineList in linesByHeight)
        {
            MergeContinuousLines(lineList);
        }

        // Add the generated contour lines as children of the node.
        foreach (ContourLine contourLine in contourLines)
        {
            AddChild(contourLine);
        }
    }

    // Merges continuous line segments to form complete contour lines.
    public void MergeContinuousLines(List<Line> lines)
    {
        float height = lines[0].GetHeight();
        ContourLine contourLine = new ContourLine();
        contourLine.AddPoint(lines[0].GetStart(), 0);
        float searchRadius = stepSize / 2;

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
                if (contourLine.GetPointCount() > 3 && (contourLine.GetArea() > smallestAllowedRadius))
                {
                    contourLine.SetTheme(theme);
                    contourLine.RemoveKnots();
                    contourLine.CubicBezier();
                    contourLine.QuadraticBezier();
                    contourLine.SetHeight(height);
                    contourLine.AddHeightLabels();
                    contourLines.Add(contourLine);
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
        Vector2 b = new Vector2(x + stepSize, y);
        Vector2 c = new Vector2(x + stepSize, y + stepSize);
        Vector2 d = new Vector2(x, y + stepSize);

        // Get the height values at the four points.
        float a_f = heightMapData.GetPixel(x, y).R8;
        float b_f = heightMapData.GetPixel(x + stepSize, y).R8;
        float c_f = heightMapData.GetPixel(x + stepSize, y + stepSize).R8;
        float d_f = heightMapData.GetPixel(x, y + stepSize).R8;

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
            Line line = new Line(q, p, isoValue);
            lines.Add(line);
        }
        if (caseId == LineShapes.BottomRight || caseId == LineShapes.AllButButtomRight)
        {
            q = new Vector2(b.X, b.Y);
            p = new Vector2(d.X, d.Y);
            q.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
            p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
            Line line = new Line(q, p, isoValue);
            lines.Add(line);
        }
        if (caseId == LineShapes.Bottom || caseId == LineShapes.Top)
        {
            q = new Vector2(a.X, a.Y);
            p = new Vector2(b.X, b.Y);
            q.Y = a.Y + (d.Y - a.Y) * ((isoValue - a_f) / (d_f - a_f));
            p.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
            Line line = new Line(q, p, isoValue);
            lines.Add(line);
        }
        if (caseId == LineShapes.TopRight || caseId == LineShapes.AllButTopRight)
        {
            q = new Vector2(c.X, c.Y);
            p = new Vector2(a.X, a.Y);
            q.Y = c.Y + (b.Y - c.Y) * ((isoValue - c_f) / (b_f - c_f));
            p.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
            Line line = new Line(q, p, isoValue);
            lines.Add(line);
        }
        if (caseId == LineShapes.Right || caseId == LineShapes.Left)
        {
            q = new Vector2(a.X, a.Y);
            p = new Vector2(d.X, d.Y);
            q.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
            p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
            Line line = new Line(q, p, isoValue);
            lines.Add(line);

        }
        if (caseId == LineShapes.TopLeft || caseId == LineShapes.AllButTopLeft)
        {
            q = new Vector2(d.X, d.Y);
            p = new Vector2(b.X, b.Y);
            q.Y = d.Y + (a.Y - d.Y) * ((isoValue - d_f) / (a_f - d_f));
            p.X = b.X + (a.X - b.X) * ((isoValue - b_f) / (a_f - b_f));
            Line line = new Line(q, p, isoValue);
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
                Line line = new Line(q, p, isoValue);
                lines.Add(line);
                q = new Vector2(c.X, c.Y);
                p = new Vector2(d.X, d.Y);
                q.Y = c.Y + (b.Y - c.Y) * ((isoValue - c_f) / (b_f - c_f));
                p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
                line = new Line(q, p, isoValue);
                lines.Add(line);
            }
            else
            {
                q = new Vector2(b.X, b.Y);
                p = new Vector2(a.X, a.Y);
                q.Y = b.Y + (c.Y - b.Y) * ((isoValue - b_f) / (c_f - b_f));
                p.X = a.X + (b.X - a.X) * ((isoValue - a_f) / (b_f - a_f));
                Line line = new Line(q, p, isoValue);
                lines.Add(line);
                q = new Vector2(d.X, d.Y);
                p = new Vector2(c.X, c.Y);
                q.Y = d.Y + (a.Y - d.Y) * ((isoValue - d_f) / (a_f - d_f));
                p.X = c.X + (d.X - c.X) * ((isoValue - c_f) / (d_f - c_f));
                line = new Line(q, p,  isoValue);
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
                Line line = new Line(q, p, isoValue);
                lines.Add(line);
                q = new Vector2(d.X, d.Y);
                p = new Vector2(c.X, c.Y);
                q.Y = d.Y + (a.Y - d.Y) * ((isoValue - d_f) / (a_f - d_f));
                p.X = c.X + (d.X - c.X) * ((isoValue - c_f) / (d_f - c_f));
                line = new Line(q, p, isoValue);
                lines.Add(line);
            }
            else
            {
                q = new Vector2(a.X, a.Y);
                p = new Vector2(b.X, b.Y);
                q.Y = a.Y + (d.Y - a.Y) * ((isoValue - a_f) / (d_f - a_f));
                p.X = b.X + (a.X - b.X) * ((isoValue - b_f) / (a_f - b_f));
                Line line = new Line(q, p, isoValue);
                lines.Add(line);
                q = new Vector2(c.X, c.Y);
                p = new Vector2(d.X, d.Y);
                q.Y = c.Y + (b.Y - c.Y) * ((isoValue - c_f) / (b_f - c_f));
                p.X = d.X + (c.X - d.X) * ((isoValue - d_f) / (c_f - d_f));
                line = new Line(q, p, isoValue);
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
