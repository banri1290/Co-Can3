using UnityEngine;

public class CookingScoreCalclater : GameSystem
{
    [Header("スコア設定")]
    [Tooltip("味の好みによる基本スコア")]
    [SerializeField] private int tasteScore = 5;

    public override bool CheckSettings()
    {
        bool allSettingsAreCorrect = true;
        return allSettingsAreCorrect;
    }

    public void Init()
    {
        Debug.Log("CookingScoreCalclaterの初期化が完了しました。");
    }

    public int CalculateScore(Dish dish)
    {
        int score = 0;
        return score;
    }
}