using Duck.Content;
using Duck.Graphics.Mesh;
using Duck.Serialization;

namespace Game.Components;

[AutoSerializable]
public partial struct PlayerControllerComponent
{
    public IAssetReference<StaticMesh>? ProjectileAsset = default;
    public int CameraEntityId = -1;

    public float FireRatePerSecond = 0.5f;
    public float LastFireTime = 0f;

    public PlayerControllerComponent()
    {
    }
}
