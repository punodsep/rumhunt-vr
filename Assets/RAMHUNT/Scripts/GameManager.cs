using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Idle, Countdown, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Rules")]
    public int startHP = 3;        // HP ผู้เล่น
    public float roundDuration = 60f;      // เวลาทั้งหมด (วินาที)

    public event Action<GameState> OnStateChanged;
    public event Action<int> OnHPChanged;
    public event Action<int> OnScoreChanged;

    public GameState CurrentState { get; private set; }
    public int Score { get; private set; }
    public int HP { get; private set; }
    public float TimeRemaining { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => SetState(GameState.Idle);

    void Update()
    {
        if (CurrentState != GameState.Playing) return;
        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f) EndGame(false);
    }

    public void StartGame()
    {
        Score = 0;
        HP = startHP;
        TimeRemaining = roundDuration;

        OnScoreChanged?.Invoke(Score);
        OnHPChanged?.Invoke(HP);
        SetState(GameState.Countdown);

        // countdown 3 วิแล้วเริ่ม
        Invoke(nameof(BeginPlaying), 3f);
    }

    void BeginPlaying()
    {
        SetState(GameState.Playing);
        GestureChallenge.Instance.StartChallenge();
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void LoseHP()
    {
        HP = Mathf.Max(0, HP - 1);
        OnHPChanged?.Invoke(HP);
        if (HP <= 0) EndGame(false);
    }

    void EndGame(bool win)
    {
        GestureChallenge.Instance.StopChallenge();
        SetState(GameState.GameOver);
    }

    public void RestartGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    void SetState(GameState s)
    {
        CurrentState = s;
        OnStateChanged?.Invoke(s);
    }
}