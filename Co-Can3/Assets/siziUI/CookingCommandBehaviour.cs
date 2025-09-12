//======================================================================
// CookingCommandBehaviour.cs
// 料理コマンドのUI制御を行うMonoBehaviour
// 各コマンドの材料・アクションのテキストとボタンの配置・操作を管理します
//======================================================================

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CookingCommandBehaviour : MonoBehaviour
{
    enum SpriteSizeOption
    {
        NonKeepAspect, // アスペクト比を維持しない
        KeepAspectWithCurrentWidth, // 現在の幅を維持してアスペクト比を調整
        KeepAspectWithCurrentHeight, // 現在の高さを維持してアスペクト比を調整
    }

    public class SelectCommandEvent : UnityEvent<int, int> { }

    [Header("ここに食材・調理法を選択するUIをD&D")]
    [SerializeField] private GameObject commandUIPrefab; // UIプレハブを登録
    [SerializeField] private GameObject commandCanvas; // Canvasオブジェクトを登録
    [SerializeField] private GameObject commandUIParent; // UIの親オブジェクトを登録
    [SerializeField] private string materialUIPrefabName = "MaterialUI"; // 食材UIの名前
    [SerializeField] private string actionUIPrefabName = "ActionUI"; // 調理法UIの名前
    [SerializeField] private bool commandCanvasStartActive = false; // ゲーム開始時にコマンドUIを表示するか

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

    private UnityEvent checkAllSettings = new();
    private SelectCommandEvent previousMaterialEvent = new();
    private SelectCommandEvent nextMaterialEvent = new();
    private SelectCommandEvent previousActionEvent = new();
    private SelectCommandEvent nextActionEvent = new();
    private UnityEvent submitCommandEvent = new();

    private int currentChobinUIIndex;

    public int CommandCount => commandCount; // コマンドの数
    public int CurrentChobinUIIndex => currentChobinUIIndex;

    public UnityEvent CheckAllSettings => checkAllSettings;
    public SelectCommandEvent PreviousMaterialEvent => previousMaterialEvent;
    public SelectCommandEvent NextMaterialEvent => nextMaterialEvent;
    public SelectCommandEvent PreviousActionEvent => previousActionEvent;
    public SelectCommandEvent NextActionEvent => nextActionEvent;
    public UnityEvent SubmitCommandEvent => submitCommandEvent;

    // 初期化処理
    void Start()
    {
        currentChobinUIIndex = 0;
        if (commandCanvasStartActive) commandCanvas.SetActive(false);
    }

    // 毎フレームの更新処理（未使用）
    void Update()
    {

    }

    /// <summary>
    /// 設定の確認とUIの初期化
    /// </summary>
    public bool CheckSettings()
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

        if (commandCanvas == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("指示UIを表示するCanvasオブジェクトが設定されていません。");
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

        return commandUIisCorrect;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        commandCanvas.SetActive(commandCanvasStartActive);
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            InitUIParent();
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
        }
#else
        InitUIParent();
#endif

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
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.delayCall -= InitUIParent; // 一度だけ実行するために解除
        }
#endif
    }

    private void InitUI()
    {
        //ボタンのサイズオプションに応じてサイズを調整
        ButtonSize = SpriteSize(leftButtonSprite, ButtonSize, buttonSizeOption);
#if UNITY_EDITOR
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
#endif

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

            materialLeftButton.onClick.AddListener(() => previousMaterialEvent.Invoke(currentChobinUIIndex, commandIndex));
            materialRightButton.onClick.AddListener(() => nextMaterialEvent.Invoke(currentChobinUIIndex, commandIndex));
            actionLeftButton.onClick.AddListener(() => previousActionEvent.Invoke(currentChobinUIIndex, commandIndex));
            actionRightButton.onClick.AddListener(() => nextActionEvent.Invoke(currentChobinUIIndex, commandIndex));
        }
    }

    public void UpdateMaterialText(int commandIndex, string name)
    {
        if (materialUITexts == null)
        {
            return;
        }
        else if (commandIndex < 0 || commandIndex >= materialUITexts.Length)
        {
            return;
        }
        materialUITexts[commandIndex].text = name;
    }

    public void UpdateActionText(int commandIndex, string name)
    {
        if (actionUITexts == null)
        {
            return;
        }
        else if (commandIndex < 0 || commandIndex >= actionUITexts.Length)
        {
            return;
        }
        actionUITexts[commandIndex].text = name;
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
        commandCanvas.SetActive(true);
        currentChobinUIIndex = chobinIndex;
    }

    public void SubmitCommand()
    {
        submitCommandEvent.Invoke();
        commandCanvas.SetActive(false);
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        checkAllSettings.Invoke();
    }
#endif
}