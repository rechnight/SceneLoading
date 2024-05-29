// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;

[GlobalClass]
public partial class SceneDataSO : Resource
{
    [Export] public string SceneName;
    [Export] public AudioStream MusicTrack;
    [Export] public TransitionSettingsSO TransitionSettings;
}