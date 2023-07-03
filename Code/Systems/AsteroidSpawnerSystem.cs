using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck;
using Duck.Content;
using Duck.Physics;
using Duck.Physics.Components;
using Duck.Renderer;
using Duck.Renderer.Components;
using Duck.Renderer.Materials;
using Duck.Renderer.Mesh;
using Duck.Renderer.Shaders;
using Duck.Renderer.Textures;
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

    public AsteroidSpawnerSystem(World world, IPhysicsModule physicsModule, IContentModule contentModule)
        : base(world)
    {
        _world = world;
        _physicsWorld = physicsModule.GetOrCreatePhysicsWorld(world);

        var asteroidShader = contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://game/asteroid.shader")),
                contentModule.Database.GetAsset<VertexShader>(new Uri("file:///Builtin/Shaders/lit.vert")).MakeSharedReference(),
                contentModule.Database.GetAsset<FragmentShader>(new Uri("file:///Builtin/Shaders/lit.frag")).MakeSharedReference()
            )
        );

        var textures = new[] { "A", "B", "C", "E", "F" };

        for (var i = 1; i <= 5; i++) {
            var mesh = contentModule.Database.GetAsset<StaticMesh>(new Uri($"file:///POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_0{i}.fbx"));

            var material = contentModule.Database.Register(
                new Material(
                    new AssetImportData(new Uri($"memory://game/astroid{i}.mat"))
                )
            );

            var texture = textures[Random.Shared.Next(0, textures.Length - 1)];

            material.Shader = asteroidShader.MakeSharedReference();
            material.DiffuseTexture = contentModule.Database.GetAsset<Texture2D>(new Uri($"file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_{texture}.png")).MakeSharedReference();
            material.Specular = new Vector3D<float>(0.5f, 0.5f, 0.5f);
            material.Shininess = 8.0f;

            mesh.Material = material.MakeSharedReference();

            _asteroidMeshes.Add(mesh);
        }
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run()
    {
        return;
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

        ref var transformComponent = ref asteroid.Get<TransformComponent>();
        transformComponent.Scale = Vector3D<float>.One;

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
            _world.Destroy(asteroid);
        } else {
            asteroid.Get<RigidBodyComponent>().AddForce(
                Vector3D.Normalize(planets[0].Get<TransformComponent>().Position - asteroid.Get<TransformComponent>().Position) * SpawnForceMultiplier,
                RigidBodyComponent.ForceMode.VelocityChange
            );
        }
    }

    private Entity CreateAsteroid(World world, in Vector3D<float> position)
    {
        var entity = world.Create<TransformComponent, BoundingSphereComponent, RigidBodyComponent, StaticMeshComponent>();
        entity.Add<PawnTag>();
        entity.Add<EnemyTag>();

        ref var rigidBody = ref entity.Get<RigidBodyComponent>();
        rigidBody.Type = RigidBodyComponent.BodyType.Dynamic;
        rigidBody.Mass = 2000;
        rigidBody.AngularDamping = 1f;
        rigidBody.IsGravityEnabled = false;
        rigidBody.AxisLock = RigidBodyComponent.Lock.LinearY;

        ref var boundingSphereComponent = ref entity.Get<BoundingSphereComponent>();
        boundingSphereComponent.Radius = 375f;

        ref var meshComponent = ref entity.Get<StaticMeshComponent>();
        meshComponent.Mesh = _asteroidMeshes[Random.Shared.Next(0, _asteroidMeshes.Count - 1)].MakeSharedReference();

        ref var transform = ref entity.Get<TransformComponent>();
        transform.Position = position;
        transform.Rotation = Quaternion<float>.Identity;
        transform.Scale = new Vector3D<float>(2f, 2f, 2f);

        return entity;
    }
}
