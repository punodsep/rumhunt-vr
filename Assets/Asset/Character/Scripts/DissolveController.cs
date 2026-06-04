using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
namespace KarmaLabs.DissolveEffect
{

public class DissolveController : MonoBehaviour
{
        [Header("Skinned Meshes ")]
        public SkinnedMeshRenderer[] SkinnedMeshes;  // เปลี่ยนจาก MeshRenderer Mesh

        public VisualEffect VFXGraph;
        public float dissolveRate = 0.0125f;
        public float refreshRate = 0.025f;

        private Coroutine dissolveCoroutine;
        private Material[][] _allMaterials;  // เก็บ material ของทุก mesh

        void Start()
        {
            // เก็บ materials ของทุกชิ้น
            _allMaterials = new Material[SkinnedMeshes.Length][];
            for (int i = 0; i < SkinnedMeshes.Length; i++)
                _allMaterials[i] = SkinnedMeshes[i].materials;
        }

        // เรียกจาก GhostAnimController
        public void StartDissolveFromController()
        {
            if (dissolveCoroutine == null)
                dissolveCoroutine = StartCoroutine(DissolveCo());
        }

        IEnumerator DissolveCo()
        {
            if (VFXGraph != null) VFXGraph.Play();

            float counter = 0;
            while (counter < 1f)
            {
                counter += dissolveRate;
                // set ทุก mesh ทุก material พร้อมกัน
                foreach (var mats in _allMaterials)
                    foreach (var mat in mats)
                        mat.SetFloat("_DissolveAmount", counter);

                yield return new WaitForSeconds(refreshRate);
            }

            dissolveCoroutine = null;
        }

        public void ResetEffect()
        {
            if (dissolveCoroutine != null)
            {
                StopCoroutine(dissolveCoroutine);
                dissolveCoroutine = null;
            }

            foreach (var mats in _allMaterials)
                foreach (var mat in mats)
                    mat.SetFloat("_DissolveAmount", 0);

            if (VFXGraph != null) { VFXGraph.Stop(); VFXGraph.Play(); }
        }
    }
}
