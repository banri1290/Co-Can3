//======================================================================
// CookingCommandBehaviour.cs
// 料理コマンドのUI制御を行うMonoBehaviour
// 各コマンドの材料・アクションのテキストとボタンの配置・操作を管理します
//======================================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingCommandBehaviour_arc : MonoBehaviour
{
    // コマンド情報を保持する構造体
    [System.Serializable]
    struct CookingCommand
    {
        [Header("食材")]
        [SerializeField] private GameObject Material; // 食材を選択するUI
        [Header("調理法")]
        [SerializeField] private GameObject Action;   // 調理法を選択するUI

        public GameObject MaterialObject => Material; // 食材を選択するUIを取得
        public GameObject ActionObject => Action;     // 調理法を選択するUIを取得
    }

    [Header("ここに食材・調理法を選択するUIグループをD&D")]
    [SerializeField] private CookingCommand[] Commands; // コマンド配列
    [Header("食材・調理法の名前リスト")]
    [SerializeField] private string[] MaterialName;     // 食材名を登録
    [SerializeField] private string[] ActionName;       // 調理法名を登録

    [Header("リストの配置")]
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

    TextMeshProUGUI[] MaterialTexts; // 食材名を表示するテキスト
    TextMeshProUGUI[] ActionTexts;   // 調理法名を表示するテキスト
    private int[] currentMaterialIndex; // 現在の食材インデックス
    private int[] currentActionIndex;   // 現在の調理法インデックス

    // 初期化処理
    void Start()
    {
        currentMaterialIndex = new int[Commands.Length];
        currentActionIndex = new int[Commands.Length];
        init();
    }

    /// <summary>
    /// UI要素の初期化と配置、ボタンイベントの設定
    /// </summary>
    private void init()
    {
        // ボタンのx座標を調整
        if(commandLeftButtonPosition.x >=0)
        {
            commandLeftButtonPosition.x = -1e-5f; // 左ボタンの位置が右側に来ないようにする
            Debug.LogWarning("左ボタンの位置が右側に設定されています。左ボタンは左側に配置してください。");
        }
        if (commandRightButtonPosition.x <= 0)
        {
            commandRightButtonPosition.x = 1e-5f; // 右ボタンの位置が左側に来ないようにする
            Debug.LogWarning("右ボタンの位置が左側に設定されています。右ボタンは右側に配置してください。");
        }

        TextMeshProUGUI[] materialTexts = new TextMeshProUGUI[Commands.Length];
        TextMeshProUGUI[] actionTexts = new TextMeshProUGUI[Commands.Length];
        Button[] materialLeftButtons = new Button[Commands.Length];
        Button[] materialRightButtons = new Button[Commands.Length];
        Button[] actionLeftButtons = new Button[Commands.Length];
        Button[] actionRightButtons = new Button[Commands.Length];
        RectTransform[] textRects = new RectTransform[Commands.Length * 2];
        RectTransform[] LeftButtonRects = new RectTransform[Commands.Length * 2];
        RectTransform[] RightButtonRects = new RectTransform[Commands.Length * 2];

        currentMaterialIndex = new int[Commands.Length];
        currentActionIndex = new int[Commands.Length];

        bool allCommandsAreCorrect = true;
        for (int i = 0; i < Commands.Length; i++)
        {
            // 食材・調理法オブジェクトがnullでないか確認
            if (Commands[i].MaterialObject != null && Commands[i].ActionObject != null)
            {
                bool materialHasText = false;
                bool actionHasText = false;
                bool materialHasLeftButton = false;
                bool materialHasRightButton = false;
                bool actionHasLeftButton = false;
                bool actionHasRightButton = false;
                // 食材オブジェクトの子要素をチェック
                foreach (Transform child in Commands[i].MaterialObject.transform)
                {
                    if (child.GetComponent<RectTransform>())
                    {
                        if (child.GetComponent<TextMeshProUGUI>())
                        {
                            textRects[i * 2] = child.GetComponent<RectTransform>();
                            materialTexts[i] = child.GetComponent<TextMeshProUGUI>();
                            materialHasText = true;
                        }
                        if (child.GetComponent<Button>())
                        {
                            RectTransform buttonRect = child.GetComponent<RectTransform>();
                            if (buttonRect.anchoredPosition.x < 0)
                            {
                                materialLeftButtons[i] = child.GetComponent<Button>();
                                LeftButtonRects[i * 2] = buttonRect;
                                materialHasLeftButton = true;
                            }
                            else
                            {
                                materialRightButtons[i] = child.GetComponent<Button>();
                                RightButtonRects[i * 2] = buttonRect;
                                materialHasRightButton = true;
                            }
                        }
                    }
                }
                // 調理法オブジェクトの子要素をチェック
                foreach (Transform child in Commands[i].ActionObject.transform)
                {
                    if (child.GetComponent<RectTransform>())
                    {
                        if (child.GetComponent<TextMeshProUGUI>())
                        {
                            textRects[i * 2 + 1] = child.GetComponent<RectTransform>();
                            actionTexts[i] = child.GetComponent<TextMeshProUGUI>();
                            actionHasText = true;
                        }
                        if (child.GetComponent<Button>())
                        {
                            RectTransform buttonRect = child.GetComponent<RectTransform>();
                            if (buttonRect.anchoredPosition.x < 0)
                            {
                                actionLeftButtons[i] = child.GetComponent<Button>();
                                LeftButtonRects[i * 2 + 1] = buttonRect;
                                actionHasLeftButton = true;
                            }
                            else
                            {
                                actionRightButtons[i] = child.GetComponent<Button>();
                                RightButtonRects[i * 2 + 1] = buttonRect;
                                actionHasRightButton = true;
                            }
                        }
                    }
                }
                // 必要なUI要素が揃っているか判定
                if (materialHasText && actionHasText && materialHasLeftButton && materialHasRightButton && actionHasLeftButton && actionHasRightButton)
                {
                    currentMaterialIndex[i] = 0;
                    currentActionIndex[i] = 0;
                }
                else
                {
                    allCommandsAreCorrect = false;
                    Debug.LogError($"Cooking command {i} のセットアップが正しくありません。材料・アクションそれぞれにテキストと左右ボタンが必要です。");
                }
            }
            else
            {
                // nullの場合はエラー
            }
        }
        // 全てのコマンドが正しくセットアップされている場合のみUIを初期化
        if (allCommandsAreCorrect)
        {
            MaterialTexts = materialTexts;
            ActionTexts = actionTexts;
            currentMaterialIndex = new int[Commands.Length];
            for (int i = 0; i < Commands.Length; i++)
            {
                int index = i; // ラムダ式用ローカル変数
                currentMaterialIndex[index] = 0;
                // ボタンイベントのリスナーを初期化
                materialLeftButtons[index].onClick.RemoveAllListeners();
                materialRightButtons[index].onClick.RemoveAllListeners();
                actionLeftButtons[index].onClick.RemoveAllListeners();
                actionRightButtons[index].onClick.RemoveAllListeners();
                // 材料・アクションの左右ボタンにイベントを追加
                materialLeftButtons[index].onClick.AddListener(() =>
                    UpdateText(MaterialTexts[index], MaterialName, ref currentMaterialIndex[index], currentMaterialIndex[index] - 1)
                );
                materialRightButtons[index].onClick.AddListener(() =>
                    UpdateText(MaterialTexts[index], MaterialName, ref currentMaterialIndex[index], currentMaterialIndex[index] + 1)
                );
                actionLeftButtons[index].onClick.AddListener(() =>
                    UpdateText(ActionTexts[index], ActionName, ref currentActionIndex[index], currentActionIndex[index] - 1)
                );
                actionRightButtons[index].onClick.AddListener(() =>
                    UpdateText(ActionTexts[index], ActionName, ref currentActionIndex[index], currentActionIndex[index] + 1)
                );
                // UI要素の配置・サイズ設定
                Commands[i].MaterialObject.GetComponent<RectTransform>().anchoredPosition
                    = commandPosition + new Vector2(0, -commandDelta.y * i);
                Commands[i].ActionObject.GetComponent<RectTransform>().anchoredPosition
                    = commandPosition + new Vector2(commandDelta.x, -commandDelta.y * i);
                for (int j = 0; j < 2; j++)
                {
                    textRects[i * 2 + j].anchoredPosition = commandTextPosition;
                    textRects[i * 2 + j].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textSize.x);
                    textRects[i * 2 + j].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textSize.y);

                    LeftButtonRects[i * 2 + j].anchoredPosition = commandLeftButtonPosition;
                    LeftButtonRects[i * 2 + j].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ButtonSize.x);
                    LeftButtonRects[i * 2 + j].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ButtonSize.y);
                    if (leftButtonSprite != null)
                    {
                        LeftButtonRects[i * 2 + j].GetComponent<Image>().sprite = leftButtonSprite;
                    }
                    LeftButtonRects[i * 2 + j].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text="";

                    RightButtonRects[i * 2 + j].anchoredPosition = commandRightButtonPosition;
                    RightButtonRects[i * 2 + j].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ButtonSize.x);
                    RightButtonRects[i * 2 + j].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ButtonSize.y);
                    if (rightButtonSprite != null)
                    {
                        RightButtonRects[i * 2 + j].GetComponent<Image>().sprite = rightButtonSprite;
                    }
                    RightButtonRects[i * 2 + j].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
                }
                materialTexts[i].fontSize = fontSize;
                actionTexts[i].fontSize = fontSize;
                materialTexts[i].color = fontColor;
                actionTexts[i].color = fontColor;
                if(fontAsset != null)
                {
                    materialTexts[i].font = fontAsset;
                    actionTexts[i].font = fontAsset;
                }
                // 初期テキスト表示
                UpdateText(MaterialTexts[i], MaterialName, ref currentMaterialIndex[i], 0);
                UpdateText(ActionTexts[i], ActionName, ref currentActionIndex[i], 0);
            }
            Debug.Log("CookingCommandBehaviourの初期化が正常に完了しました。");
        }
    }

    // 毎フレームの更新処理（未使用）
    void Update()
    {

    }

    /// <summary>
    /// テキストの更新処理（インデックス循環）
    /// </summary>
    /// <param name="tmp">更新対象のTextMeshProUGUI</param>
    /// <param name="texts">表示するテキスト配列</param>
    /// <param name="index">現在のインデックス（参照渡し）</param>
    /// <param name="newIndex">新しいインデックス</param>
    private void UpdateText(TextMeshProUGUI tmp, string[] texts, ref int index, int newIndex)
    {
        if (newIndex < 0) newIndex += texts.Length;
        if (newIndex >= texts.Length) newIndex -= texts.Length;
        index = newIndex;
        tmp.text = texts[index];
    }

#if UNITY_EDITOR
    /// <summary>
    /// インスペクター上で値が変更された際に自動初期化
    /// </summary>
    private void OnValidate()
    {
        init();
    }
#endif
}

