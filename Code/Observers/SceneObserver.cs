using System;
using Duck;
using Duck.GameFramework;
using Duck.Scene;
using Duck.Scene.Events;
using Duck.ServiceBus;
using Game.Scenes;

namespace Game.Observers;

public class SceneObserver : IObserver
{
    private readonly IApplication _app;
    private readonly ISceneModule _sceneModule;

    private IGameScene? _currentScene;

    public SceneObserver(IApplication app)
    {
        _app = app;
        _sceneModule = app.GetModule<ISceneModule>();
        
        var eventBus = _app.GetModule<IEventBus>();
        eventBus.AddListener<SceneWasCreated>(OnSceneCreated);
        eventBus.AddListener<SceneWasMadeActive>(OnActiveSceneChanged);
        eventBus.AddListener<SceneWasUnloaded>(OnSceneUnloaded);
    }

    private void OnSceneCreated(SceneWasCreated ev)
    {
        switch (ev.Scene.Name) {
            case GameConstants.LevelMainMenu:
                ev.Scene.Script = new MainMenu(ev.Scene, _app);
                break;

            case GameConstants.LevelRound:
                ev.Scene.Script = new GameRoundScene(ev.Scene, _app);
                break;
        }
        
        if (_app is ApplicationBase appBase) {
            appBase.PopulateSystemCompositionWithDefaults(ev.Scene, ev.Scene.SystemComposition);
        }

        ev.Scene.IsActive = true;
    }

    private void OnActiveSceneChanged(SceneWasMadeActive ev)
    {
        if (null != _currentScene) {
            _sceneModule.Unload(_currentScene.Scene);
        }

        _currentScene = ev.Scene.Script as IGameScene;
    }

    private void OnSceneUnloaded(SceneWasUnloaded ev)
    {
    }
}
