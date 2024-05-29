// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;
using System;

public partial class GameScene : Control
{
    [Export] private SceneDataSO _nextScene;

    private SceneLoader _sceneLoader;
    private InputManager _inputManager;

    public override void _Ready()
    {
        _sceneLoader = GetNode<SceneLoader>("/root/SceneLoader");
        _inputManager = GetNode<InputManager>("/root/InputManager");
        _inputManager.OnInputActionEvent += WaitForConfirmInput;
    }

    private void WaitForConfirmInput(InputManager.InputAction action, InputManager.PressType type)
    {
        if (action is InputManager.InputAction.Confirm && type is InputManager.PressType.JustPressed)
        {
            _sceneLoader.ChangeScenes(_nextScene, GetParent(), this);
            _inputManager.OnInputActionEvent -= WaitForConfirmInput;
        }
    }
}