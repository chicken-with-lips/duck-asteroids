using System;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck;
using Duck.Input;
using Duck.Physics;
using Duck.Physics.Components;
using Duck.Renderer;
using Duck.Renderer.Components;
using Game.Components;
using Silk.NET.Maths;
using CommandBuffer = Arch.CommandBuffer.CommandBuffer;
using MathF = Duck.Math.MathF;

namespace Game.Systems;

public partial class PlayerControllerSystem : BaseSystem<World, float>, IBufferedSystem
{
    public CommandBuffer CommandBuffer { get; set; }

    private readonly InputAxis _moveForward;
    private readonly InputAxis _moveRight;
    private readonly InputAxis _mouseX;
    private readonly InputAxis _mouseY;

    private readonly InputAction _fire;
    private readonly World _world;
    private readonly IRendererModule _rendererModule;
    private readonly IPhysicsWorld _physicsWorld;

    public PlayerControllerSystem(World world, IInputModule input, IPhysicsModule physicsModule, IRendererModule rendererModule)
        : base(world)
    {
        _world = world;
        _rendererModule = rendererModule;
        _physicsWorld = physicsModule.GetOrCreatePhysicsWorld(_world);

        _moveForward = input.GetAxis("MoveForward");
        _moveRight = input.GetAxis("StrafeRight");
        _mouseX = input.GetAxis("MouseX");
        _mouseY = input.GetAxis("MouseY");
        _fire = input.GetAction("Fire");
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, ref PlayerControllerComponent playerController, ref TransformComponent transform, ref RigidBodyComponent rigidBody)
    {
        var moveForwardValue = Vector3D.Normalize(transform.Forward) * 5000f * _moveForward.Value * Time.DeltaFrame;
        var moveRightValue = Vector3D.Normalize(transform.Right) * 5000f * _moveRight.Value * Time.DeltaFrame;
        var mouseLocationValue = new Vector3D<float>(_mouseX.Value, _mouseY.Value, 0);

        var cameraEntity = playerController.CameraEntity.Entity;
        var camera = cameraEntity.Get<CameraComponent>();
        var cameraTransform = cameraEntity.Get<TransformComponent>();

        if (moveForwardValue.LengthSquared > 0) {
            rigidBody.AddForce(moveForwardValue, RigidBodyComponent.ForceMode.VelocityChange);
        }

        if (moveRightValue.LengthSquared > 0) {
            rigidBody.AddForce(moveRightValue, RigidBodyComponent.ForceMode.VelocityChange);
        }

        var aimPosition = camera.ScreenToWorldPosition(_rendererModule.GameView, cameraTransform, mouseLocationValue);
        aimPosition.Y = 0;
        aimPosition *= Vector3D.Distance(transform.Position, cameraTransform.Position);

        var torque = ComputeRotation(entity, cameraTransform.Position + aimPosition, transform);

        if (!float.IsNaN(torque.X) && !float.IsNaN(torque.Y) && !float.IsNaN(torque.Z)) {
            rigidBody.AddTorque(torque, RigidBodyComponent.ForceMode.VelocityChange);
        }

        if (_fire.IsActivated) {
            SpawnProjectile(ref playerController, transform);
        }
    }

    private void SpawnProjectile(ref PlayerControllerComponent playerController, in TransformComponent parentTransform)
    {
        if (playerController.LastFireTime > 0 && (playerController.LastFireTime + playerController.FireRatePerSecond) > Time.Elapsed) {
            return;
        }

        playerController.LastFireTime = Time.Elapsed;

        var bullet = _world.Create(
            new TransformComponent {
                Position = parentTransform.Position + (parentTransform.Forward * 500f),
                Rotation = Quaternion<float>.Identity,
                Scale = Vector3D<float>.One,
            },
            new BoundingSphereComponent {
                Radius = 75f,
            },
            new RigidBodyComponent {
                Mass = 100,
                Type = RigidBodyComponent.BodyType.Dynamic,
                AngularDamping = 0f,
                LinearDamping = 0f,
                AxisLock = RigidBodyComponent.Lock.LinearY | RigidBodyComponent.Lock.AngularZ | RigidBodyComponent.Lock.AngularX,
                IsGravityEnabled = false,
            },
            new StaticMeshComponent {
                Mesh = playerController.ProjectileAsset,
            },
            new DestroyAfterTimeComponent {
                Lifetime = 10f,
            },
            new ProjectileTag()
        );

        ref var rigidBody = ref bullet.Get<RigidBodyComponent>();
        rigidBody.AddForce(Vector3D.Normalize(parentTransform.Forward) * 10000f, RigidBodyComponent.ForceMode.VelocityChange);
    }

    private Vector3D<float> ComputeRotation(in Entity entity, Vector3D<float> aimPosition, in TransformComponent playerTransform)
    {
        var rigidBody = _physicsWorld.GetRigidBody(entity);

        if (null == rigidBody) {
            return Vector3D<float>.Zero;
        }

        var aimDirection = playerTransform.Position + new Vector3D<float>(-aimPosition.X, playerTransform.Position.Y, -aimPosition.Z);
        var desiredRotation = MathF.LookRotation(Vector3D.Normalize(aimDirection), Vector3D<float>.UnitY);

        var frequency = 6f;
        var damping = 1f;
        var kp = (6f * frequency) * (6f * frequency) * 0.25f;
        var kd = 4.5f * frequency * damping;

        var q = desiredRotation * Quaternion<float>.Inverse(playerTransform.Rotation);

        // Q can be the-long-rotation-around-the-sphere eg. 350 degrees
        // We want the equivalent short rotation eg. -10 degrees
        // Check if rotation is greater than 190 degrees == q.w is negative
        if (q.W < 0) {
            // Convert the quaternion to equivalent "short way around" quaternion
            q.X = -q.X;
            q.Y = -q.Y;
            q.Z = -q.Z;
            q.W = -q.W;
        }

        MathF.ToAngleAxis(q, out var xMag, out var xTmp);

        // FIXME: convert to world space
        var rotation = rigidBody.CenterMassLocalPose.Quaternion;

        var x = Vector3D.Normalize(xTmp) * MathF.Deg2Rad;

        var angularWS = Vector3D.Transform(rigidBody.AngularVelocity.ToGeneric(), playerTransform.Rotation);
        angularWS = rigidBody.AngularVelocity.ToGeneric();

        var rotInertia2World = Quaternion<float>.Multiply(rotation.ToGeneric(), playerTransform.Rotation);
        var pidv = kp * x * xMag - kd * angularWS;
        pidv = MathF.Multiply(Quaternion<float>.Inverse(rotInertia2World), pidv) * new Vector3D<float>(12f, 12f, 12f);
        pidv = MathF.Multiply(rotInertia2World, pidv);

        pidv *= 0.001f;

        return pidv;
    }
}
