using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GuestCtrl : MonoBehaviour
{
    [SerializeField] GuestBehaviour GuestPrafab;
    [SerializeField] Transform spawnSpot;
    [SerializeField] Transform orderingSpot;
    [SerializeField] Transform waitingServeSpot;
    [SerializeField] Transform exitSpot;
    [SerializeField] Vector3 waitingOrderOffset = new Vector3(0, 0, 1);
    [SerializeField] Vector3 waitingServeOffset = new(0, 0, 1); // 注文待ちの客の待機場所のオフセット
    [SerializeField] Vector3 waitingDirection = new(1, 0, 0); // 注文待ちの客の向き
    [SerializeField] float SpawnIntervalMin;
    [SerializeField] float SpawnIntervalMax;
    [SerializeField] int totalGuestNum; // 呼び出す客の総数
    [SerializeField] int maxGuestNum; // 同時に存在できる客の最大数

    [Header("客の設定")]
    [SerializeField] private float speed = 1f; // 客の移動速度

    List<GuestBehaviour> guestList = new();

    private bool isActive = false;
    private float spawnTimer = 0f;

    private int guestComeCounter;
    private int guestOrderCounter;
    private int guestExitCounter;

    private UnityEvent checkAllSettings = new();
    private UnityEvent hasGuestWaitingForOrderEvent = new();
    private UnityEvent leftGuestWaitingForOrderEvent = new();

    public UnityEvent CheckAllSettings => checkAllSettings;
    public UnityEvent HasGuestWaitingForOrder => hasGuestWaitingForOrderEvent;
    public UnityEvent LeftGuestWaitingForOrder => leftGuestWaitingForOrderEvent;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            CountSpawnTimer();
        }
    }

    public bool CheckSetings()
    {
        if (SpawnIntervalMin < 0)
        {
            Debug.LogWarning("SpawnIntervalMinの値が不正です。0以上の値に修正します。");
            SpawnIntervalMin = 0;
        }
        if (SpawnIntervalMax < SpawnIntervalMin)
        {
            Debug.LogWarning("SpawnIntervalMaxの値が不正です。SpawnIntervalMinと同じ値に修正します。");
            SpawnIntervalMax = SpawnIntervalMin;
        }

        bool AllSettingsAreCorrect = true;
        if (GuestPrafab == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("GuestPrafabが設定されていません。");
        }
        if (spawnSpot == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("spawnSpotが設定されていません。");
        }
        if (orderingSpot == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("orderingSpotが設定されていません。");
        }
        if (waitingServeSpot == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("waitingServeSpotが設定されていません。");
        }
        if (exitSpot == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("exitSpotが設定されていません。");
        }

        return AllSettingsAreCorrect;
    }

    public void Init()
    {
        isActive = true;
        guestComeCounter = 0;
        guestOrderCounter = 0;
        guestExitCounter = 0;

        Debug.Log("GuestCtrlの初期化が完了しました。");
    }

    private void CountSpawnTimer()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && guestComeCounter < totalGuestNum && guestExitCounter - guestComeCounter < maxGuestNum)
        {
            SpawnGuest();
        }
    }

    private void SpawnGuest()
    {
        GuestBehaviour newGuest = Instantiate(GuestPrafab, spawnSpot.position, Quaternion.identity);
        guestList.Add(newGuest);
        newGuest.SetSpeed(speed);
        newGuest.GuestEventInstance.RemoveAllListeners();
        newGuest.GuestEventInstance.AddListener(SendGuestMessage);
        newGuest.Init(guestComeCounter);

        int differenceCount = guestComeCounter - guestOrderCounter;
        newGuest.SetDestination(orderingSpot.position + waitingOrderOffset * differenceCount);

        guestComeCounter++;
        spawnTimer = Random.Range(SpawnIntervalMin, SpawnIntervalMax);
    }

    /// <summary>
    /// 注文を受け付けたときに呼び出すメゾット
    /// </summary>
    public void ReceiveOrder()
    {
        leftGuestWaitingForOrderEvent.Invoke();
        guestList[guestOrderCounter].SetState(GuestBehaviour.Status.WaitingDish);
        for (int i = guestExitCounter; i < guestOrderCounter + 1; i++)
        {
            int differenceCount = i - guestExitCounter;
            guestList[i].SetDestination(waitingServeSpot.position + waitingServeOffset * differenceCount);
        }
        for (int i = guestOrderCounter + 1; i < guestComeCounter; i++)
        {
            int differenceCount = i - guestOrderCounter - 1;
            guestList[i].SetDestination(orderingSpot.position + waitingOrderOffset * differenceCount);
        }

        guestOrderCounter++;
    }

    public void ServeDish()
    {
        guestList[guestExitCounter].SetState(GuestBehaviour.Status.GotDish);
        guestList[guestExitCounter].SetDestination(exitSpot.position);
        for (int i = guestExitCounter + 1; i < guestOrderCounter; i++)
        {
            int differenceCount = i - guestExitCounter - 1;
            guestList[i].SetDestination(waitingServeSpot.position + waitingServeOffset * differenceCount);
        }
        guestExitCounter++;
    }

    private void SendGuestMessage(int guestId)
    {
        guestList[guestId].SetDirection(waitingDirection);
        GuestBehaviour.Status status = guestList[guestId].CurrentStatus;
        switch (status)
        {
            case GuestBehaviour.Status.None:
                break;
            case GuestBehaviour.Status.Entering:
                if (guestId == guestOrderCounter)
                {
                    hasGuestWaitingForOrderEvent.Invoke();
                    guestList[guestId].SetState(GuestBehaviour.Status.Ordering);
                }
                else
                {
                    guestList[guestId].SetState(GuestBehaviour.Status.WaitingOrder);
                }
                break;
            case GuestBehaviour.Status.WaitingOrder:
                if (guestId == guestOrderCounter)
                {
                    hasGuestWaitingForOrderEvent.Invoke();
                    guestList[guestId].SetState(GuestBehaviour.Status.Ordering);
                }
                break;
            case GuestBehaviour.Status.Ordering:
                break;
            case GuestBehaviour.Status.WaitingDish:
                break;
            case GuestBehaviour.Status.GotDish:
                if (guestId == guestExitCounter)
                {
                    Destroy(guestList[guestId].gameObject);
                    guestList[guestId] = null;
                }
                break;
            default:
                break;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        checkAllSettings.Invoke();
    }
#endif
}