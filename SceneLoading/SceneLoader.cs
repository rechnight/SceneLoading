// --------------- Copyright (C) RC --------------- //
// STRONG is what happens when you run out of weak! //

using Godot;
using System;
using System.Collections.Generic;

public partial class SceneLoader : Node
{
    [Export] private string _scenesFolderPath;
    [Export] private PackedScene _defaultTransition;

    private Dictionary<string, string> _sceneDictionary = new();

    private SceneDataSO _sceneToLoad;
    private SceneDataSO _currentlyLoadedScene;
    private string _scenePath;

    private SceneTransition _sceneTransition;
    private Node _sceneParent;
    private Node _sceneToUnload;

    private bool _isLoading;
    private Timer _loadingMonitorTimer;

    public Action OnLoadingStarted;
    public Action OnSceneChanged;
    public Action OnLoadingFinished;

    public Action OnContentFinishedLoading;
    public Action<string> OnContentInvalid;
    public Action<string> OnContentFailedToLoad;

    public override void _Ready()
    {
        LoadScenesIntoDictionary();
    }

    public void ChangeScenes(SceneDataSO sceneToLoad, Node sceneParent = null, Node sceneToUnload = null)
    {
        if (_isLoading || _currentlyLoadedScene == sceneToLoad)
            return;

        _isLoading = true;
        _sceneToLoad = sceneToLoad;
        _scenePath = GetScenePath(sceneToLoad.SceneName);

        sceneParent ??= GetTree().Root;
        _sceneParent = sceneParent;
        _sceneToUnload = sceneToUnload;

        AddSceneTransition();
    }

    public void LoadNewScene()
    {
        _sceneParent.AddChild((ResourceLoader.LoadThreadedGet(_scenePath) as PackedScene).Instantiate());

        if (_sceneToUnload != null && _sceneToUnload != GetTree().Root)
        {
            _sceneToUnload.QueueFree();
        }

        _currentlyLoadedScene = _sceneToLoad;
        OnSceneChanged?.Invoke(); //I'm using to Signal Audio Player to play the music on the SceneDataSO
    }

    private void AddSceneTransition()
    {
        _sceneTransition = _sceneToLoad.TransitionSettings.Prefab == null
            ? _defaultTransition.Instantiate() as SceneTransition
            : _sceneToLoad.TransitionSettings.Create();

        AddChild(_sceneTransition);
        _sceneTransition.StartTransition();
        _sceneTransition.OnTransitionIn += LoadSceneContent;
        _sceneTransition.OnTransitionOut += HandleTransitionOut;
    }

    private void LoadSceneContent()
    {
        OnLoadingStarted?.Invoke();

        ResourceLoader.LoadThreadedRequest(_scenePath);

        _loadingMonitorTimer = new Timer
        {
            WaitTime = 0.1f
        };
        _loadingMonitorTimer.Timeout += MonitorLoadingStatus;

        GetTree().Root.AddChild(_loadingMonitorTimer);
        _loadingMonitorTimer.Start();

        _sceneTransition.OnTransitionIn -= LoadSceneContent;
    }

    private void MonitorLoadingStatus()
    {
        var loadingProgress = new Godot.Collections.Array();
        var loadingStatus = ResourceLoader.LoadThreadedGetStatus(_scenePath, loadingProgress);
        switch (loadingStatus)
        {
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                OnContentInvalid?.Invoke(_scenePath);
                _loadingMonitorTimer.Stop();
                break;
            case ResourceLoader.ThreadLoadStatus.InProgress:
                _sceneTransition.LoadingProgress = (float)loadingProgress[0] * 100;
                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
                OnContentFailedToLoad?.Invoke(_scenePath);
                _loadingMonitorTimer.Stop();
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                _loadingMonitorTimer.Stop();
                _loadingMonitorTimer.QueueFree();
                OnContentFinishedLoading?.Invoke(); //Signal Scene Transition of content loading done
                break;
        }
    }

    private void HandleTransitionOut()
    {
        _isLoading = false;
        OnLoadingFinished?.Invoke();

        _sceneTransition.OnTransitionOut -= HandleTransitionOut;
    }

    private void LoadScenesIntoDictionary()
    {
        var directory = DirAccess.Open(_scenesFolderPath);
        if (directory!= null)
        {
            directory.ListDirBegin();
            while (true)
            {
                var fileName = directory.GetNext();
                if (string.IsNullOrEmpty(fileName))
                    break;

                if (fileName.EndsWith(".tscn"))
                {
                    var sceneName = fileName.GetFile().GetBaseName();
                    var fullPath = $"{_scenesFolderPath}{fileName}";
                    _sceneDictionary[sceneName] = fullPath;
                }
            }
            directory.ListDirEnd();
        }
        else
        {
            GD.PrintErr($"Failed to open directory: {_scenesFolderPath}");
        }
    }

    private string GetScenePath(string sceneName)
    {
        if (_sceneDictionary.TryGetValue(sceneName, out var path))
        {
            return path;
        }
        GD.PrintErr($"Scene '{sceneName}' not found in dictionary.");
        return null;
    }
}