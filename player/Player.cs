using Godot;

[GlobalClass]
public partial class Player : GameBody
{
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
    public float EmptySpeed { get; set; }
    [Export]
    public float AngularSpeed { get; set; }

    [Export]
    public double ShootDelay { get; private set; }
    private double _remainingShootDelay;

    public double RemainingSpinOutTime { get; private set; }
    public bool IsSpinningOut { get { return RemainingSpinOutTime > 0.0f; } }
    [Export]
    private float SpinOutSpeed { get; set; }
    private float _origAngularDamp;

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

        /*if (!IsSpinningOut && Input.IsActionPressed("shoot"))
            SpinOut(3.0);*/
    }

    private void UpdateMovement(double delta)
    {
        bool isBoosting = Input.IsActionPressed("boost") && !IsFuelEmpty;

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

        float input = Input.GetAxis("backward", "forward");
        ApplyForce(Transform.BasisXform(Vector2.Right) * input * speed);

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
        GD.Print("Pew pew :3");
    }

    private void UpdateAsteroidPickUp(double delta)
    {
        if (Input.IsActionPressed("interact")) { }
    }

    public void PickUpAsteroid()
    {

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
	
	public override void Die() {
	}
}
