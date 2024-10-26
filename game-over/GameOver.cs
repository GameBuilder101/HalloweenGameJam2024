using Godot;
using System;

public partial class GameOver : Control
{
    [Export]
    private string _gameplay;
    [Export]
    private string _titleScreen;

    [Export]
    private RichTextLabel _statsLabel;

    public static int Score { get; set; }
    public static double TimeSurvived { get; set; }
    public static int AsteroidsCollected { get; set; }
    public static int AsteroidsShot { get; set; }

    public override void _Ready()
    {
        base._Ready();
        _statsLabel.Text = $"Score: {Score}\nTime Survived: {TimeSurvived}\nAsteroids Collected: {AsteroidsCollected}\nAsteroids Shot: {AsteroidsShot}";
    }

    public void Restart()
    {
        GetTree().CallDeferred("change_scene_to_file", _gameplay);
    }

    public void BackToTitle()
    {
        GetTree().CallDeferred("change_scene_to_file", _titleScreen);
    }
}
