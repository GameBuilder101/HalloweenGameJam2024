using Godot;
using System;

public partial class BlackHole : Node2D
{
	[Export]
	private CircleShape2D AttractingArea;
	[Export]
	private CircleShape2D BlackHoleArea;
	[Export]
	private Sprite2D BlackHoleImage;
	
	private const long HoleImageSize = 1; 
	
	private float rad = 0.0f;
	
	public float GetRadius() {
		return rad;
	}
	
	
	public void SetRadius(float holeRad, float fieldRad) {
		rad = holeRad;
		float HoleScale = holeRad / HoleImageSize;
		
		AttractingArea.Radius = fieldRad;
		BlackHoleArea.Radius = HoleScale;
		Vector2 scale = BlackHoleImage.Scale;
		scale.X = HoleScale;
		scale.Y = HoleScale;
		BlackHoleImage.Scale = scale;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void _on_area_body_entered(Node2D body) {
		if (!(body is IDamageable))
			return;
		IDamageable damageable = (IDamageable)body;
        damageable.Kill();
    }
}
