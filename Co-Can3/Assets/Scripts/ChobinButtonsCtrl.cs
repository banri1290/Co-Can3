using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChobinButtonsCtrl : GameSystem
{
    enum SpriteSizeOption
    {
        NonKeepAspect, // アスペクト比を維持しない
        KeepAspectWithCurrentWidth, // 現在の幅を維持してアスペクト比を維持
        KeepAspectWithCurrentHeight, // 現在の高さを維持してアスペクト比を維持
    }

<<<<<<< Updated upstream
=======
    enum ButtonState
    {
        None,
        Waiting,
        Performing
    }

>>>>>>> Stashed changes
    [System.Serializable]
    private class ButtonType
    {
        [Tooltip("選択ボタンの画像")]
        [SerializeField] private Sprite sprite;

        [Tooltip("チョビンからのボタンの相対位置")]
        [SerializeField] private Vector3 buttonOffset;
        [Tooltip("選択ボタンの基本サイズ")]
        [SerializeField] private Vector2 size;
        [Tooltip("ボタンサイズをスプライトのアスペクト比に合わせて調整する方法")]
        [SerializeField] private SpriteSizeOption sizeOption = SpriteSizeOption.NonKeepAspect;

<<<<<<< Updated upstream
        public Vector3 ButtonOffset => buttonOffset;
=======
        public Sprite Sprite => sprite;
        public Vector2 Size => size;
        public Vector3 Offset => offset;
>>>>>>> Stashed changes

        public void SetSprite(RectTransform rectTransform)
        {
            if (rectTransform == null) return;

            rectTransform.sizeDelta = size;
            Image image = rectTransform.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
            }
        }

        public void AdjustSizeToSprite()
        {
            if (sprite == null) return;
            float aspectRatio = sprite.rect.width / sprite.rect.height;
            switch (sizeOption)
            {
                case SpriteSizeOption.NonKeepAspect:
                    // そのまま
                    break;
                case SpriteSizeOption.KeepAspectWithCurrentWidth:
                    size.y = size.x / aspectRatio;
                    break;
                case SpriteSizeOption.KeepAspectWithCurrentHeight:
                    size.x = size.y * aspectRatio;
                    break;
            }
        }
    }

    [Header("参照設定")]
    [Tooltip("チョビンのパラメータ設定")]
    [SerializeField] private ChobinSetting chobinSetting;
    [Tooltip("チョビン選択ボタンUIを配置するCanvas")]
    [SerializeField] private GameObject chobinButtonCanvas;
    [Tooltip("チョビン選択ボタンのプレハブ")]
    [SerializeField] private GameObject chobinButtonPrefab;
    [Tooltip("生成されるチョビン選択ボタンのプレハブ名")]
    [SerializeField] private string chobinButtonPrefabName = "ChobinButton";

    [Header("ボタンの見た目設定")]
    [Tooltip("待機中のボタンの見た目")]
    [SerializeField] private ButtonType waitingButtonType;
<<<<<<< Updated upstream

    [Header("エディタ設定")]
    [Tooltip("エディタ再生中以外でもボタンを表示するかどうか")]
    [SerializeField] private bool buttonIsActiveInEditor = true;

    private EventWithInt showCommandEvent = new();
=======
    [Tooltip("実行中のボタンの見た目")]
    [SerializeField] private ButtonType performingButtonType;

    [Header("エディタ上での表示設定")]
    [Tooltip("エディタ上でどのタイプのボタンを表示するか")]
    [SerializeField] private ButtonState showButtonStateInEditor = ButtonState.None;

    private ChobinBehaviour[] chobins = null;
    private ChobinButton[] chobinButtons = null;

    private EventWithInt showCommandEvent = new();
    private EventWithInt quitCommandEvent = new();
>>>>>>> Stashed changes
    public EventWithInt ShowCommand => showCommandEvent;
    public ChobinSetting ChobinSetting => chobinSetting;
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

<<<<<<< Updated upstream
        if (chobinSetting == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("ChobinSettingオブジェクトが設定されていません。");
        }
        else
        {
            bool chobinSettingIsCorrect = chobinSetting.CheckSettings();
            if (!chobinSettingIsCorrect)
            {
                AllSettingAreCorrect = false;
                Debug.LogError("ChobinSettingが設定されていません。");
            }
        }
        if (chobinButtonCanvas == null)
=======
        if (chobinButtonCanvas is null)
>>>>>>> Stashed changes
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン用のCanvasオブジェクトが設定されていません。");
        }
        if (chobinButtonPrefab == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン用のボタンプレハブが設定されていません。");
        }
<<<<<<< Updated upstream
        else if (chobinButtonPrefab.GetComponent<Button>() == null || chobinButtonPrefab.GetComponent<Image>() == null || chobinButtonPrefab.GetComponent<RectTransform>() == null)
=======
        else if (chobinButtonPrefab.GetComponent<Button>() is null ||
                 chobinButtonPrefab.GetComponent<ChobinButton>() is null)
>>>>>>> Stashed changes
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン用のボタンプレハブのコンポーネントが正しく設定されていません。");
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
<<<<<<< Updated upstream
            bool changeChobinCount = (chobinButtonCanvas.transform.childCount != GetChobinCount());
            if (changeChobinCount)
=======
            if (chobinButtonCanvas.transform.childCount != chobins.Length)
>>>>>>> Stashed changes
            {
                Debug.Log("チョビンの数が変更されたため、チョビン用ボタンを再生成します。");
                EditorApplication.delayCall += InitChobinButtonParent; // チョビン用ボタンの子オブジェクトを削除
            }
            else
            {
<<<<<<< Updated upstream
                InitChobins(); // チョビンのパラメータを更新
=======
                InitChobinButtons(); // チョビン用ボタンの初期化
>>>>>>> Stashed changes
            }
        }
#else
        InitChobinButtonParent();
#endif
<<<<<<< Updated upstream

        Debug.Log("ChobinSettingの初期化が完了しました。");
=======
        Debug.Log("ChobinButtonsCtrlの初期化が完了しました。");
>>>>>>> Stashed changes
    }



    private void InitChobinButtonParent()
    {
<<<<<<< Updated upstream
=======
        chobinButtons = new ChobinButton[chobins.Length];
>>>>>>> Stashed changes
        // 既存のUIオブジェクトを削除
        while (chobinButtonCanvas.transform.childCount > 0)
        {
            DestroyImmediate(chobinButtonCanvas.transform.GetChild(0).gameObject);
        }
<<<<<<< Updated upstream
        for (int i = 0; i < GetChobinCount(); i++)
=======
        for (int i = 0; i < chobins.Length; i++)
>>>>>>> Stashed changes
        {
            GameObject chobinButtonObject = null;
#if UNITY_EDITOR
            chobinButtonObject = PrefabUtility.InstantiatePrefab(chobinButtonPrefab, chobinButtonCanvas.transform) as GameObject;
#else
            chobinButtonObject = Instantiate(chobinButtonPrefab, chobinButtonCanvas.transform);
#endif
            chobinButtonObject.name = chobinButtonPrefabName + "_" + i; // チョビンボタンの名前を設定
        }

        InitChobins();
#if UNITY_EDITOR
<<<<<<< Updated upstream
        EditorApplication.delayCall -= InitChobinButtonParent; // 重複して実行されるのを防ぐために削除
=======
        if (!EditorApplication.isPlaying)
            EditorApplication.delayCall -= InitChobinButtonParent; // 重複して実行されるのを防ぐために削除
>>>>>>> Stashed changes
#endif
    }

    private void InitChobins()
    {
<<<<<<< Updated upstream
=======
#if UNITY_EDITOR
        bool recallInitButtonParent = false;
        if (chobinButtons == null)
        {
            recallInitButtonParent = true;
        }
        else if (chobinButtons.Length != chobins.Length)
        {
            recallInitButtonParent = true;
        }
        else for (int i = 0; i < chobinButtons.Length; i++)
            {
                if (chobinButtons[i] == null)
                {
                    recallInitButtonParent = true;
                    break;
                }
            }
        if (recallInitButtonParent)
        {
            Debug.LogWarning("チョビン用のボタン配列が不正です。再度初期化を行います。");
            EditorApplication.delayCall += InitChobinButtonParent; // チョビン用ボタンの子オブジェクトを削除
            return;
        }
#endif

>>>>>>> Stashed changes
        ButtonType buttonType = waitingButtonType;
        waitingButtonType.AdjustSizeToSprite();

        for (int i = 0; i < GetChobinCount(); i++)
        {
<<<<<<< Updated upstream
            // UIの位置を設定
            GameObject chobinButtonObject = chobinButtonCanvas.transform.GetChild(i).gameObject;
            Button chobinButton = chobinButtonObject.GetComponent<Button>();
            RectTransform chobinButtonRect = chobinButtonObject.GetComponent<RectTransform>();
            buttonType.SetSprite(chobinButtonRect);
            // チョビンの位置に合わせてボタンの位置を設定
            if (chobinButton != null)
            {
                int chobinIndex = i; // ローカル変数を使用してクロージャーの問題を回避
                chobinButton.onClick.RemoveAllListeners();
                chobinButton.onClick.AddListener(() => showCommandEvent.Invoke(chobinIndex));
            }
            if (chobinSetting != null && chobinSetting.Chobins != null && i < chobinSetting.Chobins.Length)
            {
                chobinSetting.Chobins[i].SetSelectButton(chobinButtonObject, waitingButtonType.ButtonOffset);
            }
=======
            ChobinButton chobinButton = chobinButtons[i];
            chobinButton.Init();
>>>>>>> Stashed changes

#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
<<<<<<< Updated upstream
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
        chobinSetting.Init();
    }

    private int GetChobinCount()
    {
        if (chobinSetting == null || chobinSetting.Chobins == null) return 0;
        return chobinSetting.Chobins.Length;
=======
                chobinButton.SetActive(false);
            }
            else
            {
                switch (showButtonStateInEditor)
                {
                    case ButtonState.None:
                        chobinButton.SetActive(false);
                        break;
                    case ButtonState.Waiting:
                        SetWaitingButton(i);
                        chobinButton.SetActive(true);
                        break;
                    case ButtonState.Performing:
                        SetPerformingButton(i);
                        chobinButton.SetActive(true);
                        break;
                }
            }
#else
            chobinButton.SetActive(false);
#endif
        }
    }
    
    public void HideButton(int chobinId)
    {
        if (chobinButtons == null) return;
        else if (chobinId >= chobinButtons.Length) return;
        ChobinButton chobinButton = chobinButtons[chobinId];
        if (chobinButton != null)
        {
            chobinButton.SetActive(false);
        }
    }

    public void SetWaitingButton(int chobinId)
    {
        int id = chobinId; // ローカル変数にコピーしてクロージャの問題を回避
        void Action() => showCommandEvent.Invoke(id);
        ResetButton(chobinId, waitingButtonType, Action);
    }

    public void SetPerformingButton(int chobinId)
    {
        int id = chobinId; // ローカル変数にコピーしてクロージャの問題を回避
        void Action() => quitCommandEvent.Invoke(id);
        ResetButton(chobinId, performingButtonType, Action);
    }

    private void ResetButton(int chobinId, ButtonType buttonType, UnityAction action)
    {
        if (chobinButtons == null) return;
        else if (chobinId >= chobinButtons.Length) return;
        ChobinButton chobinButton = chobinButtons[chobinId];
        if (chobinButton != null)
        {
            chobinButton.Set(chobins[chobinId].transform, action);
            chobinButton.SetAppearance(buttonType.Sprite, buttonType.Size, buttonType.Offset);
            chobinButton.SetActive(true);
        }
    }

    public void SetButtonDirection(float angle)
    {
        if (chobinButtons == null) return;
        foreach (var chobinButton in chobinButtons)
        {
            chobinButton.SetButtonDirection(angle);
        }
>>>>>>> Stashed changes
    }
}