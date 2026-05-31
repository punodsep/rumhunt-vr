using System.Collections;
using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    public TextMeshProUGUI comboText;

    public Color combo2Color = new Color(1f, 0.9f, 0.2f);
    public Color combo3Color = new Color(1f, 0.6f, 0.1f);
    public Color combo4Color = new Color(1f, 0.2f, 0.8f);

    Coroutine _anim;

    void Start()
    {
        comboText.text = "";
        GestureChallenge.Instance.OnRoundEnd += OnRoundEnd;
    }

    void OnDestroy()
    {
        if (GestureChallenge.Instance != null)
            GestureChallenge.Instance.OnRoundEnd -= OnRoundEnd;
    }

    void OnRoundEnd(ScoreGrade grade, int combo, int multiplier)
    {
        if (grade != ScoreGrade.Perfect || combo < 2) return;

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(ShowCombo(combo, multiplier));
    }

    IEnumerator ShowCombo(int combo, int multiplier)
    {
        Color color = combo >= 4 ? combo4Color
                    : combo >= 3 ? combo3Color
                    : combo2Color;

        comboText.text = $"COMBO x{multiplier}";
        comboText.color = new Color(color.r, color.g, color.b, 1f);

        yield return new WaitForSeconds(0.8f);

        // fade out
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            comboText.color = new Color(color.r, color.g, color.b, 1f - t / 0.4f);
            yield return null;
        }

        comboText.text = "";
    }
}