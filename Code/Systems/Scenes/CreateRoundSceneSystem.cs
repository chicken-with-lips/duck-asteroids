using System;
using Duck;
using Duck.Content;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Shaders;
using Duck.Math;
using Duck.Physics.Components;
using Duck.Scene;
using Duck.Scene.Components;
using Duck.Scene.Events;
using Duck.Ui.Assets;
using Duck.Ui.Components;
using Game.Components;
using Game.Components.Tags;
using Game.UI;
using Silk.NET.Maths;

namespace Game.Systems.Scenes;

public class CreateRoundSceneSystem : RunSystemBase<SceneWasCreated>
{
    private readonly IContentModule _contentModule;

    private IAsset<StaticMesh>? _planetMesh;
    private IAsset<StaticMesh>? _bulletMesh;
    private IAsset<StaticMesh>? _spaceshipMesh;
    private IAsset<UserInterface>? _hudAsset;

    public CreateRoundSceneSystem(IWorld world, IApplication app, ISceneModule sceneModule, IContentModule contentModule)
    {
        _contentModule = contentModule;

        Filter = Filter<SceneWasCreated>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref SceneWasCreated component)
    {
        var scene = component.Scene;

        if (scene.Name != GameConstants.LevelRound) {
            return;
        }

        LoadContent();
        CreateHud(scene, scene.World);
        InitializeRound(scene.World);

        scene.IsActive = true;
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
                new AssetImportData(new Uri("memory://game.shader")),
                vertShader.MakeSharedReference(),
                fragShader.MakeSharedReference()
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
                new AssetImportData(new Uri("memory://game/spaceship.mesh")),
                new BufferObject<TexturedVertex>(vertices),
                new BufferObject<uint>(indices),
                shaderProgram.MakeSharedReference()
            )
        );

        _planetMesh = _contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Planet_01.fbx");
        _bulletMesh = _contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/FX_Meshes/SM_SphereGeo.fbx");
        _hudAsset = _contentModule.Import<UserInterface>("UI/Hud.rml");
    }

    private void InitializeRound(IWorld world)
    {
        CreatePlayer(world, CreateCamera(world));
        CreatePlanet(world);
    }

    private void CreateHud(IScene scene, IWorld world)
    {
        var mainMenu = world.CreateEntity();

        ref var contextComponent = ref mainMenu.Get<ContextComponent>();
        contextComponent.Name = "Hud";
        contextComponent.ShouldReceiveInput = true;

        ref var uiComponent = ref mainMenu.Get<UserInterfaceComponent>();
        uiComponent.ContextName = "Hud";
        uiComponent.Interface = _hudAsset?.MakeUniqueReference();
        uiComponent.Script = new Hud(scene);
    }

    private IEntity CreateCamera(IWorld world)
    {
        var cameraEntity = world.CreateEntity();
        cameraEntity.Get<CameraControllerComponent>();

        ref var transformComponent = ref cameraEntity.Get<TransformComponent>();
        transformComponent.Position = new Vector3D<float>(0, 10000, 0);
        transformComponent.Rotation = Quaternion<float>.CreateFromYawPitchRoll(
            MathHelper.ToRadians(0),
            MathHelper.ToRadians(90),
            MathHelper.ToRadians(0)
        );

        ref var cameraComponent = ref cameraEntity.Get<CameraComponent>();
        cameraComponent.FieldOfView = 75f;
        cameraComponent.NearClipPlane = 0.1f;
        cameraComponent.FarClipPlane = 20000f;
        cameraComponent.IsActive = true;

        return cameraEntity;
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
        meshComponent.Mesh = _planetMesh?.MakeSharedReference();

        ref var transform = ref entity.Get<TransformComponent>();
        transform.Rotation = Quaternion<float>.Identity;
        transform.Scale = new Vector3D<float>(0.05f, 0.05f, 0.05f);

        ref var health = ref entity.Get<HealthComponent>();
        health.Value = 150;
    }

    private void CreatePlayer(IWorld world, IEntity camera)
    {
        var playerEntity = world.CreateEntity();
        playerEntity.Get<PawnTag>();
        playerEntity.Get<PlayerTag>();

        ref var controllerComponent = ref playerEntity.Get<PlayerControllerComponent>();
        controllerComponent.ProjectileAsset = _bulletMesh?.MakeSharedReference();
        controllerComponent.CameraEntityId = camera.Id;

        ref var rigidBody = ref playerEntity.Get<RigidBodyComponent>();
        rigidBody.Mass = 100;
        rigidBody.Type = RigidBodyComponent.BodyType.Dynamic;
        rigidBody.AngularDamping = 1f;
        rigidBody.AxisLock = RigidBodyComponent.Lock.LinearY | RigidBodyComponent.Lock.AngularZ | RigidBodyComponent.Lock.AngularX;
        rigidBody.IsGravityEnabled = false;

        ref var boundingBoxComponent = ref playerEntity.Get<BoundingBoxComponent>();
        boundingBoxComponent.Box = new Box3D<float>(-150f, -150f, -175f, 150f, 150f, 175f);

        ref var playerMeshComponent = ref playerEntity.Get<MeshComponent>();
        playerMeshComponent.Mesh = _spaceshipMesh?.MakeSharedReference();

        ref var transform = ref playerEntity.Get<TransformComponent>();
        transform.Position = new Vector3D<float>(0, 0, -2500f);
        transform.Rotation = Quaternion<float>.Identity;
    }
}
