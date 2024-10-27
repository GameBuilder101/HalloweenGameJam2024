using Godot;
using System;

public partial class StartFader : ColorRect
{
	[Export]
	private double _delay = 0.0;
	[Export]
	private double _duration = 2.0;
	private double _time;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Color color = Color;
		color.A = 1.0f;
		Color = color;
		_time = _duration;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_delay -= delta;
		if (_delay < 0.0)
			_delay = 0.0;

		if (_delay > 0.0)
			return;

		_time -= delta;
		if (_time < 0.0)
			_time = 0.0;

		Color color = Color;
		color.A = (float)(_time / _duration);
		Color = color;
	}
}
