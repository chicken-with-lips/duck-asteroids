using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Renderer;
using Game.Components;

namespace Game.Systems;

public partial class GameOverSystem : BaseSystem<World, float>
{
    private readonly World _world;
    private readonly RendererModule _rendererModule;

    public GameOverSystem(World world, RendererModule rendererModule)
        : base(world)
    {
        _world = world;
        _rendererModule = rendererModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run()
    {
        if (_world.CountEntities(new QueryDescription().WithAll<PlanetTag>()) > 0) {
            return;
        }

        var scene = _rendererModule.GetLoadedScene(GameConstants.LevelRound);

        if (null != scene) {
            _rendererModule.UnloadScene(scene);
        }

        scene = _rendererModule.GetLoadedScene(GameConstants.LevelMainMenu);
        
        if (scene != null) {
            scene.IsActive = true;
        }
    }
}
