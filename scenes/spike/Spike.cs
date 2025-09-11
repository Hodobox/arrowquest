using Godot;
using System;

public partial class Spike : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Disable(bool yes)
	{
		this.FindChild("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, yes);
	}
}
