using Godot;
using System;

[GlobalClass]
public partial class Asteroid : RigidBody2D, IDamageable
{
	public float Health { get; private set; } = 1.0f;
	[Export]
	public double SpinOutDuration { get; set; } = 4.0;
	[Export] private PackedScene FuelScene;

	[Export]
	public int Score { get; set; } = 100;
	[Export]
	public int DepositBonusFuel { get; set; } = 0;

	[Export]
	public int FuelMultiplier { get; set; } = 1;

	[Export]
	private GpuParticles2D _killParticles;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		GameManager.Instance.Asteroids.Add(this);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		GameManager.Instance.Asteroids.Remove(this);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		// if the asteroid goes outside the game radius, destroy it
		if (Position.Length() > GameManager.Instance.GameBoundsRadius + 800.0f) 
		{
			GameManager.Instance.Asteroids.Remove(this);
			QueueFree();
		}
	}

	public void ChangeHealth(float value)
	{
		Health += value;
		if (Health <= 0.0f)
			Kill();
	}

	private static RandomNumberGenerator rng = new RandomNumberGenerator();

	public void Kill()
	{
		int numFuel = (int) (_Scale * 5 * FuelMultiplier);
		float maxVel = (_Scale * 100);
		for (int i = 0; i < numFuel; i++) {
			Fuel fuel = (Fuel) FuelScene.Instantiate();
			fuel.SetPosAndVel(
				Position,
				LinearVelocity + Vector2.FromAngle(
					rng.RandfRange(0, (float) (2 * Math.PI))) * (50 + rng.RandfRange(0, maxVel))
			);
			GetParent().CallDeferred("add_child", fuel);
		}

		_killParticles.Reparent(GetTree().Root);
		_killParticles.Emitting = true;
		QueueFree();
	}

	private void PickUpAreaBodyEntered(Node node)
	{
		if (Freeze || !(node is Player))
			return;
		((Player)node).SpinOut(SpinOutDuration);
	}
	
	private float _Scale = 0.0f;
	
	public void SetScale(float scale) {
		_Scale = scale;
		foreach(Node2D child in this.GetChildren())
		{
			child.Scale = new Vector2(child.Scale.X * scale, child.Scale.Y * scale);
		}
	}
}
