using System;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Graphics.Components;
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
            (!World.IsAlive(objectivePointer.CameraController) || !World.Has<TransformComponent>(objectivePointer.CameraController.Entity))
            || (!World.IsAlive(objectivePointer.Player) || !World.Has<TransformComponent>(objectivePointer.Player.Entity))) {
            return;
        }

        var playerTransform = World.Get<TransformComponent>(objectivePointer.Player.Entity);
        var cameraController = World.Get<CameraControllerComponent>(objectivePointer.CameraController.Entity);

        if (!World.IsAlive(cameraController.PointOfInterest)) {
            return;
        }

        var targetTransform = World.Get<TransformComponent>(cameraController.PointOfInterest.Entity);

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
