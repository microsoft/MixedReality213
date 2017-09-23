using UnityEngine;

namespace MRDL.Controllers
{
    public enum PointerSurfaceResultEnum
    {
        None,
        Valid,
        Invalid,
        HotSpot,
    }

    public class PhysicsPointer : MonoBehaviour
    {
        public float TotalLength { get; protected set; }
        public Vector3 StartPoint { get; protected set; }
        public Vector3 TargetPoint { get; protected set; }
        public Vector3 StartPointNormal { get; protected set; }
        public Vector3 TargetPointNormal { get; protected set; }
        public Vector3 PointerForward { get; protected set; }
        public PointerSurfaceResultEnum TargetResult { get; protected set; }
        public virtual float TargetPointOrientation { get; set; }

        public Gradient GetColor (PointerSurfaceResultEnum targetResult)
        {
            switch (targetResult)
            {
                case PointerSurfaceResultEnum.None:
                default:
                    return lineColorNoTarget;

                case PointerSurfaceResultEnum.Valid:
                    return lineColorValid;

                case PointerSurfaceResultEnum.Invalid:
                    return lineColorInvalid;

                case PointerSurfaceResultEnum.HotSpot:
                    return lineColorHotSpot;
            }
        }

        public bool Active {
            get { return active; }
            set { active = value; }
        }

        [SerializeField]
        protected Gradient lineColorValid;
        [SerializeField]
        protected Gradient lineColorInvalid;
        [SerializeField]
        protected Gradient lineColorHotSpot;
        [SerializeField]
        protected Gradient lineColorNoTarget;
        [SerializeField]
        protected LayerMask validLayers = 1; // Default
        [SerializeField]
        protected LayerMask invalidLayers = 1 << 2; // Ignore raycast
        [SerializeField]
        protected bool detectTriggers = false;
        [SerializeField]
        protected bool active;

        protected RaycastHit targetHit;

        public static bool CheckForHotSpot(Collider checkCollider, out NavigationHotSpot hotSpot)
        {
            hotSpot = null;

            if (checkCollider == null)
                return false;

            if (checkCollider.attachedRigidbody != null)
                hotSpot = checkCollider.attachedRigidbody.GetComponent<NavigationHotSpot>();
            else
                hotSpot = checkCollider.GetComponent<NavigationHotSpot>();

            return hotSpot != null;
        }
    }
}
