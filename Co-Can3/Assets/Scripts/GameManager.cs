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
        [SerializeField] private string name; // �H�ނ̖��O
        public string Name => name; // �H�ނ̖��O���擾
    }
    [System.Serializable]
    public struct Action
    {
        [SerializeField] private string name; // �����@�̖��O
        [SerializeField] private Transform kitchinSpot; // ������̈ʒu

        public string Name => name; // �����@�̖��O���擾
        public Transform KitchinSpot => kitchinSpot; //������̈ʒu���擾
    }
    [SerializeField] private Material[] materials; // �H�ނ̔z��
    [SerializeField] private Action[] actions; // �����@�̔z��

    [Header("�w��UI�𐧌䂷��I�u�W�F�N�g")]
    [SerializeField] private CookingCommandBehaviour cookingCommandBehaviour; // �w��UI�𐧌䂷��I�u�W�F�N�g
    [Header("�`���r���̐ݒ���ꊇ���ĊǗ�����I�u�W�F�N�g")]
    [SerializeField] private ChobinSetting chobinSetting; // �`���r���̐ݒ���ꊇ���ĊǗ�����I�u�W�F�N�g
    [Header("�q���Ăяo���ĊǗ�����I�u�W�F�N�g")]
    [SerializeField] private GuestCtrl guestCtrl; // �q���Ăяo���ĊǗ�����I�u�W�F�N�g

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
            Debug.LogError("�H�ރ��X�g����ł��B���Ȃ��Ƃ�1�̐H�ނ�ǉ����Ă��������B");
        }
        if (actions.Length == 0)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("�����@���X�g����ł��B���Ȃ��Ƃ�1�̒����@��ǉ����Ă��������B");
        }
        else
        {
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i].KitchinSpot == null)
                {
                    AllSettingsAreCorrect = false;
                    Debug.LogError($"�����@ {actions[i].Name} �ɒ������Transform���ݒ肳��Ă��܂���B");
                }
            }
        }

        if (cookingCommandBehaviour == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("�w��UI�𐧌䂷��I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }
        else
        {
            cookingCommandBehaviour.CheckAllSettings.RemoveAllListeners();
            cookingCommandBehaviour.CheckAllSettings.AddListener(CheckSettingOnValidate);

            bool cookingCommandBehaviourSettingsAreCorrect = cookingCommandBehaviour.CheckSettings();
            if (!cookingCommandBehaviourSettingsAreCorrect)
            {
                AllSettingsAreCorrect = false;
                Debug.LogError("�w��UI�𐧌䂷��I�u�W�F�N�g�̐ݒ�Ɍ�肪����܂��B");
            }
        }

        if (chobinSetting == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("�`���r���̐ݒ���ꊇ���ĊǗ�����I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }
        else
        {
            chobinSetting.CheckAllSettings.RemoveAllListeners();
            chobinSetting.CheckAllSettings.AddListener(CheckSettingOnValidate);

            bool chobinSettingSettingsAreCorrect = chobinSetting.CheckSettings();
            if (!chobinSettingSettingsAreCorrect)
            {
                AllSettingsAreCorrect = false;
                Debug.LogError("�`���r���̐ݒ���ꊇ���ĊǗ�����I�u�W�F�N�g�̐ݒ�Ɍ�肪����܂��B");
            }
        }

        if (guestCtrl == null)
        {
            AllSettingsAreCorrect = false;
            Debug.LogError("�q���Ăяo���ĊǗ�����I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }
        else
        {
            guestCtrl.CheckAllSettings.RemoveAllListeners();
            guestCtrl.CheckAllSettings.AddListener(CheckSettingOnValidate);

            bool guestCtrlSettingsAreCorrect = guestCtrl.CheckSetings();
            if (!guestCtrlSettingsAreCorrect)
            {
                AllSettingsAreCorrect = false;
                Debug.LogError("�q���Ăяo���ĊǗ�����I�u�W�F�N�g�̐ݒ�Ɍ�肪����܂��B");
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

        Debug.Log("GameManager�̏�����������Ɋ������܂����B");
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
            SetMaterial(chobinIndex, i, 0); // �e�`���r���̊e�w���̐H�ނ��������i�ŏ��̐H�ނɐݒ�j
            SetAction(chobinIndex, i, 0); // �e�`���r���̊e�w���̒����@���������i�ŏ��̒����@�ɐݒ�j
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
            Debug.LogError($"�w���C���f�b�N�X {commandIndex} ���͈͊O�ł��B0����{commandCount - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        if (materialIndex < 0 || materialIndex >= materials.Length)
        {
            Debug.LogError($"�H�ރC���f�b�N�X {materialIndex} ���͈͊O�ł��B0����{materials.Length - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        GetChobin(currentChobinUIIndex).SetMaterial(commandIndex, materialIndex);
#if UNITY_EDITOR
        if (EditorApplication.delayCall == null)
        {
            cookingCommandBehaviour.UpdateMaterialText(commandIndex, materials[materialIndex].Name); // �H�ނ̃e�L�X�g���X�V
        }
#else
        cookingCommandBehaviour.UpdateMaterialText(commandIndex, materials[materialIndex].Name); // �H�ނ̃e�L�X�g���X�V
#endif
    }
    private void SetAction(int currentChobinUIIndex, int commandIndex, int actionIndex)
    {
        int commandCount = cookingCommandBehaviour.CommandCount;
        if (commandIndex < 0 || commandIndex >= commandCount)
        {
            Debug.LogError($"�w���C���f�b�N�X {commandIndex} ���͈͊O�ł��B0����{commandCount - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        if (actionIndex < 0 || actionIndex >= actions.Length)
        {
            Debug.LogError($"�����@�C���f�b�N�X {actionIndex} ���͈͊O�ł��B0����{actions.Length - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        GetChobin(currentChobinUIIndex).SetAction(commandIndex, actionIndex, actions[actionIndex].KitchinSpot);

#if UNITY_EDITOR
        if (EditorApplication.delayCall == null)
        {
            cookingCommandBehaviour.UpdateActionText(commandIndex, actions[actionIndex].Name); // �����@�̃e�L�X�g���X�V
        }
#else
        cookingCommandBehaviour.UpdateActionText(commandIndex, actions[actionIndex].Name); // �����@�̃e�L�X�g���X�V
#endif
    }

    private void ShowCommand(int chobinIndex)
    {
        if (chobinIndex < 0 || chobinIndex >= chobinSetting.Chobins.Length)
        {
            Debug.LogError($"�`���r���C���f�b�N�X {chobinIndex} ���͈͊O�ł��B0����{chobinSetting.Chobins.Length - 1}�͈̔͂Ŏw�肵�Ă��������B");
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
            Debug.Log("Play���[�h�ɓ��钼�O�̂��߁A�ݒ�̃`�F�b�N�Ə��������X�L�b�v���܂��B");
            return;
        }
        CheckSettings();
#endif
    }
}
