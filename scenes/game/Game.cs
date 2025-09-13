using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node, IUndoable
{
	Godot.Collections.Array<string> level_names = ["tutorial", "tutorial_spike", "basic_one", "basic_medium", "basic_larger", "basic_larger_wildcard", "tutorial_twodirs", "twodirs_one", "wildcard_shuffle", "tight_shuffle", "tutorial_wall", "hard_wall"];
	public int current_level_index = 0;
	Level level = null;

	// Per-level stuff
	Arrows arrows;
	Godot.Collections.Array<Player> players = [];
	// TODO: just get a grid position or something
	Godot.Collections.Array<Wall> walls = [];
	Godot.Collections.Array<Sprite2D> arrow_sprites = [];


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


	private void LoadLevel(int num_level)
	{

		string level_name = level_names[num_level];
		this.current_level_index = num_level;
		this.states = new List<State>();

		level?.QueueFree();

		PackedScene requested_level_scene = GD.Load<PackedScene>($"res://levels/{level_name}.tscn");
		level = requested_level_scene.Instantiate() as Level;
		this.AddChild(level);

		this.arrows = new Arrows(level.arrows);
		while (this.arrow_sprites.Count > this.arrows.num_arrows)
		{
			this.arrow_sprites[this.arrow_sprites.Count - 1].QueueFree();
			this.arrow_sprites.RemoveAt(this.arrow_sprites.Count - 1);
		}
		foreach ((Sprite2D arrow_sprite, int sprite_index) in this.arrow_sprites.Select((sprite, i) => (sprite, i + 1)))
		{
			arrow_sprite.GlobalPosition = new Vector2(sprite_index * Constants.TILE_SIZE, (level.Height + 1) * Constants.TILE_SIZE);
		}
		while (this.arrow_sprites.Count < this.arrows.num_arrows)
		{
			int sprite_index = this.arrow_sprites.Count + 1;
			Sprite2D arrow_sprite = new Sprite2D();
			arrow_sprite.Name = $"ArrowSprite{sprite_index}";
			// TODO: each level should have a height. Use that instead of rando constant.
			arrow_sprite.GlobalPosition = new Vector2(sprite_index * Constants.TILE_SIZE, (level.Height + 1) * Constants.TILE_SIZE);

			this.arrow_sprites.Add(arrow_sprite);
			this.AddChild(arrow_sprite);
		}

		this.players = [];
		foreach (Node player in level.Tiles.GetChildren())
		{
			Player p = player as Player;
			if (p != null)
			{
				players.Add(p);
			}
		}
		foreach (Node spike in level.Tiles.GetChildren())
		{
			Spike s = spike as Spike;
			if (s != null)
			{
				s.BodyEntered += SomethingSteppedOnSpike;
				s.Disable(false);
			}
		}
		this.walls = [];
		foreach (Node wall in level.Tiles.GetChildren())
		{
			Wall w = wall as Wall;
			if (w != null)
			{
				walls.Add(w);
			}
		}
	}

	private bool AnyoneAlive()
	{
		return players.Where(p => p.alive).Any();
	}
	public bool Won()
	{
		return this.arrows.Completed();
	}

	private void DisplayArrows()
	{

		for (int i = 0; i < this.arrows.num_arrows; ++i)
		{
			string sprite_name = this.arrows.GetArrowSpriteName(i);
			string sprite_file = Arrows.GetArrowSpritePath(sprite_name);
			Texture2D texture = GD.Load(sprite_file) as Texture2D;
			float size = texture.GetSize().X;
			float scale = Constants.TILE_SIZE / size;

			this.arrow_sprites[i].Scale = new Vector2(scale, scale);
			this.arrow_sprites[i].Texture = texture;

			if (i < this.arrows.next_arrow_index)
			{
				this.arrow_sprites[i].Modulate = new Color(1f, 1f, 0f);
			}
			else
			{
				this.arrow_sprites[i].Modulate = new Color(1f, 1f, 1f);
			}

		}

		if (this.Won())
		{
			RichTextLabel display = this.FindChild("Instructions") as RichTextLabel;
			display.Text = "You Win!";
			return;
		}

	}

	private void SomethingSteppedOnSpike(Node2D body)
	{
		Player p = body as Player;

		if (p != null)
		{
			p.alive = false;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.LoadLevel(this.current_level_index);

		RichTextLabel display = this.FindChild("Instructions") as RichTextLabel;
		display.Text = "Arrow keys or WASD to move. Z to undo. R to restart.";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{

		if (this.Won() && current_level_index + 1 < this.level_names.Count)
		{
			this.LoadLevel(current_level_index + 1);
			return;
		}

		this.DisplayArrows();

		Direction? maybe_direction = this.GetInputDirection();
		bool undo = Input.IsActionJustPressed("undo");
		bool restart = Input.IsActionJustPressed("restart");
		if (undo == false && restart == false && maybe_direction == null) return;

		if (undo)
		{
			this.Undo();
			return;
		}

		if (restart)
		{
			this.SaveState();
			this.ApplyInitialState();
			return;
		}

		if (!this.AnyoneAlive())
		{
			return;
		}

		// We are going to do something - save state
		this.SaveState();

		Direction direction = maybe_direction.Value;
		foreach (Player p in players.Where(p => p.alive))
		{
			Vector2 p_destination = p.WouldTryToMoveTo(direction);
			if (walls.Where(w => w.GlobalPosition == p_destination).Any()) continue;

			p.Move(direction);
		}

		this.arrows.ApplyMoveToArrows(direction);
	}


	private Direction? GetInputDirection()
	{
		Direction? inputted_direction = null;
		int num_inputs = 0;

		if (Input.IsActionJustPressed("move_up"))
		{
			num_inputs += 1;
			inputted_direction = Direction.UP;
		}
		if (Input.IsActionJustPressed("move_right"))
		{
			num_inputs += 1;
			inputted_direction = Direction.RIGHT;
		}
		if (Input.IsActionJustPressed("move_down"))
		{
			num_inputs += 1;
			inputted_direction = Direction.DOWN;
		}
		if (Input.IsActionJustPressed("move_left"))
		{
			num_inputs += 1;
			inputted_direction = Direction.LEFT;
		}

		return num_inputs != 1 ? null : inputted_direction;
	}

}
