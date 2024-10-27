using Godot;
using System;

[GlobalClass]
public partial class EnemyAudio : AudioStreamPlayer2D
{
	[Export] Enemy enemy;
	[Export] AudioStream wander;
	[Export] AudioStream chase;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (enemy.IsSeekingPlayer && Stream != chase)
		{
			Stream = chase;
			Play();
		}
		else if (!enemy.IsSeekingPlayer && Stream != wander)
		{
			Stream = wander;
			Play();
		}
	}
}
