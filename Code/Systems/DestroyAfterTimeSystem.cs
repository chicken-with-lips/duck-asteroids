using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck;
using Duck.Platform;
using Game.Components;

namespace Game.Systems;

public partial class DestroyAfterTimeSystem : BaseSystem<World, float>
{
    public DestroyAfterTimeSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, ref DestroyAfterTimeComponent destroyAfter)
    {
        destroyAfter.Lifetime -= Time.DeltaFrame;

        if (destroyAfter.Lifetime <= 0) {
            World.Destroy(entity);
        }
    }
}
