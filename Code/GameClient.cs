using System;
using System.Collections.Generic;
using Arch.Core;
using Duck.AI.Systems;
using Duck.Audio;
using Duck.Audio.Systems;
using Duck.Content;
using Duck.GameFramework;
using Duck.GameFramework.GameClient;
using Duck.GameHost;
using Duck.Graphics;
using Duck.Graphics.Device;
using Duck.Graphics.Events;
using Duck.Graphics.Materials;
using Duck.Graphics.Mesh;
using Duck.Graphics.Shaders;
using Duck.Graphics.Systems;
using Duck.Graphics.Textures;
using Duck.Input;
using Duck.Physics;
using Duck.Physics.Systems;
using Duck.ServiceBus;
using Duck.Ui;
using Game.Scenes;
using Game.Systems;
using Silk.NET.Maths;

namespace Game
{
    public class GameClient : GameClientBase
    {
        private readonly List<StaticMesh> _asteroidMeshes = new();

        #region Methods

        public override void Tick()
        {
            base.Tick();

            if (Application.GetModule<IInputModule>().IsKeyDown(InputName.Escape)) {
                Application.Shutdown();
            }
        }

        private bool yep;

        protected override void InitializeClient(IGameClientInitializationContext context)
        {
            ImportContent(GetModule<IContentModule>());
            InitializeInput(GetModule<IInputModule>());

            GetModule<IEventBus>().AddListener<SceneWasCreated>(ev => {
                var scene = ev.Scene;

                if (context.Application is ApplicationBase appBase) {
                    PopulateSystemCompositionWithDefaults(scene, scene.World, scene.SystemRoot);
                }

                switch (scene.Name) {
                    case GameConstants.LevelMainMenu:
                        (new CreateMainMenuSceneSystem()).Run(ev.Scene);
                        break;

                    case GameConstants.LevelRound:
                        (new CreateRoundSceneSystem(GetModule<IContentModule>())).Run(ev.Scene);
                        break;
                }
            });

            GetModule<IEventBus>().AddListener<SceneEnteredPlayMode>(ev => {
                var scene = ev.Scene;
                var world = scene.World;
                var systemComposition = scene.SystemRoot;

                if (yep) {
                    if (context.Application is ApplicationBase appBase) {
                        PopulateSystemCompositionWithDefaults(scene, scene.World, scene.SystemRoot);
                    }
                }

                yep = true;

                switch (scene.Name) {
                    case GameConstants.LevelMainMenu:
                        systemComposition.SimulationGroup
                            .Add(new MainMenuSystem(world, scene, GetModule<IUIModule>(), GetModule<IContentModule>(), GetModule<IRendererModule>(), Application));
                        break;

                    case GameConstants.LevelRound:
                        systemComposition.SimulationGroup
                            .Add(new HudSystem(world, scene, GetModule<IUIModule>(), GetModule<IContentModule>()))
                            // .Add(new GameOverSystem(world, GetModule<IRendererModule>(), GetModule<IContentModule>(), GetModule<IAudioModule>()))
                            .Add(new CameraControllerSystem(world))
                            .Add(new ObjectivePointerSystem(world))
                            .Add(new PlayerControllerSystem(world, GetModule<IInputModule>(), GetModule<IPhysicsModule>(), GetModule<IRendererModule>(), GetModule<IContentModule>()))
                            .Add(new DestroyAfterTimeSystem(world))
                            .Add(new DestroyAfterHealthEmptySystem(world))
                            // .Add(new AsteroidSpawnerSystem(world, _asteroidMeshes, GetModule<IPhysicsModule>(), context.Application.GetModule<IContentModule>()))
                            .Add(new AsteroidCollisionSystem(world, GetModule<IContentModule>()));
                        break;
                }
            });

            GetModule<IEventBus>().AddListener<SceneWasMadeActive>(ev => {
                foreach (var loadedScene in GetModule<IRendererModule>().Scenes) {
                    if (loadedScene.IsActive && loadedScene != ev.Scene) {
                        loadedScene.IsActive = false;
                    } else {
                        GetModule<IRendererModule>().PrimaryView.Scene = loadedScene;
                    }
                }
            });

            GetModule<IRendererModule>().CreateScene(GameConstants.LevelRound);
        }

        private void ImportContent(IContentModule contentModule)
        {
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/FX_Meshes/SM_SphereGeo.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_01.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_02.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_03.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_04.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_05.fbx");

            contentModule.Database.Register(new SoundClip(new AssetImportData(new Uri("file:///Retro_8Bit_Sounds/Weapons/retro_laser_gun_shoot_15.wav"))));
            contentModule.Database.Register(new SoundClip(new AssetImportData(new Uri("file:///Retro_8Bit_Sounds/Alarms_Sirens/retro_alarm_siren_loop_15.wav"))));
            contentModule.Database.Register(new SoundClip(new AssetImportData(new Uri("file:///Retro_8Bit_Sounds/Explosion/retro_explosion_deep_03.wav"))));
            contentModule.Database.Register(new SoundClip(new AssetImportData(new Uri("file:///Retro_8Bit_Sounds/8-bit-video-game-fail-version-2-145478.wav"))));

            // contentModule.Database.Register(new Font(new AssetImportData(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont"))));

            var fragShader = contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/lit.frag"))));
            var vertShader = contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Builtin/Shaders/lit.vert"))));

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_A.png")),
                    1024, 1024
                )
            );

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_B.png")),
                    1024, 1024
                )
            );

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_C.png")),
                    1024, 1024
                )
            );

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_D.png")),
                    1024, 1024
                )
            );

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_E.png")),
                    1024, 1024
                )
            );

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_F.png")),
                    1024, 1024
                )
            );

            contentModule.Database.Register(
                new Texture2D(
                    new AssetImportData(new Uri("file:///POLYGON_ScifiSpace/Textures/PolygonSciFiSpace_Ship_Mask_04.png")),
                    2048, 2048
                )
            );

            var planetShader = contentModule.Database.Register(
                new ShaderProgram(
                    new AssetImportData(new Uri("memory://game/planet.shader")),
                    vertShader.MakeSharedReference(),
                    fragShader.MakeSharedReference()
                )
            );

            var planetMaterial = contentModule.Database.Register(
                new Material(
                    new AssetImportData(new Uri("memory://game/planet.mat"))
                )
            );

            planetMaterial.Shader = planetShader.MakeSharedReference();
            planetMaterial.DiffuseTexture = contentModule.Database.GetAsset<Texture2D>(new Uri("file:///POLYGON_ScifiSpace/Textures/Planet/PolygonSpace_Planet_01_D.png")).MakeSharedReference();
            planetMaterial.Specular = new Vector3D<float>(0.5f, 0.5f, 0.5f);
            planetMaterial.Shininess = 8.0f;

            var enemyShipMaterial = contentModule.Database.Register(
                new Material(
                    new AssetImportData(new Uri("memory://game/enemy-ship.mat"))
                )
            );
            
            enemyShipMaterial.Shader = planetShader.MakeSharedReference();
            enemyShipMaterial.DiffuseTexture = contentModule.Database.GetAsset<Texture2D>(new Uri("file:///POLYGON_ScifiSpace/Textures/PolygonSciFiSpace_Ship_Mask_04.png")).MakeSharedReference();
            enemyShipMaterial.Specular = new Vector3D<float>(0.5f, 0.5f, 0.5f);
            enemyShipMaterial.Shininess = 8.0f;

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
                2, 0, 1
            };

            contentModule.Database.Register(
                new StaticMesh(
                    new AssetImportData(new Uri("memory://game/spaceship.mesh")),
                    new BufferObject<TexturedVertex>(vertices),
                    new BufferObject<uint>(indices),
                    GetModule<IRendererModule>().RenderSystem.FallbackMaterial.MakeSharedReference()
                )
            );

            var arrowVertices = new[] {
                new TexturedVertex(
                    new Vector3D<float>(0f, 0f, 150f),
                    new Vector3D<float>(0, 0, 0),
                    new Vector2D<float>(0, 0)
                ),
                new TexturedVertex(
                    new Vector3D<float>(-350f, 0f, -250f),
                    new Vector3D<float>(0, 0, 0),
                    new Vector2D<float>(0, 0)
                ),
                new TexturedVertex(
                    new Vector3D<float>(350f, 0, -250f),
                    new Vector3D<float>(0, 0, 0),
                    new Vector2D<float>(0, 0)
                )
            };

            var arrowIndices = new uint[] {
                2, 1, 0
            };

            contentModule.Database.Register(
                new StaticMesh(
                    new AssetImportData(new Uri("memory://game/arrow.mesh")),
                    new BufferObject<TexturedVertex>(arrowVertices),
                    new BufferObject<uint>(arrowIndices),
                    GetModule<IRendererModule>().RenderSystem.FallbackMaterial.MakeSharedReference()
                )
            );

            var planetMesh = contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Planet_01.fbx");
            planetMesh.Material = planetMaterial.MakeSharedReference();

            var enemyShipMesh = contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Ship_Fighter_05.fbx");
            enemyShipMesh.Material = enemyShipMaterial.MakeSharedReference();

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

        private void InitializeInput(IInputModule input)
        {
            InputActionBuilder.Create()
                .WithName("Fire")
                .AddBinding(InputName.Space)
                .AddBinding(InputName.MouseButtonLeft)
                .Build(input);

            InputAxisBuilder.Create()
                .WithName("MoveForward")
                .AddBinding(InputName.W, 1.0f)
                .AddBinding(InputName.S, -1.0f)
                .Build(input);

            InputAxisBuilder.Create()
                .WithName("StrafeRight")
                .AddBinding(InputName.A, 1.0f)
                .AddBinding(InputName.D, -1.0f)
                .Build(input);

            InputAxisBuilder.Create()
                .WithName("MouseX")
                .AddBinding(InputName.MouseAbsoluteX, 1.0f)
                .Build(input);

            InputAxisBuilder.Create()
                .WithName("MouseY")
                .AddBinding(InputName.MouseAbsoluteY, 1.0f)
                .Build(input);
        }

        private void PopulateSystemCompositionWithDefaults(IScene scene, World world, SystemRoot composition)
        {
            // TODO: auto-populate systems
            composition.EarlySimulationGroup
                .Add(new PhysXPullChanges(world, (PhysicsWorld)GetModule<IPhysicsModule>().GetOrCreatePhysicsWorld(world)));

            composition.SimulationGroup
                .Add(new RigidBodyLifecycleSystem(world, GetModule<IPhysicsModule>()))
                .Add(new AgentTargetAtEntitySystem(world))
                .Add(new AgentSteeringSystem(world))
                .Add(new JointSystem(world, GetModule<IPhysicsModule>()))
                .Add(new ActiveCameraSystem(scene, world, GetModule<IRendererModule>()))
                .Add(new LoadStaticMeshSystem(world, GetModule<IContentModule>()))
                .Add(new PlaySoundSystem(world, GetModule<IAudioModule>()));

            composition.LateSimulationGroup
                .Add(new PhysXPushChangesSystem(world, (PhysicsWorld)GetModule<IPhysicsModule>().GetOrCreatePhysicsWorld(world)));

            composition.PresentationGroup
                .Add(new RenderSceneSystem(world, GetModule<IRendererModule>().GraphicsDevice));

            composition.ExitFrameGroup
                .Add(new RemoveCollisionEventsSystem(world));
        }

        #endregion
    }
}
