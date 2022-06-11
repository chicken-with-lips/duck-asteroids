using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics.Components;
using Duck.Math;
using Duck.Scene.Components;
using Game.Components;
using Silk.NET.Maths;

namespace Game.Systems;

public class CameraControllerSystem : RunSystemBase<TransformComponent, CameraControllerComponent>
{
    private readonly IWorld _world;

    public CameraControllerSystem(IWorld world)
    {
        _world = world;

        Filter = Filter<TransformComponent, CameraControllerComponent>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref TransformComponent transformComponent, ref CameraControllerComponent controllerComponent)
    {
        Quaternion<float> destRot = Quaternion<float>.CreateFromAxisAngle(Vector3D<float>.UnitZ, 0);
        // Quaternion<float> relativeRot = Quaternion<float>.Inverse(transformComponent.Rotation) * destRot;
        Quaternion<float> pointDown = Quaternion<float>.CreateFromYawPitchRoll(0, MathHelper.ToRadians(-90f), 0);

        // transformComponent.Rotation = destRot * relativeRot * pointDown;
        // transformComponent.Rotation = destRot * relativeRot * pointDown;
    }
}
