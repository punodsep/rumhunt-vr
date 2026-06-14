using UnityEngine;

public class HitEffects : MonoBehaviour
{
    [Header("Particle Prefabs")]
    public ParticleSystem hitEffectPrefab;
    public Transform effectSpawnPoint;

    public void CharacterEffects_OnHit()
    {
        if (hitEffectPrefab == null || effectSpawnPoint == null) return;

        
        float forwardOffset = 1f; 
        Vector3 spawnPos = effectSpawnPoint.position + transform.forward * forwardOffset;

        ParticleSystem effect = Instantiate(
            hitEffectPrefab,
            spawnPos,
            effectSpawnPoint.rotation
        );

        Destroy(effect.gameObject, 2.08f);
    }
}
