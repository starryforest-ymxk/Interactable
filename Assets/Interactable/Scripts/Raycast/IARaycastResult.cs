using UnityEngine;
using UnityEngine.Serialization;

namespace Interactable
{
    public struct IARaycastResult
    {
        private GameObject m_gameObject; // Game object hit by the raycast

        /// <summary>
        /// The GameObject that was hit by the raycast.
        /// </summary>
        public GameObject gameObject
        {
            get => m_gameObject;
            set => m_gameObject = value;
        }

        /// <summary>
        /// BaseRaycaster that raised the hit.
        /// </summary>
        public IABaseRaycaster raycaster;

        /// <summary>
        /// Distance to the hit.
        /// </summary>
        public float distance;

        /// <summary>
        /// Hit index
        /// </summary>
        public float index;

        /// <summary>
        /// Used by raycasters where elements may have the same unit distance, but have specific ordering.
        /// </summary>
        public int depth;

        /// <summary>
        /// The world position of the where the raycast has hit.
        /// </summary>
        public Vector3 worldPosition;

        /// <summary>
        /// The normal at the hit location of the raycast.
        /// </summary>
        public Vector3 worldNormal;

        /// <summary>
        /// The screen position from which the raycast was generated.
        /// </summary>
        public Vector2 screenPosition;

        /// <summary>
        /// The display index from which the raycast was generated.
        /// </summary>
        public int displayIndex;

        /// <summary>
        /// Is there an associated module and a hit GameObject.
        /// </summary>
        public bool isValid => raycaster != null && gameObject != null;

        /// <summary>
        /// Reset the result.
        /// </summary>
        public void Clear()
        {
            gameObject = null;
            raycaster = null;
            distance = 0;
            index = 0;
            depth = 0;
            worldNormal = Vector3.up;
            worldPosition = Vector3.zero;
            screenPosition = Vector3.zero;
        }

        public override string ToString()
        {
            if (!isValid)
                return "";

            return "Name: " + gameObject + "\n" +
                   "module: " + raycaster + "\n" +
                   "distance: " + distance + "\n" +
                   "index: " + index + "\n" +
                   "depth: " + depth + "\n" +
                   "worldNormal: " + worldNormal + "\n" +
                   "worldPosition: " + worldPosition + "\n" +
                   "screenPosition: " + screenPosition;
        }
    }
}