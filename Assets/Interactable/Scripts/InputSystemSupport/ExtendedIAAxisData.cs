#if ENABLE_INPUT_SYSTEM

namespace Interactable.InputSystemSupport
{
    internal class ExtendedIAAxisData : IAAxisData
    {
        public ExtendedIAAxisData(IASystem system)
            : base(system)
        {
        }

        public override string ToString()
        {
            return $"MoveDir: {MoveDir}\nMoveVector: {MoveVector}";
        }
    }
}
#endif