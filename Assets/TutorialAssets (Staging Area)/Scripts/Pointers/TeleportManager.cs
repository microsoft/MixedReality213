using HoloToolkit.Unity;
using System;
using System.Collections;
using UnityEngine;

namespace MRDL.Controllers
{
    public class TeleportManager : Singleton<TeleportManager>
    {
        public enum StateEnum
        {
            None,           // No action
            Disabled,       // Can't take action
            Initiating,     // Choosing a position and rotation
            Ready,          // Has a valid position and rotation
            Teleporting,    // Teleporting to target position
        }

        public enum PointerBehaviorEnum
        {
            Override,   // If another pointer initiates telportation, overrides any in progress
            Locked,     // First pointer to initiate telportation stays
        }

        public Action<PhysicsPointer> OnTeleportInitiate;
        public Action<PhysicsPointer> OnTeleportCancel;
        public Action<PhysicsPointer> OnTeleportBegin;
        public Action<PhysicsPointer> OnTeleportEnd;

        public StateEnum State
        {
            get
            {
                return state;
            }
        }
                
        private void Update()
        {
            // TEMP controller input
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ParabolicPointer pp = GameObject.FindObjectOfType<ParabolicPointer>();
                InitiateTeleport(pp);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                TryToTeleport();
            }
            // TEMP rotation input
            if (Input.GetKeyDown(KeyCode.R))
            {
                orientationOffset -= 5f;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                orientationOffset += 5f;
            }

        }

        private void InitiateTeleport(PhysicsPointer newPointer)
        {
            if (state != StateEnum.None)
            {
                return;
            }

            currentPointer = newPointer;
            StartCoroutine(TeleportOverTime());
        }

        private void TryToTeleport()
        {
            switch (state)
            {
                case StateEnum.Ready:
                    // Proceed with the teleport
                    state = StateEnum.Teleporting;
                    break;

                default:
                    // Cancel the teleport
                    state = StateEnum.None;
                    break;
            }
        }

        private IEnumerator TeleportOverTime()
        {
            state = StateEnum.Initiating;
            currentPointer.Active = true;
            orientationOffset = 0f;

            if (OnTeleportInitiate != null)
                OnTeleportInitiate(currentPointer);

            while (isActiveAndEnabled)
            {
                // Use the pointer to choose a target position
                while (state == StateEnum.Initiating || state == StateEnum.Ready)
                {
                    currentPointer.TargetPointOrientation = orientationOffset + Camera.main.transform.eulerAngles.y;//Veil.Instance.HeadTransform.eulerAngles.y;

                    switch (currentPointer.TargetResult)
                    {
                        case PointerSurfaceResultEnum.HotSpot:
                        case PointerSurfaceResultEnum.Valid:
                            state = StateEnum.Ready;
                            targetPosition = currentPointer.TargetPoint;
                            targetRotation = new Vector3(0f, currentPointer.TargetPointOrientation, 0f);
                            break;

                        default:
                            state = StateEnum.Initiating;
                            break;
                    }
                    yield return null;
                }

                // If the state has been set to teleporting
                // Do the teleport
                if (state == StateEnum.Teleporting) {
                    startPosition = playBounds.position;
                    startRotation = playBounds.eulerAngles;

                    if (OnTeleportBegin != null)
                        OnTeleportBegin(currentPointer);

                    float startTime = Time.unscaledTime;
                    while (Time.unscaledTime < startTime + teleportDuration)
                    {
                        float normalizedTime = (Time.unscaledTime - startTime) / teleportDuration;
                        playBounds.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
                        playBounds.rotation = Quaternion.Lerp(Quaternion.Euler(startRotation), Quaternion.Euler(targetRotation), normalizedTime);
                        // TODO do effects here
                        yield return null;
                    }

                    // Set the play bounds to the final position
                    playBounds.position = targetPosition;
                    playBounds.eulerAngles = targetRotation;

                    if (OnTeleportEnd != null)
                       OnTeleportEnd(currentPointer);
                }
                else
                {
                    if (OnTeleportCancel != null)
                        OnTeleportCancel(currentPointer);
                }

                // Now that we're done with teleporting, reset state
                // (Don't override disabled state)
                if (state != StateEnum.Disabled)
                    state = StateEnum.None;

                currentPointer.Active = false;
                yield return null;

            }

            yield break;
        }

        [SerializeField]
        private Transform playBounds;
        [SerializeField]
        private float teleportDuration = 0.25f;
        [SerializeField]
        private PointerBehaviorEnum pointerBehavior = PointerBehaviorEnum.Override;

        private PhysicsPointer currentPointer;
        private float orientationOffset = 0f;

        private Vector3 startPosition;
        private Vector3 startRotation;

        private Vector3 targetPosition;
        private Vector3 targetRotation;
        private StateEnum state;
    }
}