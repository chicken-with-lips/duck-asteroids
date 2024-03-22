using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.AI.Components;
using Duck.Content;
using Duck.Graphics;
using Duck.Graphics.Components;
using Duck.Graphics.Mesh;
using Duck.Physics;
using Duck.Physics.Components;
using Duck.Platform;
using Game.Components;
using Silk.NET.Maths;
using CommandBuffer = Arch.CommandBuffer.CommandBuffer;
using MathF = Duck.Math.MathF;

namespace Game.Systems;

public partial class AsteroidSpawnerSystem : BaseSystem<World, float>, IBufferedSystem
{
    public CommandBuffer CommandBuffer { get; set; }

    private const float SpawnMinRadius = 9000f;
    private const float SpawnMaxRadius = 10000f;
    private const float SpawnForceMultiplier = 1200;
    private const float SpawnRateSeconds = 2.5f;
    private const float SpawnRateJitterSeconds = 0.5f;

    private readonly World _world;
    private readonly List<StaticMesh> _asteroidMeshes = new();
    private readonly IPhysicsWorld _physicsWorld;

    private float _nextSpawnTime = 0;

    public AsteroidSpawnerSystem(World world, List<StaticMesh> asteroidMeshes, IPhysicsModule physicsModule, IContentModule contentModule)
        : base(world)
    {
        _world = world;
        _physicsWorld = physicsModule.GetOrCreatePhysicsWorld(world);
        _asteroidMeshes = asteroidMeshes;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run()
    {
        var planets = new List<Entity>();
        _world.GetEntities(new QueryDescription().WithAll<PlanetTag>(), planets);

        if (_world.CountEntities(new QueryDescription().WithAll<PlanetTag>()) == 0) {
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

        ref var transformComponent = ref World.Get<TransformComponent>(asteroid);
        transformComponent.Scale = Vector3D<float>.One;
        
        ref var massComponent = ref World.Get<MassComponent>(asteroid);
        massComponent.ForceMultiplier = 10000f;

        for (var placementIteration = 0; placementIteration < 100; placementIteration++) {
            var radius = minRadius + ((maxRadius - minRadius) * Random.Shared.NextSingle());
            var theta = Random.Shared.NextSingle() * 2f * MathF.PI;
            var position = new Vector3D<float>(radius * MathF.Cos(theta), 0, radius * MathF.Sin(theta));

            var placementWithBuffer = new BoundingSphereComponent() {
                Radius = World.Get<BoundingSphereComponent>(asteroid).Radius * 3
            };

            if (!_physicsWorld.Overlaps(placementWithBuffer, position, transformComponent.Rotation)) {
                transformComponent.Position = position;
                placed = true;
                break;
            }
        }

        if (!placed) {
            _world.Destroy(asteroid);
        } else {
            World.Get<RigidBodyComponent>(asteroid).AddForce(
                Vector3D.Normalize(World.Get<TransformComponent>(planets[0]).Position - World.Get<TransformComponent>(asteroid).Position) * massComponent.ForceMultiplier,
                RigidBodyComponent.ForceMode.VelocityChange
            );
        }
    }

    private Entity CreateAsteroid(World world, in Vector3D<float> position)
    {
        return world.Create(
            new TransformComponent {
                Position = position,
                Rotation = Quaternion<float>.Identity,
                Scale = new Vector3D<float>(2f, 2f, 2f),
            },
            new BoundingSphereComponent {
                Radius = 375f,
            },
            new RigidBodyComponent {
                Type = RigidBodyComponent.BodyType.Dynamic,
                AngularDamping = 1f,
                IsGravityEnabled = false,
                AxisLock = RigidBodyComponent.Lock.LinearY,
                MaxLinearVelocity = 800f,
            },
            new MassComponent {
                Value = 1,
                ForceMultiplier = 10000f,
            },
            new StaticMeshComponent {
                Mesh = _asteroidMeshes[Random.Shared.Next(0, _asteroidMeshes.Count - 1)].MakeSharedReference(),
            },
            new PawnTag(),
            new EnemyTag()
        );
    }
}
