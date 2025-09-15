using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public string customerName;
    public List<string> Likes;         // 好きな食材
    public List<string> Dislikes;      // 嫌いな食材
    public string EmotionIngredient;   // 感情に対応する食材
}