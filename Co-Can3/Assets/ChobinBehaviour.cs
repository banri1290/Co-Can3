using System;
using UnityEngine;
using UnityEngine.AI;

public class ChobinBehaviour : MonoBehaviour
{
    public enum Status
    {
        CommandWaitiating = 0,
        Moving = 1,
        Performing = 2,
    }

    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private float performingTimeLength = 2f;

    private Transform[] target;
    private int[] MaterialIndex;
    private int[] ActionIndex;
    private int currentIndex;
    Status status;
    private float performingTime;

    public int[] materialIndex => MaterialIndex;
    public int[] actionIndex => ActionIndex;

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
                break;
            case Status.Moving:
                MovingBehave();
                break;
            case Status.Performing:
                PerformingBehave();
                break;
        }
    }

    void MovingBehave()
    {
        if ((transform.position - navAgent.destination).magnitude < 1e-5)
        {
            status = Status.Performing;
            performingTime = performingTimeLength;
            transform.rotation = target[currentIndex].rotation;
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
                status = Status.Moving;
                currentIndex++;
                navAgent.SetDestination(target[currentIndex].position);
            }
            else
            {
                status = Status.CommandWaitiating;
            }
        }
    }

    public void InitIndex(int count)
    {
        target = new Transform[count];
        MaterialIndex = new int[count];
        ActionIndex = new int[count];
        for (int i = 0; i < count; i++)
        {
            target[i] = null;
            MaterialIndex[i] = 0;
            ActionIndex[i] = 0;
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

    public void SetState(Status s)
    {
        status = s;
        if (s == Status.Moving)
        {
            currentIndex = 0;
            navAgent.SetDestination(target[currentIndex].position);
        }
    }

    public void SetPerformingTimeLength(float time)
    {
        performingTimeLength = time;
    }
}