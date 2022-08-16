using System;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Game.Components.Tags;

namespace Game.Systems;

public class GameOverSystem : SystemBase
{
    private readonly IWorld _world;
    private readonly IFilter<PlanetTag> _filter;

    public GameOverSystem(IWorld world)
    {
        _world = world;

        _filter = Filter<PlanetTag>(world)
            .Build();
    }

    public override void Run()
    {
        if (_filter.EntityRemovedList.Length == 0) {
            return;
        }
        
        Console.WriteLine("GAME OVER");
    }
}
