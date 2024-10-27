using Godot;
using System;

public partial class TextParticle : Label
{
	private static Color outlineColor = new Color(0, 0, 0);

	private Color textColor;
	private double LifeTime;
	private double TotalLifeTime;

	public TextParticle() {}

	public TextParticle(
		string text,
		Vector2 position,
		double lifeTime
	) {
		this.GrowHorizontal = (GrowDirection) 2;
		this.Text = text;
		this.Position = position;
		this.HorizontalAlignment = (HorizontalAlignment) 1;
		this.LifeTime = lifeTime;
		this.TotalLifeTime = lifeTime;
		this.ZIndex = 1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.LifeTime -= delta;
		this.GlobalPosition += new Vector2(0.0f, -1.1f * (float)delta);

		if (this.LifeTime <= 0) {
			this.QueueFree();
		}
	}
}
