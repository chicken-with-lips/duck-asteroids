using Duck;
using Duck.Scene.Events;
using Duck.ServiceBus;
using Game.Scenes;

namespace Game.Observers;

public class SceneObserver : IObserver
{
    private readonly IApplication _app;
    private IGameScene? _currentScene;

    public SceneObserver(IApplication app)
    {
        _app = app;
        _app.GetModule<IEventBus>().AddListener<SceneWasLoaded>(OnSceneLoaded);
    }

    private void OnSceneLoaded(SceneWasLoaded ev)
    {
        IGameScene? newScene = null;

        switch (ev.Scene.Name) {
            case GameConstants.LevelMainMenu:
                newScene = new MainMenu(_app);
                break;

            case GameConstants.LevelRound:
                newScene = new GameRoundScene(_app);
                break;
        }

        if (null != newScene) {
            newScene.Load(ev.Scene);

            if (null != _currentScene) {
                _currentScene.Unload();
            }

            _currentScene = newScene;
        }
    }
}
