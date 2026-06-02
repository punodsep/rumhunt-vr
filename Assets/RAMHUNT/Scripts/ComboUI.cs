using System.Collections;
using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    public TextMeshProUGUI comboText;

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
        comboText.text = $"x{multiplier}";

        yield return new WaitForSeconds(0.8f);

        // fade out
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            yield return null;
        }

        comboText.text = "";
    }
}