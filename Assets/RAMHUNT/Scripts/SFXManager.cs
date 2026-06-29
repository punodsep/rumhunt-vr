using System.Collections;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("SFX Clips")]
    public AudioClip[] sfxCombo;
    public AudioClip[] sfxGhostAttack;
    public AudioClip sfxGhostDie;
    public AudioClip sfxMiss;
    public AudioClip sfxPerfect;
    public AudioClip sfxPlayerAttack;
    public AudioClip sfxPlayerDie;
    public AudioClip sfxUIClick;
    public AudioClip sfxCountdown;

    [Header("Delay")]
    public float comboDelay = 0.1f;
    public float ghostAttackDelay = 1f;
    public float playerAttackDelay = 1f;

    AudioSource _source;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _source = GetComponent<AudioSource>();
    }

    void Start()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        if (GestureChallenge.Instance != null)
            GestureChallenge.Instance.OnRoundEnd -= OnRoundEnd;
    }

    void OnStateChanged(GameState s, bool playerWon)
    {
        if (s == GameState.GameOver && playerWon)
            Play(sfxGhostDie);

        if (s == GameState.GameOver && !playerWon)
            Play(sfxPlayerDie);
    }

    void OnRoundEnd(ScoreGrade grade, int combo, int multiplier)
    {
        switch (grade)
        {
            case ScoreGrade.Perfect:
                Play(sfxPerfect);

                if (combo >= 2)
                    StartCoroutine(PlayRandomAfterDelay(sfxCombo, comboDelay));

                break;

            case ScoreGrade.Good:
                Play(sfxPerfect);
                break;

            case ScoreGrade.Miss:
                Play(sfxMiss);
                StartCoroutine(PlayRandomAfterDelay(sfxGhostAttack, ghostAttackDelay));
                break;
        }
    }

    IEnumerator PlayAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(clip);
    }

    IEnumerator PlayRandomAfterDelay(AudioClip[] clips, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayRandom(clips);
    }

    public void PlayUIClick() => Play(sfxUIClick);
    public void PlayPlayerAttack() => Play(sfxPlayerAttack);
    public void PlayCountdown() => Play(sfxCountdown);

    // ── Helpers ───────────────────────────────────────────────────
    public void Play(AudioClip clip)
    {
        if (clip == null) return;
        _source.PlayOneShot(clip);
    }

    public void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            _source.PlayOneShot(clip);
    }
}