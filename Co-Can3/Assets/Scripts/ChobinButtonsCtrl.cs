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

        public Vector3 ButtonOffset => buttonOffset;

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

    [Header("エディタ設定")]
    [Tooltip("エディタ再生中以外でもボタンを表示するかどうか")]
    [SerializeField] private bool buttonIsActiveInEditor = true;

    private EventWithInt showCommandEvent = new();
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
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン用のCanvasオブジェクトが設定されていません。");
        }
        if (chobinButtonPrefab == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("チョビン用のボタンプレハブが設定されていません。");
        }
        else if (chobinButtonPrefab.GetComponent<Button>() == null || chobinButtonPrefab.GetComponent<Image>() == null || chobinButtonPrefab.GetComponent<RectTransform>() == null)
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
            bool changeChobinCount = (chobinButtonCanvas.transform.childCount != GetChobinCount());
            if (changeChobinCount)
            {
                Debug.Log("チョビンの数が変更されたため、チョビン用ボタンを再生成します。");
                EditorApplication.delayCall += InitChobinButtonParent; // チョビン用ボタンの子オブジェクトを削除
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
        for (int i = 0; i < GetChobinCount(); i++)
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
        EditorApplication.delayCall -= InitChobinButtonParent; // 重複して実行されるのを防ぐために削除
#endif
    }

    private void InitChobins()
    {
        ButtonType buttonType = waitingButtonType;
        waitingButtonType.AdjustSizeToSprite();

        for (int i = 0; i < GetChobinCount(); i++)
        {
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
        chobinSetting.Init();
    }

    private int GetChobinCount()
    {
        if (chobinSetting == null || chobinSetting.Chobins == null) return 0;
        return chobinSetting.Chobins.Length;
    }
}