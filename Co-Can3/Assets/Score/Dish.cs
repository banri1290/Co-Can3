using System.Collections.Generic;

public class Dish
{
    // 選んだ材料
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

        // 調理工程に応じて加点（例: 1工程につき2点）
        score += Steps * 2;

        // 調理時間による加点（短いほど加点）
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