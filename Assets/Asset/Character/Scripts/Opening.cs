using UnityEngine;

public class Opening : MonoBehaviour
{
    [Header("VFX")]
    public GameObject vfxPrefab;
    public Transform vfxSpawnPoint;
    public float vfxDelay = 0.3f;

    private bool _isPlaying = false;
    private bool _vfxSpawned = false;
    private float _timer = 0f;

    public void PlayOpeningVFX()
    {
        _isPlaying = true;
        _timer = 0f;
        _vfxSpawned = false;
    }

    void Update()
    {
        if (!_isPlaying || _vfxSpawned) return;

        _timer += Time.deltaTime;
        if (_timer >= vfxDelay)
        {
            SpawnVFX();
            _vfxSpawned = true;
            _isPlaying = false;
        }
    }

    void SpawnVFX()
    {
        if (vfxPrefab == null) return;
        Vector3 pos = vfxSpawnPoint != null
            ? vfxSpawnPoint.position
            : transform.position;
        GameObject vfx = Instantiate(vfxPrefab, pos, Quaternion.identity);
        ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(vfx, 3f);
    }
}
