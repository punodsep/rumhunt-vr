using System;
using System.Collections;
using UnityEngine;

public class GestureChallenge : MonoBehaviour
{
    public static GestureChallenge Instance { get; private set; }

    [Header("References")]
    public GestureDetector detector;

    [Header("Gesture Pool")]
    public GestureData[] gesturePool;

    [Header("Timing")]
    public float timePerGesture = 5f;

    public event Action<GestureData> OnNewChallenge;
    public event Action OnSuccess;
    public event Action OnFail;

    public GestureData CurrentTarget { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsActive { get; private set; }

    // ── เพิ่ม flag กันชนกัน ──────────────────────────────────────
    bool _roundEnded;

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
            // เริ่มรอบใหม่
            _roundEnded = false;
            TimeRemaining = timePerGesture;
            PickNewGesture();

            // นับเวลาถอยหลัง
            while (TimeRemaining > 0f && IsActive && !_roundEnded)
            {
                TimeRemaining -= Time.deltaTime;
                yield return null;
            }

            if (!IsActive) yield break;

            // ถ้าไม่มีใครเคลียร์รอบ (ไม่ใช่ Success) = Fail
            if (!_roundEnded)
            {
                _roundEnded = true;
                OnFail?.Invoke();
                yield return new WaitForSeconds(1.2f);
            }
            else
            {
                // Success → หน่วงนิดนึงให้ feedback แสดงก่อน
                yield return new WaitForSeconds(0.8f);
            }
        }
    }

    void PickNewGesture()
    {
        if (gesturePool == null || gesturePool.Length == 0) return;

        GestureData next = CurrentTarget;
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
        if (!IsActive || CurrentTarget == null || _roundEnded) return;

        if (detected == CurrentTarget)
        {
            _roundEnded = true;  // ปิดรอบ กัน Fail ตาม
            TimeRemaining = 0f;
            OnSuccess?.Invoke();
        }
    }
}