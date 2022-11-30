using System;
using Duck.Scene;
using Duck.Scene.Scripting;

namespace Game.Scenes;

public interface IGameScene : ISceneScript, IDisposable
{
    public IScene Scene { get; }
}
