using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GuestCtrl : GameSystem
{
    [Header("参照設定")]
    [Tooltip("生成する客のプレハブ")]
    [SerializeField] private GuestBehaviour[] guestPrefabs;
    [Tooltip("客の出現場所")]
    [SerializeField] private Transform spawnSpot;
    [Tooltip("客が注文をする場所")]
    [SerializeField] private Transform orderingSpot;
    [Tooltip("客が料理を待つ場所")]
    [SerializeField] private Transform waitingServeSpot;
    [Tooltip("客が退店する場所")]
    [SerializeField] private Transform exitSpot;

    [Header("客の出現と待機設定")]
    [Tooltip("注文待ちの列における客同士の間隔")]
    [SerializeField] private Vector3 waitingOrderOffset = new Vector3(0, 0, 1);
    [Tooltip("提供待ちの列における客同士の間隔")]
    [SerializeField] private Vector3 waitingServeOffset = new(0, 0, 1);
    [Tooltip("待機中の客が向く方向")]
    [SerializeField] private Vector3 waitingDirection = new(1, 0, 0);
    [Tooltip("客が出現する最短間隔（秒）")]
    [SerializeField] private float SpawnIntervalMin;
    [Tooltip("客が出現する最長間隔（秒）")]
    [SerializeField] private float SpawnIntervalMax;
    [Tooltip("ゲーム中に登場する客の総数")]
    [SerializeField] private int totalGuestNum;
    [Tooltip("店内に同時に存在できる客の最大数")]
    [SerializeField] private int maxGuestNum;

    [Header("客のパラメータ設定")]
    [Tooltip("客の移動速度")]
    [SerializeField] private float speed = 1f;

    List<GuestBehaviour> guestList = new();

    private bool isActive = false;
    private float spawnTimer = 0f;
<<<<<<< Updated upstream
=======
    private bool hasGuestWaitingOrder = false;
>>>>>>> Stashed changes

    private int guestComeCounter;
    private int guestOrderCounter;
    private int guestExitCounter;
<<<<<<< Updated upstream
    private int currentGuestNum;

    private UnityEvent hasGuestWaitingForOrderEvent = new();
    private UnityEvent leftGuestWaitingForOrderEvent = new();

    public UnityEvent HasGuestWaitingForOrder => hasGuestWaitingForOrderEvent;
    public UnityEvent LeftGuestWaitingForOrder => leftGuestWaitingForOrderEvent;
    public int CurrentGuestNum => currentGuestNum;
=======
    private int guestHasCookedCounter;

    private UnityEvent hasComeGuestEvent = new();
    private UnityEvent allGuestExitEvent = new();

    public UnityEvent HasComeGuest => hasComeGuestEvent;
    public UnityEvent AllGuestExit => allGuestExitEvent;

    public int WaitingGuestNum => guestOrderCounter - guestExitCounter;
    public bool HasGuestWaitingOrder => hasGuestWaitingOrder;
>>>>>>> Stashed changes

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            CountSpawnTimer();
        }
    }

    public override bool CheckSettings()
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
<<<<<<< Updated upstream
        if (guestPrefabs == null || guestPrefabs.Length == 0)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("guestPrefabsが設定されていません。");
=======
        if (guestPrefabs.Length == 0)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("guestPrefabsが設定されていません。");
        }
        else
        {
            for (int i = 0; i < guestPrefabs.Length; i++)
            {
                if (guestPrefabs[i] == null)
                {
                    AllSettingsAreCorrect = false;
                    Debug.LogError("guestPrefabs[" + i + "]が設定されていません。");
                }
            }
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        currentGuestNum = 0;
=======
        guestHasCookedCounter = 0;
        hasGuestWaitingOrder = false;
>>>>>>> Stashed changes

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
        int prefabIndex = Random.Range(0, guestPrefabs.Length);
        GuestBehaviour newGuest = Instantiate(guestPrefabs[prefabIndex], spawnSpot.position, Quaternion.identity);
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
    /// 注文を受け付けた時に呼び出されるメソッド
    /// </summary>
    public void ReceiveOrder()
    {
<<<<<<< Updated upstream
        leftGuestWaitingForOrderEvent.Invoke();
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
        guestHasCookedCounter++;
>>>>>>> Stashed changes
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
        currentGuestNum--;
    }

    public GuestBehaviour GetServedGuest()
    {
<<<<<<< Updated upstream
          int index = guestExitCounter - 1;
    if (index < 0 || index >= guestList.Count)
    {
        Debug.LogWarning($"[GuestCtrl] ServedGuestを取得できません。guestExitCounter={guestExitCounter}, guestList.Count={guestList.Count}");
        return null;
=======
        return guestList[guestExitCounter - 1];
    }

    public GuestBehaviour GetOrderGuest()
    {
        return guestList[guestOrderCounter];
    }

    public void InformCookingQuit()
    {
        guestHasCookedCounter--;
    }


    private void GuestOnCounter(int guestId)
    {
        GuestBehaviour guest = guestList[guestId];
        if (guestId == guestOrderCounter)
        {
            hasGuestWaitingOrder = true;
            hasComeGuestEvent.Invoke();
            guest.SetState(GuestBehaviour.Status.Ordering);
        }
        else if (guest.CurrentStatus == GuestBehaviour.Status.Entering)
        {
            guest.SetState(GuestBehaviour.Status.WaitingOrder);
        }
>>>>>>> Stashed changes
    }
    return guestList[index];
}

    private void SendGuestMessage(int guestId)
    {
        GuestBehaviour guest = guestList[guestId];
        guest.SetDirection(waitingDirection);
        GuestBehaviour.Status status = guest.CurrentStatus;
        switch (status)
        {
            case GuestBehaviour.Status.None:
                break;
            case GuestBehaviour.Status.Entering:
<<<<<<< Updated upstream
                if (guestId == guestOrderCounter)
                {
                    hasGuestWaitingForOrderEvent.Invoke();
                    guest.SetState(GuestBehaviour.Status.Ordering);
                }
                else
                {
                    guest.SetState(GuestBehaviour.Status.WaitingOrder);
                }
                guest.StartWaiting();
                currentGuestNum++;
                break;
            case GuestBehaviour.Status.WaitingOrder:
                if (guestId == guestOrderCounter)
                {
                    hasGuestWaitingForOrderEvent.Invoke();
                    guest.SetState(GuestBehaviour.Status.Ordering);
                }
=======
                GuestOnCounter(guestId);
                guest.StartWaiting();
                break;
            case GuestBehaviour.Status.WaitingOrder:
                GuestOnCounter(guestId);
>>>>>>> Stashed changes
                break;
            case GuestBehaviour.Status.Ordering:
                break;
            case GuestBehaviour.Status.WaitingDish:
                break;
            case GuestBehaviour.Status.GotDish:
                if (guestId < guestExitCounter)
                {
                    Destroy(guest.gameObject);
                    guest = null;
                    if (guestExitCounter == totalGuestNum)
                        allGuestExitEvent.Invoke();
                }
                break;
            default:
                break;
        }
    }
}