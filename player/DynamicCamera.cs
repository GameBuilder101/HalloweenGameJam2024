using Godot;
using System;

[GlobalClass]
public partial class DynamicCamera : Camera2D
{
    public static DynamicCamera Instance { get; private set; }

	[Export]
	private Node2D TrackTarget { get; set; }

	[Export]
	private float _ambientShakeIntensity;
	public float AmbientShakeScale { get; private set; }
    private double _boostShakeSmooth;

    private double _shakeTime;
	private double _shakeDuration = 1.0;
	private float _shakeIntensity;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Instance = this;
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 pos = Position;
        pos.X = TrackTarget.Position.X;
        pos.Y = TrackTarget.Position.Y;

        AmbientShakeScale = Mathf.Clamp(1.0f - TrackTarget.Position.Length() / (GameManager.Instance.BlackHole.BlackHoleRadius * 3.0f), 0.0f, 1.0f);
        foreach (Enemy enemy in EnemyManager.Instance.Enemies)
            AmbientShakeScale += Mathf.Clamp(1.0f - TrackTarget.Position.DistanceTo(enemy.Position) / 800.0f, 0.0f, 1.0f) * 0.3f;

        if (GameManager.Instance.Player.isBoosting)
        {
            _boostShakeSmooth += delta * 0.5;
            if (_boostShakeSmooth > 1.0)
                _boostShakeSmooth = 1.0;
        }
        else
        {
            _boostShakeSmooth -= delta * 0.5;
            if (_boostShakeSmooth < 0.0)
                _boostShakeSmooth = 0.0;
        }

        AmbientShakeScale += (float)_boostShakeSmooth * 0.065f;
        AmbientShakeScale = Mathf.Clamp(AmbientShakeScale, 0.0f, 1.0f);

        _shakeTime -= delta;
        if (_shakeTime < 0.0)
            _shakeTime = 0.0;

        float combinedIntensity = AmbientShakeScale * _ambientShakeIntensity + (float)(_shakeTime / _shakeDuration) * _shakeIntensity;

        float angle = GD.Randf() * Mathf.Pi * 2.0f;
        pos += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * combinedIntensity;
        Position = pos;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
    }

	public void Shake(double duration, float intensity)
	{
		_shakeTime = duration;
		_shakeDuration = duration;
		_shakeIntensity = intensity;
        GD.Print("a");
	}
}
