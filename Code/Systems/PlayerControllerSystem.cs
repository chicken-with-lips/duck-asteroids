using Duck;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics.Components;
using Duck.Input;
using Duck.Physics.Components;
using Duck.Scene.Components;
using Game.Components;
using Game.Components.Tags;
using Silk.NET.Maths;

namespace Game.Systems;

public class PlayerControllerSystem : RunSystemBase<PlayerControllerComponent, TransformComponent, RigidBodyComponent>
{
    private readonly InputAxis _moveForward;
    private readonly InputAxis _moveRight;
    private readonly InputAction _fire;
    private readonly IWorld _world;

    public PlayerControllerSystem(IWorld world, IInputModule input)
    {
        _world = world;

        _moveForward = input.GetAxis("MoveForward");
        _moveRight = input.GetAxis("TurnRight");
        _fire = input.GetAction("Fire");

        Filter = Filter<PlayerControllerComponent, TransformComponent, RigidBodyComponent>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref PlayerControllerComponent playerController, ref TransformComponent transform, ref RigidBodyComponent rigidBody)
    {
        var moveForwardValue = Vector3D.Normalize(transform.Forward) * 1000f * _moveForward.Value;
        var moveRightValue = Vector3D.Normalize(transform.Up) * 10f * _moveRight.Value;

        if (moveForwardValue.Length > 0) {
            rigidBody.AddForce(moveForwardValue, RigidBodyComponent.ForceMode.Acceleration);
        }

        if (moveRightValue.Length > 0) {
            rigidBody.AddTorque(moveRightValue, RigidBodyComponent.ForceMode.Acceleration);
        }

        if (_fire.IsActivated) {
            SpawnProjectile(ref playerController, transform);
        }
    }

    private void SpawnProjectile(ref PlayerControllerComponent playerController, TransformComponent parentTransform)
    {
        if (playerController.LastFireTime != 0 && (playerController.LastFireTime + playerController.FireRatePerSecond) > Time.Elapsed) {
            return;
        }

        playerController.LastFireTime = Time.Elapsed;

        var bullet = _world.CreateEntity();
        bullet.Get<ProjectileTag>();

        ref var destroyAfter = ref bullet.Get<DestroyAfterTimeComponent>();
        destroyAfter.Lifetime = 10f;

        ref var rigidBody = ref bullet.Get<RigidBodyComponent>();
        rigidBody.Mass = 100;
        rigidBody.Type = RigidBodyComponent.BodyType.Dynamic;
        rigidBody.AngularDamping = 0f;
        rigidBody.LinearDamping = 0f;
        rigidBody.AxisLock = RigidBodyComponent.Lock.LinearY | RigidBodyComponent.Lock.AngularZ | RigidBodyComponent.Lock.AngularX;
        rigidBody.IsGravityEnabled = false;
        rigidBody.AddForce(Vector3D.Normalize(parentTransform.Forward) * 10000f, RigidBodyComponent.ForceMode.VelocityChange);

        ref var boundingSphere = ref bullet.Get<BoundingSphereComponent>();
        boundingSphere.Radius = 75f;

        ref var meshComponent = ref bullet.Get<MeshComponent>();
        meshComponent.Mesh = playerController.ProjectileAsset;

        ref var bulletTransform = ref bullet.Get<TransformComponent>();
        bulletTransform.Position = parentTransform.Position + (parentTransform.Forward * 300f);
        bulletTransform.Rotation = Quaternion<float>.Identity;
    }
}
