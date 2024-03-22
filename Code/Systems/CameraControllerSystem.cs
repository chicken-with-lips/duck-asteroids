using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Animation.Tweening;
using Duck.Math;
using Duck.Graphics.Components;
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
            (!World.IsAlive(cameraController.Target) || !World.Has<TransformComponent>(cameraController.Target.Entity))
            || (!World.IsAlive(cameraController.Player) || !World.Has<TransformComponent>(cameraController.Player.Entity))) {
            return;
        }

        var playerTransform = World.Get<TransformComponent>(cameraController.Player.Entity);

        if (World.IsAlive(cameraController.PointOfInterest) && World.Has<TransformComponent>(cameraController.PointOfInterest.Entity)) {
            // do we need to switch target?
            var poiTransform = World.Get<TransformComponent>(cameraController.PointOfInterest.Entity);
            var distance = Vector3D.Distance(playerTransform.Position, poiTransform.Position);

            if (distance >= 7500f) {
                cameraController.Target = cameraController.Player;
            } else {
                cameraController.Target = cameraController.PointOfInterest;
            }
        }

        var targetTransform = World.Get<TransformComponent>(cameraController.Target.Entity);

        var targetPosition = targetTransform.Position;
        targetPosition.Y = transform.Position.Y;

        var targetDistance = Vector3D.Distance(transform.Position, targetPosition);

        if (targetDistance == 0f) {
            return;
        }

        transform.Position = MathF.Lerp(transform.Position, targetPosition, Easing.OutSine(0.05f));
    }
}
