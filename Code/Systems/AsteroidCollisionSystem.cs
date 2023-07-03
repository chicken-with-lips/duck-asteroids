using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Physics.Events;
using Game.Components;

namespace Game.Systems;

public partial class AsteroidCollisionSystem : BaseSystem<World, float>
{
    public AsteroidCollisionSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, ref PhysicsCollision collision)
    {
        if (!collision.A.IsAlive() || !collision.B.IsAlive()) {
            return;
        }

        if (collision.A.Entity.Has<EnemyTag>() && collision.B.Entity.Has<PlanetTag>()) {
            World.Destroy(collision.A);

            ref var health = ref collision.B.Entity.Get<HealthComponent>();
            health.Value -= 50;
        } else if (collision.A.Entity.Has<ProjectileTag>()) {
            World.Destroy(collision.A);

            if (collision.B.Entity.Has<EnemyTag>()) {
                World.Destroy(collision.B);
            }
        }
    }
}
