using Godot;
using System;
using System.Collections.Generic;

// Represents a 2D line segment defined by its start and end points.
public class Line 
{
    private Vector2 Start;
    private Vector2 End;
    private float Height;

    // Constructs a new Line object with the specified start and end points.
    public Line(Vector2 Start, Vector2 End, float Height)
    {
        this.Start = Start;
        this.End = End;
        this.Height = Height;
    }

    // Gets the starting point of the line segment.
    public Vector2 GetStart()
    {
        return Start;
    }

    // Gets the ending point of the line segment.
    public Vector2 GetEnd()
    {
        return End;
    }

    // Gets the height value of the line
    public float GetHeight()
    {
        return Height;
    }

}
