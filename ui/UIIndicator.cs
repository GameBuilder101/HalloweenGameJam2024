using Godot;
using System;

[GlobalClass]
public partial class UIIndicator : Sprite2D
{
	[Export] Node2D symbol;
	[Export] bool blackHole;
	[Export] float radius;
	[Export] Node2D player;
	[Export] Node2D target;

	Vector2 direction;
	float angle;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GameManager.Instance.Player;
		if (blackHole)
			target = GameManager.Instance.BlackHole;
		else
			target = GameManager.Instance.DepositPoint;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		direction = target.Position - player.Position;
		angle = direction.Angle() + Mathf.Pi / 2;
		if (Rotation != angle)
		{
			Rotation = angle;
			symbol.Rotation = -angle;
		}
		if (direction.Length() <= radius && Visible)
		{
			Visible = false;
			symbol.Visible = false;
		}
		else if (direction.Length() > radius && !Visible)
		{
			Visible = true;
			symbol.Visible = true;
		}
	}
}
