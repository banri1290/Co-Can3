using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChobinBehaviour : MonoBehaviour
{
    public enum Status
    {
        CommandWaitiating = 0,
        Moving = 1,
        Performing = 2,
        ServingDish = 3,
        BackToWaitingSpot = 4
    }

    [Header("コンポーネント参照")]
    [Tooltip("移動に使用するNavMeshAgent")]
    [SerializeField] private NavMeshAgent navAgent;
    [Tooltip("このチョビンを選択するためのボタンオブジェクト")]
    [SerializeField] private GameObject selectButton;

    [Header("行動設定")]
    [Tooltip("待機場所のTransform")]
    [SerializeField] private Transform waitingSpot;
    [Tooltip("料理を提供する場所のTransform")]
    [SerializeField] private Transform servingSpot;
    [Tooltip("チョビンの頭上に表示される選択ボタンのオフセット")]
    [SerializeField] private Vector3 selectButtonOffset = new Vector3(0f, 2f, 0f);
    [Tooltip("調理作業にかかる時間")]
    [SerializeField] private float performingTimeLength = 2f;
    [Tooltip("待機場所に到着したとみなす半径")]
    [SerializeField] private float waitingSpotRadius = 1f;
    [SerializeField] private Slider performingTimeSlider;

    private UnityEvent serveEvent = new();

    private Transform[] target;
    private int[] materialIndex;
    private int[] actionIndex;
    private Transform cameraObj;
    private RectTransform buttonRect;
    private int id;
    private int currentIndex;
    private Status status;
    private float performingTime;
    private bool hasGuestWaitingForOrderFlag;

    public GameObject SelectButton => selectButton;
    public int[] MaterialIndex => materialIndex;
    public int[] ActionIndex => actionIndex;
    public int ID => id;

    // Start is called before the first frame update
    void Start()
    {
        cameraObj = Camera.main.transform;
        currentIndex = 0;
        performingTime = 0f;
        hasGuestWaitingForOrderFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (status)
        {
            case Status.CommandWaitiating:
                CommandWaiting();
                break;
            case Status.Moving:
                MovingBehave();
                break;
            case Status.Performing:
                PerformingBehave();
                break;
            case Status.ServingDish:
                ServingDishBehave();
                break;
            case Status.BackToWaitingSpot:
                BackToWaitingSpot();
                break;
        }

        if (status == Status.Performing && performingTimeSlider != null)
        {
            performingTimeSlider.maxValue = performingTimeLength;
            performingTimeSlider.value = performingTimeLength - performingTime;
        }
        else if (performingTimeSlider != null && status != Status.Performing)
        {
            performingTimeSlider.value = 0f;
        }
    }

    public void Init(int _id, int count)
    {
        id = _id;
        target = new Transform[count];
        materialIndex = new int[count];
        actionIndex = new int[count];
        SetState(Status.CommandWaitiating);
        CommandWaiting();
        for (int i = 0; i < count; i++)
        {
            target[i] = null;
            MaterialIndex[i] = 0;
            ActionIndex[i] = 0;

            SetMaterial(i, 0);
            SetAction(i, 0);
        }
    }

    public void SetWaitingSpot(Transform _waitingSpot, float _waitingSpotRadius)
    {
        waitingSpot = _waitingSpot;
        waitingSpotRadius = _waitingSpotRadius;
    }

    public void SetServingSpot(Transform _servingSpot)
    {
        servingSpot = _servingSpot;
    }

    public void SetSelectButton(GameObject _selectButton, Vector3 _selectButtonOffset)
    {
        selectButton = _selectButton;
        buttonRect = selectButton.GetComponent<RectTransform>();
        selectButtonOffset = _selectButtonOffset;
    }

    public void SetPerformingTimeLength(float _performingTimeLength)
    {
        performingTimeLength = _performingTimeLength;
    }

    void CommandWaiting()
    {
        selectButton.transform.position = transform.position + selectButtonOffset;
    }

    void MovingBehave()
    {
        if ((transform.position - navAgent.destination).magnitude < 1e-5)
        {
            SetState(Status.Performing);
        }
    }

    void PerformingBehave()
    {
        performingTime -= Time.deltaTime;
        if (performingTime < 0f)
        {
            performingTime = 0f;
            if (currentIndex < actionIndex.Length - 1)
            {
                currentIndex++;
                SetState(Status.Moving);
            }
            else
            {
                SetState(Status.ServingDish);
            }
        }
    }

    void ServingDishBehave()
    {
        if ((transform.position - servingSpot.position).magnitude < 0.1)
        {
            SetState(Status.BackToWaitingSpot);
        }
    }

    void BackToWaitingSpot()
    {
        if ((transform.position - waitingSpot.position).magnitude < waitingSpotRadius)
        {
            SetState(Status.CommandWaitiating);
        }
    }

    public void SetMaterial(int index, int material)
    {
        MaterialIndex[index] = material;
    }

    public void SetAction(int index, int action)
    {
        ActionIndex[index] = action;
    }

    public void SetHasGuestFlag(bool flag)
    {
        hasGuestWaitingForOrderFlag = flag;
        selectButton.SetActive(flag && status == Status.CommandWaitiating);
    }

    public void SetCommand(UnityAction action, Transform[] _target)
    {
        serveEvent.RemoveAllListeners();
        serveEvent.AddListener(action);
        target = _target;
        currentIndex = 0;
        SetState(Status.Moving);
    }

    public void SetButtonDirection(float angle)
    {
        buttonRect.rotation = Quaternion.Euler(0, angle, 0);
    }

    void SetState(Status s)
    {
        status = s;
        switch (status)
        {
            case Status.CommandWaitiating:
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    selectButton.SetActive(hasGuestWaitingForOrderFlag);
                    navAgent.SetDestination(transform.position);
                }

#else
                navAgent.SetDestination(transform.position);
                selectButton.SetActive(hasGuestWaitingForOrderFlag);
#endif
                break;
            case Status.Moving:
                navAgent.SetDestination(target[currentIndex].position);
                selectButton.SetActive(false);
                break;
            case Status.Performing:
                performingTime = performingTimeLength;
                transform.rotation = target[currentIndex].rotation;
                if (performingTimeSlider != null)
                {
                    performingTimeSlider.maxValue = performingTimeLength;
                    performingTimeSlider.value = 0f;
                }
                break;
            case Status.ServingDish:
                navAgent.SetDestination(servingSpot.position);
                break;
            case Status.BackToWaitingSpot:
                serveEvent.Invoke();
                navAgent.SetDestination(waitingSpot.position);
                break;
        }
    }
}