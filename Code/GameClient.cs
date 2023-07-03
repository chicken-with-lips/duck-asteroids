using System;
using Duck.Content;
using Duck.GameFramework;
using Duck.GameFramework.GameClient;
using Duck.GameHost;
using Duck.Input;
using Duck.Physics;
using Duck.Renderer;
using Duck.Renderer.Device;
using Duck.Renderer.Events;
using Duck.Renderer.Materials;
using Duck.Renderer.Mesh;
using Duck.Renderer.Shaders;
using Duck.Renderer.Textures;
using Duck.ServiceBus;
using Duck.Ui;
using Game.Scenes;
using Game.Systems;
using Silk.NET.Maths;

namespace Game
{
    public class GameClient : GameClientBase
    {
        #region Methods

        public override void Tick()
        {
            base.Tick();

            if (Application.GetModule<IInputModule>().IsKeyDown(InputName.Escape)) {
                Application.Shutdown();
            }
        }

        protected override void InitializeClient(IGameClientInitializationContext context)
        {
            ImportContent(GetModule<IContentModule>());
            InitializeInput(GetModule<IInputModule>());

            GetModule<IEventBus>().AddListener<SceneWasCreated>(ev => {
                var scene = ev.Scene;
                var world = scene.World;
                var systemComposition = scene.SystemRoot;

                if (context.Application is ApplicationBase appBase) {
                    appBase.PopulateSystemCompositionWithDefaults(world, systemComposition);
                }

                switch (scene.Name) {
                    case GameConstants.LevelMainMenu:
                        (new CreateMainMenuSceneSystem()).Run(ev.Scene);
                        break;

                    case GameConstants.LevelRound:
                        (new CreateRoundSceneSystem(GetModule<IContentModule>())).Run(ev.Scene);
                        break;
                }

                systemComposition.SimulationGroup
                    .Add(new HudSystem(world, GetModule<IUiModule>(), GetModule<IContentModule>()))
                    .Add(new MainMenuSystem(world, GetModule<IUiModule>(), GetModule<IContentModule>()))
                    .Add(new GameOverSystem(world, GetModule<RendererModule>()))
                    .Add(new CameraControllerSystem(world))
                    .Add(new ObjectivePointerSystem(world))
                    .Add(new PlayerControllerSystem(world, GetModule<IInputModule>(), GetModule<IPhysicsModule>(), GetModule<IRendererModule>()))
                    .Add(new DestroyAfterTimeSystem(world))
                    .Add(new DestroyAfterHealthEmptySystem(world))
                    .Add(new AsteroidSpawnerSystem(world, GetModule<IPhysicsModule>(), context.Application.GetModule<IContentModule>()))
                    .Add(new AsteroidCollisionSystem(world));
            });

            GetModule<IEventBus>().AddListener<SceneWasMadeActive>(ev => {
                foreach (var loadedScene in GetModule<RendererModule>().GetLoadedScenes()) {
                    if (loadedScene.IsActive && loadedScene != ev.Scene) {
                        loadedScene.IsActive = false;
                    } else {
                        GetModule<RendererModule>().GameView.Scene = new WeakReference<IScene>(loadedScene);
                    }
                }
            });

            GetModule<RendererModule>().CreateScene(GameConstants.LevelRound);
        }

        private void ImportContent(IContentModule contentModule)
        {
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/FX_Meshes/SM_SphereGeo.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_01.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_02.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_03.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_04.fbx");
            contentModule.Import<StaticMesh>("POLYGON_ScifiSpace/Meshes/SM_Env_Astroid_05.fbx");

            contentModule.Database.Register(new Font(new AssetImportData(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont"))));

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

        #endregion
    }
}
