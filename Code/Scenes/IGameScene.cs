using Duck.Scene;

namespace Game.Scenes;

public interface IGameScene
{
    public void Load(IScene scene);
    public void Unload();
}
