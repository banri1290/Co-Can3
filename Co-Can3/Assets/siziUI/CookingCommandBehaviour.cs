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

    enum SpriteSizeOption
    {
        NonKeepAspect, // �A�X�y�N�g����ێ����Ȃ�
        KeepAspectWithCurrentWidth, // ���݂̕����ێ����ăA�X�y�N�g��𒲐�
        KeepAspectWithCurrentHeight, // ���݂̍������ێ����ăA�X�y�N�g��𒲐�
    }

    [Header("�`���r���̐ݒ�")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [SerializeField] private Transform WaitingSpot; // �ҋ@�ꏊ
    [SerializeField] private float chobinSpeed; // �`���r���̈ړ����x
    [SerializeField] private float chobinAcceleration; // �`���r���̉����x
    [SerializeField] private float performingTimeLength = 2f; // �����ɂ����鎞��
    [SerializeField] private float waitingSpotRadius = 1f; // �ҋ@�ꏊ�̔��a
    [Header("�`���r����I������{�^���ƕ\������Canvas��D&D")]
    [SerializeField] private GameObject chobinButtonCanvas; // �`���r����UI��\������Canvas
    [SerializeField] private GameObject chobinButtonPrefab; // �`���r����UI�v���n�u
    [SerializeField] private Sprite chobinButtonSprite; // �`���r����UI�̃X�v���C�g
    [SerializeField] private string chobinButtonPrefabName = "ChobinButton"; // �`���r����UI�̖��O
    [SerializeField] private Vector3 chobinButtonOffset; // �`���r����UI�̈ʒu
    [SerializeField] private Vector2 chobinButtonSize; // �`���r����UI�̃T�C�Y
    [SerializeField] private SpriteSizeOption chobinButtonSizeOption = SpriteSizeOption.NonKeepAspect; // �`���r����UI�̃T�C�Y�I�v�V����
    [SerializeField] private bool chobinButtonCanvasStartActive = true; // �Q�[���J�n���Ƀ`���r���I���{�^����\�����邩

    [Header("�����ɐH�ށE�����@��I������UI��D&D")]
    [SerializeField] private GameObject commandUIPrefab; // UI�v���n�u��o�^
    [SerializeField] private GameObject commandCanvas; // Canvas�I�u�W�F�N�g��o�^
    [SerializeField] private GameObject commandUIParent; // UI�̐e�I�u�W�F�N�g��o�^
    [SerializeField] private string materialUIPrefabName = "MaterialUI"; // �H��UI�̖��O
    [SerializeField] private string actionUIPrefabName = "ActionUI"; // �����@UI�̖��O
    [SerializeField] private bool commandCanvasStartActive = false; // �Q�[���J�n���ɃR�}���hUI��\�����邩
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
    [SerializeField] private SpriteSizeOption buttonSizeOption = SpriteSizeOption.NonKeepAspect; // �{�^���T�C�Y�I�v�V����

    // UI�v�f�̎Q��
    private RectTransform[] materialUIRects;
    private TextMeshProUGUI[] materialUITexts;
    private Button[] materialLeftButtons;
    private Button[] materialRightButtons;
    private RectTransform[] actionUIRects;
    private TextMeshProUGUI[] actionUITexts;
    private Button[] actionLeftButtons;
    private Button[] actionRightButtons;

    private int currentChobinUIIndex;

    // ����������
    void Start()
    {
        currentChobinUIIndex = 0;
        ChackSettings();
        if (commandCanvasStartActive) ShowCommand(currentChobinUIIndex);
    }

    // ���t���[���̍X�V�����i���g�p�j
    void Update()
    {

    }

    /// <summary>
    /// �ݒ�̊m�F��UI�̏�����
    /// </summary>
    private void ChackSettings()
    {
        bool commandUIisCorrect = true;

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
            }
        }
        else
        {
            commandUIisCorrect = false;
            Debug.LogError("ChobinBehaviour�̔z�񂪋�ł��BChobinBehaviour���A�^�b�`���Ă��������B");
        }

        if (WaitingSpot == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�ҋ@�ꏊ��Transform���ݒ肳��Ă��܂���B");
        }

        if (chobinButtonCanvas == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�`���r���I��p��Canvas�I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }
        if (chobinButtonPrefab == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�`���r���I��p�̃{�^���v���n�u���ݒ肳��Ă��܂���B");
        }
        else if (chobinButtonPrefab.GetComponent<Button>() == null || chobinButtonPrefab.GetComponent<Image>() == null || chobinButtonPrefab.GetComponent<RectTransform>() == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�`���r���I��p�̃{�^���v���n�u�̃R���|�[�l���g���������ݒ肳��Ă��܂���B");
        }

        if (MaterialList.Length == 0)
        {
            commandUIisCorrect = false;
            Debug.LogError("�H�ރ��X�g����ł��B���Ȃ��Ƃ�1�̐H�ނ�ǉ����Ă��������B");
        }
        if (ActionList.Length == 0)
        {
            commandUIisCorrect = false;
            Debug.LogError("�����@���X�g����ł��B���Ȃ��Ƃ�1�̒����@��ǉ����Ă��������B");
        }
        else
        {
            for (int i = 0; i < ActionList.Length; i++)
            {
                if (ActionList[i].KitchinSpot == null)
                {
                    commandUIisCorrect = false;
                    Debug.LogError($"�����@ {ActionList[i].Name} �ɒ������Transform���ݒ肳��Ă��܂���B");
                }
            }
        }

        if (commandCanvas == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�w��UI��\������Canvas�I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }
        else
        {
            commandCanvas.SetActive(commandCanvasStartActive);
        }

        if (commandUIParent == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�R�}���hUI�̐e�I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }

        // �H�ށE�����@�I�u�W�F�N�g��null�łȂ����m�F
        if (commandUIPrefab != null)
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
            Debug.LogError("�R�}���hUI�v���n�u���ݒ肳��Ă��܂���B");
        }
        // �S�ẴR�}���h���������Z�b�g�A�b�v����Ă���ꍇ�̂�UI��������
        if (commandUIisCorrect)
        {
            Init();
        }
    }

    /// <summary>
    /// ������
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
                Debug.Log("�R�}���h�����ύX���ꂽ���߁AUI���ď��������܂��B");
                EditorApplication.delayCall += InitUIParent; // UI�̐e�I�u�W�F�N�g��������
            }
            else
            {
                InitUI(); // UI�̃X�^�C�����X�V
            }
            bool changeChobinCount = (chobinButtonCanvas.transform.childCount != chobins.Length);
            if (changeChobinCount)
            {
                Debug.Log("�`���r���̐����ύX���ꂽ���߁A�`���r���I���{�^�����ď��������܂��B");
                EditorApplication.delayCall += InitChobinButtonParent; // �`���r���I���{�^���̐e�I�u�W�F�N�g��������
            }
            else
            {
                InitChobins(); // �`���r���̃p�����[�^���X�V
            }
        }

        Debug.Log("CookingCommandBehaviour�̏�����������Ɋ������܂����B");
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
            materialUIObject.name = materialUIPrefabName + "_" + i; // �H��UI�̖��O��ݒ�
            actionUIObject.name = actionUIPrefabName + "_" + i; // �����@UI�̖��O��ݒ�

            // UI�̈ʒu��ݒ�
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
        EditorApplication.delayCall -= InitUIParent; // ��x�������s���邽�߂ɉ���
#endif
    }

    private void InitUI()
    {
        //�{�^���̃T�C�Y�I�v�V�����ɉ����ăT�C�Y�𒲐�
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
            Debug.LogWarning("UI�v�f�̔z�񂪏���������Ă��Ȃ����A�R�}���h���ƈ�v���܂���BUI�̐e�I�u�W�F�N�g���ď��������܂��B");
            EditorApplication.delayCall += InitUIParent; // UI�̐e�I�u�W�F�N�g��������
            return;
        }

            // UI�̔z�u�ƃX�^�C����ݒ�
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

            // �e�L�X�g�̃X�^�C����ݒ�
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

            // �{�^���̃X�^�C����ݒ�
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
        }
    }

    private void InitChobinButtonParent()
    {
        // ������UI�I�u�W�F�N�g���폜
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
            chobinButtonObject.name = chobinButtonPrefabName + "_" + i; // �`���r���{�^���̖��O��ݒ�
        }

        InitChobins();
#if UNITY_EDITOR
        EditorApplication.delayCall -= InitChobinButtonParent; // ��x�������s���邽�߂ɉ���
#endif
    }

    private void InitChobins()
    {
        chobinButtonSize = SpriteSize(chobinButtonSprite, chobinButtonSize, chobinButtonSizeOption);

        for (int i = 0; i < chobins.Length; i++)
        {
            // UI�̈ʒu��ݒ�
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
                int chobinIndex = i; // ���[�J���ϐ����g�p���ăN���[�W���[�̖������
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
                    chobins[i].SetMaterial(j, 0); // ���������ɐH�ނ�ݒ�
                    chobins[i].SetAction(j, 0, ActionList[0].KitchinSpot); // ���������ɒ����@��ݒ�
                }
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

    private Vector2 SpriteSize(Sprite sprite, Vector2 targetSize, SpriteSizeOption spriteSizeOption)
    {
        Vector2 newSize = targetSize;
        switch (spriteSizeOption)
        {
            case SpriteSizeOption.NonKeepAspect:
                // ���̂܂�
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
            Debug.LogError($"�`���r���C���f�b�N�X {chobinIndex} ���͈͊O�ł��B0����{chobins.Length - 1}�͈̔͂Ŏw�肵�Ă��������B");
            return;
        }
        commandCanvas.SetActive(true);
        currentChobinUIIndex = chobinIndex;
        for (int i = 0; i < commandCount; i++)
        {
            SetMaterial(i, 0); // �H�ނ̃e�L�X�g��������
            SetAction(i, 0); // �����@�̃e�L�X�g��������
        }
    }

    public void SubmitCommand()
    {
        chobins[currentChobinUIIndex].SetCommand();
        commandCanvas.SetActive(false);
    }

#if UNITY_EDITOR
    /// <summary>
    /// �C���X�y�N�^�[��Œl���ύX���ꂽ�ۂɎ���������
    /// </summary>
    private void OnValidate()
    {
        // �������������Ăяo��
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            ChackSettings();
        }
    }
#endif
}