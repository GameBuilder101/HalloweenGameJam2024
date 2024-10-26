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
	private AudioStreamPlayer2D BlackHoleAudio;


	private const long HoleImageSize = 1; 
	
	
	public void SetRadius(float rad) {
		float GameRadius = rad;
		float HoleRadius = rad / 5;
		float HoleScale = HoleRadius / HoleImageSize;
		
		AttractingArea.Radius = (float) GameRadius;
		BlackHoleArea.Radius = HoleRadius;
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

	public void PlayAudio()
	{
		BlackHoleAudio.Play();
	}
}
