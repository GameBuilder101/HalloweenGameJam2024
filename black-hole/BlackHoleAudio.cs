using Godot;
using System;
using System.IO;
using System.IO.Ports;

[GlobalClass]
public partial class BlackHoleAudio : AudioStreamPlayer2D
{
	[Export] float panicRadius;
	[Export] AudioStream ambience;
	[Export] AudioStream ambienceMelody;
	[Export] AudioStream panic;
	[Export] AudioStream panicMelody;
	[Export] AudioStreamPlayer2D melody;

	Random rng = new Random();
	Node2D player;
	BlackHole blackHole;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GameManager.Instance.Player;
		blackHole = GameManager.Instance.BlackHole;
		OnAudioFinished();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MaxDistance = GameManager.Instance.GameBoundsRadius;
		if (Stream != panic && (blackHole.Position - player.Position).Length() <= panicRadius + blackHole.BlackHoleRadius)
			PlayPanic();
		else if (Stream != ambience && (blackHole.Position - player.Position).Length() > panicRadius + blackHole.BlackHoleRadius)
			PlayAmbience();
		else if (GetPlaybackPosition() <= delta)
			OnAudioFinished();
	}

	public void PlayAmbience()
	{
		if (Stream != ambience)
		{
            Stream = ambience;
            Play();
        }
		if (rng.NextDouble() < 0.1)
		{
			if (melody.Stream != ambienceMelody)
				melody.Stream = ambienceMelody;
			melody.Play();
		}
    }
    public void PlayPanic()
    {
        if (Stream != panic)
		{
            Stream = panic;
            Play();
        }
        if (rng.NextDouble() < 0.4)
        {
            if (melody.Stream != panicMelody)
                melody.Stream = panicMelody;
            melody.Play();
        }
    }
	public void OnAudioFinished()
	{
		if ((blackHole.Position - player.Position).Length() <= panicRadius + blackHole.BlackHoleRadius)
			PlayPanic();
		else
			PlayAmbience();
	}
}
