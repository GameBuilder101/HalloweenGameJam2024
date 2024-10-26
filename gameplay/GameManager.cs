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
	/// Gets the list of asteroids in the scene
	/// </summary>
	public List<Asteroid> Asteroids {  get { return _asteroids; } }

	private int score;

	[Export] private PackedScene asteroidPrefab;

	[Export] private int maxAsteroids = 100;
	[Export] private float minAsteroidSpeed = 10;
	[Export] private float maxAsteroidSpeed = 50;
	[Export] private float minAsteroidAngularVelocity = 5;
	[Export] private float maxAsteroidAngularVelocity = 10;
	[Export] private float minAsteroidScale = 0.5f;
	[Export] private float maxAsteroidScale = 5;

	[Export] private float minRadius = 1500;
	[Export] private float maxRadius;
	[Export] private float growthRate;
	private float gameRadius;

	private float gameTime;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;

		// initial values
		_asteroids = new List<Asteroid>();
		currentState = GameState.GamePlay;
		score = 0;
		gameRadius = minRadius;
		gameTime = 0;

		for (int i = 0; i < maxAsteroids; i++)
		{
			SpawnAsteroid();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// spawn asteroids until the number of asteroids is equal to the max
		for(uint i = 0; i < maxAsteroids - _asteroids.Count; i++)
		{
			SpawnAsteroid();
		}
		
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
			float distanceToCenter = GD.Randf() * GD.Randf() * GD.Randf() * (maxRadius - minRadius) + minRadius;
			position = new Vector2(MathF.Cos(randomPositionAngle) * distanceToCenter, MathF.Sin(randomPositionAngle) * distanceToCenter);
		} while (bufferRect.HasPoint(position));

		// create a random angle within 90 degrees in either direction of the opposite of the random angle
		float randomVelocityAngle = randomPositionAngle + (MathF.PI / 2) + (float)GD.RandRange(-MathF.PI / 4, MathF.PI / 4);

		// create a random speed
		float speed = (float)GD.RandRange(minAsteroidSpeed, maxAsteroidSpeed);

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
