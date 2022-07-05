using System;
using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics.Components;
using Duck.Graphics.Mesh;
using Duck.Physics;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Game.Components.Tags;
using Silk.NET.Maths;

namespace Game.Systems;

public class AsteroidSpawnerSystem : SystemBase
{
    private readonly IWorld _world;
    private readonly IAsset<StaticMesh> _asteroidMesh;
    private readonly IPhysicsWorld _physicsWorld;

    private int _asteroidsLeftToSpawn = 10;
    

    public AsteroidSpawnerSystem(IWorld world, IPhysicsModule physicsModule, IAsset<StaticMesh> asteroidMesh)
    {
        _world = world;
        _asteroidMesh = asteroidMesh;
        _physicsWorld = physicsModule.GetOrCreatePhysicsWorld(world);
    }

    public override void Run()
    {
        if (_asteroidsLeftToSpawn == 0) {
            return;
        }

        var minRadius = 9000f;
        var maxRadius = 10000f;

        var asteroid = CreateAsteroid(_world, Vector3D<float>.Zero);
        var placed = false;

        ref var transformComponent = ref asteroid.Get<TransformComponent>();

        for (var placementIteration = 0; placementIteration < 100; placementIteration++) {
            var radius = minRadius + ((maxRadius - minRadius) * Random.Shared.NextSingle());
            var theta = Random.Shared.NextSingle() * 2f * MathF.PI;
            var position = new Vector3D<float>(radius * MathF.Cos(theta), 0, radius * MathF.Sin(theta));

            var placementWithBuffer = new BoundingSphereComponent() {
                Radius = asteroid.Get<BoundingSphereComponent>().Radius * 3
            };

            if (!_physicsWorld.Overlaps(placementWithBuffer, position, transformComponent.Rotation)) {
                transformComponent.Position = position;
                placed = true;
                break;
            }
        }

        if (!placed) {
            _world.DeleteEntity(asteroid);
        }

        _asteroidsLeftToSpawn--;
    }

    private IEntity CreateAsteroid(IWorld world, Vector3D<float> position)
    {
        var entity = world.CreateEntity();
        entity.Get<PawnTag>();
        entity.Get<EnemyTag>();

        ref var rigidBody = ref entity.Get<RigidBodyComponent>();
        rigidBody.Type = RigidBodyComponent.BodyType.Dynamic;
        rigidBody.Mass = 2000;
        rigidBody.IsGravityEnabled = false;
        rigidBody.AxisLock = RigidBodyComponent.Lock.LinearY;

        ref var boundingSphereComponent = ref entity.Get<BoundingSphereComponent>();
        boundingSphereComponent.Radius = 375f;

        ref var meshComponent = ref entity.Get<MeshComponent>();
        meshComponent.Mesh = _asteroidMesh.MakeReference();

        ref var transform = ref entity.Get<TransformComponent>();
        transform.Position = position;
        transform.Rotation = Quaternion<float>.Identity;
        transform.Scale = new Vector3D<float>(2f, 2f, 2f);

        return entity;
    }
}
