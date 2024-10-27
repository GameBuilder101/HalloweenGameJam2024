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
	[Export]
	private Area2D gravityArea;
	
	private const long HoleImageSize = 600; 
	
	//private float rad = 0.0f;
	
	public float GravityRadius { get { return AttractingArea.Radius; } }
	public float BlackHoleRadius { get { return BlackHoleArea.Radius; } }
	
	
	public void SetRadius(float blackHoleRadius, float gravityRadius) {
		//rad = blackHoleRadius;
		float HoleScale = blackHoleRadius / HoleImageSize;
		
		AttractingArea.Radius = gravityRadius;
		BlackHoleArea.Radius = blackHoleRadius;
		Vector2 scale = BlackHoleImage.Scale;
		scale.X = HoleScale;
		scale.Y = HoleScale;
		BlackHoleImage.Scale = scale;

		// set the new gravity
		gravityArea.Gravity = blackHoleRadius * 2;
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
