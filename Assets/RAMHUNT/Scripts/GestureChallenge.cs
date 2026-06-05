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
    public float perfectWindow = 3f;

    [Header("Damage to Ghost")]
    public int perfectGhostDmg = 3;
    public int goodGhostDmg = 1;

    [Header("Score Points")]
    public int perfectPoints = 300;
    public int goodPoints = 100;

    public event Action<GestureData> OnNewChallenge;
    public event Action<GestureData> OnChallengeStart;
    public event Action<ScoreGrade, int, int> OnRoundEnd;

    // TimeRemaining สำหรับ UI timer bar
    public float PlayerWindow => _playerWindow;

    public float TimeRemaining { get; private set; }
    public GestureData CurrentTarget { get; private set; }
    public bool IsActive { get; private set; }

    float _playerWindow;   // ครึ่งหลังของ Dance anim
    bool _roundEnded;
    bool _playerWindowOpen;
    int _perfectCombo;
    Coroutine _challengeRoutine;

    void Awake() => Instance = this;

    void OnEnable() => detector.OnGestureDetected += OnGestureDetected;
    void OnDisable() => detector.OnGestureDetected -= OnGestureDetected;

    public void StartChallenge()
    {
        IsActive = true;
        _perfectCombo = 0;
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
            _roundEnded = false;
            _playerWindowOpen = false;

            int index = PickNewGesture();

            GameManager.Instance.StartCoroutine(
                GhostAnimController.Instance.PlayDanceForGesture(
                    index + 1,
                    onHalfway: () =>
                    {
                        _playerWindowOpen = true;
                        OnChallengeStart?.Invoke(CurrentTarget);
                    }));

            yield return new WaitUntil(() => _playerWindowOpen);

            float startTime = Time.time;

            while (!_roundEnded && IsActive &&
                   GhostAnimController.Instance.IsDancing)
            {
                TimeRemaining = Mathf.Max(0f,
                    PlayerWindow - (Time.time - startTime));
                yield return null;
            }

            if (!IsActive) yield break;

            if (!_roundEnded)
            {
                _roundEnded = true;
                ProcessGrade(ScoreGrade.Miss);
            }

            yield return new WaitUntil(() =>
                GhostAnimController.Instance == null ||
                !GhostAnimController.Instance.IsReacting);
        }
    }

    void OnGestureDetected(GestureData detected)
    {
        if (!IsActive || CurrentTarget == null || _roundEnded) return;
        if (!_playerWindowOpen) return;
        if (detected != CurrentTarget) return;

        _roundEnded = true;

        float elapsed = Time.time - _windowOpenTime;
        TimeRemaining = 0f;

        ScoreGrade grade = elapsed <= perfectWindow
            ? ScoreGrade.Perfect : ScoreGrade.Good;

        ProcessGrade(grade);
    }

    float _windowOpenTime;

    int PickNewGesture()
    {
        if (gesturePool == null || gesturePool.Length == 0) return 0;

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
        OnNewChallenge?.Invoke(CurrentTarget);
        return index;
    }

    void ProcessGrade(ScoreGrade grade)
    {
        int multiplier = 1;
        switch (grade)
        {
            case ScoreGrade.Perfect:
                _perfectCombo++;
                multiplier = Mathf.Max(1, _perfectCombo);
                GameManager.Instance.AddScore(perfectPoints * multiplier);
                GameManager.Instance.DamageGhost(perfectGhostDmg);
                break;
            case ScoreGrade.Good:
                _perfectCombo = 0;
                GameManager.Instance.AddScore(goodPoints);
                GameManager.Instance.DamageGhost(goodGhostDmg);
                break;
            case ScoreGrade.Miss:
                _perfectCombo = 0;
                GameManager.Instance.DamagePlayer(1);
                break;
        }
        OnRoundEnd?.Invoke(grade, _perfectCombo, multiplier);
    }
}