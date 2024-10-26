using Godot;
using System;

[GlobalClass]
public partial class Bullet : RigidBody2D, IDamageable
{
	[Export] float damage;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnBulletBodyEntered(Node body)
	{
		if (body is IDamageable && !(body is Player))
		{
			(body as IDamageable).ChangeHealth(-damage);
			QueueFree();
		}
	}
	
	public float Health {get; private set;} = 1.0f;
	
	public void ChangeHealth(float hp) {
		Health -= hp;
		if (Health <= 0) {
			Kill();
		}
	}
	
	public void Kill()
    {
        QueueFree();
    }
}
