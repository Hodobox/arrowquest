using Godot;

public partial class Level : TileMapLayer
{
	[Export]
	public string arrows { get; set; }

	// The height of the tileset, vertically, in number of tiles
	public int Height
	{
		get { return this.GetUsedRect().End.Y; }
	}
	// The width of the tileset, vertically, in number of tiles
	public int Width
	{
		get { return this.GetUsedRect().End.X; }
	}


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
