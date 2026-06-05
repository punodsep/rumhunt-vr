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

    // ── Opening ───────────────────────────────────────────────────
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

    // ── Dance ─────────────────────────────────────────────────────
    // รัน Coroutine นี้บน GameManager แล้วส่ง callback กลับมา
    public IEnumerator PlayDanceForGesture(int danceIndex,
        System.Action onHalfway)  // ← callback ตอนครึ่งทาง
    {
        IsDancing = true;

        _animator.SetInteger("DanceIndex", 0);
        yield return null;
        _animator.SetInteger("DanceIndex", danceIndex);

        string stateName = $"Dance_0{danceIndex}";

        // รอให้เข้า state Dance ก่อน
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        bool halfFired = false;

        // วนรอจนจบ animation
        while (true)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);

            if (!info.IsName(stateName)) break; // ออกจาก state = จบแล้ว

            float t = info.normalizedTime % 1f;

            // ครึ่งทาง → fire callback ครั้งเดียว
            if (!halfFired && t >= 0.5f)
            {
                halfFired = true;
                onHalfway?.Invoke();
            }

            yield return null;
        }

        _animator.SetInteger("DanceIndex", 0);
        IsDancing = false;
    }

    // ── Reaction ──────────────────────────────────────────────────
    IEnumerator PlayReactionSequence(ScoreGrade grade)
    {
        IsReacting = true;

        string stateName = "";

        switch (grade)
        {
            case ScoreGrade.Perfect:
                _animator.ResetTrigger("TriggerHurt_01");
                _animator.ResetTrigger("TriggerHurt_02");
                _animator.SetTrigger("TriggerHurt_02");
                stateName = "Hurt_02";
                break;

            case ScoreGrade.Good:
                _animator.ResetTrigger("TriggerHurt_01");
                _animator.ResetTrigger("TriggerHurt_02");
                _animator.SetTrigger("TriggerHurt_01");
                stateName = "Hurt_01";
                break;

            case ScoreGrade.Miss:
                int idx = Random.Range(1, 3);
                _animator.SetInteger("AttackIndex", 0);
                yield return null;
                _animator.SetInteger("AttackIndex", idx);
                stateName = $"Attack_0{idx}";
                break;
        }

        // รอให้เข้า state Reaction
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));

        // รอจน normalizedTime >= 0.95 (เกือบจบ)
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);

        // รอกลับ Idle
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        if (grade == ScoreGrade.Miss)
            _animator.SetInteger("AttackIndex", 0);

        IsReacting = false;
    }

    // ── Ending ────────────────────────────────────────────────────
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