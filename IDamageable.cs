using Godot;
using System;

public interface IDamageable
{
	public float Health { get; }

	public void ChangeHealth(float health);
}
