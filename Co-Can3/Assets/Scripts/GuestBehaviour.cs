using UnityEngine;

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

    public int ID => id;
    public Status CurrentStatus => status;

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
    }

    public void Init(int guestId)
    {
        id = guestId;
        isWaiting = false;
        waitTimer = 0f;
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
    }

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
        if (score <= 5) return "😡";
        if (score <= 10) return "🤨";
        if (score <= 20) return "😑";
        if (score <= 25) return "😄";
        return "😆";
    }
    // ▲▲ ここまで追記 ▲▲
}