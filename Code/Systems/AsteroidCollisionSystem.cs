using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Physics.Events;
using Game.Components;
using Game.Components.Tags;

namespace Game.Systems;

public class AsteroidCollisionSystem : RunSystemBase<PhysicsCollision>
{
    private readonly IWorld _world;

    public AsteroidCollisionSystem(IWorld world)
    {
        _world = world;

        Filter = Filter<PhysicsCollision>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref PhysicsCollision collision)
    {
        if (collision.A.Has<ProjectileTag>()) {
            _world.DeleteEntity(collision.A);

            if (collision.B.Has<EnemyTag>()) {
                _world.DeleteEntity(collision.B);
            }
        }

        if (collision.A.Has<EnemyTag>() && collision.B.Has<PlanetTag>()) {
            _world.DeleteEntity(collision.A);

            _world.GetComponent<HealthComponent>(collision.B.Id).Value -= 50;
        }
    }
}
