using UnityEngine;
using VFXTools;

public class Attack : MonoBehaviour
{
    [Header("VFX - Slash Arc")]
    public SlashArcEffect attack1SlashFX;  // ท่า 1: ปาดลงเฉียงซ้าย
    public SlashArcEffect attack2SlashFX;  // ท่า 2: ปาดขึ้นเฉียงขวา

    [Header("Spawn Point")]
    public GameObject vfxSpawnPoint;

    // ท่าที่ 1: ปาดลงเฉียงซ้าย (จากขวาบน ไป ซ้ายล่าง)
    public void PlayAttack1VFX()
    {
        if (attack1SlashFX == null) return;

        attack1SlashFX.pivotPoint = vfxSpawnPoint.transform;

        // ปรับค่าเพื่อให้เริ่มจาก ขวาบน (45 องศา) ปาดไปจบที่ ซ้ายล่าง (225 องศา)
        attack1SlashFX.startAngle = 45f;
        attack1SlashFX.arcAngle = 180f;     // หมุนไปอีก 180 องศา
        attack1SlashFX.sweepSpeed = 300f;   // ความเร็วในการปาด

        attack1SlashFX.TriggerSlash();
    }

    // ท่าที่ 2: ปาดขึ้นเฉียงขวา (reverse direction)
    public void PlayAttack2VFX()
    {
        if (attack2SlashFX == null) return;

        attack2SlashFX.pivotPoint = vfxSpawnPoint.transform;

        // ท่าปาดขึ้นขวา: เริ่มจากล่างซ้าย → บนขวา
        attack2SlashFX.startAngle = -135f;  // เริ่มซ้ายล่าง
        attack2SlashFX.arcAngle = 180f;
        attack2SlashFX.sweepSpeed = 350f;   // เร็วขึ้นนิดนึง

        attack2SlashFX.TriggerSlash();
    }
}