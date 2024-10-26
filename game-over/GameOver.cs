using Godot;
using System;

public partial class GameOver : Control
{
    [Export]
    private string _gameplay;
    [Export]
    private string _titleScreen;

    public void Restart()
    {
        GetTree().CallDeferred("change_scene_to_file", _gameplay);
    }

    public void BackToTitle()
    {
        GetTree().CallDeferred("change_scene_to_file", _titleScreen);
    }
}
