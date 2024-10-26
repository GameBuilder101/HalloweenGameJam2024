using Godot;
using System;

public partial class GameOver : Control
{
    [Export]
    private PackedScene _gameplay;
    [Export]
    private PackedScene _titleScreen;

    public void Restart()
    {
        GetTree().CallDeferred("change_scene_to_packed", _gameplay);
    }

    public void BackToTitle()
    {
        GetTree().CallDeferred("change_scene_to_packed", _titleScreen);
    }
}
