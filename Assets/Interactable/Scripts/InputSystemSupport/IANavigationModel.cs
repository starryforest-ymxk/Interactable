#if ENABLE_INPUT_SYSTEM
using UnityEngine;

namespace Interactable.InputSystemSupport
{
    public struct IANavigationModel
    {
        public Vector2 move;
        public int consecutiveMoveCount;
        public IAMoveDirection lastMoveDirection;
        public float lastMoveTime;
        public IAAxisData iaData;

        public void Reset()
        {
            move = Vector2.zero;
        }
    }
}
#endif
