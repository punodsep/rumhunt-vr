using UnityEngine;
using static OVRPlugin;

public class Attack : MonoBehaviour
{
    [Header("VFX Prefabs")]
    public GameObject attack1VFX;
    public GameObject attack2VFX;

    [Header("Spawn Point")]
    public GameObject vfxSpawnPoint;

    // ท่าที่ 1: ปาดลงเฉียงซ้าย 
    public void PlayAttack1VFX(float angleZ)
    {
        if (attack1VFX != null && vfxSpawnPoint != null)
        {
            Quaternion customRotation = vfxSpawnPoint.transform.rotation * Quaternion.Euler(0, 0, angleZ);
            GameObject fx = Instantiate(attack1VFX, vfxSpawnPoint.transform.position, customRotation);

            Destroy(fx, 2f); // อมตะ 2 วินาที
        }
    }

    // ท่าที่ 2: ปาดขึ้นเฉียงขวา 
    public void PlayAttack2VFX(float angleZ)
    {
        if (attack2VFX != null && vfxSpawnPoint != null)
        {
            // 1. คำนวณมุมหมุน
            Quaternion customRotation = vfxSpawnPoint.transform.rotation * Quaternion.Euler(0, 0, angleZ);

            // 2. คำนวณตำแหน่งเกิดใหม่ (ขยับไปทางซ้าย และลงต่ำลง)
            Vector3 spawnPosition = vfxSpawnPoint.transform.position;

            // ปรับระยะการขยับตรงนี้ (หน่วยเป็นเมตรใน Unity)
            float shiftLeft = 0.1f; 
            float shiftDown = 0.2f; 

            spawnPosition += vfxSpawnPoint.transform.right * shiftLeft;
            spawnPosition -= vfxSpawnPoint.transform.up * shiftDown;

            // 3. สั่ง Spawn ออกมา ณ ตำแหน่งที่คำนวณใหม่
            GameObject fx = Instantiate(attack2VFX, spawnPosition, customRotation);

            // 4. ปรับขนาดให้เล็กตัวลง และพลิกด้านแกน X เพื่อให้ปาดขึ้นเฉียงขวา
            float sizeMultiplier = 0.5f;
            Vector3 currentScale = fx.transform.localScale;
            fx.transform.localScale = new Vector3(
                -currentScale.x * sizeMultiplier,
                 currentScale.y * sizeMultiplier,
                 currentScale.z * sizeMultiplier
            );

            // 5. เวลาทำลาย (Destroy) ให้สั้นลง
            Destroy(fx, 0.8f);
        }
    }
}
