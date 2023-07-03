using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Game.Components;

namespace Game.Systems;

public partial class DestroyAfterHealthEmptySystem : BaseSystem<World, float>
{
    private readonly World _world;

    public DestroyAfterHealthEmptySystem(World world)
        : base(world)
    {
        _world = world;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, in HealthComponent health)
    {
        if (health.Value <= 0) {
            _world.Destroy(entity);
        }
    }
}
