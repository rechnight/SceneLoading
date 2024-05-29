using Godot;
using System;

public partial class Main : Node
{
	[Export] private PackedScene _firstScene;

	public override void _Ready()
	{
		GD.Print(_firstScene.ResourceName);
		AddChild(_firstScene.Instantiate());
	}
}
