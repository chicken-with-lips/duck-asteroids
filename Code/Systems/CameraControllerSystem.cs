using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Animation.Tweening;
using Duck.Math;
using Duck.Renderer.Components;
using Game.Components;
using Silk.NET.Maths;

namespace Game.Systems;

public partial class CameraControllerSystem : BaseSystem<World, float>
{
    public CameraControllerSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(ref TransformComponent transform, ref CameraControllerComponent cameraController)
    {
        if (
            (!cameraController.Target.IsAlive() || !cameraController.Target.Entity.Has<TransformComponent>())
            || (!cameraController.Player.IsAlive() || !cameraController.Player.Entity.Has<TransformComponent>())) {
            return;
        }

        var playerTransform = cameraController.Player.Entity.Get<TransformComponent>();

        if (cameraController.PointOfInterest.IsAlive() && cameraController.PointOfInterest.Entity.Has<TransformComponent>()) {
            // do we need to switch target?
            var poiTransform = cameraController.PointOfInterest.Entity.Get<TransformComponent>();
            var distance = Vector3D.Distance(playerTransform.Position, poiTransform.Position);

            if (distance >= 7500f) {
                cameraController.Target = cameraController.Player;
            } else {
                cameraController.Target = cameraController.PointOfInterest;
            }
        }

        var targetTransform = cameraController.Target.Entity.Get<TransformComponent>();

        var targetPosition = targetTransform.Position;
        targetPosition.Y = transform.Position.Y;

        var targetDistance = Vector3D.Distance(transform.Position, targetPosition);

        if (targetDistance == 0f) {
            return;
        }

        transform.Position = MathF.Lerp(transform.Position, targetPosition, Easing.OutSine(0.05f));
    }
}
