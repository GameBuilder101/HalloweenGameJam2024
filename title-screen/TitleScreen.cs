using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export]
	private PackedScene _gameplay;

	public void Start()
	{
        GetTree().CallDeferred("change_scene_to_packed", _gameplay);
    }

	public void Quit()
	{
		GetTree().Quit();
	}
}
