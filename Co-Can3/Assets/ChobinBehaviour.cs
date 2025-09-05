using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ChobinBehaviour : MonoBehaviour
{
    public enum CookingStage
    {
        None,
        Washed,
        Cut,
        Baked,
        Served,
        Dish
    }

    public enum Status
    {
        CommandWaitiating = 0,
        Moving = 1,
        Performing = 2,
    }

    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private float performingTimeLength = 2f;

    // 新增：不同场所对应的物体
    [SerializeField] private GameObject washedObject;
    [SerializeField] private GameObject cutObject;
    [SerializeField] private GameObject bakedObject;
    [SerializeField] private GameObject servedObject; 
    [SerializeField] private GameObject dishObject;


    private Transform[] target;
    private int[] MaterialIndex;
    private int[] ActionIndex;
    private int currentIndex;
    Status status;
    private float performingTime;

    private CookingStage currentStage = CookingStage.None;

    public int[] materialIndex => MaterialIndex;
    public int[] actionIndex => ActionIndex;

    void Start()
    {
        currentIndex = 0;
        status = Status.CommandWaitiating;
        performingTime = 0f;
        HideAllStageObjects(); // 初始化时隐藏所有物体
    }

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

    // 新增：隐藏所有场所物体
    void HideAllStageObjects()
    {
        if (washedObject != null) washedObject.SetActive(false);
        if (cutObject != null) cutObject.SetActive(false);
        if (bakedObject != null) bakedObject.SetActive(false);
        if (servedObject != null) servedObject.SetActive(false);
        if (dishObject != null) dishObject.SetActive(false);
    }

    // 只保留冲突判定部分，并显示不同物体
    void OnTriggerEnter(Collider other)
    {
        HideAllStageObjects(); // 每次进入新场所先隐藏所有物体

        if (other.CompareTag("WashStation"))
        {
            currentStage = CookingStage.Washed;
            if (washedObject != null) washedObject.SetActive(true);
        }
        else if (other.CompareTag("CutStation"))
        {
            if (currentStage == CookingStage.Washed || currentStage == CookingStage.None)
            {
                currentStage = CookingStage.Cut;
                if (cutObject != null) cutObject.SetActive(true);
            }
        }
        else if (other.CompareTag("BakeStation"))
        {
            if (currentStage == CookingStage.Cut || currentStage == CookingStage.None)
            {
                currentStage = CookingStage.Baked;
                if (bakedObject != null) bakedObject.SetActive(true);
            }
        }
        else if (other.CompareTag("ServeStation"))
        {
            currentStage = CookingStage.Served;
            if (servedObject != null) servedObject.SetActive(true);
        }
        else if (other.CompareTag("DishStation"))
        {
            currentStage = CookingStage.Dish;
            if (dishObject != null) dishObject.SetActive(true);
        }

    }
}
