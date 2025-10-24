using UnityEngine;
<<<<<<< Updated upstream
using UnityEngine.Events;
using TMPro;
using System.Collections;
=======
using System.Collections.Generic;
using TMPro;


>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
=======
    [Header("Chobin Setting")]
    [Tooltip("プレイヤーキャラクター（Chobin）の基本設定を管理するコンポーネント。")]
    [SerializeField] private ChobinSetting chobinSetting;

    [Header("Chobin Manager")]
    [Tooltip("料理人キャラクター（Chobin）の状態と行動を管理するコンポーネント。")]
    [SerializeField] private ChobinManager chobinManager;

>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
       [Header("UI References")]
    [SerializeField] private TextMeshProUGUI serveCountText;  // 提供数表示
    [SerializeField] private TextMeshProUGUI totalScoreText;  // スコア表示
    [SerializeField] private TextMeshProUGUI totalSumText;    // 合計（提供数＋スコア）表示
   
private int totalScore = 0;              // 全体スコア
private int guestProcessedCount = 0;     // 処理済みの客数（退店した客）
  private int servedCount = 0;     // 提供数

    private ChobinSetting chobinSetting;
=======
    [SerializeField] private TextMeshProUGUI serveCountText;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI totalSumText;

    int totalScore = 0;
    int guestProcessedCount = 0;
    int servedCount = 0;

    private ChobinBehaviour GetChobin(int i) => chobinSetting.Chobins[i];

    private int CompareChoninAndGuest()
    {
        int cookingNum = chobinManager.CurrentCookingNum;
        int waitingNum = guestCtrl.WaitingGuestNum;
        return cookingNum - waitingNum;
    }
>>>>>>> Stashed changes

    // Start is called before the first frame update
    void Start()
    {
        CheckSettings();
<<<<<<< Updated upstream
         // 🎯 開始時は UI を完全に非表示にする
     serveCountText?.gameObject.SetActive(false);
        totalScoreText?.gameObject.SetActive(false);
        totalSumText?.gameObject.SetActive(false);
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        validationResults.Add(ValidateComponent(chobinButtonsCtrl, "チョビンオブジェクト"));
=======
        validationResults.Add(ValidateComponent(chobinSetting, "ChobinSettingのオブジェクト"));
        validationResults.Add(ValidateComponent(chobinManager, "ChobinManagerのオブジェクト"));
        validationResults.Add(ValidateComponent(chobinButtonsCtrl, "チョビンのボタンUIを管理するオブジェクト"));
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream

=======
        chobinSetting.Init();

        for (int i = 0; i < chobinSetting.Chobins.Length; i++)
        {
            // 各チョビンに料理提供と帰還のイベントを設定
            int index = i; // ローカルコピーを作成してクロージャ問題を回避
            GetChobin(index).SetEvents(
                () => SendServeData(index),
                JudgeNeedToCook
            );
        }

>>>>>>> Stashed changes
        chobinButtonsCtrl.ShowCommand.RemoveAllListeners();
        chobinButtonsCtrl.ShowCommand.AddListener(ShowCommand);
<<<<<<< Updated upstream
=======
        chobinButtonsCtrl.QuitCommand.AddListener(ForceQuitCommand);
        chobinButtonsCtrl.SetChobins(chobinSetting.Chobins);
>>>>>>> Stashed changes
        chobinButtonsCtrl.Init();
    }

    private void InitGuestCtrl()
    {
<<<<<<< Updated upstream
        guestCtrl.HasGuestWaitingForOrder.RemoveAllListeners();
        guestCtrl.LeftGuestWaitingForOrder.RemoveAllListeners();
        guestCtrl.HasGuestWaitingForOrder.AddListener(() => InformGuestWaitingForOrder(true));
        guestCtrl.LeftGuestWaitingForOrder.AddListener(() => InformGuestWaitingForOrder(false));
=======
        guestCtrl.HasComeGuest.RemoveAllListeners();
        guestCtrl.HasComeGuest.AddListener(JudgeNeedToCook);
        guestCtrl.AllGuestExit.RemoveAllListeners();
        guestCtrl.AllGuestExit.AddListener(ShowTotalScore);
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        roundCamera.ChangeRotate.AddListener((v) => setButtonDirection(v));
=======
        roundCamera.ChangeRotate.AddListener((val) => setButtonDirection(val));
>>>>>>> Stashed changes
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
    // 🍽️ サーブ処理
    guestCtrl.ServeDish();

    // 🧍‍♂️ 料理を受け取った客を取得
    GuestBehaviour guest = guestCtrl.GetServedGuest();
    if (guest == null)
    {
<<<<<<< Updated upstream
        Debug.LogWarning("提供対象の客が見つかりません。");
        return;
=======
        guestCtrl.ServeDish();
        chobinManager.DecrementCookingNum();
        chobinButtonsCtrl.HideButton(chobinIndex);
        // 料理を受け取った客
        GuestBehaviour guest = guestCtrl.GetServedGuest();

        // ✅ 調理時間を取得（GetCookingTimeを利用）
        float cookingTime = guest.GetCookingTime();

        // 👨‍🍳 提供したチョビンを取得
        ChobinBehaviour chobin = GetChobin(chobinIndex);

        // 🧂 提供した料理の材料・調理法のIDを取得
        int[] materialIndices = new int[cookingCommandBehaviour.CommandCount];
        int[] actionIndices = new int[cookingCommandBehaviour.CommandCount];

        for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
        {
            materialIndices[i] = chobin.MaterialIndex[i];
            actionIndices[i] = chobin.ActionIndex[i];
        }

        // 🍳 Dish データを作成
        Dish dish = new Dish();
        foreach (var id in materialIndices)
        {
            dish.AddIngredient($"材料{id}");
        }

        dish.Steps = actionIndices.Length;
        dish.CookTime = cookingTime; // ✅ ← 修正済み

        // 🧮 スコア計算
        int score = cookingScoreCalclater.CalculateScore(dish, guest);

        // 🎉 リアクションを表示
        guest.ShowReaction(score);

        // 🧾 デバッグ出力
        Debug.Log(
            $"チョビン{chobinIndex}が客{guest.ID}に料理を提供しました。" +
            $"材料ID: [{string.Join(", ", materialIndices)}]、" +
            $"調理法ID: [{string.Join(", ", actionIndices)}]、" +
            $"調理時間: {cookingTime:F2}秒、" +
            $"スコア: {score}"
        );

        // ✅ 後処理
        guest.StopWaiting();
        guest.StopCooking(); // ✅ ← 調理終了を明示
        guest.SetState(GuestBehaviour.Status.GotDish);

        totalScore += score;
        guestProcessedCount++;
        servedCount++; // ✅ 提供数をカウント追加

        Debug.Log($"👥 {guestProcessedCount}人目のスコアを加算。合計スコア: {totalScore}");

>>>>>>> Stashed changes
    }

    // ✅ 調理時間を取得（GetCookingTimeを利用）
    float cookingTime = guest.GetCookingTime();

    // 👨‍🍳 提供したチョビンを取得
    ChobinBehaviour chobin = GetChobin(chobinIndex);

    // 🧂 提供した料理の材料・調理法のIDを取得
    int[] materialIndices = new int[cookingCommandBehaviour.CommandCount];
    int[] actionIndices = new int[cookingCommandBehaviour.CommandCount];

    for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
    {
<<<<<<< Updated upstream
        materialIndices[i] = chobin.MaterialIndex[i];
        actionIndices[i] = chobin.ActionIndex[i];
    }

    // 🍳 Dish データを作成
    Dish dish = new Dish();
    foreach (var id in materialIndices)
    {
        dish.AddIngredient($"材料{id}");
    }

    dish.Steps = actionIndices.Length;
    dish.CookTime = cookingTime; // ✅ ← 修正済み

    // 🧮 スコア計算
    int score = cookingScoreCalclater.CalculateScore(dish, guest);

    // 🎉 リアクションを表示
    guest.ShowReaction(score);

    // 🧾 デバッグ出力
    Debug.Log(
        $"チョビン{chobinIndex}が客{guest.ID}に料理を提供しました。" +
        $"材料ID: [{string.Join(", ", materialIndices)}]、" +
        $"調理法ID: [{string.Join(", ", actionIndices)}]、" +
        $"調理時間: {cookingTime:F2}秒、" +
        $"スコア: {score}"
    );

    // ✅ 後処理
    guest.StopWaiting();
    guest.StopCooking(); // ✅ ← 調理終了を明示
    guest.SetState(GuestBehaviour.Status.GotDish);

     totalScore += score;
    guestProcessedCount++;
    servedCount++; // ✅ 提供数をカウント追加

    Debug.Log($"👥 {guestProcessedCount}人目のスコアを加算。合計スコア: {totalScore}");

    if (guestProcessedCount >= 5)
    {
        ShowTotalScore();
    }
}
/// <summary>
/// 提供数・スコア・合計をUIに反映
/// </summary>
private void UpdateScoreUI()
{
    int totalSum = servedCount + totalScore;

    if (serveCountText != null)
        serveCountText.text = $"提供数：{servedCount}";

    if (totalScoreText != null)
        totalScoreText.text = $"スコア：{totalScore}";

    if (totalSumText != null)
        totalSumText.text = $"合計：{totalSum}";
}
private void ShowTotalScore()
{
     int totalSum = servedCount + totalScore;

    string resultText = $"提供数：{servedCount}\n" +
                        $"スコア：{totalScore}\n" +
                        $"合計：{totalSum}";
    // 🎯 テキストをアクティブ化して内容を設定
    if (serveCountText != null)
    {
        serveCountText.gameObject.SetActive(true);
        serveCountText.text = $"提供数：{servedCount}";
    }

    if (totalScoreText != null)
    {
        totalScoreText.gameObject.SetActive(true);
        totalScoreText.text = $"スコア：{totalScore}";
    }

    if (totalSumText != null)
    {
        totalSumText.gameObject.SetActive(true);
        totalSumText.text = $"合計：{totalSum}";
    }
    Debug.Log($"🏁 全員処理完了！{resultText}");
}

// -------------------------------------------

private void SubmitCommand(int chobinIndex)
{
    void serveDish() => SendServeData(chobinIndex);

    Transform[] target = new Transform[cookingCommandBehaviour.CommandCount];
    for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
    {
        target[i] = actions[GetChobin(chobinIndex).ActionIndex[i]].KitchinSpot;
    }

    GetChobin(chobinIndex).SetCommand(serveDish, target);

    // 🍳 提供前に Guest を取得して調理開始
    GuestBehaviour guest = guestCtrl.GetServedGuest();
    if (guest != null)
    {
        guest.OnCookingFinished.RemoveAllListeners();
        guest.OnCookingFinished.AddListener(() =>
        {
            UpdateScoreUI(); // 提供ごとにUI更新
        });

        guest.StartCooking();
        Debug.Log($"🍳 Guest {guest.ID} started cooking.");
    }

    // 注文受付開始
    guestCtrl.ReceiveOrder();
}

private void InformGuestWaitingForOrder(bool b)
{
    for (int i = 0; i < chobinSetting.Chobins.Length; i++)
    {
        GetChobin(i).SetHasGuestFlag(b);
    }
}

  private IEnumerator ServeAfterDelay(GuestBehaviour guest, int chobinIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
          guest.StopCooking();
        SendServeData(chobinIndex);
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
=======
        Transform[] target = new Transform[cookingCommandBehaviour.CommandCount];
        for (int i = 0; i < cookingCommandBehaviour.CommandCount; i++)
        {
            target[i] = actions[GetChobin(chobinIndex).ActionIndex[i]].KitchinSpot;
        }

        GetChobin(chobinIndex).SetCommand(target);

        // 🍳 提供前に Guest を取得して調理開始
        GuestBehaviour guest = guestCtrl.GetOrderGuest();
        if (guest != null)
        {
            guest.OnCookingFinished.RemoveAllListeners();
            guest.OnCookingFinished.AddListener(() =>
            {
                UpdateScoreUI(); // 提供ごとにUI更新
            });

            guest.StartCooking();
            Debug.Log($"🍳 Guest {guest.ID} started cooking.");
        }

        chobinManager.IncrementCookingNum();
        if (CompareChoninAndGuest() > 0)
        {
            guestCtrl.ReceiveOrder();
        }
        chobinButtonsCtrl.SetPerformingButton(chobinIndex);
        JudgeNeedToCook();
    }
    /// <summary>
    /// 提供数・スコア・合計をUIに反映
    /// </summary>
    private void UpdateScoreUI()
    {
        int totalSum = servedCount + totalScore;

        if (serveCountText != null)
            serveCountText.text = $"提供数：{servedCount}";

        if (totalScoreText != null)
            totalScoreText.text = $"スコア：{totalScore}";

        if (totalSumText != null)
            totalSumText.text = $"合計：{totalSum}";
    }
    private void ShowTotalScore()
    {
        int totalSum = servedCount + totalScore;

        string resultText = $"提供数：{servedCount}\n" +
                            $"スコア：{totalScore}\n" +
                            $"合計：{totalSum}";
        // 🎯 テキストをアクティブ化して内容を設定
        if (serveCountText != null)
        {
            serveCountText.gameObject.SetActive(true);
            serveCountText.text = $"提供数：{servedCount}";
        }

        if (totalScoreText != null)
        {
            totalScoreText.gameObject.SetActive(true);
            totalScoreText.text = $"スコア：{totalScore}";
        }

        if (totalSumText != null)
        {
            totalSumText.gameObject.SetActive(true);
            totalSumText.text = $"合計：{totalSum}";
        }
        Debug.Log($"🏁 全員処理完了！{resultText}");
    }


    private void ForceQuitCommand(int chobinIndex)
    {
        GetChobin(chobinIndex).ForceQuitCommand();
        chobinManager.DecrementCookingNum();
        JudgeNeedToCook();
        chobinButtonsCtrl.HideButton(chobinIndex);
        guestCtrl.InformCookingQuit();
    }

    private void JudgeNeedToCook()
    {
        bool needToCook = CompareChoninAndGuest() < 0 || guestCtrl.HasGuestWaitingOrder;
        for (int i = 0; i < chobinSetting.Chobins.Length; i++)
        {
            if (GetChobin(i).IsCooking) continue;
            if (needToCook)
            {
                chobinButtonsCtrl.SetWaitingButton(i);
            }
            else
            {
                chobinButtonsCtrl.HideButton(i);
            }
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
>>>>>>> Stashed changes
