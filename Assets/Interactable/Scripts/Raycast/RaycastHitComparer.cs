using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class RaycastHitComparer : IComparer<RaycastHit>
    {
        public static readonly RaycastHitComparer Instance = new();
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}