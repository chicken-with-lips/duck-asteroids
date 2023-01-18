using System;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Scene;
using Game.Components.Tags;

namespace Game.Systems;

public class GameOverSystem : SystemBase
{
    private readonly IWorld _world;
    private readonly ISceneModule _sceneModule;
    private readonly IFilter<PlanetTag> _filter;

    public GameOverSystem(IWorld world, ISceneModule sceneModule)
    {
        _world = world;
        _sceneModule = sceneModule;

        _filter = Filter<PlanetTag>(world)
            .Build();
    }

    public override void Run()
    {
        if (_filter.EntityRemovedList.Length == 0) {
            return;
        }

        _sceneModule.Unload(_sceneModule.GetLoadedScene(GameConstants.LevelRound));
        _sceneModule.GetLoadedScene(GameConstants.LevelMainMenu).IsActive = true;
    }
}
