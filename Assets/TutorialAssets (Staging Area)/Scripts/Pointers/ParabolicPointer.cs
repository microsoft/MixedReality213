using MRDL.Design;
using UnityEngine;

namespace MRDL.Controllers
{
    [RequireComponent(typeof(Parabola))]
    [ExecuteInEditMode]
    public class ParabolicPointer : PhysicsPointer
    {
        protected void OnEnable() {
            if (parabolaMain == null)
                parabolaMain = gameObject.GetComponent<Parabola>();

            parabolaMainRenderers = parabolaMain.GetComponentsInChildren<Design.LineRenderer>();
        }

        private void Update()
        {
            if (parabolaMain == null)
                return;

            PointerForward = raycastOrigin.forward;
            StartPoint = raycastOrigin.position;
            TargetPoint = raycastOrigin.position + (PointerForward * parabolaDistance) + (Vector3.down * parabolaDropDist);
            parabolaMain.SetFirstPoint(StartPoint);
            parabolaMain.SetLastPoint(TargetPoint);
            
            TotalLength = 0;
            Vector3 lastPoint = parabolaMain.GetUnclampedPoint(0f);
            Vector3 currentPoint = Vector3.zero;
            Vector3 finalTargetPoint = TargetPoint;
            float clearLength = 0f;
            float totalDistance = 0f;
            TargetResult = PointerSurfaceResultEnum.None;

            if (active)
            {
                parabolaMain.enabled = true;

                QueryTriggerInteraction triggers = detectTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
                for (int i = 1; i < lineCastResolution; i++)
                {
                    float normalizedDistance = (1f / lineCastResolution) * i;
                    currentPoint = parabolaMain.GetUnclampedPoint(normalizedDistance);
                    float stepDistance = Vector3.Distance(lastPoint, currentPoint);
                    
                    // If we haven't hit anything yet, keep trying
                    if (TargetResult == PointerSurfaceResultEnum.None)
                    {
                        // To make sure we don't accidentally skip geometry
                        // start the raycast a bit before the current point
                        // and extend a little bit past the step distance
                        if (Physics.Linecast(lastPoint, currentPoint, out targetHit, validLayers, triggers))
                        {
                            TargetResult = PointerSurfaceResultEnum.Valid;
                            finalTargetPoint = targetHit.point;
                            TargetPointNormal = targetHit.normal;
                            clearLength += Vector3.Distance(currentPoint, targetHit.point);
                            NavigationHotSpot hotSpot = null;
                            if (PhysicsPointer.CheckForHotSpot(targetHit.collider, out hotSpot))
                            {
                                TargetResult = PointerSurfaceResultEnum.HotSpot;
                                finalTargetPoint = hotSpot.transform.position;
                                TargetPointNormal = hotSpot.transform.up;
                                // Set target point orientation for hot spots
                                TargetPointOrientation = hotSpot.transform.eulerAngles.y;
                            }
                        }
                        else if (Physics.Linecast(lastPoint, currentPoint, out targetHit, invalidLayers, triggers)) {
                            TargetResult = PointerSurfaceResultEnum.Invalid;
                            finalTargetPoint = targetHit.point;
                            TargetPointNormal = targetHit.normal;
                            clearLength += Vector3.Distance(currentPoint, targetHit.point);
                        } else {
                            clearLength += stepDistance;
                        }
                    }

                    Debug.DrawLine(lastPoint + Vector3.up * 0.1f, currentPoint + Vector3.up * 0.1f, (TargetResult != PointerSurfaceResultEnum.None) ? Color.yellow : Color.cyan);
                    totalDistance += stepDistance;
                    lastPoint = currentPoint;
                }
                
                TargetPoint = finalTargetPoint;
                TotalLength = clearLength;
                parabolaMain.LineEndClamp = parabolaMain.GetNormalizedLengthFromWorldLength(clearLength, lineCastResolution);
                totalLineDistance = parabolaMain.UnclampedWorldLength;

                for (int i = 0; i < parabolaMainRenderers.Length; i++) {
                    parabolaMainRenderers[i].LineColor = GetColor(TargetResult);
                }

            } else {
                parabolaMain.enabled = false;
            }
        }
        
        [SerializeField]
        private Transform raycastOrigin;
        // TODO get keyframes from prefab so this is 'correct' on startup
        [SerializeField]
        private AnimationCurve hotSpotMagnetismCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField]
        private float parabolaDistance = 1f;
        [SerializeField]
        private float parabolaDropDist = 1f;
        [SerializeField]
        [Range(5,100)]
        private int lineCastResolution = 25;

        [SerializeField]
        private Parabola parabolaMain;
        [SerializeField]
        private Parabola parabolaBounce;
        [SerializeField]
        private MRDL.Design.LineRenderer [] parabolaMainRenderers;
        [SerializeField]
        private MRDL.Design.LineRenderer [] parabolaBounceRenderers;
        [SerializeField]
        private float totalLineDistance;
    }
}
