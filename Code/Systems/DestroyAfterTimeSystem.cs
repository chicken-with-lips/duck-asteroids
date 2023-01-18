using System;
using Duck;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Game.Components;

namespace Game.Systems;

public class DestroyAfterTimeSystem : RunSystemBase<DestroyAfterTimeComponent>
{
    private readonly IWorld _world;

    public DestroyAfterTimeSystem(IWorld world)
    {
        _world = world;

        Filter = Filter<DestroyAfterTimeComponent>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref DestroyAfterTimeComponent destroyAfter)
    {
        destroyAfter.Lifetime -= Time.DeltaFrame;

        if (destroyAfter.Lifetime <= 0) {
            _world.DeleteEntity(entityId);
        }
    }
}
