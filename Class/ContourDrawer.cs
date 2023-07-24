using Godot;
using System;
using System.Collections.Generic;

public partial class ContourDrawer : Node2D
{
    [Export]
    float heightMultiplier = 1.0f;
    [Export]
    private Image heightMap;
    [Export]
    private Color lineColour;
    [Export(PropertyHint.Range, "0.01,3.0,")]
    private float lineWidth = 1.0f;
    [Export]
    private int contourInterval = 4;
    [Export]
    private int stepSize = 3;
    private int width;
    private int height;
    List<Line2D> contourLines;

    public override void _Ready()
    {
        if (heightMap == null)
        {
            GD.PrintErr("Failed to load heightmap image");
            return;
        }
        else
        {
            width = heightMap.GetWidth();
            height = heightMap.GetHeight();
            contourLines = new List<Line2D>();
            drawContours();
        }
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach (Line2D contourLine in contourLines)
        {
            contourLine.Width = lineWidth;
            contourLine.DefaultColor = lineColour;
        }
    }
    public void drawContours()
    {
        List<List<Line>> linesByHeight = new List<List<Line>>(); ;

        for (float isoValue = 255; isoValue > 0; isoValue -= contourInterval)
        {
            List<Line> linesAtHeight = new List<Line>();
            for (int y = 0; y < height - stepSize; y += stepSize)
            {
                for (int x = 0; x < width - stepSize; x += stepSize)
                {
                    List<Line> lines = getLineCase(x, y, isoValue);
                    if (lines.Count > 0)
                    {
                        foreach (Line line in lines)
                        {
                            if (line.getStart() != line.getEnd()) // remove redundant single point lines
                            {
                                linesAtHeight.Add(line);
                                //DrawLine(line.getStart(), line.getEnd(), lineColour_iterate, lineWidth_iterate, lineAntiAliasing);
                            }
                        }
                    }
                }
            }
            if (linesAtHeight.Count != 0)
            {
                linesByHeight.Add(linesAtHeight);
            }
        }
        foreach (List<Line> lineList in linesByHeight)
        {
            mergeContinuousLines(lineList);
        }
        foreach (Line2D contourLine in contourLines)
        {
            if (contourLine.GetPointCount() > 1)
            {
                AddChild(contourLine);
                contourLine.Width = 0.4f;
                //contourLine.Gradient = new Gradient();
            }
        }
    }

    public void mergeContinuousLines(List<Line> lines)
    {
        Line2D contourLine = new Line2D();
        contourLine.AddPoint(lines[0].getStart(), 0);
        float searchRadius = stepSize/2;

        while (lines.Count > 0)
        {
            // contourLine.DefaultColor = Colors.Red;
            int indexToRemove = -1;
            Vector2 endPoint = contourLine.GetPointPosition(contourLine.GetPointCount() - 1);

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].getStart().DistanceTo(endPoint) < searchRadius)
                {
                    contourLine.AddPoint(lines[i].getEnd());
                    indexToRemove = i;
                    break;
                }
                else if (lines[i].getEnd().DistanceTo(endPoint) < searchRadius)
                {
                    contourLine.AddPoint(lines[i].getStart());
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove != -1)
            {
                lines.RemoveAt(indexToRemove);
            }
            else
            {
                // No continuous line found, move to the next line in the list
                contourLines.Add(contourLine);
                contourLine = new Line2D();
                contourLine.AddPoint(lines[0].getStart(), 0);
            }
        }
    }

    /*

    */
    private List<Line> getLineCase(int x, int y, float isoValue)
    {
        List<Line> lines = new List<Line>();
        Vector2 p;
        Vector2 q;

        Vector2 a = new Vector2(x, y);
        Vector2 b = new Vector2(x + stepSize, y);
        Vector2 c = new Vector2(x + stepSize, y + stepSize);
        Vector2 d = new Vector2(x, y + stepSize);

        float a_f = heightMap.GetPixel(x, y).R8;
        float b_f = heightMap.GetPixel(x + stepSize, y).R8;
        float c_f = heightMap.GetPixel(x + stepSize, y + stepSize).R8;
        float d_f = heightMap.GetPixel(x, y + stepSize).R8;

        LineShapes caseId = GetCaseId(a_f, b_f, c_f, d_f, isoValue);
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

    /*
    Takes the height value from 4 points in a square, then returns a binary value between 0-15 based on the number of points in the square above the isoValue.
    isoValue is based on the terrain height at which a contour line is being drawn.
    */
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
