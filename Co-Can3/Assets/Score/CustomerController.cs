using UnityEngine;

public class CustomerController : MonoBehaviour
{
    public Customer customer;                       // この客のデータ
    public CookingCommandBehaviour commandUI;       // UIスクリプト
    public CookingScoreManager scoreManager;        // スコア計算クラス

    public Dish currentDish = new Dish();           // 今回の料理情報

    void OnMouseDown()
    {
        // 1. 選択中の材料をDishに反映
        currentDish.Ingredients.Clear();
        for (int i = 0; i < commandUI.CommandCount; i++)
        {
            string ingredientName = commandUI.CurrentChobinUIIndex == i 
                ? commandUI.CurrentChobinUIIndex.ToString() // ここはUpdateMaterialTextで管理しているテキスト名に置き換え
                : "";
            if (!string.IsNullOrEmpty(ingredientName))
                currentDish.Ingredients.Add(ingredientName);
        }

        // 2. 調理時間や工程は仮でセット
        currentDish.CookTime = 40f; // デモ用
        currentDish.Steps = 3;

        // 3. スコア計算
        int score = scoreManager.CalculateScore(customer, currentDish);
        string face = scoreManager.GetEvaluationFace(score);

        Debug.Log(customer.customerName + " のスコア: " + score + " 評価: " + face);
    }
}