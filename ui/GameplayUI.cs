using Godot;
using System;

public partial class GameplayUI : Control
{
	[Export]
	private ProgressBar _fuelBar;
	[Export]
	private RichTextLabel _pickUpLabel;
    [Export]
    private RichTextLabel _dropOffLabel;
	[Export]
	private Color[] Colors;
	[Export]
	private StyleBoxFlat background;
	[Export]
	private StyleBoxFlat bar;
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
		_fuelBar.Indeterminate = player.IsSpinningOut;
		Color color = Colors[player.IsSpinningOut ? 1 : 0];
		background.BorderColor = color;
		bar.BgColor = color;
		_pickUpLabel.Visible = player.InRadiusOfAsteroid && player.PickedUpAsteroid == null && !player.IsSpinningOut;
		_dropOffLabel.Visible = player.IsInDropOffRadius && player.PickedUpAsteroid != null && !player.IsSpinningOut;
	}
}
