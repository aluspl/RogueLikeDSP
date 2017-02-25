using System;

namespace Enums
{
    [Flags]
    public enum MapElement
    {
        Floor=0,
        Floor1=1, Floor2=2, Floor3=3, Floor4=4,
        Wall=5,
        Door=6,
        LockedDoor=7,


        StartPoint, EndPoint

    }
}