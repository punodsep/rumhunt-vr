using UnityEngine;

public class WinEnding : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public SkinnedMeshRenderer[] meshRenderers;
    public float dissolveDuration = 2f;
    public bool dissolveBottomToTop = true;

    private const string DissolveParam = "_DissolveAmount";

    private Material[] _mats;
    private bool _isDissolving = false;
    private float _timer = 0f;

    void Start()
    {
        _mats = new Material[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
            _mats[i] = meshRenderers[i].material;
    }

    public void StartDissolve()
    {
        _isDissolving = true;
        _timer = 0f;
    }

    void Update()
    {
        if (!_isDissolving) return;

        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / dissolveDuration);
        float value = dissolveBottomToTop ? t : 1f - t;

        foreach (var mat in _mats)
            mat.SetFloat(DissolveParam, value);

        if (t >= 1f)
        {
            _isDissolving = false;
            gameObject.SetActive(false);
        }
    }
}
