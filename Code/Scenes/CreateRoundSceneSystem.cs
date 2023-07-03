using System;
using Arch.Core;
using Arch.Core.Extensions;
using Duck.Content;
using Duck.Physics.Components;
using Duck.Renderer;
using Duck.Renderer.Components;
using Duck.Renderer.Mesh;
using Game.Components;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

namespace Game.Scenes;

public class CreateRoundSceneSystem
{
    private readonly IContentModule _contentModule;

    private StaticMesh? _planetMesh;
    private IAsset<StaticMesh>? _bulletMesh;
    private IAsset<StaticMesh>? _arrowMesh;
    private IAsset<StaticMesh>? _spaceshipMesh;

    public CreateRoundSceneSystem(IContentModule contentModule)
    {
        _contentModule = contentModule;
    }

    public void Run(IScene scene)
    {
        LoadContent();
        CreateHud(scene.World);
        InitializeRound(scene.World);

        scene.IsActive = true;
    }

    private void LoadContent()
    {
        _spaceshipMesh = _contentModule.Database.GetAsset<StaticMesh>(new Uri("memory://game/spaceship.mesh"));
        _arrowMesh = _contentModule.Database.GetAsset<StaticMesh>(new Uri("memory://game/arrow.mesh"));
        _planetMesh = _contentModule.Database.GetAsset<StaticMesh>(new Uri("file:///POLYGON_ScifiSpace/Meshes/SM_Env_Planet_01.fbx"));
        _bulletMesh = _contentModule.Database.GetAsset<StaticMesh>(new Uri("file:///POLYGON_ScifiSpace/Meshes/FX_Meshes/SM_SphereGeo.fbx"));
    }

    private void InitializeRound(World world)
    {
        var planet = CreatePlanet(world);
        CreatePlayer(world, CreateCamera(world, planet));
        CreateDirectionalLight(world);
    }

    private void CreateHud(World world)
    {
        world.Create(
            new HudComponent()
        );
    }

    private Entity CreateCamera(World world, Entity target)
    {
        return world.Create(
            new CameraComponent {
                FieldOfView = 75f,
                NearClipPlane = 0.1f,
                FarClipPlane = 20000f,
                IsActive = true,
            },
            new CameraControllerComponent {
                PointOfInterest = target.Reference(),
                Target = target.Reference(),
            },
            new TransformComponent {
                Scale = Vector3D<float>.One,
                Position = new Vector3D<float>(0, 10000, 0),
                Rotation = Quaternion<float>.CreateFromYawPitchRoll(
                    MathF.ToRadians(0),
                    MathF.ToRadians(90),
                    MathF.ToRadians(0)
                ),
            }
        );
    }

    private Entity CreatePlanet(World world)
    {
        return world.Create(
            new RigidBodyComponent() {
                Type = RigidBodyComponent.BodyType.Kinematic,
                Mass = 50000f,
            },
            new BoundingSphereComponent {
                Radius = 23000f,
            },
            new StaticMeshComponent {
                Mesh = _planetMesh?.MakeSharedReference(),
            },
            new TransformComponent {
                Rotation = Quaternion<float>.Identity,
                Scale = new Vector3D<float>(0.05f, 0.05f, 0.05f),
            },
            new HealthComponent {
                Value = 150,
            },
            new PlanetTag()
        );
    }

    private void CreatePlayer(World world, Entity camera)
    {
        var player = world.Create(
            new TransformComponent {
                Position = new Vector3D<float>(0, 0, -3500f),
                Rotation = Quaternion<float>.Identity,
                Scale = Vector3D<float>.One,
            },
            new PlayerControllerComponent {
                ProjectileAsset = _bulletMesh?.MakeSharedReference(),
                CameraEntity = camera.Reference(),
            },
            new RigidBodyComponent {
                Mass = 100000,
                Type = RigidBodyComponent.BodyType.Dynamic,
                // AngularDamping = 1f,
                AngularDamping = 0.05f,
                AxisLock = RigidBodyComponent.Lock.LinearY | RigidBodyComponent.Lock.AngularZ | RigidBodyComponent.Lock.AngularX,
                IsGravityEnabled = false,
            },
            new BoundingBoxComponent {
                Box = new Box3D<float>(-150f, -150f, -175f, 150f, 150f, 175f),
            },
            new StaticMeshComponent {
                Mesh = _spaceshipMesh?.MakeSharedReference(),
            },
            new PawnTag(),
            new PlayerTag()
        );

        world.Create(
            new TransformComponent {
                Position = new Vector3D<float>(0, 0, -3500f),
                Rotation = Quaternion<float>.Identity,
                Scale = Vector3D<float>.One,
            },
            new BoundingBoxComponent {
                Box = new Box3D<float>(-150f, -150f, -175f, 150f, 150f, 175f),
            },
            new StaticMeshComponent {
                Mesh = _arrowMesh?.MakeSharedReference(),
            },
            new ObjectivePointerComponent {
                Player = player.Reference(),
                CameraController = camera.Reference(),
            }
        );

        ref var cmp = ref camera.Get<CameraControllerComponent>();
        cmp.Player = player.Reference();
    }

    private void CreateDirectionalLight(World world)
    {
        world.Create(
            new TransformComponent {
                Position = new Vector3D<float>(0, 0, -2500f),
                Rotation = Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitY, MathF.ToRadians(45)),
                Scale = Vector3D<float>.One,
            },
            new DirectionalLightComponent {
                Ambient = new Vector3D<float>(0.2f, 0.2f, 0.2f),
                Diffuse = new Vector3D<float>(0.5f, 0.5f, 0.5f),
                Specular = Vector3D<float>.One,
            }
        );
    }
}
