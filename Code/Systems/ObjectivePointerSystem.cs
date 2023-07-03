using System;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Renderer.Components;
using Game.Components;
using Silk.NET.Maths;
using MathF = Duck.Math.MathF;

namespace Game.Systems;

public partial class ObjectivePointerSystem : BaseSystem<World, float>
{
    public ObjectivePointerSystem(World world)
        : base(world)
    {
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(ref TransformComponent transform, in ObjectivePointerComponent objectivePointer)
    {
        if (
            (!objectivePointer.CameraController.IsAlive() || !objectivePointer.CameraController.Entity.Has<TransformComponent>())
            || (!objectivePointer.Player.IsAlive() || !objectivePointer.Player.Entity.Has<TransformComponent>())) {
            return;
        }

        var playerTransform = objectivePointer.Player.Entity.Get<TransformComponent>();
        var cameraController = objectivePointer.CameraController.Entity.Get<CameraControllerComponent>();

        if (!cameraController.PointOfInterest.IsAlive()) {
            return;
        }

        var targetTransform = cameraController.PointOfInterest.Entity.Get<TransformComponent>();

        var distance = Vector3D.Distance(playerTransform.Position, targetTransform.Position);

        if (distance < 7500f) {
            // FIXME: we can't hide entities, move it out the way for now
            transform.Position = new Vector3D<float>(9999999f, 9999999f, 9999999f);

            return;
        }

        transform.Rotation = MathF.LookRotation(playerTransform.Position - targetTransform.Position, playerTransform.Up);
        // transform.Rotation = MathF.FromToRotation(playerTransform.Rotation, targetTransform.Rotation);
        transform.Position = playerTransform.Position + (Vector3D.Normalize(targetTransform.Position - playerTransform.Position) * 2000f);
    }
}
