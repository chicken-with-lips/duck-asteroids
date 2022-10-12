using System;
using System.Collections.Generic;
using System.IO;
using Duck.Content;
using Duck.Ecs;
using Duck.GameFramework;
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
using Duck.Scene;
using Duck.Scene.Components;
using Duck.ServiceBus;
using Game.Components;
using Game.Components.Tags;
using Game.Observers;
using Game.Systems;
using Silk.NET.Core;
using Silk.NET.Maths;

namespace Game
{
    public class GameClient : GameClientBase
    {
        #region Members

        private readonly List<IObserver> _observers = new();

        #endregion

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
            var scene = Application.GetModule<ISceneModule>().Create(GameConstants.LevelRound);
            scene.IsActive = true;

            var composition = scene.SystemComposition;

            if (Application is ApplicationBase appBase) {
                appBase.PopulateSystemCompositionWithDefaults(scene, composition);
            }

            InitializeInput(GetModule<IInputModule>());

            _observers.Add(new SceneObserver(Application));
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

        #endregion
    }
}
