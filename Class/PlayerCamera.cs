using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
	private float max_zoom = 8.0f;
	private float min_zoom = 1.0f;
	[Export]
	private float zoom_increment = 0.1f;
	private Vector2 velocity = Vector2.Zero;
	private float Speed = 10f;
	private Vector2 dragStartPosition;
	private Vector2 cameraStartPosition;
	private bool cameraLock = false;
	private string customInputAction_middleMouse = "middle_mouse";
	private string customInput_zoomOut = "zoom_out";
	private string customInput_zoomIn = "zoom_in";

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed(customInputAction_middleMouse))
		{
			Vector2 mousePos = GetGlobalMousePosition();
			Vector2 deltaPos = mousePos - dragStartPosition;
			Position -= deltaPos;
		}
	}

		public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed(customInputAction_middleMouse) & !cameraLock)
		{
			dragStartPosition = GetGlobalMousePosition();
			cameraStartPosition = Position;
			cameraLock = true;
		}
		if (@event.IsActionReleased(customInputAction_middleMouse))
		{
			cameraLock = false;
		}

		if(@event.IsAction(customInput_zoomOut) & (this.Zoom.X >= min_zoom))
		{
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 mouseDifference = mousePosition - Position;
			Position -= mouseDifference/(max_zoom/(zoom_increment));
			Zoom -= new Vector2(zoom_increment, zoom_increment);
		}
		if(@event.IsAction(customInput_zoomIn) & (this.Zoom.X <= max_zoom))
		{
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 mouseDifference = mousePosition - Position;
			Position += mouseDifference/(max_zoom/(zoom_increment*5));
			Zoom += new Vector2(zoom_increment, zoom_increment);
		}
	}

	
}
