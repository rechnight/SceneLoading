// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;

public partial class LoadingScreen : Control
{
    [Export] private Label _loadingLabel;
    [Export] private ProgressBar _progressBar;

    public void UpdateLabel(string label)
    {
        _loadingLabel.Text = label;
    }

    public void UpdateProgressBar(float value)
    {
        _progressBar.Value = value;
    }
}
