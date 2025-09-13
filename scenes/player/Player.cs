using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Player : CharacterBody2D, IUndoable
{
	public bool alive = true;
	private struct State
	{
		public Vector2 position;
		public bool alive;

		public State(Vector2 position, bool alive)
		{
			this.position = position;
			this.alive = alive;
		}
	}
	private List<State> states = new List<State>();

	private State GenerateState()
	{
		return new State(GlobalPosition, this.alive);
	}
	private void ApplyState(State s)
	{
		GlobalPosition = s.position;
		this.alive = s.alive;
	}
	public void SaveState()
	{
		this.states.Add(this.GenerateState());
	}
	public void Undo()
	{
		if (this.states.Any())
		{
			this.ApplyState(this.states[this.states.Count - 1]);
			this.states.RemoveAt(this.states.Count - 1);
		}
	}
	public void ApplyInitialState()
	{
		if (this.states.Any())
		{
			this.SaveState();
			this.ApplyState(this.states[0]);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!this.alive)
		{
			this.Hide();
		}
		else
		{
			this.Show();
		}
	}

	public Vector2 WouldTryToMoveTo(Direction dir)
	{
		Vector2 velocity = MoveUtils.GetVelocityFromDirection(dir);
		return GlobalPosition + velocity * Constants.TILE_SIZE;
	}

	public void Move(Direction dir)
	{
		Vector2 velocity = MoveUtils.GetVelocityFromDirection(dir);
		this._Move(velocity);
	}

	public void _Move(Vector2 velocity)
	{
		GlobalPosition += velocity * Constants.TILE_SIZE;
	}

}
