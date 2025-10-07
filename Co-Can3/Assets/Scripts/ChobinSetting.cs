using UnityEngine;
using UnityEngine.AI;

public class ChobinSetting : GameSystem
{
    [Header("チョビンの参照")]
    [Tooltip("ゲームに登場するすべてのチョビンのリスト")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [Header("チョビンの行動場所")]
    [Tooltip("チョビンが指示を待つ場所")]
    [SerializeField] private Transform WaitingSpot;
    [Tooltip("チョビンが料理を提供する場所")]
    [SerializeField] private Transform ServingSpot;

    [Header("チョビンのパラメータ")]
    [Tooltip("チョビンの移動速度")]
    [SerializeField] private float chobinSpeed;
    [Tooltip("チョビンの加速度")]
    [SerializeField] private float chobinAcceleration;
    [Tooltip("チョビンが各調理作業にかける時間（秒）")]
    [SerializeField] private float performingTimeLength = 2f;
    [Tooltip("待機場所に到達したと判定される半径")]
    [SerializeField] private float waitingSpotRadius = 1f;

    private bool AllSettingAreCorrect = true;
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
        AllSettingAreCorrect = true;

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
            Debug.LogError("料理を提供できる場所のTransformが設定されていません。");
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
