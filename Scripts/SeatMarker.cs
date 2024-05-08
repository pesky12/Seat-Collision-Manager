using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace PeskyBox.SeatCollisionManager
{
    public enum seatType
    {
        Chair,
        Table
    }
    
    public class SeatMarker : MonoBehaviour, IEditorOnly
    {
        public bool includeInSearch = true;
        public seatType furnitureType;
    }
}