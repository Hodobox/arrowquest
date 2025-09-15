using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class Menu : Control
{

	public int HoveredLevel
	{
		get => _hoveredLevel;
		set
		{
			if (_hoveredLevel < _levelSelectors.Count)
			{
				RichTextLabel hovered = _levelSelectors[_hoveredLevel];
				if (hovered.HasThemeColorOverride("default_color"))
				{
					hovered.RemoveThemeColorOverride("default_color");
				}
			}
			_hoveredLevel = value;
		}
	}


	private int _hoveredLevel = 0;
	private List<string> _levelNames = [];
	private List<RichTextLabel> _levelSelectors = [];

	public void SetLevels(List<string> levelNames)
	{
		_levelNames = levelNames;

		while (_levelSelectors.Count > _levelNames.Count)
		{
			_levelSelectors[_levelSelectors.Count - 1].QueueFree();
			_levelSelectors.RemoveAt(_levelSelectors.Count - 1);
		}
		while (_levelSelectors.Count < _levelNames.Count)
		{
			RichTextLabel label = new RichTextLabel();
			label.Name = $"LevelLabel{_levelSelectors.Count}";
			label.AddThemeFontSizeOverride("Normal", 32);
			label.SetSize(new Vector2(20 * Constants.TILE_SIZE, Constants.TILE_SIZE));
			this.AddChild(label);
			_levelSelectors.Add(label);
		}

		for (int i = 0; i < _levelNames.Count; i++)
		{
			_levelSelectors[i].Text = _levelNames[i];
			_levelSelectors[i].Position = new Vector2(Constants.TILE_SIZE, (i + 1) * Constants.TILE_SIZE);
		}

	}

	// Moves the selection within the menu in the specified direction
	public void Move(Direction direction)
	{
		switch (direction)
		{
			case Direction.UP:
				HoveredLevel = Math.Max(HoveredLevel - 1, 0);
				break;
			case Direction.DOWN:
				HoveredLevel = Math.Min(HoveredLevel + 1, _levelNames.Count - 1);
				break;
			case Direction.LEFT:
			case Direction.RIGHT:
			default:
				break;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_hoveredLevel < _levelSelectors.Count)
		{
			RichTextLabel hovered = _levelSelectors[_hoveredLevel];
			if (!hovered.HasThemeColorOverride("default_color"))
			{
				hovered.AddThemeColorOverride("default_color", new Color(1f, 1f, 0f));
			}
		}
	}
}
