using System.Collections;
using TMPro;
using UnityEngine;

public class GestureFeedbackUI : MonoBehaviour
{
    [Header("References")]
    public GestureDetector detector;
    public TextMeshProUGUI feedbackText;
    public ParticleSystem successParticles;

    [Header("Settings")]
    public float fadeOutTime = 1.5f;

    Coroutine _fadeRoutine;

    void OnEnable()
    {
        detector.OnGestureDetected += OnDetected;
        detector.OnGestureLost += OnLost;
    }
    void OnDisable()
    {
        detector.OnGestureDetected -= OnDetected;
        detector.OnGestureLost -= OnLost;
    }

    void OnDetected(GestureData g)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        feedbackText.text = $"✓  {g.gestureName}";
        //feedbackText.color = new Color(0.3f, 1f, 0.6f, 1f);
        successParticles?.Play();
        _fadeRoutine = StartCoroutine(FadeOut(feedbackText, fadeOutTime));
    }

    void OnLost()
    {
        feedbackText.text = "";
    }

    IEnumerator FadeOut(TextMeshProUGUI tmp, float duration)
    {
        yield return new WaitForSeconds(0.5f);
        float t = 0f;
        Color start = tmp.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            tmp.color = new Color(start.r, start.g, start.b, 1f - t / duration);
            yield return null;
        }
        tmp.text = "";
    }
}