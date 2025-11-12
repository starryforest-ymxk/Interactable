using UnityEngine;

namespace Interactable
{
    public class IAAxisData : IABaseData
    {
        public Vector2 MoveVector { get; set; }
        public IAMoveDirection MoveDir { get; set; }
        public IAAxisData(IASystem system) : base(system)
        {
            MoveVector = Vector2.zero;
            MoveDir = IAMoveDirection.None;
        }
        
    }
}