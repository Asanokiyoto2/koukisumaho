
using UnityEngine;
using UnityEngine.UI;

public class AllyPartyUI : MonoBehaviour
{
    [Header("味方アイコンの表示先（5個）")]
    public Image[] allyIcons = new Image[5];

    [Header("味方キャラクターの画像（5個）")]
    public Sprite[] allySprites = new Sprite[5];

    private void Start()
    {
        RefreshIcons();
    }

    public void RefreshIcons()
    {
        if (allyIcons == null || allySprites == null)
        {
            Debug.LogError("味方アイコンの配列が設定されていません。");
            return;
        }

        if (allyIcons.Length != 5 || allySprites.Length != 5)
        {
            Debug.LogError("味方アイコンと画像はそれぞれ5個必要です。");
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            if (allyIcons[i] == null)
            {
                Debug.LogError("AllyIcon" + (i + 1) + " が未設定です。");
                continue;
            }

            allyIcons[i].sprite = allySprites[i];
            allyIcons[i].preserveAspect = true;
            allyIcons[i].enabled = allySprites[i] != null;
        }
    }
}

