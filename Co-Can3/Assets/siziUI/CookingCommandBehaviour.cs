//======================================================================
// CookingCommandBehaviour.cs
// 料理コマンドのUI制御を行うMonoBehaviour
// 各コマンドの材料・アクションのテキストとボタンの配置・操作を管理します
//======================================================================

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CookingCommandBehaviour : MonoBehaviour
{
    [System.Serializable]
    private struct Material
    {
        [SerializeField] private string name; // 食材の名前
        public string Name => name; // 食材の名前を取得
    }
    [System.Serializable]
    private struct Action
    {
        [SerializeField] private string name; // 調理法の名前
        [SerializeField] private Transform kitchinSpot; // 調理場の位置

        public string Name => name; // 調理法の名前を取得
        public Transform KitchinSpot => kitchinSpot; //調理場の位置を取得
    }

    [Header("チョビンの設定")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [SerializeField] private float performingTimeLength = 2f; // 調理にかかる時間
    [Header("ここに食材・調理法を選択するUIグループをD&D")]
    [SerializeField] private GameObject commandUIPrefab; // UIプレハブを登録
    [SerializeField] private GameObject commandUIParent; // UIの親オブジェクトを登録
    [Header("食材・調理法の名前リスト")]
    [SerializeField] private Material[] MaterialList; // 食材のリスト
    [SerializeField] private Action[] ActionList;

    [Header("リストの配置")]
    [SerializeField] private int commandCount = 3; // コマンドの数
    [SerializeField] private Vector2 commandPosition;           // コマンドの基準位置
    [SerializeField] private Vector2 commandDelta;              // コマンドごとのオフセット
    [SerializeField] private Vector2 commandTextPosition;       // テキストの位置
    [SerializeField] private Vector2 commandLeftButtonPosition; // 左ボタンの位置
    [SerializeField] private Vector2 commandRightButtonPosition;// 右ボタンの位置
    [Header("フォントを操作")]
    [SerializeField] private TMP_FontAsset fontAsset;          // 使用するフォントアセット
    [SerializeField] private Color fontColor = Color.white; // フォントカラー
    [SerializeField] private float fontSize = 16f;              // フォントサイズ
    [SerializeField] private Vector2 textSize = new Vector2(200, 30); // テキストサイズ
    [Header("ボタンを操作")]
    [SerializeField] private Sprite leftButtonSprite;  // 左ボタンのスプライト
    [SerializeField] private Sprite rightButtonSprite; // 右ボタンのスプライト
    [SerializeField] private Vector2 ButtonSize = new Vector2(100, 100); // ボタンサイズ

    // UI要素の参照
    private RectTransform[] materialUIRects;
    private TextMeshProUGUI[] materialUITexts;
    private Button[] materialLeftButtons;
    private Button[] materialRightButtons;
    private RectTransform[] actionUIRects;
    private TextMeshProUGUI[] actionUITexts;
    private Button[] actionLeftButtons;
    private Button[] actionRightButtons;
    private int currentCommandCount;

    private int currentChobinUIIndex;

    // 初期化処理
    void Start()
    {
        currentChobinUIIndex = 0;
        InitUIParent();
        for (int i = 0; i < commandCount; i++)
        {
            SetMaterial(i, 0); // 初期化時に食材を設定
            SetAction(i, 0); // 初期化時に調理法を設定
        }
    }

    // 毎フレームの更新処理（未使用）
    void Update()
    {

    }

    /// <summary>
    /// UI要素の初期化と配置、ボタンイベントの設定
    /// </summary>
    private void Init()
    {
        // UIの数を調整
        if (commandCount < 1)
        {
            commandCount = 1;
        }
        // ボタンのx座標を調整
        if (commandLeftButtonPosition.x >= 0)
        {
            commandLeftButtonPosition.x = -1e-5f; // 左ボタンの位置が右側に来ないようにする
            Debug.LogWarning("左ボタンの位置が右側に設定されています。左ボタンは左側に配置してください。");
        }
        if (commandRightButtonPosition.x <= 0)
        {
            commandRightButtonPosition.x = 1e-5f; // 右ボタンの位置が左側に来ないようにする
            Debug.LogWarning("右ボタンの位置が左側に設定されています。右ボタンは右側に配置してください。");
        }

        bool commandUIisCorrect = true;

        if (chobins.Length > 0)
        {
            for (int i = 0; i < chobins.Length; i++)
            {
                if (chobins[i] == null)
                {
                    commandUIisCorrect = false;
                    Debug.LogError($"ChobinBehaviourの配列にnullが含まれています。Chobin {i} を設定してください。");
                }
                else if (chobins[i].GetComponent<NavMeshAgent>() == null)
                {
                    commandUIisCorrect = false;
                    Debug.LogError($"ChobinBehaviourのNavMeshAgentがChobin {i} に設定されていません。NavMeshAgentを追加してください。");
                }
                else
                {
                    chobins[i].InitIndex(commandCount); // 各Chobinのインデックスを初期化
                    chobins[i].SetPerformingTimeLength(performingTimeLength); // 調理時間を設定
                }
            }
        }
        else
        {
            commandUIisCorrect = false;
            Debug.LogError("ChobinBehaviourの配列が空です。ChobinBehaviourをアタッチしてください。");
        }
        // 食材・調理法オブジェクトがnullでないか確認
        if (commandUIPrefab != null && commandUIParent != null)
        {
            bool hasText = false;
            bool hasLeftButton = false;
            bool hasRightButton = false;
            // 食材オブジェクトの子要素をチェック
            foreach (Transform child in commandUIPrefab.transform)
            {
                if (child.GetComponent<RectTransform>())
                {
                    if (child.GetComponent<TextMeshProUGUI>())
                    {
                        hasText = true;
                    }
                    if (child.GetComponent<Button>())
                    {
                        RectTransform buttonRect = child.GetComponent<RectTransform>();
                        if (buttonRect.anchoredPosition.x < 0)
                        {
                            hasLeftButton = true;
                        }
                        else
                        {
                            hasRightButton = true;
                        }
                    }
                }
            }
            // 必要なUI要素が揃っているか判定
            if (hasText && hasLeftButton && hasRightButton)
            {
            }
            else
            {
                commandUIisCorrect = false;
                if (!hasText)
                {
                    Debug.LogError("コマンドUIプレハブにTextMeshProUGUIコンポーネントが見つかりません。");
                }
                if (!hasLeftButton)
                {
                    Debug.LogError("コマンドUIプレハブに左ボタンのButtonコンポーネントが見つかりません。");
                }
                if (!hasRightButton)
                {
                    Debug.LogError("コマンドUIプレハブに右ボタンのButtonコンポーネントが見つかりません。");
                }
            }
        }
        else
        {
            commandUIisCorrect = false;
            if (commandUIPrefab == null)
            {
                Debug.LogError("コマンドUIプレハブが設定されていません。");
            }
            if (commandUIParent == null)
            {
                Debug.LogError("コマンドUIの親オブジェクトが設定されていません。");
            }
        }
        // 全てのコマンドが正しくセットアップされている場合のみUIを初期化
        if (commandUIisCorrect)
        {

#if UNITY_EDITOR
            if (currentCommandCount != commandCount)
            {
                currentCommandCount = commandCount;
                EditorApplication.delayCall += InitUIParent; // UIの親オブジェクトを初期化
            }
            else
            {
                InitUI();
            }
#endif

            Debug.Log("CookingCommandBehaviourの初期化が正常に完了しました。");
        }
    }

    private void InitUIParent()
    {
        materialUIRects = new RectTransform[commandCount];
        actionUIRects = new RectTransform[commandCount];
        materialUITexts = new TextMeshProUGUI[commandCount];
        materialLeftButtons = new Button[commandCount];
        materialRightButtons = new Button[commandCount];
        actionUITexts = new TextMeshProUGUI[commandCount];
        actionLeftButtons = new Button[commandCount];
        actionRightButtons = new Button[commandCount];
        // 既存のUIオブジェクトを削除
        while (commandUIParent.transform.childCount > 0)
        {
            DestroyImmediate(commandUIParent.transform.GetChild(0).gameObject);
        }

        for (int i = 0; i < commandCount; i++)
        {
            GameObject materialUIObject = null;
            GameObject actionUIObject = null;
#if UNITY_EDITOR
            materialUIObject = UnityEditor.PrefabUtility.InstantiatePrefab(commandUIPrefab, commandUIParent.transform) as GameObject;
            actionUIObject = UnityEditor.PrefabUtility.InstantiatePrefab(commandUIPrefab, commandUIParent.transform) as GameObject;
#else
            materialUIObject = Instantiate(commandUIPrefab, commandUIParent.transform);
            actionUIObject = Instantiate(commandUIPrefab, commandUIParent.transform);
#endif
            PrefabUtility.UnpackPrefabInstance(materialUIObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(actionUIObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            materialUIObject.name = "MaterialUI_" + i; // 食材UIの名前を設定
            actionUIObject.name = "ActionUI_" + i; // 調理法UIの名前を設定
            // UIの位置を設定
            materialUIRects[i] = materialUIObject.GetComponent<RectTransform>();
            actionUIRects[i] = actionUIObject.GetComponent<RectTransform>();

            foreach (Transform t in materialUIObject.transform)
            {
                RectTransform rectTransform = t.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    if (t.GetComponent<TextMeshProUGUI>() != null)
                    {
                        materialUITexts[i] = t.GetComponent<TextMeshProUGUI>();
                    }
                    else if (t.GetComponent<Button>() != null)
                    {
                        if (rectTransform.anchoredPosition.x < 0)
                        {
                            materialLeftButtons[i] = t.GetComponent<Button>();

                        }
                        else
                        {
                            materialRightButtons[i] = t.GetComponent<Button>();
                        }
                    }
                }
            }
            foreach (Transform t in actionUIObject.transform)
            {
                RectTransform rectTransform = t.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    if (t.GetComponent<TextMeshProUGUI>() != null)
                    {
                        actionUITexts[i] = t.GetComponent<TextMeshProUGUI>();
                    }
                    else if (t.GetComponent<Button>() != null)
                    {
                        if (rectTransform.anchoredPosition.x < 0)
                        {
                            actionLeftButtons[i] = t.GetComponent<Button>();

                        }
                        else
                        {
                            actionRightButtons[i] = t.GetComponent<Button>();
                        }
                    }
                }
            }
        }

        InitUI();
#if UNITY_EDITOR
        EditorApplication.delayCall -= InitUIParent; // 一度だけ実行するために解除
#endif
    }

    private void InitUI()
    {
        for (int i = 0; i < commandCount; i++)
        {
            materialUIRects[i].anchoredPosition = commandPosition + new Vector2(0, -commandDelta.y * i);
            actionUIRects[i].anchoredPosition = commandPosition + new Vector2(commandDelta.x, -commandDelta.y * i);

            TextMeshProUGUI materialUIText = materialUITexts[i];
            Button materialLeftButton = materialLeftButtons[i];
            Button materialRightButton = materialRightButtons[i];
            TextMeshProUGUI actionUIText = actionUITexts[i];
            Button actionLeftButton = actionLeftButtons[i];
            Button actionRightButton = actionRightButtons[i];

            materialUIText.font = fontAsset;
            materialUIText.fontSize = fontSize;
            materialUIText.color = fontColor;
            materialUIText.GetComponent<RectTransform>().anchoredPosition = commandTextPosition;
            materialUIText.GetComponent<RectTransform>().sizeDelta = textSize;
            materialLeftButton.GetComponent<Image>().sprite = leftButtonSprite;
            materialLeftButton.GetComponent<RectTransform>().anchoredPosition = commandLeftButtonPosition;
            materialLeftButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;
            materialRightButton.GetComponent<Image>().sprite = rightButtonSprite;
            materialRightButton.GetComponent<RectTransform>().anchoredPosition = commandRightButtonPosition;
            materialRightButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;

            actionUIText.font = fontAsset;
            actionUIText.fontSize = fontSize;
            actionUIText.color = fontColor;
            actionUIText.GetComponent<RectTransform>().anchoredPosition = commandTextPosition;
            actionUIText.GetComponent<RectTransform>().sizeDelta = textSize;
            actionLeftButton.GetComponent<Image>().sprite = leftButtonSprite;
            actionLeftButton.GetComponent<RectTransform>().anchoredPosition = commandLeftButtonPosition;
            actionLeftButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;
            actionRightButton.GetComponent<Image>().sprite = rightButtonSprite;
            actionRightButton.GetComponent<RectTransform>().anchoredPosition = commandRightButtonPosition;
            actionRightButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;

            // ボタンのクリックイベントを設定
            int commandIndex = i; // ローカル変数を使用してクロージャーの問題を回避
            materialLeftButton.onClick.RemoveAllListeners();
            materialRightButton.onClick.RemoveAllListeners();
            actionLeftButton.onClick.RemoveAllListeners();
            actionRightButton.onClick.RemoveAllListeners();
            materialLeftButton.onClick.AddListener(() => SetMaterial(commandIndex, (chobins[currentChobinUIIndex].materialIndex[commandIndex] - 1 + MaterialList.Length) % MaterialList.Length));
            materialRightButton.onClick.AddListener(() => SetMaterial(commandIndex, (chobins[currentChobinUIIndex].materialIndex[commandIndex] + 1) % MaterialList.Length));
            actionLeftButton.onClick.AddListener(() => SetAction(commandIndex, (chobins[currentChobinUIIndex].actionIndex[commandIndex] - 1 + ActionList.Length) % ActionList.Length));
            actionRightButton.onClick.AddListener(() => SetAction(commandIndex, (chobins[currentChobinUIIndex].actionIndex[commandIndex] + 1) % ActionList.Length));

            // 初期化時に各チョビンの食材と調理法を設定
            for (int j = 0; j < chobins.Length; j++)
            {
                chobins[j].SetMaterial(commandIndex, 0);
                chobins[j].SetAction(commandIndex, 0, ActionList[0].KitchinSpot);
            }
        }
    }

    private void SetMaterial(int commandIndex, int materialIndex)
    {
        if (commandIndex < 0 || commandIndex >= commandCount)
        {
            Debug.LogError($"コマンドインデックス {commandIndex} が範囲外です。0から{commandCount - 1}の範囲で指定してください。");
            return;
        }
        if (materialIndex < 0 || materialIndex >= MaterialList.Length)
        {
            Debug.LogError($"材料インデックス {materialIndex} が範囲外です。0から{MaterialList.Length - 1}の範囲で指定してください。");
            return;
        }
        if (materialUITexts == null)
        {
            Debug.LogError("materialUITextsが初期化されていません。init()を呼び出してください。");
            return;
        }
        chobins[currentChobinUIIndex].SetMaterial(commandIndex, materialIndex);
        UpdateMaterialText(commandIndex); // 食材のテキストを更新
    }
    private void SetAction(int commandIndex, int actionIndex)
    {
        if (commandIndex < 0 || commandIndex >= commandCount)
        {
            Debug.LogError($"コマンドインデックス {commandIndex} が範囲外です。0から{commandCount - 1}の範囲で指定してください。");
            return;
        }
        if (actionIndex < 0 || actionIndex >= ActionList.Length)
        {
            Debug.LogError($"アクションインデックス {actionIndex} が範囲外です。0から{ActionList.Length - 1}の範囲で指定してください。");
            return;
        }
        chobins[currentChobinUIIndex].SetAction(commandIndex, actionIndex, ActionList[actionIndex].KitchinSpot);
        UpdateActionText(commandIndex);
    }

    private void UpdateMaterialText(int commandIndex)
    {
        materialUITexts[commandIndex].text = MaterialList[chobins[currentChobinUIIndex].materialIndex[commandIndex]].Name;
    }

    private void UpdateActionText(int commandIndex)
    {
        actionUITexts[commandIndex].text = ActionList[chobins[currentChobinUIIndex].actionIndex[commandIndex]].Name;
    }

    public void SubmitCommand()
    {
        for (int i = 0; i < chobins.Length; i++)
        {
            chobins[i].SetState(ChobinBehaviour.Status.Moving);
        }
    }

    public void NextChobin()
    {
        currentChobinUIIndex = (currentChobinUIIndex + 1) % chobins.Length;
        for (int i = 0; i < commandCount; i++)
        {
            UpdateMaterialText(i);
            UpdateActionText(i);
        }
    }

    public void PreviousChobin()
    {
        currentChobinUIIndex = (currentChobinUIIndex - 1 + chobins.Length) % chobins.Length;
        for (int i = 0; i < commandCount; i++)
        {
            UpdateMaterialText(i);
            UpdateActionText(i);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// インスペクター上で値が変更された際に自動初期化
    /// </summary>
    private void OnValidate()
    {
        // 初期化処理を呼び出す
        Init();
    }
#endif
}