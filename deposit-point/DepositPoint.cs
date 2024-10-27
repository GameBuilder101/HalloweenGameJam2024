using Godot;
using System;

[GlobalClass]
public partial class DepositPoint : RigidBody2D
{
	[Export]
	private float _speed;
	public float OrbitRadius { get { return GameManager.Instance.GameBoundsRadius * 0.85f; } }
    [Export]
	public float CurrentObritAngle { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		CurrentObritAngle *= Mathf.Pi;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		CurrentObritAngle -= _speed * (float)delta;

        Vector2 newPos = new Vector2(Mathf.Cos(CurrentObritAngle), Mathf.Sin(CurrentObritAngle)) * OrbitRadius;
        MoveAndCollide(newPos - Position);
		Position = newPos;
		Rotation = CurrentObritAngle - Mathf.Pi * 0.5f;
	}
}
