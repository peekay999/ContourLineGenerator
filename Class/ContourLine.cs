using Godot;
using System;

public partial class ContourLine : Line2D
{

	private float height = 0.0f;
	private float area = 0.0f;

	public float getHeight()
	{
		return height;
	} 

	public void setHeight(float height)
	{
		this.height = height;
	}
	public float getArea()
	{
		for (int i = 0; i < GetPointCount(); i++)
		{
			area += GetPointPosition(i).DistanceSquaredTo(GetPointPosition(0));
		}
		area = area/GetPointCount();
		return area;
	}

	public void quadraticBezier()
	{
		for (int i = 0; i < GetPointCount()-3; i++)
		{
			float t = 0.5f;
			Vector2 p0 = GetPointPosition(i);
			Vector2 p1 = GetPointPosition(i+1);
			Vector2 p2 = GetPointPosition(i+2);
			Vector2 q0 = p0.Lerp(p1, t);
			Vector2 q1 = p1.Lerp(p2, t);
			Vector2 r = q0.Lerp(q1, t);

			RemovePoint(i+1);
			AddPoint(r, i+1);
		}
	}

	public void cubicBezier()
	{
		for (int i = 0; i < GetPointCount()-4; i++)
		{
			float t = 0.5f;
			Vector2 p0 = GetPointPosition(i);
			Vector2 p1 = GetPointPosition(i+1);
			Vector2 p2 = GetPointPosition(i+2);
			Vector2 p3 = GetPointPosition(i+3);
			Vector2 q0 = p0.Lerp(p1, t);
			Vector2 q1 = p1.Lerp(p2, t);
			Vector2 q2 = p2.Lerp(p3, t);
			Vector2 r0 = q0.Lerp(q1, t);
			Vector2 r1 = q1.Lerp(q2, t);
			Vector2 s = r0.Lerp(r1, t);

			RemovePoint(i+1);
			RemovePoint(i+2);
			AddPoint(s, i+1);
		} 
	}
}
