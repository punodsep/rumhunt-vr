using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum GameState { Idle, Countdown, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Rules")]
    public int playerMaxHP = 5;
    public int ghostMaxHP = 10;

    public event Action<GameState, bool> OnStateChanged;
    public event Action<int, int> OnPlayerHPChanged;
    public event Action<int, int> OnGhostHPChanged;
    public event Action<int> OnScoreChanged;

    public GameState CurrentState { get; private set; }
    public int Score { get; private set; }
    public int PlayerHP { get; private set; }
    public int GhostHP { get; private set; }
    public int HighScore { get; private set; }

    const string HighScoreKey = "RAMHUNT_HighScore";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    void Start() => SetState(GameState.Idle);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetHighScore();
    }

    // ── Start / End ──────────────────────────────────────────────
    public void StartGame()
    {
        Score = 0;
        PlayerHP = playerMaxHP;
        GhostHP = ghostMaxHP;

        OnScoreChanged?.Invoke(Score);
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);
        OnGhostHPChanged?.Invoke(GhostHP, ghostMaxHP);

        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        SetState(GameState.Countdown);

        while (!AudioManager.Instance.IsOpeningPlaying)
            yield return null;

        while (AudioManager.Instance.IsOpeningPlaying)
            yield return null;

        BeginPlaying();
    }

    void BeginPlaying()
    {
        SetState(GameState.Playing);
        GestureChallenge.Instance.StartChallenge();
    }

    // ── Score ────────────────────────────────────────────────────
    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    // ── Damage ───────────────────────────────────────────────────
    public void DamageGhost(int dmg)
    {
        GhostHP = Mathf.Max(0, GhostHP - dmg);
        OnGhostHPChanged?.Invoke(GhostHP, ghostMaxHP);
        if (GhostHP <= 0) EndGame(true);
    }

    public void DamagePlayer(int dmg = 1)
    {
        PlayerHP = Mathf.Max(0, PlayerHP - dmg);
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);
        if (PlayerHP <= 0) EndGame(false);
    }

    // ── End ──────────────────────────────────────────────────────
    void EndGame(bool playerWon)
    {
        GestureChallenge.Instance.StopChallenge();

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        SetState(GameState.GameOver, playerWon);
    }

    // ── Reset HighScore ──────────────────────────────────────────
    public void ResetHighScore()
    {
        HighScore = 0;
        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.Save();

        ChallengeUI ui = FindFirstObjectByType<ChallengeUI>();
        if (ui != null)
        {
            ui.highScoreText.text = "0";
            if (ui.newHighScoreText)
                ui.newHighScoreText.gameObject.SetActive(false);
        }

        Debug.Log("High Score Reset");
    }

    // ── Restart ──────────────────────────────────────────────────
    public void RestartGame()
    {
        Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SetState(GameState s, bool playerWon = false)
    {
        CurrentState = s;
        OnStateChanged?.Invoke(s, playerWon);
    }
}