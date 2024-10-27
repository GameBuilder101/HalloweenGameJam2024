using Godot;
using System;

public partial class Fuel : RigidBody2D, IDamageable
{

	[Export] private float PullStrength;
	
	public override void _IntegrateForces(PhysicsDirectBodyState2D state) {
		if (Target != null) {
			Vector2 TargetPos = Target.Position;
			float distance = TargetPos.DistanceTo(Position);
			Vector2 PosDiff = TargetPos - Position;
			Vector2 Force = PosDiff.Normalized() * PullStrength / Math.Max(
				distance,
				1
			) * state.Step;
			state.ApplyForce(Force);
		}
	}
	
	public float Health {get; private set;} = 1.0f;
	
	public void SetPosAndVel(Vector2 pos, Vector2 vel) {
		Position = pos;
		LinearVelocity = vel;
	}
	
	public void ChangeHealth(float hp) {
		Health -= hp;
		if (Health <= 0) {
			Kill();
		}
	}
	
	public void Kill() {
		QueueFree();
	}
	
	private Node2D Target;
	
	public void SetTarget(Node2D target) {
		Target = target;
	}
}
