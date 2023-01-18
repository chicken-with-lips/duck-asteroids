using Duck.Content;
using Duck.Ecs.Events;
using Duck.GameFramework;
using Duck.GameFramework.GameClient;
using Duck.GameHost;
using Duck.Input;
using Duck.Physics;
using Duck.Scene;
using Duck.ServiceBus;
using Game.Systems;
using Game.Systems.Scenes;

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
            InitializeInput(GetModule<IInputModule>());

            context.Application.GetModule<IEventBus>().AddListener<WorldWasCreated>(ev => {
                var world = ev.World;
                var systemComposition = world.SystemComposition;

                if (context.Application is ApplicationBase appBase) {
                    appBase.PopulateSystemCompositionWithDefaults(systemComposition);
                }

                systemComposition
                    .Add(new CreateMainMenuSceneSystem(world, context.Application, context.Application.GetModule<ISceneModule>(), context.Application.GetModule<IContentModule>()))
                    .Add(new CreateRoundSceneSystem(world, context.Application, context.Application.GetModule<ISceneModule>(), context.Application.GetModule<IContentModule>()))
                    .Add(new UnloadSceneSystem(world, context.Application.GetModule<ISceneModule>()))
                    .Add(new GameOverSystem(systemComposition.World, context.Application.GetModule<ISceneModule>()))
                    .Add(new CameraControllerSystem(systemComposition.World))
                    .Add(new PlayerControllerSystem(systemComposition.World, context.Application.GetModule<IInputModule>()))
                    .Add(new DestroyAfterTimeSystem(systemComposition.World))
                    .Add(new DestroyAfterHealthEmptySystem(systemComposition.World))
                    .Add(new AsteroidSpawnerSystem(systemComposition.World, context.Application.GetModule<IPhysicsModule>(), context.Application.GetModule<IContentModule>()))
                    .Add(new AsteroidCollisionSystem(systemComposition.World));
            });

            Application.GetModule<ISceneModule>().Create(GameConstants.LevelRound);
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
