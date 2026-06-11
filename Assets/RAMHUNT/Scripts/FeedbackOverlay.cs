// FeedbackOverlay.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackOverlay : MonoBehaviour
{
    [Header("References")]
    public Image overlayImage;
    public Transform headTransform;   // CenterEyeAnchor

    [Header("Sprites")]
    public Sprite perfectSprite;
    public Sprite goodSprite;
    public Sprite missSprite;

    [Header("Settings")]
    public float displayDuration = 2f;
    public float fadeDuration = 0.4f;
    public float distanceFromEye = 1.5f;
    public float heightOffset = 0f;

    Coroutine _routine;

    void Start()
    {
        overlayImage.gameObject.SetActive(false);
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;
    }

    void OnDestroy()
    {
        if (GestureChallenge.Instance != null)
            GestureChallenge.Instance.OnRoundEnd -= OnRoundEnd;
    }

    void LateUpdate()
    {
        if (!overlayImage.gameObject.activeSelf) return;
        if (headTransform == null) return;

        // ติดหน้าผู้เล่นตลอด
        Vector3 forward = headTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        transform.position = headTransform.position
                           + forward * distanceFromEye
                           + Vector3.up * heightOffset;

        transform.rotation = Quaternion.LookRotation(
            transform.position - headTransform.position);
    }

    void OnRoundEnd(ScoreGrade grade, int combo, int multiplier)
    {
        Sprite sprite = grade switch
        {
            ScoreGrade.Perfect => perfectSprite,
            ScoreGrade.Good => goodSprite,
            ScoreGrade.Miss => missSprite,
            _ => null
        };

        if (sprite == null) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowOverlay(sprite));
    }

    IEnumerator ShowOverlay(Sprite sprite)
    {
        // ตั้งค่า
        overlayImage.sprite = sprite;
        overlayImage.color = new Color(1f, 1f, 1f, 1f);
        overlayImage.gameObject.SetActive(true);

        // แสดงค้างไว้
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            overlayImage.color = new Color(1f, 1f, 1f, 1f - t / fadeDuration);
            yield return null;
        }

        overlayImage.gameObject.SetActive(false);
    }
}