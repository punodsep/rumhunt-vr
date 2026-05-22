using System;
using System.Collections;
using UnityEngine;

public enum ScoreGrade { Perfect, Good, Miss }

public class GestureChallenge : MonoBehaviour
{
    public static GestureChallenge Instance { get; private set; }

    [Header("References")]
    public GestureDetector detector;

    [Header("Gesture Pool")]
    public GestureData[] gesturePool;

    [Header("Timing")]
    public float timePerGesture = 5f;

    [Tooltip("ทำภายในกี่วินาทีแรก = Perfect")]
    public float perfectWindow = 2f;

    [Header("Damage to Ghost")]
    public int perfectGhostDmg = 3;
    public int goodGhostDmg = 1;

    [Header("Score Points")]
    public int perfectPoints = 300;
    public int goodPoints = 100;

    public event Action<GestureData> OnNewChallenge;
    public event Action<ScoreGrade> OnRoundEnd;

    public GestureData CurrentTarget { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsActive { get; private set; }

    bool _roundEnded;
    Coroutine _challengeRoutine;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        detector.OnGestureDetected += OnGestureDetected;
    }

    void OnDisable()
    {
        detector.OnGestureDetected -= OnGestureDetected;
    }

    public void StartChallenge()
    {
        IsActive = true;
        _challengeRoutine = StartCoroutine(ChallengeLoop());
    }

    public void StopChallenge()
    {
        IsActive = false;

        if (_challengeRoutine != null)
            StopCoroutine(_challengeRoutine);
    }

    IEnumerator ChallengeLoop()
    {
        while (IsActive)
        {
            _roundEnded = false;

            TimeRemaining = timePerGesture;

            PickNewGesture();

            while (TimeRemaining > 0f && IsActive && !_roundEnded)
            {
                TimeRemaining -= Time.deltaTime;
                yield return null;
            }

            if (!IsActive)
                yield break;

            if (!_roundEnded)
            {
                // หมดเวลา = Miss
                _roundEnded = true;

                ProcessGrade(ScoreGrade.Miss);

                yield return new WaitForSeconds(1.2f);
            }
            else
            {
                yield return new WaitForSeconds(0.8f);
            }
        }
    }

    void OnGestureDetected(GestureData detected)
    {
        if (!IsActive || CurrentTarget == null || _roundEnded)
            return;

        if (detected != CurrentTarget)
            return;

        _roundEnded = true;

        // เวลาที่ใช้ไป
        float elapsed = timePerGesture - TimeRemaining;

        // หยุด timer
        TimeRemaining = 0f;

        // Perfect / Good
        ScoreGrade grade =
            elapsed <= perfectWindow
            ? ScoreGrade.Perfect
            : ScoreGrade.Good;

        ProcessGrade(grade);
    }

    void ProcessGrade(ScoreGrade grade)
    {
        switch (grade)
        {
            case ScoreGrade.Perfect:
                GameManager.Instance.AddScore(perfectPoints);
                GameManager.Instance.DamageGhost(perfectGhostDmg);
                break;

            case ScoreGrade.Good:
                GameManager.Instance.AddScore(goodPoints);
                GameManager.Instance.DamageGhost(goodGhostDmg);
                break;

            case ScoreGrade.Miss:
                GameManager.Instance.DamagePlayer(1);
                break;
        }

        OnRoundEnd?.Invoke(grade);
    }

    void PickNewGesture()
    {
        if (gesturePool == null || gesturePool.Length == 0)
            return;

        GestureData next = CurrentTarget;

        int tries = 0;

        while (next == CurrentTarget && tries < 10)
        {
            next = gesturePool[
                UnityEngine.Random.Range(0, gesturePool.Length)
            ];

            tries++;
        }

        CurrentTarget = next;

        OnNewChallenge?.Invoke(CurrentTarget);
    }
}