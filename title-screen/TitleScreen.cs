using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export]
	private string _gameplay;

	public void Start()
	{
        GetTree().CallDeferred("change_scene_to_file", _gameplay);
    }

	public void Quit()
	{
		GetTree().Quit();
	}
}
