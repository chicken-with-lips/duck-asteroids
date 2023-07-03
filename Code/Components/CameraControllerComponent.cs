using Arch.Core;
using Duck.Serialization;

namespace Game.Components;

[AutoSerializable]
public partial struct CameraControllerComponent
{
    public EntityReference Player;
    public EntityReference PointOfInterest;
    public EntityReference Target;
}
