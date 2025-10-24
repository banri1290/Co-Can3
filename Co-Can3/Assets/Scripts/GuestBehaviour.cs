using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;

public class GuestBehaviour : MonoBehaviour
{
    public enum Status
    {
        None = -1, // 未設定
        Entering = 0, // 入店中
        WaitingOrder = 1, // 注文待ち
        Ordering = 2, // 注文中
        WaitingDish = 3,  // 提供待ち
        GotDish = 4, // 料理を受け取り退店中
    }

    public class GuestEvent : UnityEvent<int> { }
    [Tooltip("客の移動速度。GuestCtrlから設定されます。")]
    [SerializeField] private float speed;

    private GuestEvent guestEvent = new();

    private int id;
    private Status status = Status.None;
    private Vector3 targetPosition;
    private bool hasMovedFlag = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

      private bool isCooking = false;
    private float cookingStartTime = 0f;
    private float cookingElapsed = 0f;

    public int ID => id;
    public Status CurrentStatus => status;

    public List<string> LikedIngredients = new List<string>();    // 好きな食材
public List<string> HatedIngredients = new List<string>();    // 嫌いな食材
public List<string> EmotionIngredients = new List<string>();  // 感情対応食材

    public GuestEvent GuestEventInstance => guestEvent;
    public float WaitingTimer => waitTimer;

   [Header("🟡 絵文字関連")]
    [SerializeField] private TextMeshProUGUI reactionText;
    [SerializeField] private string[] orderEmojis = { "😅", "🍣", "🍜", "🍕", "🥗" };

    public UnityEvent OnCookingFinished;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if (isWaiting) CountWaitingTime();
           if (isCooking)
        UpdateCookingTime(); // ← ここを追加
    }

    public void Init(int guestId)
    {
        id = guestId;
         StopWaiting();    
        isWaiting = false;
         StopCooking(); 
        waitTimer = 0f;
           hasMovedFlag = false; 
        SetState(Status.Entering);
          // 🍽️ 客IDに応じて絵文字を決定
      if (orderEmojis != null && orderEmojis.Length > 0)
        {
            int index = guestId % orderEmojis.Length;
            selectedOrderEmoji = orderEmojis[index];
        }
        else
        {
            selectedOrderEmoji = "😅"; // デフォルト
        }
    }

    public void SetSpeed(float _speed) => speed = _speed;

    public void SetDirection(Vector3 _direction)
    {
        Vector3 direction = _direction.normalized;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0); // Adjust rotation to face the target
    }

    public void SetDestination(Vector3 target)
    {
        targetPosition = target;
        hasMovedFlag = false;

        Vector3 direction = targetPosition - transform.position;
        SetDirection(direction);
    }

    private void Move()
    {
        if (hasMovedFlag) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (transform.position == targetPosition)
        {
            hasMovedFlag = true;
            guestEvent.Invoke(id);
        }
    }
    
    private void CountWaitingTime()
    {
        waitTimer += Time.deltaTime;
    }

    public void StartWaiting()
    {
        isWaiting = true;
        waitTimer = 0f;
              Debug.Log($"[GuestBehaviour] Guest {id} started waiting.");
    }
public void StopWaiting()
{
    isWaiting = false;
    waitTimer = 0f; // ✅ 念のためリセット
       Debug.Log($"[GuestBehaviour] Guest {id} stopped waiting (reset timer).");
}
    // 🍳 ====== ここから調理時間管理部分 ======
    public void StartCooking()
    {
        isCooking = true;
        cookingStartTime = Time.realtimeSinceStartup;
        cookingElapsed = 0f;
        Debug.Log($"🍳 Guest {id} started cooking at {cookingStartTime}");
    }

    private void UpdateCookingTime()
    {
        cookingElapsed = Time.realtimeSinceStartup - cookingStartTime;
    }

    public float GetCookingTime()
    {
        if (isCooking)
        return Time.realtimeSinceStartup - cookingStartTime; // 調理中は現在時刻との差
    return cookingElapsed; // 停止後は確定値
    }

    private void Awake()
    {
        if (OnCookingFinished == null)
            OnCookingFinished = new UnityEvent();
                // 🟡 最初は頭上の絵文字を非表示
    if (reactionText != null)
        reactionText.gameObject.SetActive(false);
    }

    public void StopCooking()
    {
        if (isCooking)
        {
             isCooking = false;
        cookingElapsed = Time.realtimeSinceStartup - cookingStartTime; // ✅ 停止時点で確定
        Debug.Log($"🍽️ Guest {id} finished cooking. Total time: {cookingElapsed:F2}秒");
           OnCookingFinished?.Invoke(); // 完了イベント
        }
    }
    // 🍳 ====== ここまで追加 ======
    public void SetState(Status _status)
    {
        status = _status;
        switch (status)
        {
            case Status.Entering:
                // 入店中の処理
                break;
            case Status.WaitingOrder:
            ShowOrderEmoji(); // 🍔 注文絵文字を出す
                break;
            case Status.Ordering:
                break;
            case Status.WaitingDish:
                // 提供待ちの処理
                break;
            case Status.GotDish:
                isWaiting = false;
                  StopCooking(); // ✅ 料理完了時に止める
                break;
        }
    }
        // ▼▼ ここから追記 ▼▼

    public void ShowReaction(int score)
     {
        if (reactionText == null) return;
        string emoji = GetEmoji(score);
        reactionText.text = emoji;
        reactionText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideReaction));
        Invoke(nameof(HideReaction), 2f);
    }

    private void HideReaction()
    {
        if (reactionText != null)
            reactionText.gameObject.SetActive(false);
    }

    private string GetEmoji(int score)
    {
        if (score <= 5) return "😅";
        if (score <= 10) return "😀";
        if (score <= 20) return "😄";
        if (score <= 25) return "😁";
        return "😆";
    }
    // ▲▲ ここまで追記 ▲▲
    // 🍽️ ====== 注文絵文字管理 ======
[SerializeField] private string[] orderEmoji = { "😅", "😀", "😄", "😁" };  // デフォルト注文絵文字
private string selectedOrderEmoji; // この客が使う絵文字
/// <summary>
/// 注文開始時に頭上に絵文字を表示
/// </summary>
public void ShowOrderEmoji(string emoji = null)
{
    if (reactionText == null)
    {
        Debug.LogWarning($"ゲスト {id} にリアクションTextが設定されていません。");
        return;
    }
   CancelInvoke(nameof(HideReaction)); // 🔸 以前の非表示予約をキャンセル
    reactionText.gameObject.SetActive(true);
      // emoji 引数があればそれを使い、なければ selectedOrderEmoji を使う
    reactionText.text = emoji ?? selectedOrderEmoji;
}

/// <summary>
/// 料理を渡したあとにリアクション絵文字へ切り替える
/// </summary>
public void ShowReactionAndHideOrder(int score)
{
    if (reactionText == null) return;

    string emoji = GetEmoji(score);
    reactionText.text = emoji;
    reactionText.gameObject.SetActive(true);

    // 2秒後に非表示
    CancelInvoke(nameof(HideReaction));
    Invoke(nameof(HideReaction), 2f);
}
}
