//======================================================================
// CookingCommandBehaviour.cs
// �����R�}���h��UI������s��MonoBehaviour
// �e�R�}���h�̍ޗ��E�A�N�V�����̃e�L�X�g�ƃ{�^���̔z�u�E������Ǘ����܂�
//======================================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingCommandBehaviour_arc : MonoBehaviour
{
    // �R�}���h����ێ�����\����
    [System.Serializable]
    struct CookingCommand
    {
        [Header("�H��")]
        [SerializeField] private GameObject Material; // �H�ނ�I������UI
        [Header("�����@")]
        [SerializeField] private GameObject Action;   // �����@��I������UI

        public GameObject MaterialObject => Material; // �H�ނ�I������UI���擾
        public GameObject ActionObject => Action;     // �����@��I������UI���擾
    }

    [Header("�����ɐH�ށE�����@��I������UI�O���[�v��D&D")]
    [SerializeField] private CookingCommand[] Commands; // �R�}���h�z��
    [Header("�H�ށE�����@�̖��O���X�g")]
    [SerializeField] private string[] MaterialName;     // �H�ޖ���o�^
    [SerializeField] private string[] ActionName;       // �����@����o�^

    [Header("���X�g�̔z�u")]
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

    TextMeshProUGUI[] MaterialTexts; // �H�ޖ���\������e�L�X�g
    TextMeshProUGUI[] ActionTexts;   // �����@����\������e�L�X�g
    private int[] currentMaterialIndex; // ���݂̐H�ރC���f�b�N�X
    private int[] currentActionIndex;   // ���݂̒����@�C���f�b�N�X

    // ����������
    void Start()
    {
        currentMaterialIndex = new int[Commands.Length];
        currentActionIndex = new int[Commands.Length];
        init();
    }

    /// <summary>
    /// UI�v�f�̏������Ɣz�u�A�{�^���C�x���g�̐ݒ�
    /// </summary>
    private void init()
    {
        // �{�^����x���W�𒲐�
        if(commandLeftButtonPosition.x >=0)
        {
            commandLeftButtonPosition.x = -1e-5f; // ���{�^���̈ʒu���E���ɗ��Ȃ��悤�ɂ���
            Debug.LogWarning("���{�^���̈ʒu���E���ɐݒ肳��Ă��܂��B���{�^���͍����ɔz�u���Ă��������B");
        }
        if (commandRightButtonPosition.x <= 0)
        {
            commandRightButtonPosition.x = 1e-5f; // �E�{�^���̈ʒu�������ɗ��Ȃ��悤�ɂ���
            Debug.LogWarning("�E�{�^���̈ʒu�������ɐݒ肳��Ă��܂��B�E�{�^���͉E���ɔz�u���Ă��������B");
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
            // �H�ށE�����@�I�u�W�F�N�g��null�łȂ����m�F
            if (Commands[i].MaterialObject != null && Commands[i].ActionObject != null)
            {
                bool materialHasText = false;
                bool actionHasText = false;
                bool materialHasLeftButton = false;
                bool materialHasRightButton = false;
                bool actionHasLeftButton = false;
                bool actionHasRightButton = false;
                // �H�ރI�u�W�F�N�g�̎q�v�f���`�F�b�N
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
                // �����@�I�u�W�F�N�g�̎q�v�f���`�F�b�N
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
                // �K�v��UI�v�f�������Ă��邩����
                if (materialHasText && actionHasText && materialHasLeftButton && materialHasRightButton && actionHasLeftButton && actionHasRightButton)
                {
                    currentMaterialIndex[i] = 0;
                    currentActionIndex[i] = 0;
                }
                else
                {
                    allCommandsAreCorrect = false;
                    Debug.LogError($"Cooking command {i} �̃Z�b�g�A�b�v������������܂���B�ޗ��E�A�N�V�������ꂼ��Ƀe�L�X�g�ƍ��E�{�^�����K�v�ł��B");
                }
            }
            else
            {
                // null�̏ꍇ�̓G���[
            }
        }
        // �S�ẴR�}���h���������Z�b�g�A�b�v����Ă���ꍇ�̂�UI��������
        if (allCommandsAreCorrect)
        {
            MaterialTexts = materialTexts;
            ActionTexts = actionTexts;
            currentMaterialIndex = new int[Commands.Length];
            for (int i = 0; i < Commands.Length; i++)
            {
                int index = i; // �����_���p���[�J���ϐ�
                currentMaterialIndex[index] = 0;
                // �{�^���C�x���g�̃��X�i�[��������
                materialLeftButtons[index].onClick.RemoveAllListeners();
                materialRightButtons[index].onClick.RemoveAllListeners();
                actionLeftButtons[index].onClick.RemoveAllListeners();
                actionRightButtons[index].onClick.RemoveAllListeners();
                // �ޗ��E�A�N�V�����̍��E�{�^���ɃC�x���g��ǉ�
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
                // UI�v�f�̔z�u�E�T�C�Y�ݒ�
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
                // �����e�L�X�g�\��
                UpdateText(MaterialTexts[i], MaterialName, ref currentMaterialIndex[i], 0);
                UpdateText(ActionTexts[i], ActionName, ref currentActionIndex[i], 0);
            }
            Debug.Log("CookingCommandBehaviour�̏�����������Ɋ������܂����B");
        }
    }

    // ���t���[���̍X�V�����i���g�p�j
    void Update()
    {

    }

    /// <summary>
    /// �e�L�X�g�̍X�V�����i�C���f�b�N�X�z�j
    /// </summary>
    /// <param name="tmp">�X�V�Ώۂ�TextMeshProUGUI</param>
    /// <param name="texts">�\������e�L�X�g�z��</param>
    /// <param name="index">���݂̃C���f�b�N�X�i�Q�Ɠn���j</param>
    /// <param name="newIndex">�V�����C���f�b�N�X</param>
    private void UpdateText(TextMeshProUGUI tmp, string[] texts, ref int index, int newIndex)
    {
        if (newIndex < 0) newIndex += texts.Length;
        if (newIndex >= texts.Length) newIndex -= texts.Length;
        index = newIndex;
        tmp.text = texts[index];
    }

#if UNITY_EDITOR
    /// <summary>
    /// �C���X�y�N�^�[��Œl���ύX���ꂽ�ۂɎ���������
    /// </summary>
    private void OnValidate()
    {
        init();
    }
#endif
}

