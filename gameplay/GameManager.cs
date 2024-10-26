using Godot;
using System;

public enum GameState
{
	GamePlay,
	Pause,
	GameOver
}

[GlobalClass]
public partial class GameManager : Node
{
	#region singleton
	private static GameManager instance;

	/// <summary>
	/// Gets the instance of the singleton
	/// </summary>
	public static GameManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameManager();
			}
			return instance;
		}
	}
	#endregion

	private GameState currentState;
	private int score;

	[Export] private PackedScene asteroidPrefab;

	[Export] private int maxAsteroids;
	[Export] private int minAsteroids;
	[Export] private float maxAsteroidSpeed;
	[Export] private float minAsteroidSpeed;
	[Export] private float minAsteroidAngularVelocity;
	[Export] private float maxAsteroidAngularVelocity;
	[Export] private float minAsteroidScale;
	[Export] private float maxAsteroidScale;

	[Export] private float startRadius;
	private float gameRadius;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// initial values
		currentState = GameState.GamePlay;
		score = 0;
		gameRadius = startRadius;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// adds the given amount of points to the store
	/// </summary>
	/// <param name="amount">amount of points to add</param>
	public void AddScore(int amount)
	{
		score += amount;
	}

	private void SpawnAsteroids()
	{
		// spawn a random number of asteroids
		uint randomAsteroidCount = (uint)GD.RandRange(minAsteroids, maxAsteroids);
		for(uint i = 0; i < randomAsteroidCount; i++)
		{
			// random angle to spawn the asteroid
			float randomPositionAngle = (float)GD.RandRange(0, 2 * MathF.PI);

			// create a random vector 2 on the circumfrence
			Vector2 position = new Vector2(MathF.Cos(randomPositionAngle) * gameRadius, MathF.Sin(randomPositionAngle) * gameRadius);

			// create a random angle within 90 degrees in either direction of the oppositr of the random angle
			float randomVelocityAngle = randomPositionAngle + (MathF.PI * 2) + (float)GD.RandRange(-MathF.PI, MathF.PI);

			// create a random speed
			float speed = (float)GD.RandRange(minAsteroidSpeed, maxAsteroidSpeed);

			// create a velocity
			Vector2 velocity = new Vector2(MathF.Cos(randomVelocityAngle) * speed, MathF.Sin(randomVelocityAngle) * speed);

			// create a random angular velocity
			float angularVelocity = (float)GD.RandRange(minAsteroidAngularVelocity, maxAsteroidAngularVelocity);

			// create a random scale

			float randomScale = (float)GD.RandRange(minAsteroidScale, maxAsteroidScale);

			RigidBody2D asteroid = (RigidBody2D)asteroidPrefab.Instantiate();
			asteroid.Position = position;
			asteroid.LinearVelocity = velocity;
			asteroid.AngularVelocity = angularVelocity;
			asteroid.Scale = new Vector2(randomScale, randomScale);
		}
	}
}
