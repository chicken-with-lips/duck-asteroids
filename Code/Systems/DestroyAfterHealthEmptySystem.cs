using Duck.Ecs;
using Duck.Ecs.Systems;
using Game.Components;

namespace Game.Systems;

public class DestroyAfterHealthEmptySystem : RunSystemBase<HealthComponent>
{
    private readonly IWorld _world;

    public DestroyAfterHealthEmptySystem(IWorld world)
    {
        _world = world;

        Filter = Filter<HealthComponent>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref HealthComponent healthComponent)
    {
        if (healthComponent.Value <= 0) {
            _world.DeleteEntity(entityId);
        }
    }
}
