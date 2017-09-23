using System.Collections.Generic;
using UnityEngine;

namespace MRDL.Controllers
{
    [ExecuteInEditMode]
    public class TeleporPad : MonoBehaviour
    {
        protected void OnEnable() {
            if (Application.isPlaying && (padMaterials == null || padMaterials.Length == 0)) {
                List<Material> padMaterialsList = new List<Material>();
                Renderer[] renderers = targetTransform.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers) {
                    padMaterialsList.Add(r.material);
                }
                padMaterials = padMaterialsList.ToArray();
            }
        }

        protected void Update() {
            if (pointer == null)
                return;

            if (pointer.Active) {
                targetTransform.gameObject.SetActive(true);
                targetTransform.position = pointer.TargetPoint;
                targetTransform.up = pointer.TargetPointNormal;
                targetTransform.Rotate(0f, pointer.TargetPointOrientation, 0f);

                if (Application.isPlaying) {
                    float offset = Mathf.Repeat(colorOffset, 1f);
                    if (animateColorOffset)
                    {
                        offset = Mathf.Repeat(colorOffset + (Time.unscaledTime * animationSpeed), 1f);
                    }
                    for (int i = 0; i < padMaterials.Length; i++) {
                        padMaterials[i].color = pointer.GetColor(pointer.TargetResult).Evaluate(offset);
                    }
                }
            } else {
                targetTransform.gameObject.SetActive(false);
            }
        }

        [SerializeField]
        private PhysicsPointer pointer;
        [SerializeField]
        private Transform targetTransform;
        [SerializeField]
        [Range(0f,1f)]
        private float colorOffset = 0f;
        [SerializeField]
        private bool animateColorOffset = true;
        [SerializeField]
        private float animationSpeed = 0.5f;

        private Material[] padMaterials;
    }
}