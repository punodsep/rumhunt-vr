using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeUI : MonoBehaviour
{
    [Header("Challenge Display")]
    public TextMeshProUGUI gestureNameText;
    public Image timerBar;
    public Image feedbackImage;
    public Sprite perfectSprite;
    public Sprite goodSprite;
    public Sprite missSprite;
    public TextMeshProUGUI countdownText;  // สำหรับ 3, 2, 1, Start!

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
        GestureChallenge.Instance.OnRoundEnd -= OnRoundEnd;
    }

    void Start()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (newHighScoreText)
            newHighScoreText.gameObject.SetActive(false);

        feedbackImage.gameObject.SetActive(false);
        gestureNameText.text = "";
        countdownText.text = "";
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        float ratio = Mathf.Clamp01(
            GestureChallenge.Instance.TimeRemaining /
            GestureChallenge.Instance.timePerGesture);

        timerBar.fillAmount = ratio;
        timerBar.color = ratio > 0.5f ? timerSafeColor
                       : ratio > 0.25f ? timerWarningColor
                       : timerDangerColor;
    }

    // ── State ─────────────────────────────────────────────────────
    void OnStateChanged(GameState s)
    {
        startPanel.SetActive(s == GameState.Idle);
        gamePanel.SetActive(s == GameState.Playing || s == GameState.Countdown);
        gameOverPanel.SetActive(s == GameState.GameOver);

        if (s == GameState.Countdown)
        {
            gestureNameText.text = "";
            feedbackImage.gameObject.SetActive(false);
            StartCoroutine(ShowCountdown());
        }

        if (s == GameState.Playing)
            countdownText.text = "";

        if (s == GameState.GameOver)
            ShowGameOver();
    }

    void ShowGameOver()
    {
        int score = GameManager.Instance.Score;
        int highScore = GameManager.Instance.HighScore;

        finalScoreText.text = $"{score}";
        highScoreText.text = $"{highScore}";

        if (newHighScoreText)
            newHighScoreText.gameObject.SetActive(score >= highScore);
    }

    IEnumerator ShowCountdown()
    {
        for (int i = 3; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            countdownText.color = Color.white;
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "เริ่ม!";
        countdownText.color = new Color(0.3f, 1f, 0.5f);
        yield return new WaitForSeconds(0.6f);
        countdownText.text = "";
    }

    // ── Challenge ─────────────────────────────────────────────────
    void OnNewChallenge(GestureData g)
    {
        gestureNameText.text = g.gestureName;
        feedbackImage.gameObject.SetActive(false);
        feedbackImage.color = Color.white;
    }

    void OnRoundEnd(ScoreGrade grade, int combo, int multiplier)
    {
        StopCoroutine(nameof(ShowFeedbackRoutine));

        switch (grade)
        {
            case ScoreGrade.Perfect:
                StartCoroutine(ShowFeedbackRoutine(perfectSprite));
                break;
            case ScoreGrade.Good:
                StartCoroutine(ShowFeedbackRoutine(goodSprite));
                break;
            case ScoreGrade.Miss:
                StartCoroutine(ShowFeedbackRoutine(missSprite));
                break;
        }
    }

    // ── Feedback Helpers ──────────────────────────────────────────
    void ShowFeedbackSprite(Sprite sprite)
    {
        if (sprite == null) return;
        feedbackImage.sprite = sprite;
        feedbackImage.color = Color.white;
        feedbackImage.gameObject.SetActive(true);
    }

    IEnumerator ShowFeedbackRoutine(Sprite sprite)
    {
        ShowFeedbackSprite(sprite);

        yield return new WaitForSeconds(0.8f);

        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            feedbackImage.color = new Color(1f, 1f, 1f, 1f - t / 0.4f);
            yield return null;
        }

        feedbackImage.gameObject.SetActive(false);
    }

    // ── HP Bars ───────────────────────────────────────────────────
    void OnPlayerHPChanged(int current, int max)
    {
        if (playerHPBar) playerHPBar.fillAmount = (float)current / max;
        if (playerHPText) playerHPText.text = $"{current}/{max}";
    }

    void OnGhostHPChanged(int current, int max)
    {
        if (ghostHPBar) ghostHPBar.fillAmount = (float)current / max;
        if (ghostHPText) ghostHPText.text = $"{current}/{max}";
    }

    // ── Score ─────────────────────────────────────────────────────
    void OnScoreChanged(int s) => scoreText.text = $"{s}";

    // ── Buttons ───────────────────────────────────────────────────
    public void OnStartButton() => GameManager.Instance.StartGame();
    public void OnRestartButton() => GameManager.Instance.RestartGame();
}