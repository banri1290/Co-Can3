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

    enum SpriteSizeOption
    {
        NonKeepAspect, // アスペクト比を維持しない
        KeepAspectWithCurrentWidth, // 現在の幅を維持してアスペクト比を調整
        KeepAspectWithCurrentHeight, // 現在の高さを維持してアスペクト比を調整
    }

    [Header("チョビンの設定")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [SerializeField] private Transform WaitingSpot; // 待機場所
    [SerializeField] private float chobinSpeed; // チョビンの移動速度
    [SerializeField] private float chobinAcceleration; // チョビンの加速度
    [SerializeField] private float performingTimeLength = 2f; // 調理にかかる時間
    [SerializeField] private float waitingSpotRadius = 1f; // 待機場所の半径
    [Header("チョビンを選択するボタンと表示するCanvasをD&D")]
    [SerializeField] private GameObject chobinButtonCanvas; // チョビンのUIを表示するCanvas
    [SerializeField] private GameObject chobinButtonPrefab; // チョビンのUIプレハブ
    [SerializeField] private Sprite chobinButtonSprite; // チョビンのUIのスプライト
    [SerializeField] private string chobinButtonPrefabName = "ChobinButton"; // チョビンのUIの名前
    [SerializeField] private Vector3 chobinButtonOffset; // チョビンのUIの位置
    [SerializeField] private Vector2 chobinButtonSize; // チョビンのUIのサイズ
    [SerializeField] private SpriteSizeOption chobinButtonSizeOption = SpriteSizeOption.NonKeepAspect; // チョビンのUIのサイズオプション
    [SerializeField] private bool chobinButtonCanvasStartActive = true; // ゲーム開始時にチョビン選択ボタンを表示するか

    [Header("ここに食材・調理法を選択するUIをD&D")]
    [SerializeField] private GameObject commandUIPrefab; // UIプレハブを登録
    [SerializeField] private GameObject commandCanvas; // Canvasオブジェクトを登録
    [SerializeField] private GameObject commandUIParent; // UIの親オブジェクトを登録
    [SerializeField] private string materialUIPrefabName = "MaterialUI"; // 食材UIの名前
    [SerializeField] private string actionUIPrefabName = "ActionUI"; // 調理法UIの名前
    [SerializeField] private bool commandCanvasStartActive = false; // ゲーム開始時にコマンドUIを表示するか
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
    [SerializeField] private SpriteSizeOption buttonSizeOption = SpriteSizeOption.NonKeepAspect; // ボタンサイズオプション

    // UI要素の参照
    private RectTransform[] materialUIRects;
    private TextMeshProUGUI[] materialUITexts;
    private Button[] materialLeftButtons;
    private Button[] materialRightButtons;
    private RectTransform[] actionUIRects;
    private TextMeshProUGUI[] actionUITexts;
    private Button[] actionLeftButtons;
    private Button[] actionRightButtons;

    private int currentChobinUIIndex;

    // 初期化処理
    void Start()
    {
        currentChobinUIIndex = 0;
        ChackSettings();
        if (commandCanvasStartActive) ShowCommand(currentChobinUIIndex);
    }

    // 毎フレームの更新処理（未使用）
    void Update()
    {

    }

    /// <summary>
    /// 設定の確認とUIの初期化
    /// </summary>
    private void ChackSettings()
    {
        bool commandUIisCorrect = true;

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
            }
        }
        else
        {
            commandUIisCorrect = false;
            Debug.LogError("ChobinBehaviourの配列が空です。ChobinBehaviourをアタッチしてください。");
        }

        if (WaitingSpot == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("待機場所のTransformが設定されていません。");
        }

        if (chobinButtonCanvas == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("チョビン選択用のCanvasオブジェクトが設定されていません。");
        }
        if (chobinButtonPrefab == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("チョビン選択用のボタンプレハブが設定されていません。");
        }
        else if (chobinButtonPrefab.GetComponent<Button>() == null || chobinButtonPrefab.GetComponent<Image>() == null || chobinButtonPrefab.GetComponent<RectTransform>() == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("チョビン選択用のボタンプレハブのコンポーネントが正しく設定されていません。");
        }

        if (MaterialList.Length == 0)
        {
            commandUIisCorrect = false;
            Debug.LogError("食材リストが空です。少なくとも1つの食材を追加してください。");
        }
        if (ActionList.Length == 0)
        {
            commandUIisCorrect = false;
            Debug.LogError("調理法リストが空です。少なくとも1つの調理法を追加してください。");
        }
        else
        {
            for (int i = 0; i < ActionList.Length; i++)
            {
                if (ActionList[i].KitchinSpot == null)
                {
                    commandUIisCorrect = false;
                    Debug.LogError($"調理法 {ActionList[i].Name} に調理場のTransformが設定されていません。");
                }
            }
        }

        if (commandCanvas == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("指示UIを表示するCanvasオブジェクトが設定されていません。");
        }
        else
        {
            commandCanvas.SetActive(commandCanvasStartActive);
        }

        if (commandUIParent == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("コマンドUIの親オブジェクトが設定されていません。");
        }

        // 食材・調理法オブジェクトがnullでないか確認
        if (commandUIPrefab != null)
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
            Debug.LogError("コマンドUIプレハブが設定されていません。");
        }
        // 全てのコマンドが正しくセットアップされている場合のみUIを初期化
        if (commandUIisCorrect)
        {
            Init();
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    private void Init()
    {
        if (EditorApplication.isPlaying)
        {
            InitUIParent();
            InitChobinButtonParent();
        }
        else
        {
            bool changeCommandCount = (commandUIParent.transform.childCount != commandCount * 2);
            bool changeMaterialUIName = (commandUIParent.transform.GetChild(0).name != materialUIPrefabName + "_0");
            bool changeActionUIName = (commandUIParent.transform.GetChild(1).name != actionUIPrefabName + "_0");
            if (changeCommandCount || changeMaterialUIName || changeActionUIName)
            {
                Debug.Log("コマンド数が変更されたため、UIを再初期化します。");
                EditorApplication.delayCall += InitUIParent; // UIの親オブジェクトを初期化
            }
            else
            {
                InitUI(); // UIのスタイルを更新
            }
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

        Debug.Log("CookingCommandBehaviourの初期化が正常に完了しました。");
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
            materialUIObject.name = materialUIPrefabName + "_" + i; // 食材UIの名前を設定
            actionUIObject.name = actionUIPrefabName + "_" + i; // 調理法UIの名前を設定

            // UIの位置を設定
            materialUIRects[i] = materialUIObject.GetComponent<RectTransform>();
            actionUIRects[i] = actionUIObject.GetComponent<RectTransform>();

            foreach (Transform t in materialUIObject.transform)
            {
                if (t.TryGetComponent<RectTransform>(out var rectTransform))
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
                if (t.TryGetComponent<RectTransform>(out var rectTransform))
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
        //ボタンのサイズオプションに応じてサイズを調整
        ButtonSize = SpriteSize(leftButtonSprite, ButtonSize, buttonSizeOption);

        bool recallInitUIParent = false;
        if (materialUIRects == null)
        {
            recallInitUIParent = true;
        }
        else if (materialUIRects.Length != commandCount)
        {
            recallInitUIParent = true;
        }
        if (recallInitUIParent)
        {
            Debug.LogWarning("UI要素の配列が初期化されていないか、コマンド数と一致しません。UIの親オブジェクトを再初期化します。");
            EditorApplication.delayCall += InitUIParent; // UIの親オブジェクトを初期化
            return;
        }

            // UIの配置とスタイルを設定
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

            // テキストのスタイルを設定
            materialUIText.rectTransform.anchoredPosition = commandTextPosition;
            materialUIText.rectTransform.sizeDelta = textSize;
            materialUIText.font = fontAsset;
            materialUIText.color = fontColor;
            materialUIText.fontSize = fontSize;
            actionUIText.rectTransform.anchoredPosition = commandTextPosition;
            actionUIText.rectTransform.sizeDelta = textSize;
            actionUIText.font = fontAsset;
            actionUIText.color = fontColor;
            actionUIText.fontSize = fontSize;

            // ボタンのスタイルを設定
            materialLeftButton.GetComponent<RectTransform>().anchoredPosition = commandLeftButtonPosition;
            materialLeftButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;
            Image materialLeftButtonImage = materialLeftButton.GetComponent<Image>();
            if (materialLeftButtonImage != null && leftButtonSprite != null)
            {
                materialLeftButtonImage.sprite = leftButtonSprite;
            }
            materialRightButton.GetComponent<RectTransform>().anchoredPosition = commandRightButtonPosition;
            materialRightButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;
            Image materialRightButtonImage = materialRightButton.GetComponent<Image>();
            if (materialRightButtonImage != null && rightButtonSprite != null)
            {
                materialRightButtonImage.sprite = rightButtonSprite;
            }
            actionLeftButton.GetComponent<RectTransform>().anchoredPosition = commandLeftButtonPosition;
            actionLeftButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;
            Image actionLeftButtonImage = actionLeftButton.GetComponent<Image>();
            if (actionLeftButtonImage != null && leftButtonSprite != null)
            {
                actionLeftButtonImage.sprite = leftButtonSprite;
            }
            actionRightButton.GetComponent<RectTransform>().anchoredPosition = commandRightButtonPosition;
            actionRightButton.GetComponent<RectTransform>().sizeDelta = ButtonSize;
            Image actionRightButtonImage = actionRightButton.GetComponent<Image>();
            if (actionRightButtonImage != null && rightButtonSprite != null)
            {
                actionRightButtonImage.sprite = rightButtonSprite;
            }

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
        }
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
            RectTransform chobinButtonRect = chobinButtonObject.GetComponent<RectTransform>();
            chobinButtonRect.sizeDelta = chobinButtonSize;
            Button chobinButton = chobinButtonObject.GetComponent<Button>();
            Image chobinImage = chobinButtonObject.GetComponent<Image>();
            if (chobinImage != null)
            {
                chobinImage.sprite = chobinButtonSprite;
            }
            if (chobinButton != null)
            {
                int chobinIndex = i; // ローカル変数を使用してクロージャーの問題を回避
                chobinButton.onClick.RemoveAllListeners();
                chobinButton.onClick.AddListener(() => ShowCommand(chobinIndex));
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
                chobins[i].SetSelectButton(chobinButtonObject, chobinButtonOffset);
                chobins[i].SetPerformingTimeLength(performingTimeLength);
                chobins[i].Init(i, commandCount);
                for (int j = 0; j < commandCount; j++)
                {
                    chobins[i].SetMaterial(j, 0); // 初期化時に食材を設定
                    chobins[i].SetAction(j, 0, ActionList[0].KitchinSpot); // 初期化時に調理法を設定
                }
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

    public void ShowCommand(int chobinIndex)
    {
        if (chobinIndex < 0 || chobinIndex >= chobins.Length)
        {
            Debug.LogError($"チョビンインデックス {chobinIndex} が範囲外です。0から{chobins.Length - 1}の範囲で指定してください。");
            return;
        }
        commandCanvas.SetActive(true);
        currentChobinUIIndex = chobinIndex;
        for (int i = 0; i < commandCount; i++)
        {
            SetMaterial(i, 0); // 食材のテキストを初期化
            SetAction(i, 0); // 調理法のテキストを初期化
        }
    }

    public void SubmitCommand()
    {
        chobins[currentChobinUIIndex].SetCommand();
        commandCanvas.SetActive(false);
    }

#if UNITY_EDITOR
    /// <summary>
    /// インスペクター上で値が変更された際に自動初期化
    /// </summary>
    private void OnValidate()
    {
        // 初期化処理を呼び出す
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            ChackSettings();
        }
    }
#endif
}