// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;
using System;
using System.Collections.Generic;

public partial class InputManager : Node
{
    [Export] private UpdateType _updateOn;
    [Export] private InputDeviceType _inputDevice;

    private const float DOUBLE_PRESS_TIME = 0.25f;
    private const float LONG_PRESS_TIME = 1.0f;

    private readonly Dictionary<string, float> _lastPressTimes = new();
    private readonly Dictionary<string, float> _pressDurations = new();
    private readonly Dictionary<string, bool> longPressTriggered = new();

    public enum UpdateType { Update, FixedUpdate }
    public enum PressType { JustPressed, Pressed, JustReleased, SinglePress, DoublePress, LongPress }
    public enum InputAction { Left, Right, Up, Down, Confirm, Cancel }
    public enum InputDeviceType { Mouse, Keyboard, Joypad }
    public InputDeviceType InputDevice
    {
        get => _inputDevice;
        set => SetInputDevice(value);
    }

    public event Action<InputDeviceType> OnInputDeviceChanged;
    public event Action<InputAction, PressType> OnInputActionEvent;
    public event Action<MouseButtonMask, Vector2> OnMouseButtonEvent;
    public event Action<Vector2> OnMouseMotionEvent;

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseMotion || inputEvent is InputEventMouseButton)
        {
            InputDevice = InputDeviceType.Mouse;
            HandleMouseInput(inputEvent);
        }
        else if (inputEvent is InputEventKey)
        {
            InputDevice = InputDeviceType.Keyboard;
        }
        else if (inputEvent is InputEventJoypadMotion || inputEvent is InputEventJoypadButton)
        {
            InputDevice = InputDeviceType.Joypad;
        }
    }

    public override void _Process(double delta)
    {
        if (_updateOn is UpdateType.Update)
        {
            CheckForInput((float)delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_updateOn is UpdateType.FixedUpdate)
        {
            CheckForInput((float)delta);
        }
    }

    public void CheckForInput(float delta)
    {
        HandleInput(InputAction.Left, delta);
        HandleInput(InputAction.Right, delta);
        HandleInput(InputAction.Up, delta);
        HandleInput(InputAction.Down, delta);
        HandleInput(InputAction.Confirm, delta);
        HandleInput(InputAction.Cancel, delta);
    }

    private void SetInputDevice(InputDeviceType type)
    {
        _inputDevice = type;
        OnInputDeviceChanged?.Invoke(_inputDevice);
    }

    private void HandleMouseInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            OnMouseMotionEvent?.Invoke(mouseMotion.GlobalPosition);
        }
        if (inputEvent is InputEventMouseButton mouseButton)
        {
            OnMouseButtonEvent?.Invoke(mouseButton.ButtonMask, mouseButton.GlobalPosition);
        }
    }

    private void HandleInput(InputAction action, float delta)
    {
        string actionName = action.ToString();

        if (Input.IsActionJustPressed(actionName))
        {
            float currentTime = Time.GetTicksMsec() / 1000.0f;

            OnInputActionEvent?.Invoke(action, PressType.JustPressed);

            if (_lastPressTimes.TryGetValue(actionName, out float value) && currentTime - value <= DOUBLE_PRESS_TIME)
            {
                OnInputActionEvent?.Invoke(action, PressType.DoublePress);
                _lastPressTimes[actionName] = -DOUBLE_PRESS_TIME;
            }
            else
            {
                _lastPressTimes[actionName] = currentTime;
                _pressDurations[actionName] = 0;
                longPressTriggered[actionName] = false;
            }
        }

        if (Input.IsActionPressed(actionName))
        {
            _pressDurations[actionName] += delta;

            OnInputActionEvent?.Invoke(action, PressType.Pressed);

            if (!longPressTriggered[actionName] && _pressDurations[actionName] >= LONG_PRESS_TIME)
            {
                OnInputActionEvent?.Invoke(action, PressType.LongPress);
                longPressTriggered[actionName] = true;
            }
        }

        if (Input.IsActionJustReleased(actionName))
        {
            float currentTime = Time.GetTicksMsec() / 1000.0f;

            OnInputActionEvent?.Invoke(action, PressType.JustReleased);

            if (_pressDurations[actionName] < LONG_PRESS_TIME && currentTime - _lastPressTimes[actionName] <= DOUBLE_PRESS_TIME)
            {
                OnInputActionEvent?.Invoke(action, PressType.SinglePress);
            }

            _pressDurations[actionName] = 0;
            longPressTriggered[actionName] = false;
        }
    }
}