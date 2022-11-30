using System;
using Duck.Ecs;
using Duck.Physics.Events;
using Duck.ServiceBus;
using Game.Components;
using Game.Components.Tags;
using Silk.NET.Assimp;

namespace Game.Observers;

public class PawnCollisionObserver : ISystem
{
    private readonly IWorld _world;

    public PawnCollisionObserver(IWorld world, IEventBus eventBus)
    {
        _world = world;

        eventBus.AddListener<PhysicsCollision>(OnPawnCollision);
    }

    private void OnPawnCollision(PhysicsCollision collision)
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
