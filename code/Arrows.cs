using System.Collections.Generic;
using System.Linq;
using Godot;

// All possible actions that might be required from the player
// If modifying, ensure that Encode, Decode, and GetSpriteName takes it into account.
public enum ArrowAction
{
	UP,
	RIGHT,
	DOWN,
	LEFT,
	UPDOWN,
	LEFTRIGHT,
	UPLEFT,
	UPRIGHT,
	DOWNLEFT,
	DOWNRIGHT,
	NOTUP,
	NOTRIGHT,
	NOTDOWN,
	NOTLEFT,
	BACK,
	NOTBACK,
	FORWARD,
	NOTFORWARD,
	PERPENDICULAR,
	PARALLEL,
	ANYDIRECTION
}

// TODO: this class needs cleanup. Split into multiple sections or something. Partial? Maybe?
// and comment methods
public class Arrows : IUndoable
{
	public int num_arrows;
	public int next_arrow_index = 0;
	public List<ArrowAction> arrows;
	public Direction? last_user_action;

	public Arrows(string arrows) : this(DecodeArrowActions(arrows)) { }

	public Arrows(List<ArrowAction> arrows)
	{
		this.arrows = arrows;
		this.next_arrow_index = 0;
		this.num_arrows = arrows.Count;
	}

	public static char ARROW_ENCODING_SEPARATOR = ';';

	// The DecodeArrowActions and EncodeArrowActions should form a bijection, i.e. Decode(Encode(x)) === x.
	// Bijection dictionaries exist, but I feel like implementing and using one is overkill. This shouldn't be modified often.
	public static List<ArrowAction> DecodeArrowActions(string encoded_arrows)
	{
		return encoded_arrows.Split(ARROW_ENCODING_SEPARATOR).Select<string, ArrowAction>(arr =>
		{
			return arr switch
			{
				"U" => ArrowAction.UP,
				"R" => ArrowAction.RIGHT,
				"D" => ArrowAction.DOWN,
				"L" => ArrowAction.LEFT,
				"UD" => ArrowAction.UPDOWN,
				"LR" => ArrowAction.LEFTRIGHT,
				"UL" => ArrowAction.UPLEFT,
				"UR" => ArrowAction.UPRIGHT,
				"DL" => ArrowAction.DOWNLEFT,
				"DR" => ArrowAction.DOWNRIGHT,
				"!U" => ArrowAction.NOTUP,
				"!R" => ArrowAction.NOTRIGHT,
				"!D" => ArrowAction.NOTDOWN,
				"!L" => ArrowAction.NOTLEFT,
				"B" => ArrowAction.BACK,
				"!B" => ArrowAction.NOTBACK,
				"F" => ArrowAction.FORWARD,
				"!F" => ArrowAction.NOTFORWARD,
				"P" => ArrowAction.PERPENDICULAR,
				"!P" => ArrowAction.PARALLEL,
				"*" => ArrowAction.ANYDIRECTION,
				_ => throw new System.NotImplementedException()
			};
		}).ToList();
	}

	public static string EncodeArrowActions(List<ArrowAction> arrows)
	{
		return string.Join(ARROW_ENCODING_SEPARATOR, arrows.Select<ArrowAction, string>(arr =>
			{
				return arr switch
				{
					ArrowAction.UP => "U",
					ArrowAction.RIGHT => "R",
					ArrowAction.DOWN => "D",
					ArrowAction.LEFT => "L",
					ArrowAction.UPDOWN => "UD",
					ArrowAction.LEFTRIGHT => "LR",
					ArrowAction.UPLEFT => "UL",
					ArrowAction.UPRIGHT => "UR",
					ArrowAction.DOWNLEFT => "DL",
					ArrowAction.DOWNRIGHT => "DR",
					ArrowAction.NOTUP => "!U",
					ArrowAction.NOTRIGHT => "!R",
					ArrowAction.NOTDOWN => "!D",
					ArrowAction.NOTLEFT => "!L",
					ArrowAction.BACK => "B",
					ArrowAction.NOTBACK => "!B",
					ArrowAction.FORWARD => "F",
					ArrowAction.NOTFORWARD => "!F",
					ArrowAction.PERPENDICULAR => "P",
					ArrowAction.PARALLEL => "!P",
					ArrowAction.ANYDIRECTION => "*",
					_ => throw new System.NotImplementedException()
				};
			}

		));
	}

	private static ArrowAction GetSimplestArrowActionWhichAllows(List<Direction> directions)
	{
		bool up = directions.Contains(Direction.UP),
			right = directions.Contains(Direction.RIGHT),
			down = directions.Contains(Direction.DOWN),
			left = directions.Contains(Direction.LEFT);

		// 4
		if (up && right && down && left) return ArrowAction.ANYDIRECTION;
		// 3
		if (up && right && down) return ArrowAction.NOTLEFT;
		if (up && right && left) return ArrowAction.NOTDOWN;
		if (up && down && left) return ArrowAction.NOTRIGHT;
		if (right && down && left) return ArrowAction.NOTUP;
		// 2
		if (up && right) return ArrowAction.UPRIGHT;
		if (up && down) return ArrowAction.UPDOWN;
		if (up && left) return ArrowAction.UPLEFT;
		if (right && down) return ArrowAction.DOWNRIGHT;
		if (right && left) return ArrowAction.LEFTRIGHT;
		if (left && down) return ArrowAction.DOWNLEFT;
		// 1
		if (up) return ArrowAction.UP;
		if (right) return ArrowAction.RIGHT;
		if (down) return ArrowAction.DOWN;
		if (left) return ArrowAction.LEFT;
		// 0
		GD.PrintErr("Requested arrow action which allows no directions. Returning notfoward.");
		return ArrowAction.NOTFORWARD;
	}

	public bool Completed()
	{
		return this.next_arrow_index == this.num_arrows;
	}

	private static string GetArrowSpriteName(ArrowAction arr)
	{
		return arr switch
		{
			ArrowAction.UP => "arrow_up",
			ArrowAction.RIGHT => "arrow_right",
			ArrowAction.DOWN => "arrow_down",
			ArrowAction.LEFT => "arrow_left",
			ArrowAction.UPDOWN => "arrow_updown",
			ArrowAction.LEFTRIGHT => "arrow_leftright",
			ArrowAction.UPLEFT => "arrow_leftup",
			ArrowAction.UPRIGHT => "arrow_rightup",
			ArrowAction.DOWNLEFT => "arrow_leftdown",
			ArrowAction.DOWNRIGHT => "arrow_rightdown",
			ArrowAction.NOTUP => "arrow_notup",
			ArrowAction.NOTRIGHT => "arrow_notright",
			ArrowAction.NOTDOWN => "arrow_notdown",
			ArrowAction.NOTLEFT => "arrow_notleft",
			ArrowAction.BACK => "arrow_back",
			ArrowAction.NOTBACK => "arrow_notback",
			ArrowAction.FORWARD => "arrow_forward",
			ArrowAction.NOTFORWARD => "arrow_notforward",
			ArrowAction.PERPENDICULAR => "arrow_perpendicular",
			ArrowAction.PARALLEL => "arrow_parallel",
			ArrowAction.ANYDIRECTION => "arrow_any",
			_ => throw new System.NotImplementedException()
		};
	}

	// Resolves a possibly complex ArrowAction (e.g. back, parallel) into a possibly
	// simplified form
	private ArrowAction ResolveArrowAction(ArrowAction arr)
	{
		if (this.last_user_action == null) return arr;

		// Really? It can't tell a nullable<T> after null check is really a T?
		Direction last_action = this.last_user_action.Value;

		return arr switch
		{
			ArrowAction.BACK => GetSimplestArrowActionWhichAllows([last_action.Opposite()]),
			ArrowAction.NOTBACK => GetSimplestArrowActionWhichAllows([last_action, .. last_action.Perpendicular()]),
			ArrowAction.FORWARD => GetSimplestArrowActionWhichAllows([last_action]),
			ArrowAction.NOTFORWARD => GetSimplestArrowActionWhichAllows([last_action.Opposite(), .. last_action.Perpendicular()]),
			ArrowAction.PERPENDICULAR => GetSimplestArrowActionWhichAllows(last_action.Perpendicular()),
			ArrowAction.PARALLEL => GetSimplestArrowActionWhichAllows(last_action.Parallel()),
			_ => arr
		};
	}

	public string GetArrowSpriteName(int index)
	{
		if (index < 0 || index >= this.num_arrows)
		{
			GD.PrintErr($"Requested sprite for arrow at index {index}, but arrow object only has {this.num_arrows} arrows");
			return "error";
		}

		if (index != this.next_arrow_index) { return GetArrowSpriteName(this.arrows[index]); }
		else { return GetArrowSpriteName(this.ResolveArrowAction(this.arrows[index])); }
	}

	public static string GetArrowSpritePath(string arrow_sprite_name)
	{
		return $"res://sprites/arrows/{arrow_sprite_name}.png";
	}

	private struct State
	{
		public int next_arrow_index;
		public Direction? last_user_action;

		public State(int next_arrow_index, Direction? last_user_action)
		{
			this.next_arrow_index = next_arrow_index;
			this.last_user_action = last_user_action;
		}
	}
	private List<State> states = [];
	private State GenerateState()
	{
		return new State(this.next_arrow_index, this.last_user_action);
	}
	private void ApplyState(State s)
	{
		this.next_arrow_index = s.next_arrow_index;
		this.last_user_action = s.last_user_action;
	}
	public void SaveState()
	{
		this.states.Add(this.GenerateState());
	}
	public void ApplyInitialState()
	{
		if (this.states.Any())
		{
			this.SaveState();
			this.ApplyState(this.states[0]);
		}
	}
	public void Undo()
	{
		if (this.states.Any())
		{
			this.ApplyState(this.states[this.states.Count - 1]);
			this.states.RemoveAt(this.states.Count - 1);
		}
	}

	private List<Direction> AllowedDirectionsForAction(ArrowAction arr)
	{
		return this.ResolveArrowAction(arr) switch
		{
			ArrowAction.UP => [Direction.UP],
			ArrowAction.RIGHT => [Direction.RIGHT],
			ArrowAction.DOWN => [Direction.DOWN],
			ArrowAction.LEFT => [Direction.LEFT],
			ArrowAction.UPDOWN => [Direction.UP, Direction.DOWN],
			ArrowAction.LEFTRIGHT => [Direction.LEFT, Direction.RIGHT],
			ArrowAction.UPLEFT => [Direction.UP, Direction.LEFT],
			ArrowAction.UPRIGHT => [Direction.UP, Direction.RIGHT],
			ArrowAction.DOWNLEFT => [Direction.DOWN, Direction.LEFT],
			ArrowAction.DOWNRIGHT => [Direction.DOWN, Direction.RIGHT],
			ArrowAction.NOTUP => [Direction.RIGHT, Direction.DOWN, Direction.LEFT],
			ArrowAction.NOTRIGHT => [Direction.UP, Direction.DOWN, Direction.LEFT],
			ArrowAction.NOTDOWN => [Direction.UP, Direction.RIGHT, Direction.LEFT],
			ArrowAction.NOTLEFT => [Direction.UP, Direction.RIGHT, Direction.DOWN],
			ArrowAction.ANYDIRECTION => [Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT],
			_ => []
		};
	}

	private bool DirMatchesArrow(Direction dir, ArrowAction arr)
	{
		return this.AllowedDirectionsForAction(arr).Contains(dir);
	}

	public void ApplyMoveToArrows(Direction dir)
	{
		if (this.next_arrow_index == this.arrows.Count) return;

		if (this.DirMatchesArrow(dir, this.arrows[this.next_arrow_index]))
		{
			this.next_arrow_index += 1;
		}
		else
		{
			this.next_arrow_index = 0;
		}

		this.last_user_action = dir;
	}
}
