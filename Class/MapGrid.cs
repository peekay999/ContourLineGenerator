using Godot;
using System;
using System.Collections.Generic;

public partial class MapGrid : Control
{
	private float Width;
	private float Height;
	private List<Line2D> GridLines;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GridLines = new List<Line2D>();
		
		ContourDrawer contourDrawer = GetChild<ContourDrawer>(0);
		GD.Print(contourDrawer.GetWidth());

		int Width = contourDrawer.GetWidth();
		int Height = contourDrawer.GetHeight();

		//Add eastings
		for (int i = 0; i < Width; i += 1000)
		{
			Line2D easting = new Line2D();
			easting.AddPoint(new Vector2(i,0));
			easting.AddPoint(new Vector2(i,Height));
			GridLines.Add(easting);

		}

		//Add northings
		for (int i = 0; i < Height; i += 1000)
		{
			Line2D easting = new Line2D();
			easting.AddPoint(new Vector2(0,i));
			easting.AddPoint(new Vector2(Width,i));
			GridLines.Add(easting);
		}
		foreach (Line2D gridLine in GridLines)
		{
			gridLine.Width = 1.5f;
			gridLine.DefaultColor = Colors.Black;
			gridLine.ZIndex = 5;
			AddChild(gridLine);
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
