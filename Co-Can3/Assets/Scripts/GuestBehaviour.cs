using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

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

    public class GuestEvent : UnityEngine.Events.UnityEvent<int> { }

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
    }

    public void SetSpeed(float _speed)
    {
        speed = _speed;
    }

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

  public UnityEvent OnCookingFinished; // ← 追加

    private void Awake()
    {
        if (OnCookingFinished == null)
            OnCookingFinished = new UnityEvent();
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
    [SerializeField] private TMPro.TextMeshProUGUI reactionText; // 頭上に表示するテキスト

    public void ShowReaction(int score)
    {
        if (reactionText == null)
        {
            Debug.LogWarning($"ゲスト {id} にリアクションTextが設定されていません。");
            return;
        }

        string emoji = GetEmoji(score);
        reactionText.text = emoji;
        reactionText.gameObject.SetActive(true);

        // 2秒後に非表示
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
}