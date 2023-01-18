using System;
using Duck;
using Duck.Ecs;
using Duck.Scene;
using Duck.Ui;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;
using Game.Components.Tags;

namespace Game.UI;

public class MainMenu : IUserInterfaceLoaded, IUserInterfaceUnloaded
{
    private readonly IScene _scene;
    private readonly ISceneModule _sceneModule;
    private readonly IApplication _app;

    public MainMenu(IScene scene, ISceneModule sceneModule, IApplication app)
    {
        _scene = scene;
        _sceneModule = sceneModule;
        _app = app;
    }

    public void OnLoaded(RmlUserInterface ui)
    {
        var play = ui.Document.GetElementById("btn-play");

        if (play != null) {
            ui.AddEventListener(play, "click", @event => {
                _scene.IsActive = false;
                _sceneModule.GetOrCreateScene(GameConstants.LevelRound);
            });
        }

        var quit = ui.Document.GetElementById("btn-quit");

        if (quit != null) {
            ui.AddEventListener(quit, "click", @event => _app.Shutdown());
        }
    }

    public void OnUnloaded(RmlUserInterface ui)
    {
        Console.WriteLine("UNLOADED");
    }
}
