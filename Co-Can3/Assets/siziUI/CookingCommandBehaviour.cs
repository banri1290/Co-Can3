//======================================================================
// CookingCommandBehaviour.cs
// �����R�}���h��UI������s��MonoBehaviour
// �e�R�}���h�̍ޗ��E�A�N�V�����̃e�L�X�g�ƃ{�^���̔z�u�E������Ǘ����܂�
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
        NonKeepAspect, // �A�X�y�N�g����ێ����Ȃ�
        KeepAspectWithCurrentWidth, // ���݂̕����ێ����ăA�X�y�N�g��𒲐�
        KeepAspectWithCurrentHeight, // ���݂̍������ێ����ăA�X�y�N�g��𒲐�
    }
    public class SelectCommandEvent : UnityEvent<int, int> { }

    [Header("�����ɐH�ށE�����@��I������UI��D&D")]
    [SerializeField] private GameObject commandUIPrefab; // UI�v���n�u��o�^
    [SerializeField] private GameObject commandCanvas; // Canvas�I�u�W�F�N�g��o�^
    [SerializeField] private GameObject commandUIParent; // UI�̐e�I�u�W�F�N�g��o�^
    [SerializeField] private string materialUIPrefabName = "MaterialUI"; // �H��UI�̖��O
    [SerializeField] private string actionUIPrefabName = "ActionUI"; // �����@UI�̖��O
    [SerializeField] private bool commandCanvasStartActive = false; // �Q�[���J�n���ɃR�}���hUI��\�����邩

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

    private UnityEvent checkAllSettings = new();
    private SelectCommandEvent previousMaterialEvent = new();
    private SelectCommandEvent nextMaterialEvent = new();
    private SelectCommandEvent previousActionEvent = new();
    private SelectCommandEvent nextActionEvent = new();
    private UnityEvent submitCommandEvent = new();

    private int currentChobinUIIndex;

    public int CommandCount => commandCount; // �R�}���h�̐�
    public int CurrentChobinUIIndex => currentChobinUIIndex;

    public UnityEvent CheckAllSettings => checkAllSettings;
    public SelectCommandEvent PreviousMaterialEvent => previousMaterialEvent;
    public SelectCommandEvent NextMaterialEvent => nextMaterialEvent;
    public SelectCommandEvent PreviousActionEvent => previousActionEvent;
    public SelectCommandEvent NextActionEvent => nextActionEvent;
    public UnityEvent SubmitCommandEvent => submitCommandEvent;

    // ����������
    void Start()
    {
        currentChobinUIIndex = 0;
        if (commandCanvasStartActive) commandCanvas.SetActive(false);
    }

    // ���t���[���̍X�V�����i���g�p�j
    void Update()
    {

    }

    /// <summary>
    /// �ݒ�̊m�F��UI�̏�����
    /// </summary>
    public bool CheckSettings()
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

        if (commandCanvas == null)
        {
            commandUIisCorrect = false;
            Debug.LogError("�w��UICanvas�I�u�W�F�N�g���ݒ肳��Ă��܂���B");
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

        return commandUIisCorrect;
    }

    /// <summary>
    /// ������
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
                Debug.Log("�R�}���h�����ύX���ꂽ���߁AUI���ď��������܂��B");
                EditorApplication.delayCall += InitUIParent; // UI�̐e�I�u�W�F�N�g��������
            }
            else
            {
                InitUI(); // UI�̃X�^�C�����X�V
            }
        }
#else
        InitUIParent();
#endif

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
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.delayCall -= InitUIParent; // ��x�������s���邽�߂ɉ���
        }
#endif
    }

    private void InitUI()
    {
        //�{�^���̃T�C�Y�I�v�V�����ɉ����ăT�C�Y�𒲐�
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
            Debug.LogWarning("UI�v�f�̔z�񂪏���������Ă��Ȃ����A�R�}���h���ƈ�v���܂���BUI�̐e�I�u�W�F�N�g���ď��������܂��B");
            EditorApplication.delayCall += InitUIParent; // UI�̐e�I�u�W�F�N�g��������
            return;
        }
#endif

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
        commandCanvas.SetActive(true);
        currentChobinUIIndex = chobinIndex;
    }

    public void SubmitCommand()
    {
        submitCommandEvent.Invoke();
        commandCanvas.SetActive(false);
    }

    public string GetMaterialName(int index)
    {
        if (materialUITexts != null && index >= 0 && index < materialUITexts.Length)
            return materialUITexts[index].text;
        return "";
    }

    public float GetCookTime()
    {
        // デモ用: 実際にはUIの値や変数を取得
        return 40f;
    }

    public int GetSelectedSteps()
    {
        // デモ用: 実際にはUIの値や変数を取得
        return 3;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        checkAllSettings.Invoke();
    }
#endif
}