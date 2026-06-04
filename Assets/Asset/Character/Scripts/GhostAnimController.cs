using System.Collections;
using UnityEngine;

public class GhostAnimController : MonoBehaviour
{
    public static GhostAnimController Instance { get; private set; }
    public bool IsReacting { get; private set; }

    Animator animator;
    Opening opening;
    WinEnding ending;

    const int FLOAT_UP_LAYER = 1;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        opening = GetComponent<Opening>();
        ending = GetComponent<WinEnding>();

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

    // =====================================================
    // Opening
    // =====================================================

    IEnumerator PlayOpeningSequence()
    {
        SetVisible(true);

        animator.SetLayerWeight(FLOAT_UP_LAYER, 1f);

        yield return new WaitForSeconds(3f); // FloatUp 3 วิ

        animator.SetLayerWeight(FLOAT_UP_LAYER, 0f);

        animator.SetTrigger("TriggerOpening");

        opening?.PlayOpeningVFX();

        yield return new WaitForSeconds(3f); // Opening Dance

        while (AudioManager.Instance != null &&
               AudioManager.Instance.IsOpeningPlaying)
        {
            yield return null;
        }
    }

    // =====================================================
    // Dance
    // =====================================================

    public IEnumerator PlayDanceForGesture(int danceIndex)
    {
        animator.SetInteger("DanceIndex", 0);

        yield return null;

        animator.SetInteger("DanceIndex", danceIndex);

        yield return new WaitForSeconds(9f);

        animator.SetInteger("DanceIndex", 0);
    }

    // =====================================================
    // Reaction
    // =====================================================

    IEnumerator PlayReactionSequence(ScoreGrade grade)
    {
        IsReacting = true;

        switch (grade)
        {
            case ScoreGrade.Perfect:

                animator.ResetTrigger("TriggerHurt_01");
                animator.ResetTrigger("TriggerHurt_02");

                animator.SetTrigger("TriggerHurt_02");

                yield return new WaitForSeconds(3f);

                break;

            case ScoreGrade.Good:

                animator.ResetTrigger("TriggerHurt_01");
                animator.ResetTrigger("TriggerHurt_02");

                animator.SetTrigger("TriggerHurt_01");

                yield return new WaitForSeconds(3f);

                break;

            case ScoreGrade.Miss:

                int attackIndex = Random.Range(1, 3);

                animator.SetInteger("AttackIndex", 0);

                yield return null;

                animator.SetInteger("AttackIndex", attackIndex);

                yield return new WaitForSeconds(3f);

                animator.SetInteger("AttackIndex", 0);

                break;
        }

        IsReacting = false;
    }

    // =====================================================
    // Ending
    // =====================================================

    IEnumerator PlayEndingSequence(bool playerWon)
    {
        if (playerWon)
        {
            animator.SetTrigger("TriggerEndWin");

            yield return new WaitForSeconds(4f);
        }
        else
        {
            animator.SetTrigger("TriggerEndLose");

            ending?.StartDissolve();

            yield return new WaitForSeconds(4f);
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}