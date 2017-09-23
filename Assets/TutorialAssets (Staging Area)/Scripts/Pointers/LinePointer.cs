using MRDL.Design;
using UnityEngine;

namespace MRDL.Controllers
{
    [RequireComponent(typeof(Line))]
    [ExecuteInEditMode]
    public class LinePointer : PhysicsPointer {

        protected void OnEnable()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = gameObject.GetComponentsInChildren<Design.LineRenderer>();
        }

        protected void OnDisable()
        {
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = false;
        }

        protected void Update()
        {
            TargetResult = PointerSurfaceResultEnum.None;

            if (line == null)
                line = gameObject.GetComponent<Line>();

            if (raycastOrigin == null)
                return;

            PointerForward = raycastOrigin.forward;
            // Set the orientation based on our forward
            TargetPointOrientation = Quaternion.LookRotation(PointerForward).eulerAngles.y;
            // TODO use the controller to set additional orientation

            if (active)
            {
                StartPoint = raycastOrigin.position;
                StartPointNormal = raycastOrigin.forward;

                QueryTriggerInteraction queryTriggers = (detectTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
                // Try to detect valid layers
                if (Physics.Raycast(StartPoint, StartPointNormal, out targetHit, maxDistance, validLayers.value, queryTriggers))
                {
                    // Make this a valid hit by default
                    TargetPoint = targetHit.point;
                    TargetPointNormal = targetHit.normal;
                    TargetResult = PointerSurfaceResultEnum.Valid;
                    // Then see if we've hit a hotspot that overrides this target point
                    NavigationHotSpot hotSpot = null;
                    if (PhysicsPointer.CheckForHotSpot (targetHit.collider, out hotSpot))
                    {   
                        TargetPoint = hotSpot.transform.position;
                        TargetPointNormal = hotSpot.transform.up;
                        TargetResult = PointerSurfaceResultEnum.HotSpot;
                    }
                    TotalLength = Vector3.Distance(StartPoint, TargetPoint);

                }
                else if (Physics.Raycast(StartPoint, StartPointNormal, out targetHit, maxDistance, invalidLayers.value, queryTriggers))
                {
                    // Invalid hit
                    TargetPoint = targetHit.point;
                    TargetPointNormal = targetHit.normal;
                    TargetResult = PointerSurfaceResultEnum.Invalid;
                    TotalLength = Vector3.Distance(StartPoint, TargetPoint);
                }
                else
                {
                    // No hit at all
                    TargetResult = PointerSurfaceResultEnum.None;
                    TotalLength = maxDistance;
                    TargetPoint = StartPoint + PointerForward * maxDistance;
                }

                // Set the line & line renderer props
                line.enabled = true;
                line.SetFirstPoint(StartPoint);
                line.SetLastPoint(TargetPoint);

                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].LineColor = GetColor(TargetResult);
                }
            }
            else
            {
                line.enabled = false;
            }
        }

        [SerializeField]
        private Line line;
        [SerializeField]
        protected Design.LineRenderer[] renderers;
        [SerializeField]
        private Transform raycastOrigin;
        [SerializeField]
        private float maxDistance = 100f;
    }
}
