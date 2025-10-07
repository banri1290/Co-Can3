using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Material
{
    [Tooltip("材料の名前")]
    [SerializeField] private string name; // 材料の名前
    [Tooltip("材料の画像")]
    [SerializeField] private Sprite sprite; // 材料の画像

    public string Name => name; // 材料の名前を取得
    public Sprite Sprite => sprite; // 材料の画像を取得
}
[System.Serializable]
public struct Action
{
    [Tooltip("調理方法の名前（例：切る、焼く）")]
    [SerializeField] private string name;
    [Tooltip("調理法の画像")]
    [SerializeField] private Sprite sprite;
    [Tooltip("この調理を行う場所のTransform")]
    [SerializeField] private Transform kitchinSpot;

    public string Name => name; // 調理方法の名前を取得
    public Sprite Sprite => sprite; // 調理法の画像を取得
    public Transform KitchinSpot => kitchinSpot; //調理する場所を取得
}

public class Dish
{
    // 完成した材料
    public List<string> Ingredients { get; private set; }

    // 調理工程数
    public int Steps { get; set; }

    // 調理時間（秒）
    public float CookTime { get; set; }

    // コンストラクタ
    public Dish()
    {
        Ingredients = new List<string>();
        Steps = 0;
        CookTime = 0f;
    }

    // 材料を追加するメソッド
    public void AddIngredient(string ingredient)
    {
        if (!Ingredients.Contains(ingredient))
        {
            Ingredients.Add(ingredient);
        }
    }

    // スコアを計算して返す
    public int CalculateScore()
    {
        int score = 0;

        // 材料の数に応じて加点（例: 1材料につき5点）
        score += Ingredients.Count * 5;

        // 調理工程数に応じて加点（例: 1工程につき2点）
        score += Steps * 2;

        // 調理時間に応じて加点（短いほど高得点）
        if (CookTime < 10f) score += 5;
        else if (CookTime < 20f) score += 3;

        return score;
    }

    // スコア計算の拡張例: 特定の材料でボーナス
    public int CalculateScoreWithBonus(List<string> bonusIngredients)
    {
        int score = CalculateScore();
        foreach (var bonus in bonusIngredients)
        {
            if (Ingredients.Contains(bonus))
            {
                score += 2; // ボーナス加点
            }
        }
        return score;
    }
}