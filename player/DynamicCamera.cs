using Godot;
using System;

[GlobalClass]
public partial class DynamicCamera : Camera2D
{
	[Export]
	private Node2D TrackTarget { get; set; }

	[Export]
	private float _ambientShakeIntensity;
	public float AmbientShakeScale { get; private set; }

	private double _shakeTime;
	private double _shakeDuration = 1.0;
	private float _shakeIntensity;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        Vector2 pos = Position;
        pos.X = TrackTarget.Position.X;
        pos.Y = TrackTarget.Position.Y;
        Position = pos;

        AmbientShakeScale = Mathf.Clamp(1.0f - TrackTarget.Position.Length() / GameManager.Instance.BlackHole.BlackHoleRadius * 4.0f, 0.0f, 1.0f);
		foreach (Enemy enemy in EnemyManager.Instance.Enemies)
            AmbientShakeScale += Mathf.Clamp(1.0f - TrackTarget.Position.DistanceTo(enemy.Position) / 800.0f, 0.0f, 1.0f) * 0.5f;
        AmbientShakeScale = Mathf.Clamp(AmbientShakeScale, 0.0f, 1.0f);

		_shakeTime -= delta;
		if (_shakeTime < 0.0)
			_shakeTime = 0.0;

		float combinedIntensity = AmbientShakeScale * _ambientShakeIntensity + (float)(_shakeTime / _shakeDuration) * _shakeIntensity;

    }
}
