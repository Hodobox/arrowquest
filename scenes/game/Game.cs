using Godot;
using System.Collections.Generic;
using System.Linq;

// TODO: rename everything to fit C# naming conventions
public partial class Game : Node
{
	readonly List<string> level_names = ["tutorial", "tutorial_spike", "basic_one", "basic_medium", "basic_larger", "basic_larger_wildcard", "tutorial_twodirs", "twodirs_one", "wildcard_shuffle", "tight_shuffle", "tutorial_wall", "hard_wall", "tutorial_fbp", "fbp_one"];
	public int current_level_index = 0;
	Level level = null;

	private Menu _menu;
	private bool _showingMenu = false;

	private List<Sprite2D> arrow_sprites = [];

	private void LoadLevel(int num_level)
	{
		string level_name = level_names[num_level];
		this.current_level_index = num_level;

		level?.QueueFree();

		PackedScene requested_level_scene = GD.Load<PackedScene>($"res://levels/{level_name}.tscn");
		level = requested_level_scene.Instantiate() as Level;
		this.AddChild(level);

		while (this.arrow_sprites.Count > this.level.arrows.num_arrows)
		{
			this.arrow_sprites[this.arrow_sprites.Count - 1].QueueFree();
			this.arrow_sprites.RemoveAt(this.arrow_sprites.Count - 1);
		}
		foreach ((Sprite2D arrow_sprite, int sprite_index) in this.arrow_sprites.Select((sprite, i) => (sprite, i + 1)))
		{
			arrow_sprite.GlobalPosition = new Vector2(sprite_index * Constants.TILE_SIZE, (level.Height + 1) * Constants.TILE_SIZE);
		}
		while (this.arrow_sprites.Count < this.level.arrows.num_arrows)
		{
			int sprite_index = this.arrow_sprites.Count + 1;
			Sprite2D arrow_sprite = new Sprite2D();
			arrow_sprite.Name = $"ArrowSprite{sprite_index}";
			// TODO: each level should have a height. Use that instead of rando constant.
			arrow_sprite.GlobalPosition = new Vector2(sprite_index * Constants.TILE_SIZE, (level.Height + 1) * Constants.TILE_SIZE);

			this.arrow_sprites.Add(arrow_sprite);
			this.AddChild(arrow_sprite);
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
	}


	private void DisplayArrows()
	{
		for (int i = 0; i < this.level.arrows.num_arrows; ++i)
		{
			string sprite_name = this.level.arrows.GetArrowSpriteName(i);
			string sprite_file = Arrows.GetArrowSpritePath(sprite_name);
			Texture2D texture = GD.Load(sprite_file) as Texture2D;
			float size = texture.GetSize().X;
			float scale = Constants.TILE_SIZE / size;

			this.arrow_sprites[i].Scale = new Vector2(scale, scale);
			this.arrow_sprites[i].Texture = texture;

			if (i < this.level.arrows.next_arrow_index)
			{
				this.arrow_sprites[i].Modulate = new Color(1f, 1f, 0f);
			}
			else
			{
				this.arrow_sprites[i].Modulate = new Color(1f, 1f, 1f);
			}

		}
	}

	private void ToggleMenu()
	{
		_showingMenu = !_showingMenu;

		if (_showingMenu)
		{
			level.SetVisible(false);
			foreach (Sprite2D sprite in arrow_sprites)
			{
				sprite.Hide();
			}
			_menu.HoveredLevel = this.current_level_index;
			_menu.Show();
		}
		else
		{
			level.SetVisible(true);
			foreach (Sprite2D sprite in arrow_sprites)
			{
				sprite.Show();
			}
			_menu.Hide();
		}
	}

	private void HandleMenuActions()
	{
		Direction? maybe_direction = this.GetInputDirection();
		bool selection_made = Input.IsActionJustPressed("menu_confirm");
		if (maybe_direction == null && !selection_made) return;

		if (selection_made)
		{
			int level_index = _menu.HoveredLevel;
			if (level_index != current_level_index)
			{
				this.LoadLevel(level_index);
			}
			this.ToggleMenu();
			return;
		}

		_menu.Move(maybe_direction.Value);
	}

	private void HandleLevelActions()
	{
		if (this.level.Won)
		{
			if (current_level_index + 1 < this.level_names.Count)
			{
				this.LoadLevel(current_level_index + 1);
				return;
			}

			RichTextLabel display = this.FindChild("Instructions") as RichTextLabel;
			display.Text = "You Win!";
		}

		this.DisplayArrows();

		Direction? maybe_direction = this.GetInputDirection();
		bool undo = Input.IsActionJustPressed("undo");
		bool restart = Input.IsActionJustPressed("restart");
		if (undo == false && restart == false && maybe_direction == null) return;

		if (undo)
		{
			this.level.Undo();
			return;
		}

		if (restart)
		{
			this.level.ApplyInitialState();
			return;
		}

		bool moved = this.level.Move(maybe_direction.Value);

		if (moved && this.level.Won)
		{
			(this.FindChild("SoundLevelWon") as AudioStreamPlayer).Play();
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
		_menu = this.FindChild("Menu") as Menu;
		_menu.SetLevels(level_names);
		_menu.Hide();

		this.LoadLevel(this.current_level_index);

		RichTextLabel display = this.FindChild("Instructions") as RichTextLabel;
		display.Text = "Arrow keys or WASD to move. Z to undo. R to restart. ESC for level select.";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{

		bool toggleMenu = Input.IsActionJustPressed("toggle_menu");

		if (toggleMenu)
		{
			ToggleMenu();
			return;
		}

		if (_showingMenu)
		{
			HandleMenuActions();
			return;
		}
		else
		{
			HandleLevelActions();
		}
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
