using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeUI : MonoBehaviour
{
    [Header("Challenge Display")]
    public TextMeshProUGUI gestureNameText;   // ชื่อท่าที่ต้องทำ
    public Image timerBar;          // fillAmount = TimeRemaining/timePerGesture
    public TextMeshProUGUI feedbackText;      // SUCCESS / MISS

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

    private void Start()
    {
        //OnStartButton();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        // อัปเดต timer bar
        float challenge = GestureChallenge.Instance.TimeRemaining;
        float total = GestureChallenge.Instance.timePerGesture; // expose เป็น public
        float ratio = Mathf.Clamp01(challenge / total);
        timerBar.fillAmount = ratio;
        timerBar.color = ratio > 0.5f ? timerSafeColor
                       : ratio > 0.25f ? timerWarningColor
                       : timerDangerColor;

        // เวลาเกมรวม
        gameTimerText.text = $"{GameManager.Instance.TimeRemaining:F0}s";
    }

    void OnStateChanged(GameState s)
    {
        startPanel.SetActive(s == GameState.Idle || s == GameState.Countdown);
        gamePanel.SetActive(s == GameState.Playing);
        gameOverPanel.SetActive(s == GameState.GameOver);

        if (s == GameState.Countdown)
            StartCoroutine(ShowCountdown());

        if (s == GameState.GameOver)
            finalScoreText.text = $"Score : {GameManager.Instance.Score}";
    }

    IEnumerator ShowCountdown()
    {
        gestureNameText.text = "";
        for (int i = 3; i >= 1; i--)
        {
            feedbackText.text = i.ToString();
            feedbackText.color = Color.white;
            yield return new WaitForSeconds(1f);
        }
        feedbackText.text = "Start!";
        yield return new WaitForSeconds(0.5f);
        feedbackText.text = "";
    }

    void OnNewChallenge(GestureData g)
    {
        gestureNameText.text = g.gestureName;
        feedbackText.text = "";
    }

    void OnSuccess()
    {
        GameManager.Instance.AddScore(100);
        StartCoroutine(ShowFeedback("✓ Correct!", successColor));
    }

    void OnFail()
    {
        GameManager.Instance.LoseHP();
        StartCoroutine(ShowFeedback("Time's Up", failColor));
    }

    IEnumerator ShowFeedback(string msg, Color color)
    {
        feedbackText.text = msg;
        feedbackText.color = color;

        //feedbackText.transform.localScale = Vector3.one * 1.4f;
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            //feedbackText.transform.localScale = Vector3.Lerp(
                //Vector3.one * 1.4f, Vector3.one, t / 0.2f);
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            feedbackText.color = new Color(
                color.r, color.g, color.b, 1f - t / 0.3f);
            yield return null;
        }
        feedbackText.text = "";
    }

    void OnHPChanged(int hp) => hpText.text = "HP: " + new string('♥', hp);
    void OnScoreChanged(int s) => scoreText.text = $"Score: {s}";

    public void OnStartButton() => GameManager.Instance.StartGame();
    public void OnRestartButton() => GameManager.Instance.RestartGame();
}