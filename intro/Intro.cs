using Godot;
using System;

public partial class Intro : Control
{
    [Export]
    private string _gameplay;

    public void Start()
    {
        GetTree().CallDeferred("change_scene_to_file", _gameplay);
    }
}
