using Godot;
using System.Collections.Generic;

public partial class EnemyManager : Node
{
	public static EnemyManager Instance { get; private set; }

	[Export]
	public double intensityTimeRange;
	[Export]
	public double rangeDelay;
	public double IntensityScale
	{
		get
		{
			if (GameManager.Instance.GameTime <= rangeDelay)
				return 0.0;
			return Mathf.Clamp((GameManager.Instance.GameTime - rangeDelay) / intensityTimeRange, 0.0, 1.0);
		}
	}

    [Export]
    public int maxEnemyCountOverTime;
	public int IdealEnemyCount { get { return (int)(maxEnemyCountOverTime * IntensityScale); } }
	/// <summary>
	/// The avoidance distance of enemies from the center. This grows smaller with time
	/// </summary>
	public float AvoidanceDist
	{
		get
		{
			float gameBounds = GameManager.Instance.GameBoundsRadius;
			float blackHole = GameManager.Instance.BlackHole.BlackHoleRadius;
            float from = (gameBounds - blackHole) * 0.3f + blackHole;
            float to = (gameBounds - blackHole) * 0.1f + blackHole;
            return Mathf.Lerp(from, to, (float)IntensityScale);
		}
	}

    [Export] private PackedScene _enemyPrefab;
    /// <summary>
    /// The list of enemies in the scene.
    /// </summary>
    public List<Enemy> Enemies { get; private set; } = new List<Enemy>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		for (int i = 0; i < IdealEnemyCount - Enemies.Count; i++)
			SpawnEnemy();

    }

	public void SpawnEnemy()
	{
		float angle = GD.Randf() * Mathf.Pi * 2.0f;
		Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (GameManager.Instance.GameBoundsRadius + 800.0f);

        Enemy enemy = (Enemy)_enemyPrefab.Instantiate();
		enemy.Position = pos;

        GetParent().CallDeferred("add_child", enemy);
    }
}
