using System;
using System.IO;
using Duck.Content;
using Duck.Ecs;
using Duck.GameFramework.GameClient;
using Duck.GameHost;
using Duck.Graphics.Components;
using Duck.Graphics.Device;
using Duck.Graphics.Mesh;
using Duck.Graphics.Shaders;
using Duck.Input;
using Duck.Math;
using Duck.Physics;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Duck.ServiceBus;
using Game.Components;
using Game.Components.Tags;
using Game.Observers;
using Game.Systems;
using Silk.NET.Maths;

namespace Game
{
    public class GameClient : GameClientBase
    {
        #region Members

        private StaticMesh? _spaceshipMesh;
        private StaticMesh? _planetMesh;
        private StaticMesh? _asteroidMesh;
        private StaticMesh? _bulletMesh;

        #endregion

        #region Methods

        public override void Tick()
        {
            base.Tick();

            if (Application.GetModule<IInputModule>().IsKeyDown(InputName.Escape)) {
                Application.Shutdown();
            }
        }

        protected override void InitializeGame(ISystemComposition composition, IGameClientInitializationContext context)
        {
            InitializeContent(GetModule<IContentModule>());
            InitializeInput(GetModule<IInputModule>());
            InitializeRound(composition.World);

            var eventBus = Application.GetModule<IEventBus>();

            composition
                .Add(new PawnCollisionObserver(composition.World, eventBus))
                .Add(new CameraControllerSystem(composition.World))
                .Add(new PlayerControllerSystem(composition.World, Application.GetModule<IInputModule>()))
                .Add(new DestroyAfterTimeSystem(composition.World));

            var physWorld = Application.GetModule<IPhysicsModule>().GetOrCreatePhysicsWorld(composition.World);
            physWorld.Gravity = Vector3D<float>.Zero;
        }

        private void InitializeContent(IContentModule content)
        {
            content.ContentRootDirectory = "/home/jolly_samurai/Projects/chicken-with-lips/asteroids/Content";

            var fragShader = content.Database.Register(
                new FragmentShader(new AssetImportData(new Uri("file:///Shaders/shader.fs")))
            );
            var vertShader = content.Database.Register(
                new VertexShader(new AssetImportData(new Uri("file:///Shaders/shader.vs")))
            );
            var shaderProgram = content.Database.Register(
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

            _spaceshipMesh = content.Database.Register(
                new StaticMesh(
                    new AssetImportData(new Uri("memory://generated")),
                    new BufferObject<TexturedVertex>(vertices),
                    new BufferObject<uint>(indices),
                    shaderProgram.MakeReference()
                )
            );

            _planetMesh = content.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Planet_01.fbx");
            _asteroidMesh = content.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_02.fbx");
            _bulletMesh = content.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/FX_Meshes/SM_SphereGeo.fbx");
        }

        private void InitializeInput(IInputModule input)
        {
            InputActionBuilder.Create()
                .WithName("Fire")
                .AddBinding(InputName.Space)
                .Build(input);

            InputAxisBuilder.Create()
                .WithName("MoveForward")
                .AddBinding(InputName.W, 1.0f)
                .AddBinding(InputName.S, -1.0f)
                .Build(input);

            InputAxisBuilder.Create()
                .WithName("TurnRight")
                .AddBinding(InputName.A, 1.0f)
                .AddBinding(InputName.D, -1.0f)
                .Build(input);
        }

        private void InitializeRound(IWorld world)
        {
            CreateCamera(world);
            CreatePlayer(world);
            CreatePlanet(world);
            CreateAsteroid(world);
        }

        private void CreateCamera(IWorld world)
        {
            var cameraEntity = world.CreateEntity();

            ref var transformComponent = ref cameraEntity.Get<TransformComponent>();
            transformComponent.Position = new Vector3D<float>(0, 8000, 0);
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

        private void CreateAsteroid(IWorld world)
        {
            var entity = world.CreateEntity();
            entity.Get<PawnTag>();
            entity.Get<EnemyTag>();

            ref var rigidBody = ref entity.Get<RigidBodyComponent>();
            rigidBody.Type = RigidBodyComponent.BodyType.Dynamic;
            rigidBody.Mass = 2000;
            rigidBody.IsGravityEnabled = false;

            ref var boundingSphereComponent = ref entity.Get<BoundingSphereComponent>();
            boundingSphereComponent.Radius = 375f;

            ref var meshComponent = ref entity.Get<MeshComponent>();
            meshComponent.Mesh = _asteroidMesh.MakeReference();

            ref var transform = ref entity.Get<TransformComponent>();
            transform.Position = new Vector3D<float>(2000f, 0, -2500f);
            transform.Rotation = Quaternion<float>.Identity;
            transform.Scale = new Vector3D<float>(2f, 2f, 2f);
        }

        #endregion
    }
}
