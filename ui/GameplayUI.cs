using Godot;
using System;

public partial class GameplayUI : Control
{
	[Export]
	private ProgressBar _fuelBar;
	[Export]
	private RichTextLabel _pickUpLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Player player = GameManager.Instance.Player;

        _fuelBar.MaxValue = player.MaxFuel;
		_fuelBar.Value = player.Fuel;
		_pickUpLabel.Visible = player.InRadiusOfAsteroid && player.PickedUpAsteroid == null && !player.IsSpinningOut;
	}
}