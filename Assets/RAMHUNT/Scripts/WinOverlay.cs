using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class WinOverlay : MonoBehaviour
{
    [Header("References")]
    public RawImage overlayImage;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public GameObject jumpscareCanvas;
    public Image blackOverlay;     // ← Image สีดำคลุมทั้งหมด

    [Header("Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.8f;

    void Start()
    {
        overlayImage.color = new Color(1f, 1f, 1f, 0f);
        blackOverlay.color = new Color(0f, 0f, 0f, 0f);
        jumpscareCanvas.SetActive(false);

        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameState s, bool playerWon)
    {
        if (s == GameState.GameOver && playerWon)
            GameManager.Instance.StartCoroutine(PlayJumpscare());
    }

    IEnumerator PlayJumpscare()
    {
        jumpscareCanvas.SetActive(true);
        overlayImage.color = new Color(1f, 1f, 1f, 0f);
        blackOverlay.color = new Color(0f, 0f, 0f, 0f);

        // 1. Fade black เข้าก่อน
        yield return GameManager.Instance.StartCoroutine(FadeBlack(0f, 1f, fadeInDuration));

        // 2. เตรียม video
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        overlayImage.texture = videoPlayer.texture;

        // 3. Fade video เข้า
        videoPlayer.Play();
        if (audioSource) audioSource.Play();
        yield return GameManager.Instance.StartCoroutine(Fade(0f, 1f, fadeInDuration));

        // 4. รอ video จบ
        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        // 5. Fade video ออกก่อน
        yield return GameManager.Instance.StartCoroutine(Fade(1f, 0f, fadeOutDuration));

        // 6. Fade black ออก
        yield return GameManager.Instance.StartCoroutine(FadeBlack(1f, 0f, fadeOutDuration));

        jumpscareCanvas.SetActive(false);
    }

    // Fade เฉพาะ video
    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            overlayImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        overlayImage.color = new Color(1f, 1f, 1f, to);
    }

    // Fade เฉพาะ black
    IEnumerator FadeBlack(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            blackOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        blackOverlay.color = new Color(0f, 0f, 0f, to);
    }

    // Fade ทั้ง video และ black พร้อมกัน
    IEnumerator FadeAll(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            overlayImage.color = new Color(1f, 1f, 1f, alpha);
            blackOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        overlayImage.color = new Color(1f, 1f, 1f, to);
        blackOverlay.color = new Color(0f, 0f, 0f, to);
    }
}