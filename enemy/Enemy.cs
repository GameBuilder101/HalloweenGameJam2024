using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : Node2D
{
    [Export]
    public float cruiseSpeed;
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
            Acceleration += Seek(GameManager.Instance.Player.Position);
        }
        else
            Wander(delta);

        LinearVelocity += Acceleration * (float)delta;
        Acceleration = Vector2.Zero;
        Position += LinearVelocity * (float)delta;
	}

    protected void Wander(double delta)
    {
        _curWanderCooldown -= delta;

        if (_curWanderCooldown <= 0.0)
        {
            _curWanderCooldown = GD.RandRange(_minWanderCooldown, _maxWanderCooldown);

            _curWanderPos = Position + Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2.0f) * 1000.0f;

            // If trying to move outside the game bounds, move inward
            if (_curWanderPos.Length() >= GameManager.Instance.GameBoundsRadius)
                _curWanderPos = Vector2.Zero;
            else if (_curWanderPos.Length() <= EnemyManager.Instance.AvoidanceDist) // If trying to move into black hole, move outward
                _curWanderPos = -Position.Normalized() * 1000.0f;
        }

        Acceleration += Seek(_curWanderPos);
    }

    protected Vector2 Seek(Vector2 targetPos)
    {
        // calculate a desired velocity which is the vector from the object to it's target scaled by the max speed
        Vector2 desiredVelocity = (targetPos - Position).Normalized() * cruiseSpeed;

        // return the force vector required to achive the desired velocity
        return desiredVelocity - LinearVelocity;
    }

    /// <summary>
    /// Calculates the steering force required to achive a desired velocity away from a target position.
    /// </summary>
    /// <param name="targetPos">Position to flee from</param>
    /// <returns>A force vector that will steer the agent away from a location</returns>
    protected Vector2 Flee(Vector2 targetPos)
    {
        // calculate a desired velocity which is the vector from the target to this object scaled by the max speed
        Vector2 desiredVelocity = (Position - targetPos).Normalized() * cruiseSpeed;

        // return the force vector required to achive the desired velocity
        return (desiredVelocity - LinearVelocity);
    }

    /// <summary>
    /// Gets a target's future position and seeks it
    /// </summary>
    /// <param name="target">Target object</param>
    /// <param name="time">amount of time ahead the target's position will be calculated</param>
    /// <returns>Steering force which seeks an object's future position</returns>
    protected Vector2 Pursue(RigidBody2D target, float time)
    {
        Vector2 futurePos = target.Position;
        futurePos += target.LinearVelocity * time;

        return Seek(futurePos);
    }

    /// <summary>
    /// Calculates a steering force which keeps this agent separated from other given objects
    /// </summary>
    /// <param name="others">list of other objects to separate from</param>
    /// <returns>a separating steering force vector</returns>
    protected Vector2 Separate(List<Node2D> others)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;

        foreach (Node2D other in others)
        {
            // check for agents within the separation distance and ensure agent does not attempt to separate from itself
            if (other != this && Position.DistanceTo(other.Position) < separationDistance)
            {
                sum += Flee(other.Position) / Position.DistanceTo(other.Position); // separation force is greater if they are closer
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
}
