using Duck.Serialization;

namespace Game.Components;

[AutoSerializable]
public partial struct HealthComponent
{
    public int Value;
}
