using System;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Duck.Audio;
using Duck.Audio.Components;
using Duck.Content;
using Duck.Physics.Events;
using Game.Components;

namespace Game.Systems;

public partial class AsteroidCollisionSystem : BaseSystem<World, float>
{
    private readonly SoundClip _lifeLostSound;
    private readonly SoundClip _explosionSound;

    public AsteroidCollisionSystem(World world, IContentModule contentModule)
        : base(world)
    {
        _lifeLostSound = contentModule.Database.GetAsset<SoundClip>(new Uri("file:///Retro_8Bit_Sounds/Alarms_Sirens/retro_alarm_siren_loop_15.wav"));
        _explosionSound = contentModule.Database.GetAsset<SoundClip>(new Uri("file:///Retro_8Bit_Sounds/Explosion/retro_explosion_deep_03.wav"));
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in Entity entity, ref PhysicsCollision collision)
    {
        if (!World.IsAlive(collision.A) || !World.IsAlive(collision.B)) {
            return;
        }

        if (World.Has<EnemyTag>(collision.A.Entity) && World.Has<PlanetTag>(collision.B.Entity)) {
            World.Destroy(collision.A);

            // ref var health = ref collision.B.Entity.Get<HealthComponent>();
            // health.Value -= 50;

            World.Create(
                new SoundComponent {
                    Sound = _lifeLostSound.MakeSharedReference()
                }
            );
        } else if (World.Has<ProjectileTag>(collision.A.Entity)) {
            World.Destroy(collision.A);

            if (World.Has<EnemyTag>(collision.B.Entity)) {
                World.Destroy(collision.B);

                World.Create(
                    new SoundComponent {
                        Sound = _explosionSound.MakeSharedReference()
                    }
                );
            }
        }
    }
}
