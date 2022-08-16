using System;
using System.Net.Mime;
using Duck;
using Duck.Content;
using Duck.Ecs;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Shaders;
using Duck.Input;
using Duck.Math;
using Duck.Physics;
using Duck.Physics.Components;
using Duck.Scene;
using Duck.Scene.Components;
using Duck.ServiceBus;
using Game.Components;
using Game.Components.Tags;
using Game.Observers;
using Game.Systems;
using Silk.NET.Maths;

namespace Game.Scenes;

public class GameRoundScene : IGameScene
{
    #region Members

    private readonly IContentModule _contentModule;
    private readonly IPhysicsModule _physicsModule;
    private readonly IEventBus _eventBus;
    private readonly IInputModule _inputModule;

    private IAsset<StaticMesh>? _planetMesh;
    private IAsset<StaticMesh>? _bulletMesh;
    private IAsset<StaticMesh>? _asteroidMesh;
    private IAsset<StaticMesh>? _spaceshipMesh;

    #endregion

    public GameRoundScene(IApplication app)
    {
        _contentModule = app.GetModule<IContentModule>();
        _physicsModule = app.GetModule<IPhysicsModule>();
        _inputModule = app.GetModule<IInputModule>();
        _eventBus = app.GetModule<IEventBus>();
    }

    public void Load(IScene scene)
    {
        LoadContent();

        var composition = scene.SystemComposition;
        composition
            .Add(new PawnCollisionObserver(composition.World, _eventBus))
            .Add(new GameOverSystem(composition.World))
            .Add(new CameraControllerSystem(composition.World))
            .Add(new PlayerControllerSystem(composition.World, _inputModule))
            .Add(new DestroyAfterTimeSystem(composition.World))
            .Add(new DestroyAfterHealthEmptySystem(composition.World))
            .Add(new AsteroidSpawnerSystem(composition.World, _physicsModule, _asteroidMesh));

        var physWorld = _physicsModule.GetOrCreatePhysicsWorld(composition.World);
        physWorld.Gravity = Vector3D<float>.Zero;
    }

    private void LoadContent()
    {
        var fragShader = _contentModule.Database.Register(
            new FragmentShader(new AssetImportData(new Uri("file:///Shaders/shader.fs")))
        );
        var vertShader = _contentModule.Database.Register(
            new VertexShader(new AssetImportData(new Uri("file:///Shaders/shader.vs")))
        );
        var shaderProgram = _contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://generated")),
                vertShader.MakeReference(),
                fragShader.MakeReference()
            )
        );

        var vertices = new[] {
            new TexturedVertex(
                new Vector3D<float>(-100f, 0f, -100f),
                new Vector3D<float>(0, 0, 0),
                new Vector2D<float>(0, 0)
            ),
            new TexturedVertex(
                new Vector3D<float>(0, 0f, 200f),
                new Vector3D<float>(0, 0, 0),
                new Vector2D<float>(0, 0)
            ),
            new TexturedVertex(
                new Vector3D<float>(100f, 0, -100f),
                new Vector3D<float>(0, 0, 0),
                new Vector2D<float>(0, 0)
            )
        };

        var indices = new uint[] {
            2, 1, 0
        };

        _spaceshipMesh = _contentModule.Database.Register(
            new StaticMesh(
                new AssetImportData(new Uri("memory://generated")),
                new BufferObject<TexturedVertex>(vertices),
                new BufferObject<uint>(indices),
                shaderProgram.MakeReference()
            )
        );

        _planetMesh = _contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Planet_01.fbx");
        _asteroidMesh = _contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_02.fbx");
        _bulletMesh = _contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/FX_Meshes/SM_SphereGeo.fbx");
    }

    public void Unload()
    {
        throw new System.NotImplementedException();
    }

    private void InitializeRound(IWorld world)
    {
        CreateCamera(world);
        CreatePlayer(world);
        CreatePlanet(world);
    }

    private void CreateCamera(IWorld world)
    {
        var cameraEntity = world.CreateEntity();

        ref var transformComponent = ref cameraEntity.Get<TransformComponent>();
        // transformComponent.Position = new Vector3D<float>(0, 10000, 0);
        transformComponent.Position = new Vector3D<float>(0, 0, 0);
        transformComponent.Rotation = Quaternion<float>.CreateFromYawPitchRoll(
            MathHelper.ToRadians(0),
            MathHelper.ToRadians(90),
            MathHelper.ToRadians(0)
        );

        cameraEntity.Get<CameraComponent>();
        cameraEntity.Get<CameraControllerComponent>();
    }

    private void CreatePlanet(IWorld world)
    {
        var entity = world.CreateEntity();

        entity.Get<PlanetTag>();

        ref var rigidBody = ref entity.Get<RigidBodyComponent>();
        rigidBody.Type = RigidBodyComponent.BodyType.Kinematic;
        rigidBody.Mass = 50000f;

        ref var boundingSphereComponent = ref entity.Get<BoundingSphereComponent>();
        boundingSphereComponent.Radius = 23000f;

        ref var meshComponent = ref entity.Get<MeshComponent>();
        meshComponent.Mesh = _planetMesh.MakeReference();

        ref var transform = ref entity.Get<TransformComponent>();
        transform.Rotation = Quaternion<float>.Identity;
        transform.Scale = new Vector3D<float>(0.05f, 0.05f, 0.05f);

        ref var health = ref entity.Get<HealthComponent>();
        health.Value = 100;
    }

    private void CreatePlayer(IWorld world)
    {
        var playerEntity = world.CreateEntity();
        playerEntity.Get<PawnTag>();
        playerEntity.Get<PlayerTag>();

        ref var controllerComponent = ref playerEntity.Get<PlayerControllerComponent>();
        controllerComponent.ProjectileAsset = _bulletMesh.MakeReference();

        ref var rigidBody = ref playerEntity.Get<RigidBodyComponent>();
        rigidBody.Mass = 100;
        rigidBody.Type = RigidBodyComponent.BodyType.Dynamic;
        rigidBody.AngularDamping = 1f;
        rigidBody.AxisLock = RigidBodyComponent.Lock.LinearY | RigidBodyComponent.Lock.AngularZ | RigidBodyComponent.Lock.AngularX;
        rigidBody.IsGravityEnabled = false;

        ref var boundingBoxComponent = ref playerEntity.Get<BoundingBoxComponent>();
        boundingBoxComponent.Box = new Box3D<float>(-150f, -150f, -175f, 150f, 150f, 175f);

        ref var playerMeshComponent = ref playerEntity.Get<MeshComponent>();
        playerMeshComponent.Mesh = _spaceshipMesh.MakeReference();

        ref var transform = ref playerEntity.Get<TransformComponent>();
        transform.Position = new Vector3D<float>(0, 0, -2500f);
        transform.Rotation = Quaternion<float>.Identity;
    }
}
