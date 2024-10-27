using Godot;
using System.Collections.Generic;
using System;

[GlobalClass]
public partial class Player : RigidBody2D, IDamageable
{
	public float Health { get; private set; } = 1.0f;

	[Export]
	public float MaxFuel { get; private set; }
	[Export]
	public float Fuel { get; private set; }

	public const float FuelParticleWorth = 1.0f;

	public bool IsFuelEmpty { get { return Fuel <= 0.0f; } }

	private bool Alive = true;
	[Export]
	private double DeathTimeDelay;
	private double TimeDead = 0.0f;
	
	[Export]
	public float Speed { get; set; }
	[Export]
	public float BoostSpeed { get; set; }
	[Export]
	private float _boostFuelConsumption = 2.0f;
	[Export]
	public float EmptySpeed { get; set; }
	[Export]
	public float AngularSpeed { get; set; }
	[Export]
	public float BreakDamp { get; set; }

	[Export]
	public double ShootDelay { get; private set; }
	[Export] private PackedScene _bulletScene;
	[Export] private float _bulletSpeed;
	private double _remainingShootDelay;

	public double RemainingSpinOutTime { get; private set; }
	public bool IsSpinningOut { get { return RemainingSpinOutTime > 0.0f; } }
	public bool isBoosting = false;
	private bool wasBoosting = false;
	[Export]
	private float SpinOutSpeed { get; set; }
	private float _origAngularDamp;

	private List<Asteroid> _asteroidsInRadius = new List<Asteroid>();
	public bool InRadiusOfAsteroid { get { return _asteroidsInRadius.Count > 0; } }
	public Asteroid PickedUpAsteroid { get; private set; }
	[Export]
	private Node2D _pickedUpPivot;
	
	[Export]
	private GpuParticles2D smoke;

	[Export]
	private PlayerAudio audio;

	public bool IsInDropOffRadius { get; private set; }

	public override void _Ready()
	{
		base._Ready();

		Fuel = MaxFuel;
		_origAngularDamp = AngularDamp;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Alive) {
			TimeDead += delta;
			LinearVelocity = new Vector2(0, 0);
			if (DeathTimeDelay < TimeDead) {
				End();
			}
			return;
		}
		base._PhysicsProcess(delta);
		
		if (!IsSpinningOut)
		{
			UpdateMovement(delta);
			UpdateShooting(delta);
			UpdateAsteroidPickUp(delta);
		}
		else {
			UpdateSpinOut(delta);
			smoke.Emitting = false;
		}

        if (Position.DistanceTo(Vector2.Zero) > GameManager.Instance.GameBoundsRadius)
        {
            Position = Position.Normalized() * GameManager.Instance.GameBoundsRadius;
            LinearVelocity = -Position.Normalized() * 500.0f;
        }

		if (isBoosting && !wasBoosting)
			audio.StartBoost();
		else if (!isBoosting && wasBoosting)
			audio.StopBoost();
		wasBoosting = isBoosting;
    }

	private void UpdateMovement(double delta)
	{
		float input = Input.GetAxis("backward", "forward");
		isBoosting = Input.IsActionPressed("boost") && !IsFuelEmpty;
		bool isBreaking = Input.IsActionPressed("break");

		float speed = Speed;
		if (isBoosting)
		{
			speed = BoostSpeed;
			Fuel -= (float)delta * _boostFuelConsumption;
			smoke.AmountRatio = 1.0f;
		}
		else if (Fuel <= 0.0f) {
			speed = EmptySpeed;
			smoke.AmountRatio = 0.125f;
		} else if (input != 0.0f) {
			Fuel -= (float)delta;
			smoke.AmountRatio = 0.375f;
		}

		if (Fuel < 0.0f)
			Fuel = 0.0f;

		if (isBreaking && Fuel > 0.0f)
			LinearDamp = BreakDamp;
		else if (isBoosting)
			LinearDamp = 0.0f;
		else
			LinearDamp = 1.0f;

		ApplyForce(Transform.BasisXform(Vector2.Up) * input * speed);
		
		smoke.Emitting = input != 0;

		input = Input.GetAxis("left", "right");
		ApplyTorque(input * AngularSpeed);
	}

	private void UpdateShooting(double delta)
	{
		_remainingShootDelay -= delta;
		if (_remainingShootDelay <= 0.0)
			_remainingShootDelay -= 0.0;
		else
			return;

		if (Input.IsActionPressed("shoot"))
		{
			_remainingShootDelay = ShootDelay;
			Shoot();
		}
	}

	public void Shoot()
	{
		audio.Shoot();

		Bullet bullet = (Bullet)_bulletScene.Instantiate();
		bullet.Position = Position;
		bullet.Rotation = Rotation;
		bullet.LinearVelocity = Transform.BasisXform(Vector2.Up) * _bulletSpeed + LinearVelocity;

		GetParent().AddChild(bullet);
	}

	private void UpdateAsteroidPickUp(double delta)
	{
		if (Input.IsActionJustPressed("interact"))
		{
			if (PickedUpAsteroid == null && InRadiusOfAsteroid)
				PickUpAsteroid(_asteroidsInRadius[0]);
			else if (PickedUpAsteroid != null && IsInDropOffRadius)
			{
				DepositAsteroid();
			}
			else if (PickedUpAsteroid != null)
				DropAsteroid();
		}
		if (PickedUpAsteroid != null && IsInDropOffRadius) {
			DepositAsteroid();
		}
	}
	
	private void DepositAsteroid() {
		GameManager.Instance.AddScore(PickedUpAsteroid.Score);
		
		GameManager.Instance.GetParent().CallDeferred("add_child", new TextParticle(
			"+" + PickedUpAsteroid.Score,
			Position,
			1 * Math.Log(PickedUpAsteroid.Score)
		));
		
		GameManager.Instance.AsteroidsCollectedStat++;
		Fuel += PickedUpAsteroid.DepositBonusFuel;
		if (Fuel > MaxFuel)
			Fuel = MaxFuel;

        PickedUpAsteroid.QueueFree();
		PickedUpAsteroid = null;
	}

	public void PickUpAsteroid(Asteroid asteroid)
	{
		if (PickedUpAsteroid != null)
			return;
		PickedUpAsteroid = asteroid;

		PickedUpAsteroid.Reparent(_pickedUpPivot, false);
		asteroid.Position = _pickedUpPivot.Position;
		asteroid.Rotation = 0.0f;
		PickedUpAsteroid.Freeze = true;
	}

	public void DropAsteroid()
	{
		if (PickedUpAsteroid == null)
			return;

		PickedUpAsteroid.CallDeferred("reparent", GetTree().Root);
		PickedUpAsteroid.SetDeferred("freeze", false);

		PickedUpAsteroid = null;
	}

	public void SpinOut(double duration)
	{
        audio.StopBoost();

        RemainingSpinOutTime = duration;
		AngularDamp = 0.0f;
		ApplyTorqueImpulse(-Mathf.Sign(AngularVelocity) * SpinOutSpeed);
		DropAsteroid();
	}

	private void UpdateSpinOut(double delta)
	{
		RemainingSpinOutTime -= delta;
		if (RemainingSpinOutTime <= 0.0f)
			EndSpinOut();
	}

	private void EndSpinOut()
	{
		RemainingSpinOutTime = 0.0f;
		AngularDamp = _origAngularDamp;
	}

	public void ChangeHealth(float value)
	{
		Health += value;
		if (Health <= 0.0f)
			Kill();
	}
	
	public void Kill() {
		Alive = false;
		Visible = false;
		SetDeferred("Freeze", true);
	}
	
	private void End() {
		GameManager.Instance.LoadGameOver();
	}

	private void PickUpAreaBodyEntered(Node node)
	{
		if (node is Asteroid)
			_asteroidsInRadius.Add((Asteroid)node);
	}

	private void PickUpAreaBodyExited(Node node)
	{
		if (node is Asteroid)
			_asteroidsInRadius.Remove((Asteroid)node);
	}

	private void OnDropOffAreaBodyEntered(Node node)
	{
		if (node is DepositPoint)
			IsInDropOffRadius = true;
	}

	private void OnDropOffAreaBodyExited(Node node)
	{
		if (node is DepositPoint)
			IsInDropOffRadius = false;
	}

	private void _on_fuel_start_tracking_body_entered(Node2D node) {
		((Fuel) node).SetTarget(this);
	}
	
	private void _on_fuel_collection_body_entered(Node2D node) {
		if (Fuel > MaxFuel) {
			return;
		}
		((IDamageable) node).Kill();
		Fuel += FuelParticleWorth;
		
	}
}
