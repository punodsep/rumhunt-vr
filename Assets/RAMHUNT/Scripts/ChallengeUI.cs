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

    [Header("Game Stats")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameTimerText;
    public TextMeshProUGUI hpText;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Feedback Colors")]
    public Color successColor = new Color(0.3f, 1f, 0.5f);
    public Color failColor = new Color(1f, 0.3f, 0.3f);
    public Color timerSafeColor = new Color(0.3f, 1f, 0.6f);
    public Color timerWarningColor = new Color(1f, 0.6f, 0.2f);
    public Color timerDangerColor = new Color(1f, 0.2f, 0.2f);

    void OnEnable()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
        GameManager.Instance.OnHPChanged += OnHPChanged;
        GameManager.Instance.OnScoreChanged += OnScoreChanged;
        GestureChallenge.Instance.OnNewChallenge += OnNewChallenge;
        GestureChallenge.Instance.OnSuccess += OnSuccess;
        GestureChallenge.Instance.OnFail += OnFail;
    }
    void OnDisable()
    {
        GameManager.Instance.OnStateChanged -= OnStateChanged;
        GameManager.Instance.OnHPChanged -= OnHPChanged;
        GameManager.Instance.OnScoreChanged -= OnScoreChanged;
        GestureChallenge.Instance.OnNewChallenge -= OnNewChallenge;
        GestureChallenge.Instance.OnSuccess -= OnSuccess;
        GestureChallenge.Instance.OnFail -= OnFail;
    }

    void Start()
    {
        // ตั้งค่าเริ่มต้น — แสดงแค่ startPanel
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        feedbackText.text = "";
        gestureNameText.text = "";
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        float ratio = Mathf.Clamp01(
            GestureChallenge.Instance.TimeRemaining / GestureChallenge.Instance.timePerGesture);

        timerBar.fillAmount = ratio;
        timerBar.color = ratio > 0.5f ? timerSafeColor
                       : ratio > 0.25f ? timerWarningColor
                       : timerDangerColor;

        gameTimerText.text = $"{GameManager.Instance.TimeRemaining:F0}s";
    }

    // ── State ────────────────────────────────────────────────────
    void OnStateChanged(GameState s)
    {
        startPanel.SetActive(s == GameState.Idle);
        gamePanel.SetActive(s == GameState.Playing || s == GameState.Countdown);
        gameOverPanel.SetActive(s == GameState.GameOver);

        if (s == GameState.Countdown)
        {
            // ซ่อนชื่อท่าและ feedback ระหว่าง countdown
            gestureNameText.text = "";
            feedbackText.text = "";
            StartCoroutine(ShowCountdown());
        }

        if (s == GameState.GameOver)
            finalScoreText.text = $"Score: {GameManager.Instance.Score}";
    }

    IEnumerator ShowCountdown()
    {
        for (int i = 3; i >= 1; i--)
        {
            feedbackText.text = i.ToString();
            feedbackText.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(1f);
        }
        feedbackText.text = "Start!";
        feedbackText.color = successColor;
        yield return new WaitForSeconds(0.6f);
        feedbackText.text = "";
    }

    // ── Challenge Events ─────────────────────────────────────────
    void OnNewChallenge(GestureData g)
    {
        gestureNameText.text = g.gestureName;
        feedbackText.text = "";
        // reset alpha กรณี fade ค้าง
        feedbackText.color = new Color(feedbackText.color.r,
                                         feedbackText.color.g,
                                         feedbackText.color.b, 1f);
    }

    void OnSuccess()
    {
        // Score อยู่ใน GestureChallenge → GameManager แล้ว
        // UI แค่แสดง feedback
        StopAllCoroutines();
        StartCoroutine(ShowCountdown_Continue()); // นับต่อถ้ากำลัง countdown
        StartCoroutine(ShowFeedback("Correct!", successColor));
        GameManager.Instance.AddScore(1);
    }

    void OnFail()
    {
        StopAllCoroutines();
        StartCoroutine(ShowFeedback("Miss!", failColor));
        GameManager.Instance.LoseHP();
    }

    // ── Feedback ─────────────────────────────────────────────────
    IEnumerator ShowFeedback(string msg, Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;

        yield return new WaitForSeconds(0.8f);

        // fade out
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            feedbackText.color = new Color(color.r, color.g, color.b,
                                           1f - t / 0.4f);
            yield return null;
        }
        feedbackText.text = "";
    }

    // ── ใช้กัน StopAllCoroutines ทำลาย countdown กลางคัน ────────
    IEnumerator ShowCountdown_Continue() { yield break; }

    void OnHPChanged(int hp) => hpText.text = "HP: " + new string('♥', hp);
    void OnScoreChanged(int s) => scoreText.text = $"Score: {s}";

    // ── ปุ่ม ─────────────────────────────────────────────────────
    public void OnStartButton() => GameManager.Instance.StartGame();
    public void OnRestartButton() => GameManager.Instance.RestartGame();
}