using System;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck.Audio;
using Duck.Content;
using Duck.Graphics;
using Game.Components;

namespace Game.Systems;

public partial class GameOverSystem : BaseSystem<World, float>
{
    private readonly World _world;
    private readonly IRendererModule _rendererModule;
    private readonly IAudioModule _audioModule;
    private readonly SoundClip _gameOverSound;

    public GameOverSystem(World world, IRendererModule rendererModule, IContentModule contentModule, IAudioModule audioModule)
        : base(world)
    {
        _world = world;
        _rendererModule = rendererModule;
        _audioModule = audioModule;
        _gameOverSound = contentModule.Database.GetAsset<SoundClip>(new Uri("file:///Retro_8Bit_Sounds/8-bit-video-game-fail-version-2-145478.wav"));
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run()
    {
        return;
        if (_world.CountEntities(new QueryDescription().WithAll<PlanetTag>()) > 0) {
            return;
        }

        _audioModule.PlaySound(_gameOverSound.MakeSharedReference());

        var scene = _rendererModule.FindScene(GameConstants.LevelRound);

        if (null != scene) {
            _rendererModule.UnloadScene(scene);
        }

        scene = _rendererModule.FindScene(GameConstants.LevelMainMenu);
        scene.IsActive = true;
    }
}
