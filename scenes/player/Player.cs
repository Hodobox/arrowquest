using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Player : CharacterBody2D
{
	public bool alive = true;
	private struct State {
		public Vector2 position;
		public bool alive;

		public State(Vector2 position, bool alive) {
			this.position = position;
			this.alive = alive;
		}
	}
	private Stack<State> states = new Stack<State>();
	
	private State GenerateState() {
		return new State(GlobalPosition, this.alive);
	}
	private void ApplyState(State s) {
		GlobalPosition = s.position;
		this.alive = s.alive;
	}
	public void SaveState() {
		this.states.Push(this.GenerateState());
	}
	public void Undo() {
		if(this.states.Any()) {
			this.ApplyState(this.states.Pop());
		}
	}

	
	public override void _PhysicsProcess(double delta)
	{
		
	}

	public void Move(Direction dir) {
		Vector2 velocity = MoveUtils.GetVelocityFromDirection(dir); 
		this._Move(velocity);
	}

	public void _Move(Vector2 velocity) {
		GlobalPosition += velocity * Constants.TILE_SIZE;
	}

	

}
