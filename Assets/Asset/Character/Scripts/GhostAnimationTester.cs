using System.Collections;
using UnityEngine;

public class GhostAnimationTester : MonoBehaviour
{
    [Header("Target Controller")]
    [Tooltip("ลาก Object ที่ใส่สคริปต์ GhostAnimController มาใส่ที่นี่")]
    public GhostAnimController ghostController;

    private Animator _animator;

    private void Start()
    {
        if (ghostController == null)
        {
            ghostController = GetComponent<GhostAnimController>();
        }

        if (ghostController != null)
        {
            // บังคับเปิด Object ผีให้ทำงานชัวร์ๆ ก่อนดึงค่า Component
            ghostController.gameObject.SetActive(true);

            _animator = ghostController.GetComponent<Animator>();
            StartCoroutine(TestAnimationSequence());
        }
        else
        {
            Debug.LogError("[GhostTester] ไม่พบสคริปต์ GhostAnimController กรุณาเช็คใน Inspector!");
        }
    }

    private IEnumerator TestAnimationSequence()
    {
        Debug.Log("=== เริ่มต้นการทดสอบลำดับ Animation (แบบต่อเนื่องอัตโนมัติ) ===");

        // 1. Opening
        Debug.Log("Step 1: Play Opening");
        ghostController.PlayOpening();
        yield return StartCoroutine(WaitForAnimation("Opening"));

        // 2. Attack (Index 1)
        Debug.Log("Step 2: Attack (Index 1)");
        ForcePlayAttack(1);
        yield return StartCoroutine(WaitForAnimation("Attack_01"));

        // 3. Dance 1
        Debug.Log("Step 3: Dance 1");
        ForcePlayDance(1);
        yield return StartCoroutine(WaitForAnimation("Dance_01"));

        // 4. Attack (Index 2)
        Debug.Log("Step 4: Attack (Index 2)");
        ForcePlayAttack(2);
        yield return StartCoroutine(WaitForAnimation("Attack_02"));

        // 5. Dance 2
        Debug.Log("Step 5: Dance 2");
        ForcePlayDance(2);
        yield return StartCoroutine(WaitForAnimation("Dance_02"));

        // 6. เล่น Hit 1
        Debug.Log("Step 6: Play Hurt 1");
        ghostController.PlayHurt_01();
        yield return StartCoroutine(WaitForAnimation("Hit_01"));

        // 7. Dance 4
        Debug.Log("Step 7: Dance 4");
        ForcePlayDance(4);
        yield return StartCoroutine(WaitForAnimation("Dance_04"));

        // 8. เล่น Hit 2
        Debug.Log("Step 8: Play Hurt 2");
        ghostController.PlayHurt_02();
        yield return StartCoroutine(WaitForAnimation("Hit_02"));

        // 9. Dance 5
        Debug.Log("Step 9: Dance 5");
        ForcePlayDance(5);
        yield return StartCoroutine(WaitForAnimation("Dance_05"));

        // 10. Ending Win
        Debug.Log("Step 10: Play Ending - Player Win (True)");
        ghostController.PlayEnding(playerWin: true);

        Debug.Log("=== สิ้นสุดการทดสอบลำดับ Animation ===");
    }

    private IEnumerator WaitForAnimation(string stateName)
    {
        if (_animator == null) yield break;

        // รอจนกว่า Animator จะเข้าสู่ State ที่ระบุ
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        // รอจนกว่าจะเล่นไปจนจบเฟรมท้ายๆ
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);

        // รอให้ระบบเด้งกลับเข้าหน้า Idle ก่อนสลับท่าถัดไป
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
    }

    private void ForcePlayDance(int index)
    {
        if (_animator != null) _animator.SetInteger("DanceIndex", index);
    }

    private void ForcePlayAttack(int index)
    {
        if (_animator != null) _animator.SetInteger("AttackIndex", index);
    }
}