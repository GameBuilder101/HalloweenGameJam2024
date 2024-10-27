using Godot;
using System;
using System.Collections.Generic;

public enum GameState
{
	GamePlay,
	Paused
}

[GlobalClass]
public partial class GameManager : Node
{
	#region singleton
	private static GameManager _instance;

	/// <summary>
	/// Gets the instance of the singleton
	/// </summary>
	public static GameManager Instance { get { return _instance; } }
	#endregion

	private GameState currentState;
	[Export] private PackedScene _gameOverScene;

	[Export] private BlackHole _blackHole;
	[Export] private Player _player;
	[Export] private DepositPoint _depositPoint;
	[Export] private Camera2D _camera;
	private List<Asteroid> _asteroids;

	/// <summary>
	/// Gets a reference to the black hole
	/// </summary>
	public BlackHole BlackHole { get { return _blackHole; } }

	/// <summary>
	/// Gets a reference to the player
	/// </summary>
	public Player Player { get { return _player; } }

	/// <summary>
	/// Gets a reference to the deposit point
	/// </summary>
	public DepositPoint DepositPoint { get { return _depositPoint; } }

	/// <summary>
	/// Gets the list of asteroids in the scene
	/// </summary>
	public List<Asteroid> Asteroids {  get { return _asteroids; } }

	[Export] private PackedScene asteroidPrefab;

	[Export] private int maxAsteroids = 100;
	[Export] private float minAsteroidSpeed = 10;
	[Export] private float maxAsteroidSpeed = 50;
	[Export] private float minAsteroidAngularVelocity = 5;
	[Export] private float maxAsteroidAngularVelocity = 10;
	[Export] private float minAsteroidScale = 0.5f;
	[Export] private float maxAsteroidScale = 5;

	[Export] private float asteroidSpawnStartRadius; // radius the ateroid spawn area starts at
	[Export] private float gameBoundsStartRadius; // radius the bounds start at
	[Export] private float radiusInrement; // amount the radius grows per second
	[Export] private float blackHoleStartRadius; // radius the black hole starts at

	private float asteroidSpawnRadius; // current radius within which asteroids spawn
	private float gameBoundsRadius; // current radius of the bounds
	private float blackHoleRadius; // current radius of the black hole

	/// <summary>
	/// Gets the outer bounds of the map
	/// </summary>
	public float GameBoundsRadius { get { return gameBoundsRadius; } }

	private int score;
	private float gameTime;

	public int Score { get { return score; } }
	public float GameTime { get { return gameTime; } }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;

		// initial values
		_asteroids = new List<Asteroid>();
		currentState = GameState.GamePlay;
		score = 0;
		asteroidSpawnRadius = asteroidSpawnStartRadius;
		gameBoundsRadius = gameBoundsStartRadius;
		blackHoleRadius = blackHoleStartRadius;
		gameTime = 0;

		for (int i = 0; i < maxAsteroids; i++)
		{
			SpawnAsteroid();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// increment game time
		gameTime += (float)delta;

		// spawn asteroids until the number of asteroids is equal to the max
		for(uint i = 0; i < maxAsteroids - _asteroids.Count; i++)
		{
			SpawnAsteroid();
		}

		//grow game radius
		asteroidSpawnRadius += radiusInrement * (float)delta;
		gameBoundsRadius += radiusInrement * (float)delta;
		blackHoleRadius += radiusInrement * (float)delta;
		
		// grow the black hole
		_blackHole.SetRadius(blackHoleRadius, gameBoundsRadius);
		//GD.Print("blackHoleRadius" + _blackHole.BlackHoleRadius);
	}

	/// <summary>
	/// adds the given amount of points to the store
	/// </summary>
	/// <param name="amount">amount of points to add</param>
	public void AddScore(int amount)
	{
		score += amount;
	}

	/*
	private void SpawnAsteroids()
	{
		// spawn a random number of asteroids
		uint randomAsteroidCount = (uint)GD.RandRange(minAsteroids, maxAsteroids);
		for(uint i = 0; i < randomAsteroidCount; i++)
		{
			
		}
	}
	*/

	public void SpawnAsteroid()
	{
		//get the viewport bounding box
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Vector2 camPosition = _camera.Position;
		Vector2 halfSize = (viewportSize / 2.0f) * _camera.Zoom;
		Rect2 boundingBox = new Rect2(camPosition - halfSize, viewportSize * _camera.Zoom);

		// Scale the bounding box to add extra buffer
		Vector2 newSize = boundingBox.Size * 1.5f;
		Vector2 newPosition = camPosition - (newSize / 2.0f);

		// rectangle that asteroids can't spawn in
		Rect2 bufferRect = new Rect2(newPosition, newSize);

		// keep trying to create a position until one is created outside the buffer rectangle
		float randomPositionAngle;
		Vector2 position;
		do
		{
			// random angle to spawn the asteroid
			randomPositionAngle = (float)GD.RandRange(0, 2 * MathF.PI);
			// create a random vector 2 with a bias towards positions near the center
			float distanceToCenter = GD.Randf() * GD.Randf() * GD.Randf() * (asteroidSpawnRadius - _blackHole.BlackHoleRadius) + _blackHole.BlackHoleRadius;
			position = new Vector2(MathF.Cos(randomPositionAngle) * distanceToCenter, MathF.Sin(randomPositionAngle) * distanceToCenter);
		} while (bufferRect.HasPoint(position));

		// create a random angle within 90 degrees in either direction of the opposite of the random angle
		float randomVelocityAngle = randomPositionAngle + (MathF.PI / 2) + (float)GD.RandRange(-MathF.PI / 4, MathF.PI / 4);

		// create a random speed
		float speed = (float)GD.RandRange(minAsteroidSpeed, maxAsteroidSpeed);

		// speed multiplier is a number from 0-1 based on the position compared to the upper and lower bounds of the asteroid spawn area
		float speedMultiplier = position.Length() / (asteroidSpawnRadius - _blackHole.BlackHoleRadius);

		// speed should be greater the closer an asteroid spawns to the edge of the black hole
		speed *= speedMultiplier;

		// create a velocity
		Vector2 velocity = new Vector2(MathF.Cos(randomVelocityAngle) * speed, MathF.Sin(randomVelocityAngle) * speed);

		// create a random angular velocity
		float angularVelocity = (float)GD.RandRange(minAsteroidAngularVelocity, maxAsteroidAngularVelocity);

		// create a random scale

		float randomScale = (float)GD.RandRange(minAsteroidScale, maxAsteroidScale);

		Asteroid asteroid = (Asteroid)asteroidPrefab.Instantiate();
		asteroid.Position = position;
		asteroid.LinearVelocity = velocity;
		asteroid.AngularVelocity = angularVelocity;

		asteroid.SetScale(randomScale);
		
		GetParent().CallDeferred("add_child", asteroid);
	}

	public void LoadGameOver()
	{
		GetTree().CallDeferred("change_scene_to_packed", _gameOverScene);
	}
}
