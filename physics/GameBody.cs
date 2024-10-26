using Godot;
using System;

public abstract partial class GameBody : RigidBody2D, IDamageable
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public abstract void Die();
	
	public float Health { get; private set;}

	public void ChangeHealth(float health) {
		Health -= health;
		if (Health <= 0) {
			Die();
		}
	}
}
