using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace KarmaLabs.DissolveEffect
{
    public class DissolveController : MonoBehaviour
    {
        private Coroutine dissolveCoroutine;
        public VisualEffect VFXGraph;
        private List<Material> allMaterials = new List<Material>();
        public float dissolveRate = 0.0125f;
        public float refreshRate = 0.025f;

        void Start()
        {
            FetchAllMaterials();
        }

        void FetchAllMaterials()
        {
            allMaterials.Clear();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer ren in renderers)
            {
                if (ren is MeshRenderer || ren is SkinnedMeshRenderer)
                {
                    Material[] mats = ren.materials;
                    foreach (Material mat in mats)
                    {
                        if (mat != null && mat.HasProperty("_DissolveAmount"))
                        {
                            allMaterials.Add(mat);
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (dissolveCoroutine == null)
                {
                    dissolveCoroutine = StartCoroutine(DissolveCo());
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetEffect();
            }
        }

        IEnumerator DissolveCo()
        {
            if (allMaterials.Count == 0)
            {
                yield break;
            }

            if (VFXGraph != null)
            {
                VFXGraph.Play();
            }

            float counter = 0;
            while (allMaterials[0].GetFloat("_DissolveAmount") < 2)
            {
                counter += dissolveRate;
                for (int i = 0; i < allMaterials.Count; i++)
                {
                    allMaterials[i].SetFloat("_DissolveAmount", counter);
                }
                yield return new WaitForSeconds(refreshRate);
            }

            dissolveCoroutine = null;
        }

        private void ResetEffect()
        {
            if (dissolveCoroutine != null)
            {
                StopCoroutine(dissolveCoroutine);
                dissolveCoroutine = null;
            }

            if (allMaterials.Count == 0)
            {
                return;
            }

            for (int i = 0; i < allMaterials.Count; i++)
            {
                allMaterials[i].SetFloat("_DissolveAmount", 0);
            }

            if (VFXGraph != null)
            {
                VFXGraph.Stop();
            }

            dissolveCoroutine = StartCoroutine(DissolveCo());
        }
    }
}