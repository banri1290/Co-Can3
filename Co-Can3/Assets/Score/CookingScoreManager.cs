using UnityEngine;

public class CookingScoreManager : MonoBehaviour
{
    public int CalculateScore(Customer customer, Dish dish)
    {
        int score = 0;

        // 1. 好き・嫌い食材
        if (customer.Likes.Exists(x => dish.Ingredients.Contains(x)))
            score += 5;
        if (customer.Dislikes.Exists(x => dish.Ingredients.Contains(x)))
            score -= 5;

        // 2. 調理時間
        if (dish.CookTime < 45f)
            score += 10;
        else if (dish.CookTime > 45f)
            score -= 3;

        // 3. 感情に対応する食材
        if (dish.Ingredients.Contains(customer.EmotionIngredient))
            score += 5;
        else
            score -= 5;

        // 4. 調理工程
        switch (dish.Steps)
        {
            case 3: score += 10; break;
            case 2: score += 5; break;
            case 1: score += 0; break;
            case 0: score -= 10; break;
        }

        return score;
    }

    public string GetEvaluationFace(int score)
    {
        if (score >= 26) return "😄";
        if (score >= 21) return "😊";
        if (score >= 11) return "😑";
        if (score >= 6) return "😞";
        return "😡";
    }
}
