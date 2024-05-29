// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;
using System;

public partial class SceneTransition : CanvasLayer
{
    [Export] private LoadingScreen _loadingScreen;
    [Export] private AnimationPlayer _animationPlayer;

    private SceneLoader _sceneLoader;
    private InputManager _inputManager;

    private float _loadingProgress;

    public TransitionSettingsSO Settings;

    public Action OnTransitionIn;
    public Action OnTransitionOut;

    public float LoadingProgress
    {
        get => _loadingProgress;
        set => SetLoadingProgress(value);
    }

    public override void _Ready()
    {
        Visible = false;
        _loadingScreen.Visible = false;

        _sceneLoader = GetNode<SceneLoader>("/root/SceneLoader");
        _inputManager = GetNode<InputManager>("/root/InputManager");
        _sceneLoader.OnContentFinishedLoading += FinishTransition;
    }

    public async void StartTransition()
    {
        Visible = true;

        if (Settings.ShowLoadingScreen)
        {
            _loadingScreen.Visible = true;
        }

        _animationPlayer.Play("Default");
        await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        OnTransitionIn?.Invoke();
    }

    private void SetLoadingProgress(float value)
    {
        if (_loadingProgress != value)
        {
            _loadingProgress = value;
            _loadingScreen.UpdateProgressBar(_loadingProgress);
        }
    }

    private void FinishTransition()
    {
        _loadingScreen.UpdateProgressBar(100);

        if (Settings.WaitForInput)
        {
            _inputManager.OnInputActionEvent += WaitForConfirmInput;
            _loadingScreen.UpdateLabel("Press Space To Continue");
        }
        else
        {
            TransitionOut();
        }
    }

    private void WaitForConfirmInput(InputManager.InputAction action, InputManager.PressType type)
    {
        if (action is InputManager.InputAction.Confirm && type is InputManager.PressType.JustPressed)
        {
            TransitionOut();
            _inputManager.OnInputActionEvent -= WaitForConfirmInput;
        }
    }

    private async void TransitionOut()
    {
        _sceneLoader.LoadNewScene();
        _animationPlayer.PlayBackwards();

        await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        OnTransitionOut?.Invoke();
        QueueFree();
    }

    public override void _ExitTree()
    {
        _sceneLoader.OnContentFinishedLoading -= FinishTransition;
    }
}