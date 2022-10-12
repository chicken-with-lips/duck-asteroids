using System;
using Duck;
using Duck.Content;
using Duck.Scene;
using Duck.Ui;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using StbImageSharp;

namespace Game.Scenes;

public class MainMenu : IGameScene
{
    private readonly IApplication _app;

    public MainMenu(IApplication app)
    {
        _app = app;
    }

    public void Load(IScene scene)
    {
        var contentModule = _app.GetModule<IContentModule>();

        var contextAsset = contentModule.Database.Register(new Context(new AssetImportData(new Uri("memory://game/default-ui.context"))));
        var uiAsset = contentModule.Import<UserInterface>("UI/demo.rml");
        var world = scene.World;

        var mainMenu = world.CreateEntity();

        ref var contextComponent = ref mainMenu.Get<ContextComponent>();
        contextComponent.Name = "MainMenu";
        contextComponent.ShouldReceiveInput = true;

        ref var uiComponent = ref mainMenu.Get<UserInterfaceComponent>();
        uiComponent.ContextName = "MainMenu";
        uiComponent.Interface = uiAsset?.MakeUniqueReference();
        uiComponent.Script = new UI.MainMenu(_app);
    }

    public void Unload()
    {
    }
}
