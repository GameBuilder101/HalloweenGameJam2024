using Godot;
using System;

[GlobalClass]
public partial class Asteroid : RigidBody2D, IDamageable
{
    public float Health { get; private set; } = 1.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void ChangeHealth(float value)
    {
        Health += value;
        if (Health <= 0.0f)
            Kill();
    }

    public void Kill()
    {
        QueueFree();
    }
}
