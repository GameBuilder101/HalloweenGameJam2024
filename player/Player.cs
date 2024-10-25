using Godot;

[GlobalClass]
public partial class Player : RigidBody2D
{
	[Export]
	public float Speed { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        float input = Input.GetAxis("forward", "backward");
        LinearVelocity = Vector2.Right * input * Speed * (float)delta;
    }
}
