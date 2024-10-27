using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : Area2D, IDamageable
{
    [Export]
    public float Health { get; private set; } = 1.0f;

    [Export]
    public float wanderSpeed;
    [Export]
    public float chaseSpeed;
    [Export]
    public float separationDistance;

    public Vector2 Acceleration { get; set; }
    public Vector2 LinearVelocity { get; set; }

    [Export]
    private float _seekRange;
    public bool IsSeekingPlayer
    {
        get
        {
            return !GameManager.Instance.Player.IsSpinningOut && 
                Position.DistanceTo(GameManager.Instance.Player.Position) <= _seekRange &&
                GameManager.Instance.Player.Position.Length() > EnemyManager.Instance.AvoidanceDist;
        }
    }

    [Export]
    private double _spinOutTime;

    private Vector2 _curWanderPos;
    private double _curWanderCooldown;
    [Export]
    private double _minWanderCooldown;
    [Export]
    private double _maxWanderCooldown;
    private bool _curWanderState = true;

    [Export]
    private Sprite2D _innerSprite;
    [Export]
    private GpuParticles2D _killParticles;

    public override void _Ready()
    {
        base._Ready();
        EnemyManager.Instance.Enemies.Add(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        EnemyManager.Instance.Enemies.Remove(this);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);

        if (IsSeekingPlayer)
        {
            Acceleration += Seek(GameManager.Instance.Player.Position, chaseSpeed);
        }
        else
            Wander(delta);

        LinearVelocity += Acceleration * (float)delta;
        Acceleration = Vector2.Zero;
        Position += LinearVelocity * (float)delta;

        _innerSprite.Rotation += (float)delta * Mathf.Pi;
	}

    protected void Wander(double delta)
    {
        _curWanderCooldown -= delta;

        if (_curWanderCooldown <= 0.0)
        {
            _curWanderCooldown = GD.RandRange(_minWanderCooldown, _maxWanderCooldown);

            Vector2 randOffset = Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2.0f) * 150.0f;

            if (Position.Length() <= EnemyManager.Instance.AvoidanceDist * 1.7f)
                _curWanderState = false;
            else if (Position.Length() >= GameManager.Instance.GameBoundsRadius * 0.9f)
                _curWanderState = true;

            if (_curWanderState)
                _curWanderPos = Position + -Position.Normalized() * 700.0f + randOffset;
            else
                _curWanderPos = Position + Position.Normalized() * 700.0f + randOffset;
        }

        Acceleration += Seek(_curWanderPos, wanderSpeed);
    }

    protected Vector2 Seek(Vector2 targetPos, float speed)
    {
        // calculate a desired velocity which is the vector from the object to it's target scaled by the max speed
        Vector2 desiredVelocity = (targetPos - Position).Normalized() * speed;

        // return the force vector required to achive the desired velocity
        return desiredVelocity - LinearVelocity;
    }

    /// <summary>
    /// Calculates the steering force required to achive a desired velocity away from a target position.
    /// </summary>
    /// <param name="targetPos">Position to flee from</param>
    /// <returns>A force vector that will steer the agent away from a location</returns>
    protected Vector2 Flee(Vector2 targetPos, float speed)
    {
        // calculate a desired velocity which is the vector from the target to this object scaled by the max speed
        Vector2 desiredVelocity = (Position - targetPos).Normalized() * speed;

        // return the force vector required to achive the desired velocity
        return (desiredVelocity - LinearVelocity);
    }

    /// <summary>
    /// Gets a target's future position and seeks it
    /// </summary>
    /// <param name="target">Target object</param>
    /// <param name="time">amount of time ahead the target's position will be calculated</param>
    /// <returns>Steering force which seeks an object's future position</returns>
    protected Vector2 Pursue(RigidBody2D target, float time, float speed)
    {
        Vector2 futurePos = target.Position;
        futurePos += target.LinearVelocity * time;

        return Seek(futurePos, speed);
    }

    /// <summary>
    /// Calculates a steering force which keeps this agent separated from other given objects
    /// </summary>
    /// <param name="others">list of other objects to separate from</param>
    /// <returns>a separating steering force vector</returns>
    protected Vector2 Separate(List<Node2D> others, float speed)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;

        foreach (Node2D other in others)
        {
            // check for agents within the separation distance and ensure agent does not attempt to separate from itself
            if (other != this && Position.DistanceTo(other.Position) < separationDistance)
            {
                sum += Flee(other.Position, speed) / Position.DistanceTo(other.Position); // separation force is greater if they are closer
                count++;
            }
        }

        // average the force by number of others separated
        if (count > 0) // ensure there was at least one separation attempt
        {
            sum /= count;
        }

        return sum;
    }

    public void OnPlayerEntered(Node body)
    {
        if (body is Player)
            ((Player)body).SpinOut(_spinOutTime);
    }

    public void ChangeHealth(float value)
    {
        Health += value;
        if (Health <= 0.0f)
            Kill();
        LinearVelocity = Vector2.Zero;
    }

    public void Kill()
    {
        _killParticles.Reparent(GetTree().Root);
        _killParticles.Emitting = true;
        QueueFree();
    }
}
