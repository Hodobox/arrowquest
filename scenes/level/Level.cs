using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node
{
	Godot.Collections.Array<Player> players = [];

	private bool AnyoneAlive() {
		return players.Where(p => p.alive).Any();
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		Direction? maybe_direction = this.GetInputDirection();
		bool undo = Input.IsActionJustPressed("undo");
		if(undo == false && maybe_direction == null) return;

		if(undo) {
			this.Undo();
			return;
		}

		if(!this.AnyoneAlive()) {
			// return;
		}

		// We are going to do something - save state
		this.SaveState();

		Direction direction = maybe_direction.Value;

		foreach(Player p in players) {
			p.Move(direction);
		}
	}

	public void Undo() {
		foreach(Player p in this.players) {
			p.Undo();
		}
	}
	public void SaveState() {
		foreach(Player p in this.players) {
			p.SaveState();
		}
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
