// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;

[GlobalClass]
public partial class TransitionSettingsSO : Resource
{
    [Export] public PackedScene Prefab;
    [Export] public bool ShowLoadingScreen;
    [Export] public bool WaitForInput;

    public SceneTransition Create()
    {
        var sceneTransition = Prefab.Instantiate() as SceneTransition;
        sceneTransition.Settings = this;
        return sceneTransition;
    }
}