using UnityEngine;
using UnityEngine.AI;

public class ChobinSetting : GameSystem
{
    [Header("繝√Ι繝薙Φ縺ｮ蜿ら�")]
    [Tooltip("繧ｲ繝ｼ繝縺ｫ逋ｻ蝣ｴ縺吶ｋ縺吶∋縺ｦ縺ｮ繝√Ι繝薙Φ縺ｮ繝ｪ繧ｹ繝")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [Header("繝√Ι繝薙Φ縺ｮ陦悟虚蝣ｴ謇")]
    [Tooltip("繝√Ι繝薙Φ縺梧欠遉ｺ繧貞ｾ�▽蝣ｴ謇")]
    [SerializeField] private Transform WaitingSpot;
    [Tooltip("繝√Ι繝薙Φ縺梧侭逅�ｒ謠蝉ｾ帙☆繧句ｴ謇")]
    [SerializeField] private Transform ServingSpot;

    [Header("繝√Ι繝薙Φ縺ｮ繝代Λ繝｡繝ｼ繧ｿ")]
    [Tooltip("繝√Ι繝薙Φ縺ｮ遘ｻ蜍暮溷ｺｦ")]
    [SerializeField] private float chobinSpeed;
    [Tooltip("繝√Ι繝薙Φ縺ｮ蜉騾溷ｺｦ")]
    [SerializeField] private float chobinAcceleration;
    [Tooltip("繝√Ι繝薙Φ縺悟推隱ｿ逅�ｽ懈･ｭ縺ｫ縺九￠繧区凾髢難ｼ育ｧ抵ｼ")]
    [SerializeField] private float performingTimeLength = 2f;
    [Tooltip("蠕�ｩ溷ｴ謇縺ｫ蛻ｰ驕斐＠縺溘→蛻､螳壹＆繧後ｋ蜊雁ｾ")]
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
        // performingTimeLengthの値に応じてSliderを更新
        if (performingTimeSlider != null)
        {
            // 例: performingTimeLengthが最大値、0が最小値
            performingTimeSlider.maxValue = performingTimeLength;
            // performingTimeLengthの進行度を表示（ここでは仮に減少していく例）
            // 実際はChobinBehaviourから進行度を取得して反映するのが理想
            performingTimeSlider.value = Mathf.Clamp(performingTimeLength, 0, performingTimeSlider.maxValue);
        }
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
                    Debug.LogError($"ChobinBehaviour縺ｮ驟榊�縺ｫnull縺悟性縺ｾ繧後※縺�∪縺吶�hobin {i} 繧定ｨｭ螳壹＠縺ｦ縺上□縺輔＞縲");
                }
                else if (chobins[i].GetComponent<NavMeshAgent>() == null)
                {
                    AllSettingAreCorrect = false;
                    Debug.LogError($"ChobinBehaviour縺ｫNavMeshAgent縺靴hobin {i} 縺ｫ險ｭ螳壹＆繧後※縺�∪縺帙ｓ縲�avMeshAgent繧定ｿｽ蜉縺励※縺上□縺輔＞縲");
                }
            }
        }
        else
        {
            AllSettingAreCorrect = false;
            Debug.LogError("ChobinBehaviour縺ｮ驟榊�縺檎ｩｺ縺ｧ縺吶�hobinBehaviour繧偵い繧ｿ繝�メ縺励※縺上□縺輔＞縲");
        }

        if (WaitingSpot == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("蠕�ｩ溷ｴ謇縺ｮTransform縺瑚ｨｭ螳壹＆繧後※縺�∪縺帙ｓ縲");
        }
        if (ServingSpot == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("譁咏炊繧呈署萓帙〒縺阪ｋ蝣ｴ謇縺ｮTransform縺瑚ｨｭ螳壹＆繧後※縺�∪縺帙ｓ縲");
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
