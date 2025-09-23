using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChobinSetting : MonoBehaviour
{
    enum SpriteSizeOption
    {
        NonKeepAspect, // アスペクト比を維持しない
        KeepAspectWithCurrentWidth, // 現在の幅を維持してアスペクト比を調整
        KeepAspectWithCurrentHeight, // 現在の高さを維持してアスペクト比を調整
    }

    public class ShowCommandEvent : UnityEvent<int> { }

    [Header("チョビンの設定")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [SerializeField] private Transform WaitingSpot; // 待機場所
    [SerializeField] private Transform ServingSpot; // 料理を提供する場所
    [SerializeField] private float chobinSpeed; // チョビンの移動速度
    [SerializeField] private float chobinAcceleration; // チョビンの加速度
    [SerializeField] private float performingTimeLength = 2f; // 調理にかかる時間
    [SerializeField] private float waitingSpotRadius = 1f; // 待機場所の半径
    [SerializeField] private Slider performingTimeSlider;

    [Header("チョビンを選択するボタンと表示するCanvasをD&D")]
    [SerializeField] private GameObject chobinButtonCanvas; // チョビンのUIを表示するCanvas
    [SerializeField] private GameObject chobinButtonPrefab; // チョビンのUIプレハブ
    [SerializeField] private Sprite chobinButtonSprite; // チョビンのUIのスプライト
    [SerializeField] private string chobinButtonPrefabName = "ChobinButton"; // チョビンのUIの名前
    [SerializeField] private Vector3 chobinButtonOffset; // チョビンのUIの位置
    [SerializeField] private Vector2 chobinButtonSize; // チョビンのUIのサイズ
    [SerializeField] private SpriteSizeOption chobinButtonSizeOption = SpriteSizeOption.NonKeepAspect; // チョビンのUIのサイズオプション
    [SerializeField] private bool buttonIsActiveInEditor = true; // チョビンのUIを表示するかどうか

    private UnityEvent checkAllSettings = new();
    private ShowCommandEvent showCommandEvent = new();

    private bool AllSettingAreCorrect = true;
    private int commandCount = 1;

    public ChobinBehaviour[] Chobins => chobins;

    public UnityEvent CheckAllSettings => checkAllSettings;
    public ShowCommandEvent ShowCommand => showCommandEvent;

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

    public bool CheckSettings()
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
                    Debug.LogError($"ChobinBehaviourのNavMeshAgentがChobin {i} に設定されていません。NavMeshAgentを追加してください。");
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
            Debug.LogError("料理を提供する場所のTransformが設定されていません。");
        }

        if (chobinButtonCanvas == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン選択用のCanvasオブジェクトが設定されていません。");
        }
        if (chobinButtonPrefab == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン選択用のボタンプレハブが設定されていません。");
        }
        else if (chobinButtonPrefab.GetComponent<Button>() == null || chobinButtonPrefab.GetComponent<Image>() == null || chobinButtonPrefab.GetComponent<RectTransform>() == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン選択用のボタンプレハブのコンポーネントが正しく設定されていません。");
        }

        return AllSettingAreCorrect;
    }

    public void Init()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            InitChobinButtonParent();
        }
        else
        {
            bool changeChobinCount = (chobinButtonCanvas.transform.childCount != chobins.Length);
            if (changeChobinCount)
            {
                Debug.Log("チョビンの数が変更されたため、チョビン選択ボタンを再初期化します。");
                EditorApplication.delayCall += InitChobinButtonParent; // チョビン選択ボタンの親オブジェクトを初期化
            }
            else
            {
                InitChobins(); // チョビンのパラメータを更新
            }
        }
#else
        InitChobinButtonParent();
#endif

        Debug.Log("ChobinSettingの初期化が完了しました。");
    }



    private void InitChobinButtonParent()
    {
        // 既存のUIオブジェクトを削除
        while (chobinButtonCanvas.transform.childCount > 0)
        {
            DestroyImmediate(chobinButtonCanvas.transform.GetChild(0).gameObject);
        }
        for (int i = 0; i < chobins.Length; i++)
        {
            GameObject chobinButtonObject = null;
#if UNITY_EDITOR
            chobinButtonObject = UnityEditor.PrefabUtility.InstantiatePrefab(chobinButtonPrefab, chobinButtonCanvas.transform) as GameObject;
#else
            chobinButtonObject = Instantiate(chobinButtonPrefab, chobinButtonCanvas.transform);
#endif
            chobinButtonObject.name = chobinButtonPrefabName + "_" + i; // チョビンボタンの名前を設定
        }

        InitChobins();
#if UNITY_EDITOR
        EditorApplication.delayCall -= InitChobinButtonParent; // 一度だけ実行するために解除
#endif
    }

    private void InitChobins()
    {
        chobinButtonSize = SpriteSize(chobinButtonSprite, chobinButtonSize, chobinButtonSizeOption);

        for (int i = 0; i < chobins.Length; i++)
        {
            // UIの位置を設定
            GameObject chobinButtonObject = chobinButtonCanvas.transform.GetChild(i).gameObject;
            Button chobinButton = chobinButtonObject.GetComponent<Button>();
            RectTransform chobinButtonRect = chobinButtonObject.GetComponent<RectTransform>();
            chobinButtonRect.sizeDelta = chobinButtonSize;
            Image chobinImage = chobinButtonObject.GetComponent<Image>();
            if (chobinImage != null)
            {
                chobinImage.sprite = chobinButtonSprite;
            }
            if (chobinButton != null)
            {
                int chobinIndex = i; // ローカル変数を使用してクロージャーの問題を回避
                chobinButton.onClick.RemoveAllListeners();
                chobinButton.onClick.AddListener(() => showCommandEvent.Invoke(chobinIndex));
            }

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
                chobins[i].SetSelectButton(chobinButtonObject, chobinButtonOffset);
                chobins[i].SetPerformingTimeLength(performingTimeLength);
                chobins[i].Init(i, commandCount);
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                chobinButtonObject.SetActive(false);
            }
            else
            {
                chobinButtonObject.SetActive(buttonIsActiveInEditor);
            }
#else
            chobinButtonObject.SetActive(false);
#endif
        }
    }

    private Vector2 SpriteSize(Sprite sprite, Vector2 targetSize, SpriteSizeOption spriteSizeOption)
    {
        Vector2 newSize = targetSize;
        switch (spriteSizeOption)
        {
            case SpriteSizeOption.NonKeepAspect:
                // そのまま
                break;
            case SpriteSizeOption.KeepAspectWithCurrentWidth:
                if (sprite != null)
                {
                    float aspectRatio = sprite.rect.height / sprite.rect.width;
                    newSize.y = newSize.x * aspectRatio;
                }
                break;
            case SpriteSizeOption.KeepAspectWithCurrentHeight:
                if (sprite != null)
                {
                    float aspectRatio = sprite.rect.width / sprite.rect.height;
                    newSize.x = newSize.y * aspectRatio;
                }
                break;
        }
        return newSize;
    }

    public void SetCommandCount(int _commandCount)
    {
        commandCount = _commandCount;
    }

    /// <summary>
    /// エディタ上でパラメータが変更されたときに設定をチェックして初期化を行う
    /// </summary>
#if UNITY_EDITOR
    private void OnValidate()
    {
        checkAllSettings.Invoke();
    }
#endif
}
