using UnityEngine;

public class HitEffects : MonoBehaviour
{
    [Header("Particle Prefabs")]
    public ParticleSystem hitEffectPrefab;
    public Transform effectSpawnPoint;

    //public float forwardOffset = 0.5f;

    public void CharacterEffects_OnHit()
    {
        if (hitEffectPrefab == null || effectSpawnPoint == null) return;

        
        
        Vector3 spawnPos = effectSpawnPoint.position;

        ParticleSystem effect = Instantiate(
            hitEffectPrefab,
            spawnPos,
            effectSpawnPoint.rotation
        );

        Destroy(effect.gameObject, 2.08f);
    }
}
