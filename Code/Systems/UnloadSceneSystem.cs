using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Scene;
using Duck.Scene.Events;

namespace Game.Systems;

public class UnloadSceneSystem : RunSystemBase<SceneWasMadeActive>
{
    private readonly IWorld _world;
    private readonly ISceneModule _sceneModule;

    public UnloadSceneSystem(IWorld world, ISceneModule sceneModule)
    {
        _world = world;
        _sceneModule = sceneModule;

        Filter = Filter<SceneWasMadeActive>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref SceneWasMadeActive component)
    {
        foreach (var loadedScene in _sceneModule.GetLoadedScenes()) {
            if (loadedScene.IsActive && loadedScene != component.Scene) {
                loadedScene.IsActive = false;
            }
        }
    }
}
