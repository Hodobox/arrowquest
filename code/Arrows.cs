using System.Collections.Generic;
using System.Linq;
using Godot;

public class Arrows : IUndoable {
    public int num_arrows;
    public int next_arrow_index = 0;
    public string arrows;

    public Arrows(string arrows) {
        this.arrows = arrows;
        this.next_arrow_index = 0;
        this.num_arrows = arrows.Length;
    }

    public bool Completed() {
        return this.next_arrow_index == this.num_arrows;
    }
    
    private struct State {
        public int next_arrow_index;

        public State(int next_arrow_index) {
            this.next_arrow_index = next_arrow_index;
        }
    }
    private List<State> states = [];
    private State GenerateState() {
		return new State(this.next_arrow_index);
	}
	private void ApplyState(State s) {
		this.next_arrow_index = s.next_arrow_index;
	}
	public void SaveState() {
		this.states.Add(this.GenerateState());
	}
	public void ApplyInitialState() {
		if(this.states.Any()) {
			this.ApplyState(this.states[0]);
		}
	}
	public void Undo() {
		if(this.states.Any()) {
			this.ApplyState(this.states[this.states.Count-1]);
			this.states.RemoveAt(this.states.Count-1);
		}
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

    public void ApplyMoveToArrows(Direction dir) {
		if(this.next_arrow_index == this.arrows.Length) return;

		char next_arrow = this.arrows[this.next_arrow_index];

		if(this.DirMatchesArrow(dir, next_arrow)) {
			this.next_arrow_index += 1;
		}
		else {
			this.next_arrow_index = this.DirMatchesArrow(dir, this.arrows[0]) ? 1 : 0;
		}
	}
}