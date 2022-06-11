using Duck.Content;
using Duck.Graphics.Mesh;
using Duck.Serialization;

namespace Game.Components;

[AutoSerializable]
public partial struct PlayerControllerComponent
{
    public AssetReference<StaticMesh> ProjectileAsset = default;

    public float FireRatePerSecond = 0.5f;
    public float LastFireTime = 0f;

    public PlayerControllerComponent()
    {
    }
}
