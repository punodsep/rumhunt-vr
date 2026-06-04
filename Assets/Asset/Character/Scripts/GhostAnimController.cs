using System.Collections;
using UnityEngine;

public class GhostAnimController : MonoBehaviour
{
    public static GhostAnimController Instance { get; private set; }

    Animator animator;
    Opening opening;
    WinEnding ending;

    const int FLOAT_UP_LAYER = 1;

    bool isBusy;

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
        isBusy = true;

        SetVisible(true);

        animator.SetLayerWeight(FLOAT_UP_LAYER, 1f);

        yield return new WaitForSeconds(0.2f);

        animator.SetTrigger("TriggerOpening");

        opening?.PlayOpeningVFX();

        yield return new WaitForSeconds(3f);

        animator.SetLayerWeight(FLOAT_UP_LAYER, 0f);

        isBusy = false;
    }

    // =====================================================
    // Dance
    // =====================================================

    public IEnumerator PlayDanceForGesture(int danceIndex)
    {
        while (isBusy)
            yield return null;

        isBusy = true;

        animator.SetInteger("DanceIndex", danceIndex);

        yield return null;

        yield return new WaitForSeconds(3.75f);

        animator.SetInteger("DanceIndex", 0);

        isBusy = false;
    }

    // =====================================================
    // Reaction
    // =====================================================

    IEnumerator PlayReactionSequence(ScoreGrade grade)
    {
        while (isBusy)
            yield return null;

        isBusy = true;

        switch (grade)
        {
            case ScoreGrade.Perfect:

                animator.SetTrigger("TriggerHurt_02");

                yield return new WaitForSeconds(1.2f);

                break;

            case ScoreGrade.Good:

                animator.SetTrigger("TriggerHurt_01");

                yield return new WaitForSeconds(1.0f);

                break;

            case ScoreGrade.Miss:

                int attackIndex = Random.Range(1, 3);

                animator.SetInteger("AttackIndex", 0);

                yield return null;

                animator.SetInteger("AttackIndex", attackIndex);

                yield return new WaitForSeconds(1.5f);

                animator.SetInteger("AttackIndex", 0);

                break;
        }

        isBusy = false;
    }

    // =====================================================
    // Ending
    // =====================================================

    IEnumerator PlayEndingSequence(bool playerWon)
    {
        isBusy = true;

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

        isBusy = false;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}