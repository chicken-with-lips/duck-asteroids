using System;
using System.Numerics;
using Duck;
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
    private const float SpawnMinRadius = 9000f;
    private const float SpawnMaxRadius = 10000f;
    private const float SpawnForceMultiplier = 1200;
    private const float SpawnRateSeconds = 2.5f;
    private const float SpawnRateJitterSeconds = 0.5f;

    private readonly IWorld _world;
    private readonly IAsset<StaticMesh>? _asteroidMesh;
    private readonly IPhysicsWorld _physicsWorld;

    private float _nextSpawnTime = 0;

    public AsteroidSpawnerSystem(IWorld world, IPhysicsModule physicsModule, IContentModule contentModule)
    {
        _world = world;
        _physicsWorld = physicsModule.GetOrCreatePhysicsWorld(world);
        _asteroidMesh = contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_02.fbx");
    }

    public override void Run()
    {
        var planets = _world.GetEntitiesByComponent<PlanetTag>();

        if (planets.Length == 0) {
            return;
        }

        if (_nextSpawnTime > Time.Elapsed) {
            return;
        }

        _nextSpawnTime = (Time.Elapsed + SpawnRateSeconds) + (Random.Shared.NextSingle() * (SpawnRateJitterSeconds - -SpawnRateJitterSeconds) + -SpawnRateJitterSeconds);

        var minRadius = SpawnMinRadius;
        var maxRadius = SpawnMaxRadius;

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
        } else {
            asteroid.Get<RigidBodyComponent>().AddForce(
                Vector3D.Normalize(planets[0].Get<TransformComponent>().Position - asteroid.Get<TransformComponent>().Position) * SpawnForceMultiplier,
                RigidBodyComponent.ForceMode.VelocityChange
            );
        }
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
        meshComponent.Mesh = _asteroidMesh?.MakeSharedReference();

        ref var transform = ref entity.Get<TransformComponent>();
        transform.Position = position;
        transform.Rotation = Quaternion<float>.Identity;
        transform.Scale = new Vector3D<float>(2f, 2f, 2f);

        return entity;
    }
}
