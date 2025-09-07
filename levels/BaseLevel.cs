using Godot;

public partial class BaseLevel : TileMapLayer
{
	[Export]
	public string arrows { get; set; }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		UpdateInternals();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
