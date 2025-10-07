using UnityEngine;
using UnityEngine.AI;

public class ChobinSetting : GameSystem
{
    [Header("チョビンの設定")]
    [Tooltip("シーンに存在するすべてのチョビンのリスト")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [Header("チョビンの待機場所")]
    [Tooltip("チョビンが待機する場所")]
    [SerializeField] private Transform WaitingSpot;
    [Tooltip("チョビンが配膳を行う場所")]
    [SerializeField] private Transform ServingSpot;

    [Header("チョビンのパラメータ")]
    [Tooltip("チョビンの移動速度")]
    [SerializeField] private float chobinSpeed;
    [Tooltip("チョビンの加速度")]
    [SerializeField] private float chobinAcceleration;
    [Tooltip("チョビンが配膳後に行動にかける時間（秒）")]
    [SerializeField] private float performingTimeLength = 2f;
    [Tooltip("待機場所に到着したと判定される半径")]
    [SerializeField] private float waitingSpotRadius = 1f;

    private int commandCount = 1;
    public ChobinBehaviour[] Chobins => chobins;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool CheckSettings()
    {
        bool AllSettingAreCorrect = true;

        if (chobins.Length > 0)
        {
            for (int i = 0; i < chobins.Length; i++)
            {
                if (chobins[i] == null)
                {
                    AllSettingAreCorrect = false;
                    Debug.LogError($"ChobinBehaviourの配列にnullが含まれています。Chobin {i} を設定してください。");
                }
                else if (chobins[i].GetComponent<NavMeshAgent>() == null)
                {
                    AllSettingAreCorrect = false;
                    Debug.LogError($"ChobinBehaviourにNavMeshAgentがChobin {i} に設定されていません。NavMeshAgentを追加してください。");
                }
            }
        }
        else
        {
            AllSettingAreCorrect = false;
            Debug.LogError("ChobinBehaviourの配列が空です。ChobinBehaviourをアタッチしてください。");
        }

        if (WaitingSpot == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("待機場所のTransformが設定されていません。");
        }
        if (ServingSpot == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("配膳を行う場所のTransformが設定されていません。");
        }

        return AllSettingAreCorrect;
    }

    public void Init()
    {
        for (int i = 0; i < chobins.Length; i++)
        {
            if (chobins[i] != null)
            {
                NavMeshAgent agent = chobins[i].GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.speed = chobinSpeed;
                    agent.acceleration = chobinAcceleration;
                }
                chobins[i].SetWaitingSpot(WaitingSpot, waitingSpotRadius);
                chobins[i].SetServingSpot(ServingSpot);
                chobins[i].SetPerformingTimeLength(performingTimeLength);
                chobins[i].Init(i, commandCount);
            }
        }
    }

    public void SetCommandCount(int _commandCount)
    {
        commandCount = _commandCount;
    }
}
