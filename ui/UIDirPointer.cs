using Godot;
using System;

[GlobalClass]
public partial class UIDirPointer : TextureRect
{
	/// <summary>
	/// The object pointing from (the player).
	/// </summary>
	[Export]
	private Node2D _positionReference;
	/// <summary>
	/// The thing to point to.
	/// </summary>
	[Export]
	private Node2D _guidanceTarget;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Rotation = (_guidanceTarget.Position - _positionReference.Position).Angle() + Mathf.Pi * 0.5f;
	}
}
