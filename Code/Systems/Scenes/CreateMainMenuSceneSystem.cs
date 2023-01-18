using Duck;
using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Scene;
using Duck.Scene.Events;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using Game.UI;

namespace Game.Systems.Scenes;

public class CreateMainMenuSceneSystem : RunSystemBase<SceneWasCreated>
{
    private readonly IWorld _world;
    private readonly IApplication _app;
    private readonly ISceneModule _sceneModule;
    private readonly IContentModule _contentModule;

    public CreateMainMenuSceneSystem(IWorld world, IApplication app, ISceneModule sceneModule, IContentModule contentModule)
    {
        _world = world;
        _app = app;
        _sceneModule = sceneModule;
        _contentModule = contentModule;

        Filter = Filter<SceneWasCreated>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref SceneWasCreated component)
    {
        var scene = component.Scene;

        if (scene.Name != GameConstants.LevelMainMenu) {
            return;
        }

        var uiAsset = _contentModule.Import<UserInterface>("UI/MainMenu.rml");
        var world = _world;

        var mainMenu = world.CreateEntity();

        ref var contextComponent = ref mainMenu.Get<ContextComponent>();
        contextComponent.Name = GameConstants.MainMenuContext;
        contextComponent.ShouldReceiveInput = true;

        ref var uiComponent = ref mainMenu.Get<UserInterfaceComponent>();
        uiComponent.ContextName = GameConstants.MainMenuContext;
        uiComponent.Interface = uiAsset?.MakeUniqueReference();
        uiComponent.Script = new MainMenu(scene, _sceneModule, _app);

        scene.IsActive = true;
    }
}
