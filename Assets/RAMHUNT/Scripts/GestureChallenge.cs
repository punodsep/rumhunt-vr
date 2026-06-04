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
    public event Action<GestureData> OnChallengeStart;
    public event Action<ScoreGrade, int, int> OnRoundEnd;

    const float GHOST_DANCE_PREVIEW = 4.5f;
    const float PLAYER_WINDOW = 4.5f;
    const float REACTION_TIME = 3f;

    public GestureData CurrentTarget { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool IsActive { get; private set; }

    bool _roundEnded;
    Coroutine _challengeRoutine;

    int _perfectCombo = 0;

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

            PickNewGesture();

            // ดูผีรำ 4.5 วิ
            yield return new WaitForSeconds(GHOST_DANCE_PREVIEW);

            // ผู้เล่นทำตาม 4.5 วิ
            TimeRemaining = PLAYER_WINDOW;

            OnChallengeStart?.Invoke(CurrentTarget);

            while (TimeRemaining > 0f &&
                   IsActive &&
                   !_roundEnded)
            {
                TimeRemaining -= Time.deltaTime;
                yield return null;
            }

            if (!IsActive)
                yield break;

            if (!_roundEnded)
            {
                _roundEnded = true;
                ProcessGrade(ScoreGrade.Miss);
            }

            // รอ PlayReact เล่นครบ 3 วิ
            yield return new WaitForSeconds(REACTION_TIME);
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
        float elapsed = PLAYER_WINDOW - TimeRemaining;

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
        int multiplier = 1;

        switch (grade)
        {
            case ScoreGrade.Perfect:
                _perfectCombo++;
                multiplier = Mathf.Max(1, _perfectCombo); // x1, x2, x3...
                GameManager.Instance.AddScore(perfectPoints * multiplier);
                GameManager.Instance.DamageGhost(perfectGhostDmg);
                break;

            case ScoreGrade.Good:
                _perfectCombo = 0; // หยุด combo
                GameManager.Instance.AddScore(goodPoints);
                GameManager.Instance.DamageGhost(goodGhostDmg);
                break;

            case ScoreGrade.Miss:
                _perfectCombo = 0; // หยุด combo
                GameManager.Instance.DamagePlayer(1);
                break;
        }

        OnRoundEnd?.Invoke(grade, _perfectCombo, multiplier);
    }

    void PickNewGesture()
    {
        if (gesturePool == null || gesturePool.Length == 0) return;

        GestureData next = CurrentTarget;
        int index = 0;
        int tries = 0;

        while (next == CurrentTarget && tries < 10)
        {
            index = UnityEngine.Random.Range(0, gesturePool.Length);
            next = gesturePool[index];
            tries++;
        }

        CurrentTarget = next;
        OnNewChallenge?.Invoke(CurrentTarget); // Ghost เริ่มรำทันที

        // PickNewGesture
        if (GhostAnimController.Instance != null)
            GameManager.Instance.StartCoroutine(
                GhostAnimController.Instance.PlayDanceForGesture(index + 1));
    }
}