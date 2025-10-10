using UnityEngine;

public class CookingScoreCalclater : GameSystem
{
    [Header("スコア設定")]
    [Tooltip("味の好みによる基本スコア")]
    [SerializeField] private int tasteScore = 5;

    [Tooltip("待ち時間が短いほど加点するしきい値（秒）")]
    [SerializeField] private float waitingTimeThreshold = 15f;

    [Tooltip("調理ステップの重み")]
    [SerializeField] private int stepWeight = 3;

    public override bool CheckSettings() => true;

    public void Init()
    {
        Debug.Log("CookingScoreCalclaterの初期化が完了しました。");
    }

    // 🍳 Dish情報をもとにスコアを計算
    public int CalculateScore(Dish dish)
    {
        int score = 0;

        // 🧂 味の基本点
        score += tasteScore;

        // 🥬 材料数による基本点
        score += dish.Ingredients.Count * 5;

        // 🔥 調理工程数による加点
        score += dish.Steps * stepWeight;

        // ⏱ 調理時間（短いほど良い）
        if (dish.CookTime < waitingTimeThreshold)
            score += 10;
        else
            score -= 5;

        // スコアの下限を0に
        score = Mathf.Max(0, score);

        Debug.Log($"【スコア計算】材料:{dish.Ingredients.Count}個 工程:{dish.Steps} 調理時間:{dish.CookTime:F2}秒 → スコア:{score}");

        return score;
    }
}