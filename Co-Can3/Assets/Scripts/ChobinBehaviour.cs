using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class ChobinBehaviour : MonoBehaviour
{
    public enum Status
    {
        CommandWaitiating = 0,
        Moving = 1,
        Performing = 2,
        BackToWaitingSpot = 3
    }

    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private GameObject selectButton;
    [SerializeField] private Transform waitingSpot;
    [SerializeField] private Vector3 selectButtonOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float performingTimeLength = 2f;
    [SerializeField] private float waitingSpotRadius = 1f;

    private Transform[] target;
    private int[] MaterialIndex;
    private int[] ActionIndex;
    private int id;
    private int currentIndex;
    private Status status;
    private float performingTime;

    public int[] materialIndex => MaterialIndex;
    public int[] actionIndex => ActionIndex;
    public int ID => id;

    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        status = Status.CommandWaitiating;
        performingTime = 0f;
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
            case Status.BackToWaitingSpot:
                BackToWaitingSpot();
                break;
        }
    }

    public void Init(int _id, int count)
    {
        id = _id;
        target = new Transform[count];
        MaterialIndex = new int[count];
        ActionIndex = new int[count];
        SetState(Status.CommandWaitiating);
        for (int i = 0; i < count; i++)
        {
            target[i] = null;
            MaterialIndex[i] = 0;
            ActionIndex[i] = 0;
        }
    }

    public void SetWaitingSpot(Transform _waitingSpot,float _waitingSpotRadius)
    {
        waitingSpot = _waitingSpot;
        waitingSpotRadius = _waitingSpotRadius;
    }

    public void SetSelectButton(GameObject _selectButton,Vector3 _selectButtonOffset)
    {
        selectButton = _selectButton;
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
                SetState(Status.BackToWaitingSpot);
            }
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

    public void SetAction(int index, int action, Transform tar)
    {
        ActionIndex[index] = action;
        target[index] = tar;
    }

    public void SetCommand()
    {
        currentIndex = 0;
        SetState(Status.Moving);
    }

    void SetState(Status s)
    {
        status = s;
        switch(status)
        {
            case Status.CommandWaitiating:
                selectButton.SetActive(true);
                break;
            case Status.Moving:
                navAgent.SetDestination(target[currentIndex].position);
                selectButton.SetActive(false);
                break;
            case Status.Performing:
                performingTime = performingTimeLength;
                transform.rotation = target[currentIndex].rotation;
                break;
            case Status.BackToWaitingSpot:
                navAgent.SetDestination(waitingSpot.position);
                break;
        }
    }
}