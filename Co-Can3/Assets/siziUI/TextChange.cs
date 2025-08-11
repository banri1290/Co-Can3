using UnityEngine;
using TMPro;

public class SlotController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText; // 真ん中のテキスト
    [SerializeField] private string[] texts; // 切り替え候補
    private int currentIndex = 0;

    // 次のテキストへ
    public void NextItem()
    {
        currentIndex++;
        if (currentIndex >= texts.Length)
        {
            currentIndex = 0; // 最初に戻る
        }
        UpdateText();
    }

    // 前のテキストへ
    public void PreviousItem()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = texts.Length - 1; // 最後に戻る
        }
        UpdateText();
    }

    // テキスト更新
    private void UpdateText()
    {
        displayText.text = texts[currentIndex];
    }

    // 最初に表示
    private void Start()
    {
        UpdateText();
    }
}