using System.Collections;
using UnityEngine;

public class GhostAnimController : MonoBehaviour
{
    public static GhostAnimController Instance { get; private set; }
    public bool IsReacting { get; private set; }
    public bool IsDancing { get; private set; }

    Animator _animator;
    Opening _opening;
    WinEnding _ending;

    const int FLOAT_UP_LAYER = 1;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _opening = GetComponent<Opening>();
        _ending = GetComponent<WinEnding>();

        GameManager.Instance.OnStateChanged += OnStateChanged;
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;

        SetVisible(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
        if (GestureChallenge.Instance != null)
            GestureChallenge.Instance.OnRoundEnd -= OnRoundEnd;
    }

    void OnStateChanged(GameState s, bool playerWon)
    {
        if (s == GameState.Countdown)
            GameManager.Instance.StartCoroutine(PlayOpeningSequence());
        if (s == GameState.GameOver)
            GameManager.Instance.StartCoroutine(PlayEndingSequence(playerWon));
    }

    void OnRoundEnd(ScoreGrade grade, int combo, int multiplier)
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        GameManager.Instance.StartCoroutine(PlayReactionSequence(grade));
    }

    IEnumerator PlayOpeningSequence()
    {
        SetVisible(true);

        _animator.SetLayerWeight(FLOAT_UP_LAYER, 1f);
        yield return new WaitForSeconds(3f);
        _animator.SetLayerWeight(FLOAT_UP_LAYER, 0f);

        _animator.SetTrigger("TriggerOpening");
        _opening?.PlayOpeningVFX();

        yield return new WaitUntil(() =>
            AudioManager.Instance == null ||
            !AudioManager.Instance.IsOpeningPlaying);
    }

    // GhostAnimController.cs — แก้ PlayDanceForGesture
    public IEnumerator PlayDanceForGesture(int danceIndex,
        System.Action<float> onHalfway) // ← เปลี่ยนเป็น Action<float> ส่งเวลากลับมา
    {
        IsDancing = true;

        _animator.SetInteger("DanceIndex", 0);
        yield return null;
        _animator.SetInteger("DanceIndex", danceIndex);

        string stateName = $"Dance_0{danceIndex}";

        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        // ดึงความยาว animation จริงๆ
        float clipLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        float halfLength = clipLength * 0.5f; // ครึ่งหลัง = 4.5s

        bool halfFired = false;

        while (true)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(stateName)) break;

            float t = info.normalizedTime % 1f;

            if (!halfFired && t >= 0.5f)
            {
                halfFired = true;
                onHalfway?.Invoke(halfLength); // ส่งเวลาครึ่งหลังกลับ
            }

            yield return null;
        }

        _animator.SetInteger("DanceIndex", 0);
        IsDancing = false;
    }

    IEnumerator PlayReactionSequence(ScoreGrade grade)
    {
        IsReacting = true;

        switch (grade)
        {
            case ScoreGrade.Perfect:
                _animator.ResetTrigger("TriggerHurt_01");
                _animator.ResetTrigger("TriggerHurt_02");
                _animator.SetTrigger("TriggerHurt_02");
                yield return StartCoroutine(WaitForStateAndFinish("Hurt_02"));
                break;

            case ScoreGrade.Good:
                _animator.ResetTrigger("TriggerHurt_01");
                _animator.ResetTrigger("TriggerHurt_02");
                _animator.SetTrigger("TriggerHurt_01");
                yield return StartCoroutine(WaitForStateAndFinish("Hurt_01"));
                break;

            case ScoreGrade.Miss:
                int idx = Random.Range(1, 3);
                _animator.SetInteger("AttackIndex", 0);
                yield return null;
                _animator.SetInteger("AttackIndex", idx);
                yield return StartCoroutine(WaitForStateAndFinish($"Attack_0{idx}"));
                _animator.SetInteger("AttackIndex", 0);
                break;
        }

        IsReacting = false;
    }

    // รอให้เข้า state แล้วเล่นจบ มี timeout กันค้าง
    IEnumerator WaitForStateAndFinish(string stateName, float timeout = 5f)
    {
        float elapsed = 0f;

        // รอเข้า state (timeout 5s กันค้าง)
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogWarning($"[Ghost] WaitForState timeout: {stateName}");
                yield break;
            }
            yield return null;
        }

        // รอเกือบจบ
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f ||
            !_animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        // รอกลับ Idle (timeout)
        elapsed = 0f;
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogWarning($"[Ghost] WaitForIdle timeout after: {stateName}");
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator PlayEndingSequence(bool playerWon)
    {
        if (playerWon)
        {
            _animator.SetTrigger("TriggerEndWin");
            yield return new WaitUntil(() =>
                _animator.GetCurrentAnimatorStateInfo(0).IsName("EndWin"));
            yield return new WaitUntil(() =>
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
        }
        else
        {
            _animator.SetTrigger("TriggerEndLose");
            _ending?.StartDissolve();
            yield return new WaitUntil(() =>
                _animator.GetCurrentAnimatorStateInfo(0).IsName("EndLose"));
            yield return new WaitUntil(() =>
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
        }
    }

    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}