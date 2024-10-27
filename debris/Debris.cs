using Godot;
using System;

public partial class Debris : RigidBody2D, IDamageable
{
	public float Health { get; private set; } = 1.0f;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// if the debris goes outside the game radius, destroy it
		if (Position.Length() > GameManager.Instance.GameBoundsRadius + 800.0f)
		{
			GameManager.Instance.Debris.Remove(this);
			QueueFree();
		}
	}
	
	public void ChangeHealth(float value)
	{
		Health += value;
		if (Health <= 0.0f)
			Kill();
	}

	public void Kill()
	{
		GameManager.Instance.Debris.Remove(this);
		QueueFree();
	}
}
