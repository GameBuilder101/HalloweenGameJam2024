using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Player : RigidBody2D, IDamageable
{
    public float Health { get; private set; } = 1.0f;

    [Export]
    public float MaxFuel { get; private set; }
    [Export]
    public float Fuel { get; private set; }
    public bool IsFuelEmpty { get { return Fuel <= 0.0f; } }

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
    [Export]
    private float SpinOutSpeed { get; set; }
    private float _origAngularDamp;

    private List<Asteroid> _asteroidsInRadius = new List<Asteroid>();
    public Asteroid PickedUpAsteroid { get; private set; }
    [Export]
    private Node2D _pickedUpPivot;

    public override void _Ready()
    {
        base._Ready();

        Fuel = MaxFuel;
        _origAngularDamp = AngularDamp;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        if (!IsSpinningOut)
        {
            UpdateMovement(delta);
            UpdateShooting(delta);
            UpdateAsteroidPickUp(delta);
        }
        else
            UpdateSpinOut(delta);
    }

    private void UpdateMovement(double delta)
    {
        bool isBoosting = Input.IsActionPressed("boost") && !IsFuelEmpty;
        bool isBreaking = Input.IsActionPressed("break");

        float speed = Speed;
        if (isBoosting)
        {
            speed = BoostSpeed;
            Fuel -= (float)delta;
            if (Fuel < 0.0f)
                Fuel = 0.0f;
        }
        else if (Fuel <= 0.0f)
            speed = EmptySpeed;

        if (isBreaking)
            LinearDamp = BreakDamp;
        else
            LinearDamp = 0.0f;

        float input = Input.GetAxis("backward", "forward");
        ApplyForce(Transform.BasisXform(Vector2.Up) * input * speed);

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
            if (PickedUpAsteroid == null && _asteroidsInRadius.Count > 0)
                PickUpAsteroid(_asteroidsInRadius[0]);
            else if (PickedUpAsteroid != null)
                DropAsteroid();
        }
    }

    public void PickUpAsteroid(Asteroid asteroid)
    {
        if (PickedUpAsteroid != null)
            return;
        PickedUpAsteroid = asteroid;

        PickedUpAsteroid.Reparent(_pickedUpPivot, false);
        asteroid.Position = _pickedUpPivot.Position;
        PickedUpAsteroid.Freeze = true;
    }

    public void DropAsteroid()
    {
        if (PickedUpAsteroid == null)
            return;

        PickedUpAsteroid.Reparent(GetTree().Root);
        PickedUpAsteroid.Freeze = false;
    }

    public void SpinOut(double duration)
    {
        RemainingSpinOutTime = duration;
        AngularDamp = 0.0f;
        ApplyTorqueImpulse(-Mathf.Sign(AngularVelocity) * SpinOutSpeed);
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
}
