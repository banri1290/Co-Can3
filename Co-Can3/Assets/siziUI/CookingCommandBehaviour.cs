//======================================================================
// CookingCommandBehaviour.cs
// �����R�}���h��UI������s��MonoBehaviour
// �e�R�}���h�̍ޗ��E�A�N�V�����̃e�L�X�g�ƃ{�^���̔z�u�E������Ǘ����܂�
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
        [SerializeField] private string name; // �H�ނ̖��O
        public string Name => name; // �H�ނ̖��O���擾
    }
    [System.Serializable]
    private struct Action
    {
        [SerializeField] private string name; // �����@�̖��O
        [SerializeField] private Transform kitchinSpot; // ������̈ʒu

        public string Name => name; // �����@�̖��O���擾
        public Transform KitchinSpot => kitchinSpot; //������̈ʒu���擾
    }

    [Header("�`���r���̐ݒ�")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [SerializeField] private float performingTimeLength = 2f; // �����ɂ����鎞��
    [Header("�����ɐH�ށE�����@��I������UI�O���[�v��D&D")]
    [SerializeField] private GameObject commandUIPrefab; // UI�v���n�u��o�^
    [SerializeField] private GameObject commandUIParent; // UI�̐e�I�u�W�F�N�g��o�^
    [Header("�H�ށE�����@�̖��O���X�g")]
    [SerializeField] private Material[] MaterialList; // �H�ނ̃��X�g
    [SerializeField] private Action[] ActionList;

    [Header("���X�g�̔z�u")]
    [SerializeField] private int commandCount = 3; // �R�}���h�̐�
    [SerializeField] private Vector2 commandPosition;           // �R�}���h�̊�ʒu
    [SerializeField] private Vector2 commandDelta;              // �R�}���h���Ƃ̃I�t�Z�b�g
    [SerializeField] private Vector2 commandTextPosition;       // �e�L�X�g�̈ʒu
    [SerializeField] private Vector2 commandLeftButtonPosition; // ���{�^���̈ʒu
    [SerializeField] private Vector2 commandRightButtonPosition;// �E�{�^���̈ʒu
    [Header("�t�H���g�𑀍�")]
    [SerializeField] private TMP_FontAsset fontAsset;          // �g�p����t�H���g�A�Z�b�g
    [SerializeField] private Color fontColor = Color.white; // �t�H���g�J���[
    [SerializeField] private float fontSize = 16f;              // �t�H���g�T�C�Y
    [SerializeField] private Vector2 textSize = new Vector2(200, 30); // �e�L�X�g�T�C�Y
    [Header("�{�^���𑀍�")]
    [SerializeField] private Sprite leftButtonSprite;  // ���{�^���̃X�v���C�g
    [SerializeField] private Sprite rightButtonSprite; // �E�{�^���̃X�v���C�g
    [SerializeField] private Vector2 ButtonSize = new Vector2(100, 100); // �{�^���T�C�Y

    // UI�v�f�̎Q��
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

    // ����������
    void Start()
    {
        currentChobinUIIndex = 0;
        InitUIParent();
        for (int i = 0; i < commandCount; i++)
        {
            SetMaterial(i, 0); // ���������ɐH�ނ�ݒ�
            SetAction(i, 0); // ���������ɒ����@��ݒ�
        }
    }

    // ���t���[���̍X�V�����i���g�p�j
    void Update()
    {

    }

    /// <summary>
    /// UI�v�f�̏������Ɣz�u�A�{�^���C�x���g�̐ݒ�
    /// </summary>
    private void Init()
    {
        // UI�̐��𒲐�
        if (commandCount < 1)
        {
            commandCount = 1;
        }
        // �{�^����x���W�𒲐�
        if (commandLeftButtonPosition.x >= 0)
        {
            commandLeftButtonPosition.x = -1e-5f; // ���{�^���̈ʒu���E���ɗ��Ȃ��悤�ɂ���
            Debug.LogWarning("���{�^���̈ʒu���E���ɐݒ肳��Ă��܂��B���{�^���͍����ɔz�u���Ă��������B");
        }
        if (commandRightButtonPosition.x <= 0)
        {
            commandRightButtonPosition.x = 1e-5f; // �E�{�^���̈ʒu�������ɗ��Ȃ��悤�ɂ���
            Debug.LogWarning("�E�{�^���̈ʒu�������ɐݒ肳��Ă��܂��B�E�{�^���͉E���ɔz�u���Ă��������B");
        }

        bool commandUIisCorrect = true;

        if (chobins.Length > 0)
        {
            for (int i = 0; i < chobins.Length; i++)
            {
                if (chobins[i] == null)
                {
                    commandUIisCorrect = false;
                    Debug.LogError($"ChobinBehaviour�̔z���null���܂܂�Ă��܂��BChobin {i} ��ݒ肵�Ă��������B");
                }
                else if (chobins[i].GetComponent<NavMeshAgent>() == null)
                {
                    commandUIisCorrect = false;
                    Debug.LogError($"ChobinBehaviour��NavMeshAgent��Chobin {i} �ɐݒ肳��Ă��܂���BNavMeshAgent��ǉ����Ă��������B");
                }
                else
                {
                    chobins[i].InitIndex(commandCount); // �eChobin�̃C���f�b�N�X��������
                    chobins[i].SetPerformingTimeLength(performingTimeLength); // �������Ԃ�ݒ�
                }
            }
        }
        else
        {
            commandUIisCorrect = false;
            Debug.LogError("ChobinBehaviour�̔z�񂪋�ł��BChobinBehaviour���A�^�b�`���Ă��������B");
        }
        // �H�ށE�����@�I�u�W�F�N�g��null�łȂ����m�F
        if (commandUIPrefab != null && commandUIParent != null)
        {
            bool hasText = false;
            bool hasLeftButton = false;
            bool hasRightButton = false;
            // �H�ރI�u�W�F�N�g�̎q�v�f���`�F�b�N
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
            // �K�v��UI�v�f�������Ă��邩����
            if (hasText && hasLeftButton && hasRightButton)
            {
            }
            else
            {
                commandUIisCorrect = false;
                if (!hasText)
                {
                    Debug.LogError("�R�}���hUI�v���n�u��TextMeshProUGUI�R���|�[�l���g��������܂���B");
                }
                if (!hasLeftButton)
                {
                    Debug.LogError("�R�}���hUI�v���n�u�ɍ��{�^����Button�R���|�[�l���g��������܂���B");
                }
                if (!hasRightButton)
                {
                    Debug.LogError("�R�}���hUI�v���n�u�ɉE�{�^����Button�R���|�[�l���g��������܂���B");
                }
            }
        }
        else
        {
            commandUIisCorrect = false;
            if (commandUIPrefab == null)
            {
                Debug.LogError("�R�}���hUI�v���n�u���ݒ肳��Ă��܂���B");
            }
            if (commandUIParent == null)
            {
                Debug.LogError("�R�}���hUI�̐e�I�u�W�F�N�g���ݒ肳��Ă��܂���B");
            }
        }
        // �S�ẴR�}���h���������Z�b�g�A�b�v����Ă���ꍇ�̂�UI��������
        if (commandUIisCorrect)
        {

#if UNITY_EDITOR
            if (currentCommandCount != commandCount)
            {
                currentCommandCount = commandCount;
                EditorApplication.delayCall += InitUIParent; // UI�̐e�I�u�W�F�N�g��������
            }
            else
            {
                InitUI();
            }
#endif

            Debug.Log("CookingCommandBehaviour�̏�����������Ɋ������܂����B");
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
        // ������UI�I�u�W�F�N�g���폜
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
            materialUIObject.name = "MaterialUI_" + i; // �H��UI�̖��O��ݒ�
            actionUIObject.name = "ActionUI_" + i; // �����@UI�̖��O��ݒ�
            // UI�̈ʒu��ݒ�
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
        EditorApplication.delayCall -= InitUIParent; // ��x�������s���邽�߂ɉ���
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

            // �{�^���̃N���b�N�C�x���g��ݒ�
            int commandIndex = i; // ���[�J���ϐ����g�p���ăN���[�W���[�̖������
            materialLeftButton.onClick.RemoveAllListeners();
            materialRightButton.onClick.RemoveAllListeners();
            actionLeftButton.onClick.RemoveAllListeners();
            actionRightButton.onClick.RemoveAllListeners();
            materialLeftButton.onClick.AddListener(() => SetMaterial(commandIndex, (chobins[currentChobinUIIndex].materialIndex[commandIndex] - 1 + MaterialList.Length) % MaterialList.Length));
            materialRightButton.onClick.AddListener(() => SetMaterial(commandIndex, (chobins[currentChobinUIIndex].materialIndex[commandIndex] + 1) % MaterialList.Length));
            actionLeftButton.onClick.AddListener(() => SetAction(commandIndex, (chobins[currentChobinUIIndex].actionIndex[commandIndex] - 1 + ActionList.Length) % ActionList.Length));
            actionRightButton.onClick.AddListener(() => SetAction(commandIndex, (chobins[currentChobinUIIndex].actionIndex[commandIndex] + 1) % ActionList.Length));

            // ���������Ɋe�`���r���̐H�ނƒ����@��ݒ�
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
            Debug.LogError($"�R�}���h�C���f�b�N�X {commandIndex} ���͈͊O�ł��B0����{commandCount - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        if (materialIndex < 0 || materialIndex >= MaterialList.Length)
        {
            Debug.LogError($"�ޗ��C���f�b�N�X {materialIndex} ���͈͊O�ł��B0����{MaterialList.Length - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        if (materialUITexts == null)
        {
            Debug.LogError("materialUITexts������������Ă��܂���Binit()���Ăяo���Ă��������B");
            return;
        }
        chobins[currentChobinUIIndex].SetMaterial(commandIndex, materialIndex);
        UpdateMaterialText(commandIndex); // �H�ނ̃e�L�X�g���X�V
    }
    private void SetAction(int commandIndex, int actionIndex)
    {
        if (commandIndex < 0 || commandIndex >= commandCount)
        {
            Debug.LogError($"�R�}���h�C���f�b�N�X {commandIndex} ���͈͊O�ł��B0����{commandCount - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        if (actionIndex < 0 || actionIndex >= ActionList.Length)
        {
            Debug.LogError($"�A�N�V�����C���f�b�N�X {actionIndex} ���͈͊O�ł��B0����{ActionList.Length - 1}�͈̔͂Ŏw�肵�Ă��������B");
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
    /// �C���X�y�N�^�[��Œl���ύX���ꂽ�ۂɎ���������
    /// </summary>
    private void OnValidate()
    {
        // �������������Ăяo��
        Init();
    }
#endif
}