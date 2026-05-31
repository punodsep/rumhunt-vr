using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeUI : MonoBehaviour
{
    [Header("Challenge Display")]
    public TextMeshProUGUI gestureNameText;
    public Image timerBar;
    public TextMeshProUGUI feedbackText;

    [Header("HP Bars — Player")]
    public Image playerHPBar;
    public TextMeshProUGUI playerHPText;

    [Header("HP Bars — Ghost")]
    public Image ghostHPBar;
    public TextMeshProUGUI ghostHPText;

    [Header("Score")]
    public TextMeshProUGUI scoreText;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newHighScoreText;

    [Header("Timer Bar Colors")]
    public Color timerSafeColor = new Color(0.3f, 1f, 0.6f);
    public Color timerWarningColor = new Color(1f, 0.6f, 0.2f);
    public Color timerDangerColor = new Color(1f, 0.2f, 0.2f);

    [Header("HP Bar Colors")]
    public Color hpSafeColor = new Color(0.3f, 1f, 0.6f);
    public Color hpWarningColor = new Color(1f, 0.6f, 0.2f);
    public Color hpDangerColor = new Color(1f, 0.2f, 0.2f);

    [Header("Feedback Colors")]
    public Color perfectColor = new Color(1f, 0.9f, 0.2f);
    public Color goodColor = new Color(0.3f, 1f, 0.5f);
    public Color missColor = new Color(1f, 0.3f, 0.3f);

    void OnEnable()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
        GameManager.Instance.OnPlayerHPChanged += OnPlayerHPChanged;
        GameManager.Instance.OnGhostHPChanged += OnGhostHPChanged;
        GameManager.Instance.OnScoreChanged += OnScoreChanged;

        GestureChallenge.Instance.OnNewChallenge += OnNewChallenge;
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;
    }

    void OnDisable()
    {
        GameManager.Instance.OnStateChanged -= OnStateChanged;
        GameManager.Instance.OnPlayerHPChanged -= OnPlayerHPChanged;
        GameManager.Instance.OnGhostHPChanged -= OnGhostHPChanged;
        GameManager.Instance.OnScoreChanged -= OnScoreChanged;

        GestureChallenge.Instance.OnNewChallenge -= OnNewChallenge;
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;
    }

    void Start()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (newHighScoreText)
            newHighScoreText.gameObject.SetActive(false);

        feedbackText.text = "";
        gestureNameText.text = "";
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing)
            return;

        float ratio = Mathf.Clamp01(
            GestureChallenge.Instance.TimeRemaining /
            GestureChallenge.Instance.timePerGesture);

        timerBar.fillAmount = ratio;

        timerBar.color =
            ratio > 0.5f ? timerSafeColor :
            ratio > 0.25f ? timerWarningColor :
            timerDangerColor;
    }

    // ── State ─────────────────────────────────────────────

    void OnStateChanged(GameState s)
    {
        startPanel.SetActive(s == GameState.Idle);
        gamePanel.SetActive(s == GameState.Playing || s == GameState.Countdown);
        gameOverPanel.SetActive(s == GameState.GameOver);

        if (s == GameState.Countdown)
        {
            gestureNameText.text = "";
            feedbackText.text = "";
            StartCoroutine(ShowCountdown());
        }

        if (s == GameState.GameOver)
            ShowGameOver();
    }

    void ShowGameOver()
    {
        int score = GameManager.Instance.Score;
        int highScore = GameManager.Instance.HighScore;

        finalScoreText.text = $"Score {score}";
        highScoreText.text = $"Best {highScore}";

        if (newHighScoreText)
            newHighScoreText.gameObject.SetActive(score >= highScore);
    }

    IEnumerator ShowCountdown()
    {
        for (int i = 3; i >= 1; i--)
        {
            feedbackText.text = i.ToString();
            feedbackText.color = Color.white;

            yield return new WaitForSeconds(1f);
        }

        feedbackText.text = "Start!";
        feedbackText.color = goodColor;

        yield return new WaitForSeconds(0.6f);

        feedbackText.text = "";
    }

    // ── Challenge ─────────────────────────────────────────

    void OnNewChallenge(GestureData g)
    {
        gestureNameText.text = g.gestureName;

        feedbackText.text = "";
        feedbackText.color = Color.white;
    }

    void OnRoundEnd(ScoreGrade grade, int combo, int multiplier)
    {
        StopCoroutine(nameof(ShowFeedbackRoutine));

        switch (grade)
        {
            case ScoreGrade.Perfect:
                string perfectMsg = combo >= 2
                    ? $"PERFECT!  x{multiplier}"
                    : "PERFECT!";
                StartCoroutine(ShowFeedbackRoutine(perfectMsg, perfectColor));
                break;
            case ScoreGrade.Good:
                StartCoroutine(ShowFeedbackRoutine("Good", goodColor));
                break;
            case ScoreGrade.Miss:
                StartCoroutine(ShowFeedbackRoutine("Miss...", missColor));
                break;
        }
    }

    IEnumerator ShowFeedbackRoutine(string msg, Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;

        yield return new WaitForSeconds(0.8f);

        float t = 0f;

        while (t < 0.4f)
        {
            t += Time.deltaTime;

            feedbackText.color = new Color(
                color.r,
                color.g,
                color.b,
                1f - t / 0.4f
            );

            yield return null;
        }

        feedbackText.text = "";
    }

    // ── HP Bars ───────────────────────────────────────────

    void OnPlayerHPChanged(int current, int max)
    {
        float ratio = (float)current / max;

        if (playerHPBar)
        {
            playerHPBar.fillAmount = ratio;

            playerHPBar.color =
                ratio > 0.5f ? hpSafeColor :
                ratio > 0.25f ? hpWarningColor :
                hpDangerColor;
        }

        if (playerHPText)
            playerHPText.text = $"{current}/{max}";
    }

    void OnGhostHPChanged(int current, int max)
    {
        float ratio = (float)current / max;

        if (ghostHPBar)
        {
            ghostHPBar.fillAmount = ratio;

            ghostHPBar.color =
                ratio > 0.5f ? hpSafeColor :
                ratio > 0.25f ? hpWarningColor :
                hpDangerColor;
        }

        if (ghostHPText)
            ghostHPText.text = $"{current}/{max}";
    }

    // ── Score ─────────────────────────────────────────────

    void OnScoreChanged(int s)
    {
        scoreText.text = $"Score: {s}";
    }

    // ── Buttons ───────────────────────────────────────────

    public void OnStartButton()
    {
        GameManager.Instance.StartGame();
    }

    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }
}