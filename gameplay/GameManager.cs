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
	[Export] private Sprite2D _boundarySprite;
	private List<Asteroid> _asteroids;
	private List<Debris> _debris;

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

	/// <summary>
	/// Gets the list of debris in the scene
	/// </summary>
	public List<Debris> Debris { get { return _debris; } }

	[Export] private PackedScene debrisPrefab;
	[Export] private Texture2D[] debrisTextures;

	[Export] private int maxDebris = 500;
	[Export] private float minDebrisSpeed = 10;
	[Export] private float maxDebrisSpeed = 50;
	[Export] private float minDebrisAngularVelocity = 0;
	[Export] private float maxDebrisAngularVelocity = 5;
	[Export] private float minDebrisScale = 0.5f;
	[Export] private float maxDebrisScale = 5;

	[Export] private PackedScene asteroidPrefab;
	[Export] private Texture2D[] asteroidtextures;
	[Export] private Texture2D[] specialAsteroidTextures;

	[Export] private int maxAsteroids = 100;
	[Export] private float minAsteroidSpeed = 10;
	[Export] private float maxAsteroidSpeed = 50;
	[Export] private float minAsteroidAngularVelocity = 5;
	[Export] private float maxAsteroidAngularVelocity = 10;
	[Export] private float minAsteroidScale = 0.5f;
	[Export] private float maxAsteroidScale = 5;
	[Export] private float specialAsteroidChance = 0.1f;
	[Export] private float baseAsteroidScore = 100;
	[Export] private float baseSpecialAsteroidScore = 1000;

	[Export] private float asteroidIncrementTime = 3;
	private float asteroidIncrementTimer;

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

	public int AsteroidsCollectedStat { get; set; }
	public int AsteroidsShotStat { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;

		// initial values
		_asteroids = new List<Asteroid>();
		_debris = new List<Debris>();
		currentState = GameState.GamePlay;
		score = 0;
		asteroidSpawnRadius = asteroidSpawnStartRadius;
		gameBoundsRadius = gameBoundsStartRadius;
		blackHoleRadius = blackHoleStartRadius;
		gameTime = 0;
		asteroidIncrementTimer = asteroidIncrementTime;

		for (int i = 0; i < maxAsteroids; i++)
		{
			SpawnAsteroid();
		}

		for (int i = 0; i < maxDebris; i++)
		{
			SpawnDebris();
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

		// spawn debris until the number of debris is equal to the max
		for (uint i = 0; i < maxDebris - _debris.Count; i++)
		{
			SpawnDebris();
		}

		GD.Print("Max Debris: " + maxDebris);
		GD.Print("Current Debris: " + _debris.Count);

		//grow game radius
		asteroidSpawnRadius += radiusInrement * (float)delta;
		gameBoundsRadius += radiusInrement * (float)delta;
		blackHoleRadius += radiusInrement * (float)delta;
		
		// grow the black hole
		_blackHole.SetRadius(blackHoleRadius, gameBoundsRadius);
		//GD.Print("blackHoleRadius" + _blackHole.BlackHoleRadius);

		_boundarySprite.Scale = Vector2.One * (GameBoundsRadius / 4096.0f);

		//increment the number of asteroids when the timer goes off
		if(asteroidIncrementTimer < 0)
		{
			maxAsteroids++;
			asteroidIncrementTimer = asteroidIncrementTime;
		}

		asteroidIncrementTimer -= (float)delta;
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

	public void SpawnDebris()
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
			// random angle to spawn the debris
			randomPositionAngle = (float)GD.RandRange(0, 2 * MathF.PI);
			
			
			// create a random vector 2 with a bias towards positions near the center
			float distanceToCenter = GD.Randf() * (gameBoundsRadius - _blackHole.BlackHoleRadius) + _blackHole.BlackHoleRadius;
		   
			position = new Vector2(MathF.Cos(randomPositionAngle) * distanceToCenter, MathF.Sin(randomPositionAngle) * distanceToCenter);
		} while (bufferRect.HasPoint(position));

		// create a random angle within 90 degrees in either direction of the opposite of the random angle
		float randomVelocityAngle = randomPositionAngle + (MathF.PI / 2) + (float)GD.RandRange(-MathF.PI / 4, MathF.PI / 4);

		// create a random speed
		float speed = (float)GD.RandRange(minDebrisSpeed, maxDebrisSpeed);

		// speed multiplier is a number from 0-1 based on the position compared to the upper and lower bounds of the asteroid spawn area
		float speedMultiplier = position.Length() / (gameBoundsRadius - _blackHole.BlackHoleRadius);

		// speed should be greater the closer an asteroid spawns to the edge of the black hole
		speed *= speedMultiplier;

		// create a velocity
		Vector2 velocity = new Vector2(MathF.Cos(randomVelocityAngle) * speed, MathF.Sin(randomVelocityAngle) * speed);

		// create a random angular velocity
		float angularVelocity = (float)GD.RandRange(minDebrisAngularVelocity, maxDebrisAngularVelocity);

		// create a random scale
		float randomScale = (float)GD.RandRange(minDebrisScale, minDebrisScale);

		Debris debris = (Debris)debrisPrefab.Instantiate();
		debris.Position = position;
		debris.LinearVelocity = velocity;
		debris.AngularVelocity = angularVelocity;

		foreach (Node2D child in debris.GetChildren())
		{
			child.Scale = new Vector2(child.Scale.X * randomScale, child.Scale.Y * randomScale);
		}

		int randomTextureIndex = GD.RandRange(0, debrisTextures.Length - 1);
		debris.GetChild<Sprite2D>(1, true).Texture = debrisTextures[randomTextureIndex];
		debris.ZIndex = -1;

		_debris.Add(debris);

		GetParent().CallDeferred("add_child", debris);
	}

	public void SpawnAsteroid()
	{
		// decide if the spawned asteroid is special
		bool isSpecial;
		if(GD.Randf() < specialAsteroidChance)
		{
			isSpecial = true;
		}
		else
		{
			isSpecial = false;
		}

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
			float distanceToCenter; 
			//if the asteroid is special, devide position by 2
			if(isSpecial)
			{
				// create a random vector 2 with a bias towards positions near the center
				distanceToCenter = GD.Randf() * GD.Randf() * (asteroidSpawnRadius / 2 - _blackHole.BlackHoleRadius) + _blackHole.BlackHoleRadius;
			}
			else
			{
				// create a random vector 2 with a bias towards positions near the center
				distanceToCenter = GD.Randf() * GD.Randf() * (asteroidSpawnRadius - _blackHole.BlackHoleRadius) + _blackHole.BlackHoleRadius;
			}
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

		

		// if the asteroid is special use the special textures and give a higher score
		if(isSpecial)
		{
			int randomTextureIndex = GD.RandRange(0, specialAsteroidTextures.Length - 1);
			asteroid.GetChild<Sprite2D>(0, true).Texture = specialAsteroidTextures[randomTextureIndex];
			asteroid.Score = (int)(baseSpecialAsteroidScore * randomScale);
		}
		else
		{
			int randomTextureIndex = GD.RandRange(0, asteroidtextures.Length - 1);
			asteroid.GetChild<Sprite2D>(0, true).Texture = asteroidtextures[randomTextureIndex];
			asteroid.Score = (int)(baseAsteroidScore * randomScale);
			asteroid.FuelMultiplier = 3;
		}

		

		GetParent().CallDeferred("add_child", asteroid);
	}

	public void LoadGameOver()
	{
		GameOver.Score = score;
		GameOver.TimeSurvived = GameTime;
		GameOver.AsteroidsCollected = AsteroidsCollectedStat;
		GameOver.AsteroidsShot = AsteroidsShotStat;
		GetTree().CallDeferred("change_scene_to_packed", _gameOverScene);
	}
}
