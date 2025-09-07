using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node
{
	Godot.Collections.Array<Player> players = [];
	int arrow_index = 0;
	[Export]
	public string ARROWS { get; set; } = "*";

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

	private bool AnyoneAlive() {
		return players.Where(p => p.alive).Any();
	}
	public bool Won() {
		return this.arrow_index == this.ARROWS.Length;
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
		for(int i=0;i<this.ARROWS.Length;++i) {
			if(i == this.arrow_index) {
				display.AppendText(new string(['[']));
			}
			display.AppendText(new string([this.ARROWS[i]]));
			if(i == this.arrow_index) {
				display.AppendText(new string([']']));
			}
		}
	}

	private void ApplyMoveToArrows(Direction dir) {
		if(this.arrow_index == this.ARROWS.Length) return;

		char next_arrow = this.ARROWS[this.arrow_index];

		if(this.DirMatchesArrow(dir, next_arrow)) {
			this.arrow_index += 1;
		}
		else {
			this.arrow_index = this.DirMatchesArrow(dir, this.ARROWS[0]) ? 1 : 0;
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
		
		TileMapLayer tiles = this.FindChild("Tiles") as TileMapLayer;
		
		foreach(Node player in tiles.GetChildren()) {
			Player p = player as Player;
			if(p != null) {
				players.Add(p);
			}
		} 
		foreach(Node spike in tiles.GetChildren()) {
			Spike s = spike as Spike;
			if(s != null) {
				s.BodyEntered += SomethingSteppedOnSpike;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
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
