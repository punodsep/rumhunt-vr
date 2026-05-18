using System;
using System.Collections;
using UnityEngine;

public class GestureChallenge : MonoBehaviour
{
    public static GestureChallenge Instance { get; private set; }

    [Header("References")]
    public GestureDetector detector;

    [Header("Gesture Pool")]
    public GestureData[] gesturePool;      // ลาก GestureData ทั้ง 3 ใส่

    [Header("Timing")]
    [Tooltip("ผู้เล่นมีเวลากี่วินาทีต่อท่า")]
    public float timePerGesture = 5f;

    public event Action<GestureData> OnNewChallenge;   // ท่าใหม่ถูกสุ่ม
    public event Action OnSuccess;         // ทำถูก
    public event Action OnFail;            // หมดเวลา / ทำผิด

    public GestureData CurrentTarget { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsActive { get; private set; }

    Coroutine _challengeRoutine;

    void Awake() => Instance = this;

    void OnEnable() => detector.OnGestureDetected += OnGestureDetected;
    void OnDisable() => detector.OnGestureDetected -= OnGestureDetected;

    public void StartChallenge()
    {
        IsActive = true;
        _challengeRoutine = StartCoroutine(ChallengeLoop());
    }

    public void StopChallenge()
    {
        IsActive = false;
        if (_challengeRoutine != null) StopCoroutine(_challengeRoutine);
    }

    IEnumerator ChallengeLoop()
    {
        while (IsActive)
        {
            PickNewGesture();
            TimeRemaining = timePerGesture;

            while (TimeRemaining > 0f && IsActive)
            {
                TimeRemaining -= Time.deltaTime;
                yield return null;
            }

            // ถ้า loop ออกมาโดยยังไม่ Success = หมดเวลา
            if (IsActive)
            {
                OnFail?.Invoke();
                // หยุดสักครู่ก่อนท่าถัดไป
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void PickNewGesture()
    {
        if (gesturePool == null || gesturePool.Length == 0) return;

        GestureData next = CurrentTarget;
        // สุ่มจนได้ท่าที่ไม่ซ้ำ (ถ้ามีหลายท่า)
        int tries = 0;
        while (next == CurrentTarget && tries < 10)
        {
            next = gesturePool[UnityEngine.Random.Range(0, gesturePool.Length)];
            tries++;
        }

        CurrentTarget = next;
        OnNewChallenge?.Invoke(CurrentTarget);
    }

    void OnGestureDetected(GestureData detected)
    {
        if (!IsActive || CurrentTarget == null) return;

        if (detected == CurrentTarget)
        {
            // ทำถูก! หยุด loop ปัจจุบัน แล้วจะ loop ใหม่เองใน ChallengeLoop
            OnSuccess?.Invoke();
            TimeRemaining = 0f; // หยุด countdown ทันที
        }
        // ถ้า detect ท่าผิด → ไม่ทำอะไร รอให้หมดเวลาเอง
        // หรือจะ Fail ทันทีก็ uncomment บรรทัดด้านล่าง:
        // else { OnFail?.Invoke(); TimeRemaining = 0f; }
    }
}