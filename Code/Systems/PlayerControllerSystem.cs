using System;
using Duck;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Graphics.Components;
using Duck.Input;
using Duck.Math;
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
    private readonly InputAxis _mouseX;
    private readonly InputAxis _mouseY;
    private readonly InputAction _fire;
    private readonly IWorld _world;

    public PlayerControllerSystem(IWorld world, IInputModule input)
    {
        _world = world;

        _moveForward = input.GetAxis("MoveForward");
        _moveRight = input.GetAxis("StrafeRight");
        _mouseX = input.GetAxis("MouseX");
        _mouseY = input.GetAxis("MouseY");
        _fire = input.GetAction("Fire");

        Filter = Filter<PlayerControllerComponent, TransformComponent, RigidBodyComponent>(world)
            .Build();
    }

    public override void RunEntity(int entityId, ref PlayerControllerComponent playerController, ref TransformComponent transform, ref RigidBodyComponent rigidBody)
    {
        var moveForwardValue = Vector3D.Normalize(transform.Forward) * 100000f * _moveForward.Value * Time.DeltaFrame;
        var moveRightValue = Vector3D.Normalize(transform.Right) * 100000f * _moveRight.Value * Time.DeltaFrame;
        var mouseLocationValue = new Vector2D<int>((int)_mouseX.Value, (int)_mouseY.Value);
        var camera = _world.GetEntity(playerController.CameraEntityId).Get<CameraComponent>();
        var cameraTransform = _world.GetEntity(playerController.CameraEntityId).Get<TransformComponent>();

        
        
        if (moveForwardValue.LengthSquared > 0) {
            rigidBody.AddForce(moveForwardValue, RigidBodyComponent.ForceMode.Force);
        }

        if (moveRightValue.LengthSquared > 0) {
            rigidBody.AddForce(moveRightValue, RigidBodyComponent.ForceMode.Force);
        }

        // Console.WriteLine(new Vector3D<float>(_mouseX.Value, 0, _mouseY.Value) + " -> " + (transform.Position + mouseLocationValue));

        // Vector3D<float> direction = mouseLocationValue - transform.Position;
        // Vector3D<float> forward = new Vector3D<float>(direction.X, direction.Z, 0);

        var aimPosition = transform.Position + (camera.ScreenToWorldPosition(cameraTransform, mouseLocationValue) * 1000f);

        // if (forward.LengthSquared > 0.0f) {
        // Vector3D<float> aimLocation = transform.Position + mouseLocationValue;
        Vector3D<float> direction2 = aimPosition - transform.Position;
        var direction3 = new Vector3D<float>(direction2.X, transform.Position.Y, direction2.Z);
        // Console.WriteLine(direction3);
        // if (direction3.LengthSquared > 0.0f) {
        Quaternion<float> to = MathHelper.LookRotation(-direction3, Vector3D<float>.UnitY);
        transform.Rotation = to;
        // }

        ComputeTorque(to, transform, rigidBody);

        // rigidBody.AddTorque(ComputeTorque(to, transform, rigidBody), RigidBodyComponent.ForceMode.VelocityChange);
        
        // transform.Rotation = MathHelper.LookRotation(forward, Vector3D<float>.UnitY);
        // }


        // transform.Rotation = MathHelper.FromToRotation(
        // transform.Position,
        // transform.Position + new Vector3D<float>(-_mousePosition.X, transform.Position.Y, -_mousePosition.Y)
        // );

        if (_fire.IsActivated) {
            SpawnProjectile(ref playerController, transform);
        }
    }

    public Vector3D<float> ComputeTorque(Quaternion<float> desiredRotation, TransformComponent transform, RigidBodyComponent rigidBody)
    {
        //q will rotate from our current rotation to desired rotation
        var q = desiredRotation * Quaternion<float>.Inverse(transform.Rotation);
        //convert to angle axis representation so we can do math with angular velocity
        MathHelper.ToAngleAxis(q, out var xMag, out var x);

        x = Vector3D.Normalize(x);
        //w is the angular velocity we need to achieve
        var w = x * xMag * MathHelper.Deg2Rad / Time.DeltaFrame;
        w -= rigidBody.AngularVelocity;
        Console.WriteLine(rigidBody.AngularVelocity);
        //to multiply with inertia tensor local then rotationTensor coords
        var wl = InverseTransformDirection(transform, w);

        var wll = wl;
        wll = Vector3D.Transform(wll, rigidBody.InertiaTensorRotation);
        wll *= rigidBody.InertiaTensor;
        var Tl = Vector3D.Transform(wll, Quaternion<float>.Inverse(rigidBody.InertiaTensorRotation));
        var T = TransformDirection(transform, Tl);
        return T;
    }


    public Vector3D<float> InverseTransformDirection(TransformComponent transformComponent, Vector3D<float> direction)
    {
        var localToWorld =
                           Matrix4X4.CreateScale(transformComponent.Scale)
                           * Matrix4X4.CreateFromQuaternion(transformComponent.Rotation)
                           * Matrix4X4.CreateTranslation(transformComponent.Position)
                           ;

        Matrix4X4.Invert(localToWorld, out var inverseLocalToWorld);

        var r = Vector3D.Multiply(direction, new Matrix3X4<float>(inverseLocalToWorld.Column1, inverseLocalToWorld.Column2, inverseLocalToWorld.Column3));

        return new Vector3D<float>(r.X, r.Y, r.Z);
    }

    public Vector3D<float> TransformDirection(TransformComponent transformComponent, Vector3D<float> direction)
    {
        var localToWorld =
            Matrix4X4.CreateScale(transformComponent.Scale)
            * Matrix4X4.CreateFromQuaternion(transformComponent.Rotation)
            * Matrix4X4.CreateTranslation(transformComponent.Position);

        var r = Vector3D.Multiply(direction, new Matrix3X4<float>(localToWorld.Column1, localToWorld.Column2, localToWorld.Column3));

        return new Vector3D<float>(r.X, r.Y, r.Z);
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
