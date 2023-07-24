using Godot;
using System;
using System.Collections.Generic;

public class Line 
{
    private Vector2 start;
    private Vector2 end;

    public Line(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }

    public Vector2 getStart()
    {
        return start;
    }

    public Vector2 getEnd()
    {
        return end;
    }

}
