using System;
using Duck;
using Duck.Content;
using Duck.Scene;
using Duck.Scene.Scripting;
using Duck.Ui;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using StbImageSharp;

namespace Game.Scenes;

public class MainMenu : IGameScene, ISceneMadeActive
{
    public IScene Scene => _scene;

    private readonly IContentModule _contentModule;
    private readonly ISceneModule _sceneModule;
    private readonly IScene _scene;
    private readonly IApplication _app;

    public MainMenu(IScene scene, IApplication app)
    {
        _scene = scene;
        _app = app;
        _contentModule = app.GetModule<IContentModule>();
        _sceneModule = app.GetModule<ISceneModule>();
    }

    public void OnActivated()
    {
        ThrowIfDisposed();

        var contextAsset = _contentModule.Database.Register(new Context(new AssetImportData(new Uri("memory://game/default-ui.context"))));
        var uiAsset = _contentModule.Import<UserInterface>("UI/MainMenu.rml");
        var world = _scene.World;

        var mainMenu = world.CreateEntity();

        ref var contextComponent = ref mainMenu.Get<ContextComponent>();
        contextComponent.Name = "MainMenu";
        contextComponent.ShouldReceiveInput = true;

        ref var uiComponent = ref mainMenu.Get<UserInterfaceComponent>();
        uiComponent.ContextName = "MainMenu";
        uiComponent.Interface = uiAsset?.MakeUniqueReference();
        uiComponent.Script = new UI.MainMenu(_sceneModule, _app);
    }

    #region IDisposable

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        IsDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (IsDisposed) {
            throw new ObjectDisposedException("World");
        }
    }

    #endregion
}
