using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node
{
	public int current_level;
	Godot.Collections.Array<BaseLevel> levels = [];

	// Per-level stuff
	Godot.Collections.Array<Player> players = [];
	public int arrow_index = 0;
	public string arrows;
	

	private struct State{
		public int arrow_index;

		public State(int arrow_index) {
			this.arrow_index = arrow_index;
		}
	}
	private Stack<State> states = new Stack<State>();
	private State GenerateState() {
		return new State(this.arrow_index);
	}
	private void ApplyState(State s) {
		this.arrow_index = s.arrow_index;
	}
	public void Undo() {
		foreach(Player p in this.players) {
			p.Undo();
		}
		if(this.states.Any()) {
			this.ApplyState(this.states.Pop());
		}
	}
	public void SaveState() {
		foreach(Player p in this.players) {
			p.SaveState();
		}
		this.states.Push(this.GenerateState());
	}

	private string GetLevelName(int level) {
		return $"Level{level:D2}";
	}

	private void LoadLevel(int num_level) {
		
		string level_name = GetLevelName(num_level);
		this.current_level = num_level;
		this.states = new Stack<State>();

		foreach(BaseLevel level in this.levels) {
			if(level.Name != level_name) {
				level.Hide();
				continue;
			}
			level.Show();
			this.arrow_index = 0;
			this.arrows = level.arrows;

			foreach(Node player in level.GetChildren()) {
				Player p = player as Player;
				if(p != null) {
					players.Add(p);
				}
			} 
			foreach(Node spike in level.GetChildren()) {
				Spike s = spike as Spike;
				if(s != null) {
					s.BodyEntered += SomethingSteppedOnSpike;
				}
			}
		}
	}

	private bool AnyoneAlive() {
		return players.Where(p => p.alive).Any();
	}
	public bool Won() {
		return this.arrow_index == this.arrows.Length;
	}

	private bool DirMatchesArrow(Direction dir, char arrow) {
		// TODO: this needs an enum or something.
		if(arrow == '*') return true;
		if(arrow == 'U') return dir == Direction.UP;
		if(arrow == 'R') return dir == Direction.RIGHT;
		if(arrow == 'D') return dir == Direction.DOWN;
		if(arrow == 'L') return dir == Direction.LEFT;
		GD.PrintErr($"Invalid arrow character in level: '{arrow}'");
		return false;
	}

	private void DisplayArrows() {
		RichTextLabel display = this.FindChild("ArrowDisplay") as RichTextLabel;
		
		if(this.Won()) {
			display.Text = "You Win!";
			return;
		}
		
		display.Text = "";
		for(int i=0;i<this.arrows.Length;++i) {
			if(i == this.arrow_index) {
				display.AppendText(new string(['[']));
			}
			display.AppendText(new string([this.arrows[i]]));
			if(i == this.arrow_index) {
				display.AppendText(new string([']']));
			}
		}
	}

	private void ApplyMoveToArrows(Direction dir) {
		if(this.arrow_index == this.arrows.Length) return;

		char next_arrow = this.arrows[this.arrow_index];

		if(this.DirMatchesArrow(dir, next_arrow)) {
			this.arrow_index += 1;
		}
		else {
			this.arrow_index = this.DirMatchesArrow(dir, this.arrows[0]) ? 1 : 0;
		}
	}

	private void SomethingSteppedOnSpike(Node2D body) {
		Player p = body as Player;

		if(p != null) {
			p.alive = false;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		for(int i=1;i<=99;++i) {
			string level_name = GetLevelName(i);
			BaseLevel level = this.FindChild(level_name, false) as BaseLevel;
			if(level == null) {
				break;
			}

			this.levels.Add(level);
		}

		this.LoadLevel(1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{

		if(this.Won() && current_level < this.levels.Count) {
			this.LoadLevel(current_level+1);
			return;
		}

		this.DisplayArrows();

		Direction? maybe_direction = this.GetInputDirection();
		bool undo = Input.IsActionJustPressed("undo");
		if(undo == false && maybe_direction == null) return;

		if(undo) {
			this.Undo();
			return;
		}

		if(!this.AnyoneAlive()) {
			return;
		}

		// We are going to do something - save state
		this.SaveState();

		Direction direction = maybe_direction.Value;
		foreach(Player p in players.Where(p => p.alive)) {
			p.Move(direction);
		}

		this.ApplyMoveToArrows(direction);
	}


	private Direction? GetInputDirection() {
		Direction? inputted_direction = null;
		int num_inputs = 0;

		if(Input.IsActionJustPressed("move_up")) {
			num_inputs += 1;
			inputted_direction = Direction.UP;
		}
		if(Input.IsActionJustPressed("move_right")) {
			num_inputs += 1;
			inputted_direction = Direction.RIGHT;
		}
		if(Input.IsActionJustPressed("move_down")) {
			num_inputs += 1;
			inputted_direction = Direction.DOWN;
		}
		if(Input.IsActionJustPressed("move_left")) {
			num_inputs += 1;
			inputted_direction = Direction.LEFT;
		}

		return num_inputs != 1 ? null : inputted_direction;
	}
	
}
