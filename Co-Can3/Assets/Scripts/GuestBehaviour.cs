using UnityEngine;

public class GuestBehaviour : MonoBehaviour
{
    public enum Status
    {
        None = -1, // –¢İ’è
        Entering = 0, // “ü“X’†
        WaitingOrder = 1, // ’•¶‘Ò‚¿
        Ordering = 2, // ’•¶’†
        WaitingDish = 3,  // ‘Ò‹@’†
        GotDish = 4, // ’•¶ó‚¯æ‚èÏ‚İ
    }

    public class GuestEvent : UnityEngine.Events.UnityEvent<int> { }

    [SerializeField] private float speed;

    private GuestEvent guestEvent = new();

    private int id;
    private Status status = Status.None;
    private Vector3 targetPosition;
    private bool hasMovedFlag = false;

    public int ID => id;
    public Status CurrentStatus => status;

    public GuestEvent GuestEventInstance => guestEvent;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Init(int guestId)
    {
        id = guestId;
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

    public void SetState(Status _status)
    {
        status = _status;
        switch (status)
        {
            case Status.Entering:
                // “ü“X’†‚Ìˆ—
                break;
            case Status.WaitingOrder:
                // ’•¶‘Ò‚¿‚Ìˆ—
                break;
            case Status.Ordering:
                // ’•¶’†‚Ìˆ—
                break;
            case Status.WaitingDish:
                // ‘Ò‹@’†‚Ìˆ—
                break;
            case Status.GotDish:
                // ’•¶ó‚¯æ‚èÏ‚İ‚Ìˆ—
                break;
        }
    }
}
