using Godot;
using System;

[GlobalClass]
public partial class DynamicCamera : Camera2D
{
	[Export]
	private Node2D TrackTarget { get; set; }

	public float CurrentShakeDuration;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 pos = Position;
        pos.X = TrackTarget.Position.X;
        pos.Y = TrackTarget.Position.Y;
		Position = pos;
    }
}
