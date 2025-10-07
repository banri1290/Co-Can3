using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ゲーム全体の進行、主要なコンポーネント間の連携、
/// および起動時の初期設定のチェックを管理するコアスクリプトです。
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- マスターデータ設定 ---
    [Header("Master Data Settings")]
    [Tooltip("調理に使うすべての材料（Material）のリスト。メニュー画面での選択肢として使われます。")]
    [SerializeField] private Material[] materials;

    [Tooltip("プレイヤー（Chobin）が実行できるすべてのアクション（Action）のリスト。")]
    [SerializeField] private Action[] actions;

    // --- 主要な制御コンポーネント ---
    [Header("UI & Command Control")]
    [Tooltip("UIから調理コマンドを受け付け、表示を管理するビヘイビア。")]
    [SerializeField] private CookingCommandBehaviour cookingCommandBehaviour;

    [Header("Chobin Buttons Control")]
    [Tooltip("プレイヤー（Chobin）の選択とコマンド入力を管理するUIコンポーネント。")]
    [SerializeField] private ChobinButtonsCtrl chobinButtonsCtrl;

    [Header("Guest & Order Management")]
    [Tooltip("ゲスト（客）の発生、注文、料理の提供を制御するコンポーネント。")]
    [SerializeField] private GuestCtrl guestCtrl;
    [Header("Camera Settings")]
    [Tooltip("ゲーム内のカメラ制御を担当するコンポーネント。")]
    [SerializeField] private RoundCamera roundCamera;
    [Header("Score Calclating")]
    [Tooltip("料理のスコア計算を担当するコンポーネント。")]
    [SerializeField] private CookingScoreCalclater cookingScoreCalclater;

    private ChobinSetting chobinSetting;

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
        // 各コンポーネントの検証結果をリストに集約
        var validationResults = new System.Collections.Generic.List<bool>();

        if (materials.Length == 0)
        {
            Debug.LogError("材料のリストが空です。少なくとも1つの調理法を追加してください。");
            validationResults.Add(false);
        }
        if (actions.Length == 0)
        {
            Debug.LogError("調理法リストが空です。少なくとも1つの調理法を追加してください。");
            validationResults.Add(false);
        }
        else
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].KitchinSpot == null)
                {
                    Debug.LogError($"調理法 {actions[i].Name} に調理するTransformが設定されていません。");
                    validationResults.Add(false);
                }
            }
        }

        // 各コンポーネントの検証をヘルパーメソッドで行う
        validationResults.Add(ValidateComponent(cookingCommandBehaviour, "指示UIを管理するオブジェクト"));
        validationResults.Add(ValidateComponent(chobinButtonsCtrl, "チョビンオブジェクト"));
        validationResults.Add(ValidateComponent(guestCtrl, "客を出現させて管理するオブジェクト"));
        validationResults.Add(ValidateComponent(roundCamera, "CameraCtrlのオブジェクト"));
        validationResults.Add(ValidateComponent(cookingScoreCalclater, "CookingScoreCalclaterのオブジェクト"));

        // ChobinSettingはChobinButtonsCtrlに依存しているため、個別で処理
        if (chobinButtonsCtrl != null && chobinButtonsCtrl.ChobinSetting != null)
        {
            chobinSetting = chobinButtonsCtrl.ChobinSetting;
            validationResults.Add(ValidateComponent(chobinSetting, "チョビンの設定"));
        }

        // 全ての検証が通ったかチェック
        if (validationResults.TrueForAll(result => result))
        {
            Init();
        }
    }

    /// <summary>
    /// GameSystemを継承したコンポーネントのnullチェックと設定検証を行うヘルパーメソッド。
    /// </summary>
    private bool ValidateComponent(GameSystem component, string componentName)
    {
        if (component == null)
        {
            Debug.LogError($"{componentName}が設定されていません。");
            return false;
        }

        component.CheckAllSettings.RemoveAllListeners();
        component.CheckAllSettings.AddListener(CheckSettingOnValidate);

        if (!component.CheckSettings())
        {
            Debug.LogError($"{componentName}の設定に不備があります。");
            return false;
        }

        return true;
    }

    private void Init()
    {
        InitCookingCommand();
        InitChobinSetting();
        InitGuestCtrl();
        InitRoundCamera();
        cookingScoreCalclater.Init();

        Debug.Log("GameManagerの初期化が正常に完了しました。");
    }

    private void InitCookingCommand()
    {
        // イベントリスナーを一度クリアしてから再登録
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
        // CookingCommandBehaviourから正しい指示数を取得してChobinSettingに設定
        chobinSetting.SetCommandCount(cookingCommandBehaviour.CommandCount);

        chobinButtonsCtrl.ShowCommand.RemoveAllListeners();
        chobinButtonsCtrl.ShowCommand.AddListener(ShowCommand);
        chobinButtonsCtrl.Init();
    }

    private void InitGuestCtrl()
    {
        guestCtrl.HasGuestWaitingForOrder.RemoveAllListeners();
        guestCtrl.LeftGuestWaitingForOrder.RemoveAllListeners();
        guestCtrl.HasGuestWaitingForOrder.AddListener(() => InformGuestWaitingForOrder(true));
        guestCtrl.LeftGuestWaitingForOrder.AddListener(() => InformGuestWaitingForOrder(false));
        guestCtrl.Init();
    }

    private void InitRoundCamera()
    {
        void setButtonDirection(float angle)
        {
            for (int i = 0; i < chobinSetting.Chobins.Length; i++)
            {
                chobinSetting.Chobins[i].SetButtonDirection(angle);
            }
        }

        roundCamera.ChangeRotate.RemoveAllListeners();
        roundCamera.ChangeRotate.AddListener((v) => setButtonDirection(v));
        roundCamera.Init();
    }

    private void InitCommandTexts(int chobinIndex)
    {
        ChobinBehaviour chobin = GetChobin(chobinIndex);
        for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
        {
            // チョビンが現在記憶している材料と調理法をUIに反映させる
            int materialIndex = chobin.MaterialIndex[i];
            int actionIndex = chobin.ActionIndex[i];
            cookingCommandBehaviour.UpdateMaterial(i, materials[materialIndex]);
            cookingCommandBehaviour.UpdateAction(i, actions[actionIndex]);
        }
    }

    private ChobinBehaviour GetChobin(int i)
    {
        return chobinSetting.Chobins[i];
    }

    private void SetPreviousMaterial(int currentChobinUIIndex, int commandIndex)
    {
        int materialIndex = (GetChobin(currentChobinUIIndex).MaterialIndex[commandIndex] - 1 + materials.Length) % materials.Length;
        SetMaterial(currentChobinUIIndex, commandIndex, materialIndex);
    }

    private void SetNextMaterial(int currentChobinUIIndex, int commandIndex)
    {
        int materialIndex = (GetChobin(currentChobinUIIndex).MaterialIndex[commandIndex] + 1) % materials.Length;
        SetMaterial(currentChobinUIIndex, commandIndex, materialIndex);
    }

    private void SetPreviousAction(int currentChobinUIIndex, int commandIndex)
    {
        int actionIndex = (GetChobin(currentChobinUIIndex).ActionIndex[commandIndex] - 1 + actions.Length) % actions.Length;
        SetAction(currentChobinUIIndex, commandIndex, actionIndex);
    }

    private void SetNextAction(int currentChobinUIIndex, int commandIndex)
    {
        int actionIndex = (GetChobin(currentChobinUIIndex).ActionIndex[commandIndex] + 1) % actions.Length;
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
            Debug.LogError($"材料インデックス {materialIndex} が範囲外です。0から{materials.Length - 1}の範囲で指定してください。");
            return;
        }
        GetChobin(currentChobinUIIndex).SetMaterial(commandIndex, materialIndex);
#if UNITY_EDITOR
        if (EditorApplication.delayCall == null)
        {
            cookingCommandBehaviour.UpdateMaterial(commandIndex, materials[materialIndex]); // 材料のUIを更新
        }
#else
        cookingCommandBehaviour.UpdateMaterial(commandIndex, materials[materialIndex]); // 材料のUIを更新
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
        GetChobin(currentChobinUIIndex).SetAction(commandIndex, actionIndex);

#if UNITY_EDITOR
        if (EditorApplication.delayCall == null)
        {
            cookingCommandBehaviour.UpdateAction(commandIndex, actions[actionIndex]); // 調理法のUIを更新
        }
#else
        cookingCommandBehaviour.UpdateAction(commandIndex, actions[actionIndex]); // 調理法のUIを更新
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

    /// <summary>
    /// 料理を提供したときに呼び出されるメソッド
    /// </summary>
    /// <param name="chobinIndex">
    /// 料理を提供したチョビンのインデックス
    /// </param>
    private void SendServeData(int chobinIndex)
    {
        guestCtrl.ServeDish();
        // 料理を受け取った客
        GuestBehaviour guest = guestCtrl.GetServedGuest();
        // 料理を提供したチョビン
        ChobinBehaviour chobin = GetChobin(chobinIndex);
        // 提供した料理の材料・調理法のIDを取得
        int[] materialIndices = new int[cookingCommandBehaviour.CommandCount];
        int[] actionIndices = new int[cookingCommandBehaviour.CommandCount];
        for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
        {
            materialIndices[i] = chobin.MaterialIndex[i];
            actionIndices[i] = chobin.ActionIndex[i];
        }
        // 客の待ち時間を取得
        float waitingTime = guest.WaitingTimer;

        Debug.Log($"チョビン{chobinIndex}が客{guest.ID}に料理を提供しました。材料ID: [{string.Join(", ", materialIndices)}]、調理法ID: [{string.Join(", ", actionIndices)}]、待ち時間: {waitingTime:F2}秒");

        // CookingScoreCalclaterに料理・客の情報を渡してスコア計算を実行させる
        //cookingScoreCalclater.CalculateScore(materialIndices, actionIndices, waitingTime);
    }

    private void SubmitCommand(int chobinIndex)
    {
        void serveDish() => SendServeData(chobinIndex);
        Transform[] target = new Transform[cookingCommandBehaviour.CommandCount];
        for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
        {
            target[i] = actions[GetChobin(chobinIndex).ActionIndex[i]].KitchinSpot;
        }
        GetChobin(chobinIndex).SetCommand(serveDish, target);
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
            Debug.Log("Playモードに移行前のため、設定のチェックと初期化をスキップします。");
            return;
        }
        CheckSettings();
#endif
    }
}
