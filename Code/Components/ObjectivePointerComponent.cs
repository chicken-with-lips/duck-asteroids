using Arch.Core;
using Duck.Serialization;

namespace Game.Components;

[AutoSerializable]
public partial struct ObjectivePointerComponent
{
    public EntityReference Player;
    public EntityReference CameraController;
}
