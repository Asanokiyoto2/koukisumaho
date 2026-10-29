
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text comboText;
    public Text scoreText;

    [Header("コンボ演出")]
    public float comboPopScale = 1.5f;
    public float comboAnimationDuration = 0.2f;
    public float comboDisplayDuration = 0.8f;

    private Coroutine comboCoroutine;
    private Vector3 comboOriginalScale;

    private void Awake()
    {
        if (comboText != null)
        {
            comboOriginalScale = comboText.transform.localScale;
            comboText.gameObject.SetActive(false);
        }
    }

    public void UpdateUI(int combo, int score)
    {
        // コンボ表示は ShowCombo() に任せる
        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }

    public void ShowCombo(int combo)
    {
        if (comboText == null) return;

        if (comboCoroutine != null)
            StopCoroutine(comboCoroutine);

        comboCoroutine = StartCoroutine(ComboAnimation(combo));
    }

    private IEnumerator ComboAnimation(int combo)
    {
        comboText.gameObject.SetActive(true);
        comboText.text = combo + " Combo!";

        RectTransform rect = comboText.rectTransform;
        rect.localScale = comboOriginalScale * 0.5f;

        Color color = comboText.color;
        color.a = 1f;
        comboText.color = color;

        // ポンッと大きくなる
        float time = 0f;

        while (time < comboAnimationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(
                time / comboAnimationDuration
            );

            // 大きくなってから元のサイズに戻る
            float scale = Mathf.Lerp(
                0.5f,
                comboPopScale,
                t
            );

            rect.localScale = comboOriginalScale * scale;
            yield return null;
        }

        rect.localScale = comboOriginalScale;

        // 表示を維持
        yield return new WaitForSeconds(comboDisplayDuration);

        // 徐々に透明にする
        time = 0f;

        while (time < comboAnimationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(
                time / comboAnimationDuration
            );

            color.a = 1f - t;
            comboText.color = color;

            rect.localScale = Vector3.Lerp(
                comboOriginalScale,
                comboOriginalScale * 0.8f,
                t
            );

            yield return null;
        }

        comboText.gameObject.SetActive(false);
        rect.localScale = comboOriginalScale;

        color.a = 1f;
        comboText.color = color;

        comboCoroutine = null;
    }
}

