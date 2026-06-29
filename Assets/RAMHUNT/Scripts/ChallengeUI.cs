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
    public TextMeshProUGUI countdownText;

    [Header("Countdown Sprites (3-2-1)")]
    public Image countdownImage;
    public Sprite countdown3Sprite;
    public Sprite countdown2Sprite;
    public Sprite countdown1Sprite;

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
    public Image winImage;
    public Image loseImage;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newHighScoreText;

    [Header("Timer Bar Colors")]
    public Color timerSafeColor = new Color(0.3f, 1f, 0.6f);
    public Color timerWarningColor = new Color(1f, 0.6f, 0.2f);
    public Color timerDangerColor = new Color(1f, 0.2f, 0.2f);


    void Start()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        if (winImage) winImage.gameObject.SetActive(false);
        if (loseImage) loseImage.gameObject.SetActive(false);
        if (newHighScoreText) newHighScoreText.gameObject.SetActive(false);

        feedbackImage.gameObject.SetActive(false);
        gestureNameText.text = "";
        countdownText.text = "";

        if (countdownImage) countdownImage.gameObject.SetActive(false);

        GameManager.Instance.OnStateChanged += OnStateChanged;
        GameManager.Instance.OnPlayerHPChanged += OnPlayerHPChanged;
        GameManager.Instance.OnGhostHPChanged += OnGhostHPChanged;
        GameManager.Instance.OnScoreChanged += OnScoreChanged;

        GestureChallenge.Instance.OnNewChallenge += OnNewChallenge;
        GestureChallenge.Instance.OnChallengeStart += OnChallengeStart;
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        float ratio = Mathf.Clamp01(
    GestureChallenge.Instance.TimeRemaining /
    GestureChallenge.Instance.PlayerWindow);

        timerBar.fillAmount = ratio;
        timerBar.color = ratio > 0.5f ? timerSafeColor
                       : ratio > 0.25f ? timerWarningColor
                       : timerDangerColor;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnPlayerHPChanged -= OnPlayerHPChanged;
            GameManager.Instance.OnGhostHPChanged -= OnGhostHPChanged;
            GameManager.Instance.OnScoreChanged -= OnScoreChanged;
        }

        if (GestureChallenge.Instance != null)
        {
            GestureChallenge.Instance.OnNewChallenge -= OnNewChallenge;
            GestureChallenge.Instance.OnChallengeStart -= OnChallengeStart;
            GestureChallenge.Instance.OnRoundEnd -= OnRoundEnd;
        }
    }

    void OnStateChanged(GameState s, bool playerWon)
    {
        startPanel.SetActive(s == GameState.Idle);
        gamePanel.SetActive(s == GameState.Playing || s == GameState.Countdown);

        // ปิด Game Over Panel ไว้ก่อน
        if (s != GameState.GameOver)
            gameOverPanel.SetActive(false);

        if (s == GameState.Countdown)
        {
            gestureNameText.text = "";
            feedbackImage.gameObject.SetActive(false);
            StartCoroutine(ShowCountdown());
        }

        if (s == GameState.Playing)
        {
            countdownText.text = "";
            if (countdownImage) countdownImage.gameObject.SetActive(false);
        }

        if (s == GameState.GameOver)
            StartCoroutine(ShowGameOverDelayed(playerWon));
    }

    void ShowGameOver(bool playerWon)
    {
        int score = GameManager.Instance.Score;
        int highScore = GameManager.Instance.HighScore;

        finalScoreText.text = $"{score}";
        highScoreText.text = $"{highScore}";

        if (newHighScoreText)
            newHighScoreText.gameObject.SetActive(score >= highScore);

        if (winImage) winImage.gameObject.SetActive(playerWon);
        if (loseImage) loseImage.gameObject.SetActive(!playerWon);
    }

    IEnumerator ShowGameOverDelayed(bool playerWon)
    {

        yield return new WaitForSeconds(11f);

        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);

        ShowGameOver(playerWon);
    }

    IEnumerator ShowCountdown()
    {
        while (!AudioManager.Instance.IsOpeningPlaying)
            yield return null;

        while (AudioManager.Instance.IsOpeningPlaying)
            yield return null;

        // กันปัญหา frame แรก panel/canvas ยัง render ไม่ทัน ทำให้เลข 3 ไม่ขึ้น
        yield return null;
        yield return new WaitForEndOfFrame();

        // นับ 3 -> 2 -> 1 ด้วย sprite แทน text
        Sprite[] countdownSprites = { countdown3Sprite, countdown2Sprite, countdown1Sprite };

        for (int i = 0; i < countdownSprites.Length; i++)
        {
            ShowCountdownSprite(countdownSprites[i]);
            Canvas.ForceUpdateCanvases();
            SFXManager.Instance?.PlayCountdown();
            yield return new WaitForSeconds(1f);
        }

        if (countdownImage) countdownImage.gameObject.SetActive(false);
        countdownText.text = "";
    }

    void ShowCountdownSprite(Sprite sprite)
    {
        countdownText.text = "";

        if (countdownImage == null || sprite == null) return;

        countdownImage.sprite = sprite;
        countdownImage.gameObject.SetActive(true);
    }

    void OnNewChallenge(GestureData g)
    {
        gestureNameText.text = "";
        feedbackImage.gameObject.SetActive(false);
        timerBar.fillAmount = 0f;
    }

    void OnChallengeStart(GestureData g)
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

    void OnScoreChanged(int s) => scoreText.text = $"{s}";

    public void OnStartButton() => GameManager.Instance.StartGame();
    public void OnRestartButton() => GameManager.Instance.RestartGame();


}