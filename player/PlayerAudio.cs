using Godot;
using System;

[GlobalClass]
public partial class PlayerAudio : AudioStreamPlayer2D
{
	[Export] AudioStreamPlayer2D shoot;

	bool starting = false;
	bool stopping = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (starting)
		{
			if (VolumeDb + delta * 80 < 0)
				VolumeDb += (float)delta * 80;
			else
			{
				VolumeDb = 0;
				starting = false;
			}
		}
		else if (stopping)
		{
			if (VolumeDb - delta * 80 > -80)
				VolumeDb -= (float)delta * 80;
			else
			{
				VolumeDb = -80;
				stopping = false;
				Stop();
			}
		}
	}

	public void StartBoost()
	{
		if (stopping)
			stopping = false;
		starting = true;
		VolumeDb = -80;
		Play();
	}
	public void StopBoost()
	{
		if (starting)
			starting = false;
		stopping = true;
	}
	public void Shoot()
	{
		shoot.Play();
	}
}
