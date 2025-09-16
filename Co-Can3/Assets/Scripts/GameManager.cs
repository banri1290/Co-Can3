using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct Material
    {
        [SerializeField] private string name; // 食材の名前
        public string Name => name; // 食材の名前を取得
    }
    [System.Serializable]
    public struct Action
    {
        [SerializeField] private string name; // 調理法の名前
        [SerializeField] private Transform kitchinSpot; // 調理場の位置

        public string Name => name; // 調理法の名前を取得
        public Transform KitchinSpot => kitchinSpot; //調理場の位置を取得
    }
    [SerializeField] private Material[] materials; // 食材の配列
    [SerializeField] private Action[] actions; // 調理法の配列

    [Header("指示UIを制御するオブジェクト")]
    [SerializeField] private CookingCommandBehaviour cookingCommandBehaviour; // 指示UIを制御するオブジェクト
    [Header("チョビンの設定を一括して管理するオブジェクト")]
    [SerializeField] private ChobinSetting chobinSetting; // チョビンの設定を一括して管理するオブジェクト
    [Header("客を呼び出して管理するオブジェクト")]
    [SerializeField] private GuestCtrl guestCtrl; // 客を呼び出して管理するオブジェクト

    // Start is called before the first frame update
    void Start()
    {
        CheckSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CheckSettings()
    {
        bool AllSettingsAreCorrect = true;

        if (materials.Length == 0)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("食材リストが空です。少なくとも1つの食材を追加してください。");
        }
        if (actions.Length == 0)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("調理法リストが空です。少なくとも1つの調理法を追加してください。");
        }
        else
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].KitchinSpot == null)
                {
                    AllSettingsAreCorrect = false;
                    Debug.LogError($"調理法 {actions[i].Name} に調理場のTransformが設定されていません。");
                }
            }
        }

        if (cookingCommandBehaviour == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("指示UIを制御するオブジェクトが設定されていません。");
        }
        else
        {
            cookingCommandBehaviour.CheckAllSettings.RemoveAllListeners();
            cookingCommandBehaviour.CheckAllSettings.AddListener(CheckSettingOnValidate);

            bool cookingCommandBehaviourSettingsAreCorrect = cookingCommandBehaviour.CheckSettings();
            if (!cookingCommandBehaviourSettingsAreCorrect)
            {
                AllSettingsAreCorrect = false;
                Debug.LogError("指示UIを制御するオブジェクトの設定に誤りがあります。");
            }
        }

        if (chobinSetting == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("チョビンの設定を一括して管理するオブジェクトが設定されていません。");
        }
        else
        {
            chobinSetting.CheckAllSettings.RemoveAllListeners();
            chobinSetting.CheckAllSettings.AddListener(CheckSettingOnValidate);

            bool chobinSettingSettingsAreCorrect = chobinSetting.CheckSettings();
            if (!chobinSettingSettingsAreCorrect)
            {
                AllSettingsAreCorrect = false;
                Debug.LogError("チョビンの設定を一括して管理するオブジェクトの設定に誤りがあります。");
            }
        }

        if (guestCtrl == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("客を呼び出して管理するオブジェクトが設定されていません。");
        }
        else
        {
            guestCtrl.CheckAllSettings.RemoveAllListeners();
            guestCtrl.CheckAllSettings.AddListener(CheckSettingOnValidate);

            bool guestCtrlSettingsAreCorrect = guestCtrl.CheckSetings();
            if (!guestCtrlSettingsAreCorrect)
            {
                AllSettingsAreCorrect = false;
                Debug.LogError("客を呼び出して管理するオブジェクトの設定に誤りがあります。");
            }
        }

        if (AllSettingsAreCorrect)
        {
            Init();
        }
    }

    private void Init()
    {
        InitCookingCommand();
        InitChobinSetting();
        InitGuestCtrl();

        Debug.Log("GameManagerの初期化が正常に完了しました。");
    }

    private void InitCookingCommand()
    {
        cookingCommandBehaviour.PreviousMaterialEvent.RemoveAllListeners();
        cookingCommandBehaviour.NextMaterialEvent.RemoveAllListeners();
        cookingCommandBehaviour.PreviousMaterialEvent.RemoveAllListeners();
        cookingCommandBehaviour.NextActionEvent.RemoveAllListeners();

        cookingCommandBehaviour.PreviousMaterialEvent.AddListener(SetPreviousMaterial);
        cookingCommandBehaviour.NextMaterialEvent.AddListener(SetNextMaterial);
        cookingCommandBehaviour.PreviousActionEvent.AddListener(SetPreviousAction);
        cookingCommandBehaviour.NextActionEvent.AddListener(SetNextAction);

        cookingCommandBehaviour.Init();
    }

    private void InitChobinSetting()
    {
        chobinSetting.SetCommandCount(cookingCommandBehaviour.CommandCount);
        chobinSetting.ShowCommand.RemoveAllListeners();
        chobinSetting.ShowCommand.AddListener(ShowCommand);
        chobinSetting.Init();
    }

    private void InitGuestCtrl()
    {
        guestCtrl.HasGuestWaitingForOrder.RemoveAllListeners();
        guestCtrl.LeftGuestWaitingForOrder.RemoveAllListeners();
        guestCtrl.HasGuestWaitingForOrder.AddListener(()=> InformGuestWaitingForOrder(true));
        guestCtrl.LeftGuestWaitingForOrder.AddListener(() => InformGuestWaitingForOrder(false));
        guestCtrl.Init();
    }

    private void InitCommandTexts(int chobinIndex)
    {
        for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
        {
            SetMaterial(chobinIndex, i, 0); // 各チョビンの各指示の食材を初期化（最初の食材に設定）
            SetAction(chobinIndex, i, 0); // 各チョビンの各指示の調理法を初期化（最初の調理法に設定）
        }
    }

    private ChobinBehaviour GetChobin(int i)
    {
        return chobinSetting.Chobins[i];
    }

    private void SetPreviousMaterial(int currentChobinUIIndex, int commandIndex)
    {
        int materialIndex = (GetChobin(currentChobinUIIndex).materialIndex[commandIndex] - 1 + materials.Length) % materials.Length;
        SetMaterial(currentChobinUIIndex, commandIndex, materialIndex);
    }

    private void SetNextMaterial(int currentChobinUIIndex, int commandIndex)
    {
        int materialIndex = (GetChobin(currentChobinUIIndex).materialIndex[commandIndex] + 1) % materials.Length;
        SetMaterial(currentChobinUIIndex, commandIndex, materialIndex);
    }

    private void SetPreviousAction(int currentChobinUIIndex, int commandIndex)
    {
        int actionIndex = (GetChobin(currentChobinUIIndex).actionIndex[commandIndex] - 1 + actions.Length) % actions.Length;
        SetAction(currentChobinUIIndex, commandIndex, actionIndex);
    }

    private void SetNextAction(int currentChobinUIIndex, int commandIndex)
    {
        int actionIndex = (GetChobin(currentChobinUIIndex).actionIndex[commandIndex] + 1) % actions.Length;
        SetAction(currentChobinUIIndex, commandIndex, actionIndex);
    }

    private void SetMaterial(int currentChobinUIIndex, int commandIndex, int materialIndex)
    {
        int commandCount = cookingCommandBehaviour.CommandCount;
        if (commandIndex < 0 || commandIndex >= commandCount)
        {
            Debug.LogError($"指示インデックス {commandIndex} が範囲外です。0から{commandCount - 1}の範囲で指定してください。");
            return;
        }
        if (materialIndex < 0 || materialIndex >= materials.Length)
        {
            Debug.LogError($"食材インデックス {materialIndex} が範囲外です。0から{materials.Length - 1}の範囲で指定してください。");
            return;
        }
        GetChobin(currentChobinUIIndex).SetMaterial(commandIndex, materialIndex);
#if UNITY_EDITOR
        if (EditorApplication.delayCall == null)
        {
            cookingCommandBehaviour.UpdateMaterialText(commandIndex, materials[materialIndex].Name); // 食材のテキストを更新
        }
#else
        cookingCommandBehaviour.UpdateMaterialText(commandIndex, materials[materialIndex].Name); // 食材のテキストを更新
#endif
    }
    private void SetAction(int currentChobinUIIndex, int commandIndex, int actionIndex)
    {
        int commandCount = cookingCommandBehaviour.CommandCount;
        if (commandIndex < 0 || commandIndex >= commandCount)
        {
            Debug.LogError($"指示インデックス {commandIndex} が範囲外です。0から{commandCount - 1}の範囲で指定してください。");
            return;
        }
        if (actionIndex < 0 || actionIndex >= actions.Length)
        {
            Debug.LogError($"調理法インデックス {actionIndex} が範囲外です。0から{actions.Length - 1}の範囲で指定してください。");
            return;
        }
        GetChobin(currentChobinUIIndex).SetAction(commandIndex, actionIndex, actions[actionIndex].KitchinSpot);

#if UNITY_EDITOR
        if (EditorApplication.delayCall == null)
        {
            cookingCommandBehaviour.UpdateActionText(commandIndex, actions[actionIndex].Name); // 調理法のテキストを更新
        }
#else
        cookingCommandBehaviour.UpdateActionText(commandIndex, actions[actionIndex].Name); // 調理法のテキストを更新
#endif
    }

    private void ShowCommand(int chobinIndex)
    {
        if (chobinIndex < 0 || chobinIndex >= chobinSetting.Chobins.Length)
        {
            Debug.LogError($"チョビンインデックス {chobinIndex} が範囲外です。0から{chobinSetting.Chobins.Length - 1}の範囲で指定してください。");
            return;
        }
        cookingCommandBehaviour.SubmitCommandEvent.RemoveAllListeners();
        cookingCommandBehaviour.SubmitCommandEvent.AddListener(() => SubmitCommand(chobinIndex));
        cookingCommandBehaviour.ShowCommand(chobinIndex);
        InitCommandTexts(chobinIndex);
    }

    private void SubmitCommand(int chobinIndex)
    {
        UnityEvent serveDish = new();
        serveDish.AddListener(() => guestCtrl.ServeDish());
        GetChobin(chobinIndex).SetCommand(serveDish);
        guestCtrl.ReceiveOrder();
    }

    private void InformGuestWaitingForOrder(bool b)
    {
        for (int i = 0; i < chobinSetting.Chobins.Length; i++)
        {
            GetChobin(i).SetHasGuestFlag(b);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CheckSettingOnValidate();
    }
#endif
    private void CheckSettingOnValidate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.Log("Playモードに入る直前のため、設定のチェックと初期化をスキップします。");
            return;
        }
        CheckSettings();
#endif
    }
}
