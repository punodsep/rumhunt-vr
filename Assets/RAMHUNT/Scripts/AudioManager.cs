using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Menu BGM")]
    public AudioSource menuBGM_A;
    public AudioSource menuBGM_B;

    [Header("Game BGM")]
    public AudioSource openingBGM;  // BGM_Part_open.wav
    public AudioSource loopBGM;     // BGM_Part_loop.wav

    [Header("Settings")]
    public float fadeOutDuration = 2f;

    public bool IsOpeningPlaying =>
    openingBGM != null &&
    openingBGM.isPlaying;

    public float GetOpeningDuration() =>
        openingBGM.clip != null
            ? openingBGM.clip.length
            : 9.17f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // เล่น menu BGM ตั้งแต่แรก
        menuBGM_A.loop = true;
        menuBGM_B.loop = true;
        menuBGM_A.Play();
        menuBGM_B.Play();

        GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameState s, bool playerWon)
    {
        switch (s)
        {
            case GameState.Countdown:
                StartCoroutine(StartGameMusic());
                break;
            case GameState.GameOver:
                StartCoroutine(StopGameMusic());
                break;
        }
    }

    IEnumerator StartGameMusic()
    {
        // หยุด menu BGM
        menuBGM_A.Stop();
        menuBGM_B.Stop();

        // เล่น Opening (ไม่ loop)
        openingBGM.loop = false;
        openingBGM.Play();

        // รอ Opening จบ
        yield return new WaitUntil(() => !openingBGM.isPlaying);

        // ต่อด้วย Loop BGM
        loopBGM.loop = true;
        loopBGM.Play();
    }

    IEnumerator StopGameMusic()
    {
        // Fade out loop BGM
        float t = 0f;
        float startVol = loopBGM.volume;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            loopBGM.volume = Mathf.Lerp(startVol, 0f, t / fadeOutDuration);
            yield return null;
        }
        loopBGM.Stop();
        loopBGM.volume = startVol;

        // เล่น menu BGM กลับมา
        menuBGM_A.Play();
        menuBGM_B.Play();
    }

}