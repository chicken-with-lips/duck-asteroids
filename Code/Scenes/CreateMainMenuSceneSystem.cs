using Duck.Graphics;
using Duck.Graphics.Components;
using Game.Components;

namespace Game.Scenes;

public class CreateMainMenuSceneSystem
{
    public void Run(IScene scene)
    {
        var world = scene.World;

        world.Create(
            new MainMenuComponent()
        );

        world.Create(
            new CameraComponent {
                FieldOfView = 75f,
                NearClipPlane = 0.1f,
                FarClipPlane = 20000f,
                IsActive = true,
            },
            new TransformComponent()
        );

        scene.IsActive = true;
    }
}
