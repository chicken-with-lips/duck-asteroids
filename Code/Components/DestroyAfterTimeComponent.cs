using Duck.Serialization;

namespace Game.Components;

[AutoSerializable]
public partial struct DestroyAfterTimeComponent
{
    public float Lifetime = default;

    public DestroyAfterTimeComponent()
    {
    }
}
