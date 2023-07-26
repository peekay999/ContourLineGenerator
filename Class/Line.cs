using Godot;
using System;
using System.Collections.Generic;

// Represents a 2D line segment defined by its start and end points.
public class Line 
{
    private Vector2 Start;
    private Vector2 End;

    // Constructs a new Line object with the specified start and end points.
    public Line(Vector2 Start, Vector2 End)
    {
        this.Start = Start;
        this.End = End;
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

}
