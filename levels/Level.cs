using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Level : Node, IUndoable
{

	// === Public variables and properties ===
	[Export]
	private string arrows_str { get; set; }

	// The TileMapLayer of the in-game objects
	public TileMapLayer Tiles;
	// The arrows required to beat the level
	public Arrows arrows;

	// The height of the tileset, vertically, in number of tiles
	public int Height
	{
		get { return this.Tiles.GetUsedRect().End.Y; }
	}
	// The width of the tileset, vertically, in number of tiles
	public int Width
	{
		get { return this.Tiles.GetUsedRect().End.X; }
	}

	public bool Won
	{
		get { return this.arrows.Completed; }
	}

	// === Private variables and properties ===
	public List<Player> players;
	private List<Wall> walls;

	// === Public methods ===


	// true if any player in the level is alive
	public bool AnyoneAlive()
	{
		return players.Where(p => p.alive).Any();
	}

	// Performs the in-game actions when the player makes a move
	public bool Move(Direction dir)
	{
		if (!this.AnyoneAlive()) return false;

		foreach (Player p in this.players.Where(p => p.alive))
		{
			Vector2 p_destination = p.WouldTryToMoveTo(dir);
			if (walls.Where(w => w.GlobalPosition == p_destination).Any()) continue;

			p.Move(dir);
		}

		this.arrows.ApplyMoveToArrows(dir);

		return true;
	}

	// === IUndoable ===

	private struct State
	{
		public State() { }
	}
	private List<State> states = new List<State>();
	private State GenerateState()
	{
		return new State();
	}
	private void ApplyState(State s) { }
	public void SaveState()
	{
		foreach (Player p in this.players)
		{
			p.SaveState();
		}
		this.arrows.SaveState();
		this.states.Add(this.GenerateState());
	}
	public void ApplyInitialState()
	{
		foreach (Player p in this.players)
		{
			p.ApplyInitialState();
		}
		this.arrows.ApplyInitialState();
		if (this.states.Any())
		{
			this.ApplyState(this.states[0]);
		}
	}
	public void Undo()
	{
		foreach (Player p in this.players)
		{
			p.Undo();
		}
		this.arrows.Undo();
		if (this.states.Any())
		{
			this.ApplyState(this.states[this.states.Count - 1]);
			this.states.RemoveAt(this.states.Count - 1);
		}
	}

	// === Godot overrides ===

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Tiles = this.FindChild("Tiles") as TileMapLayer;
		this.Tiles.UpdateInternals();

		this.arrows = new Arrows(this.arrows_str);

		this.players = [];
		this.walls = [];
		foreach (Node tile in Tiles.GetChildren())
		{
			{
				Player p = tile as Player;
				if (p != null)
				{
					players.Add(p);
				}
			}

			{
				Wall w = tile as Wall;
				if (w != null)
				{
					walls.Add(w);
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
