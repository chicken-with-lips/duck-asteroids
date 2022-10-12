using System;
using Duck;
using Duck.Scene;
using Duck.Ui;
using Duck.Ui.RmlUi;
using Duck.Ui.Scripting;

namespace Game.UI;

public class MainMenu : IUserInterfaceLoaded
{
    private readonly IApplication _app;

    public MainMenu(IApplication app)
    {
        _app = app;
    }
    
    ~MainMenu()
    {
        // FIXME: unbind events
        Console.WriteLine("FIXME: unbind events");
    }

    public void OnLoaded(RmlUserInterface ui)
    {
        var play = ui.Document.GetElementById("btn-play");

        if (play != null) {
            ui.AddEventListener(play, "click", @event => {
                _app.GetModule<ISceneModule>().Create(GameConstants.LevelRound);
            });
        }

        var quit = ui.Document.GetElementById("btn-quit");

        if (quit != null) {
            ui.AddEventListener(quit, "click", @event => _app.Shutdown());
        }
    }
}
