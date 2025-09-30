using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChobinSetting : MonoBehaviour
{
    enum SpriteSizeOption
    {
        NonKeepAspect, // �A�X�y�N�g����ێ����Ȃ�
        KeepAspectWithCurrentWidth, // ���݂̕����ێ����ăA�X�y�N�g��𒲐�
        KeepAspectWithCurrentHeight, // ���݂̍������ێ����ăA�X�y�N�g��𒲐�
    }

    public class ShowCommandEvent : UnityEvent<int> { }

    [Header("�`���r���̐ݒ�")]
    [SerializeField] private ChobinBehaviour[] chobins;
    [SerializeField] private Transform WaitingSpot; // �ҋ@�ꏊ
    [SerializeField] private Transform ServingSpot; // ������񋟂���ꏊ
    [SerializeField] private float chobinSpeed; // �`���r���̈ړ����x
    [SerializeField] private float chobinAcceleration; // �`���r���̉����x
    [SerializeField] private float performingTimeLength = 2f; // �����ɂ����鎞��
    [SerializeField] private float waitingSpotRadius = 1f; // �ҋ@�ꏊ�̔��a
    [SerializeField] private Slider performingTimeSlider;

    [Header("�`���r����I������{�^���ƕ\������Canvas��D&D")]
    [SerializeField] private GameObject chobinButtonCanvas; // �`���r����UI��\������Canvas
    [SerializeField] private GameObject chobinButtonPrefab; // �`���r����UI�v���n�u
    [SerializeField] private Sprite chobinButtonSprite; // �`���r����UI�̃X�v���C�g
    [SerializeField] private string chobinButtonPrefabName = "ChobinButton"; // �`���r����UI�̖��O
    [SerializeField] private Vector3 chobinButtonOffset; // �`���r����UI�̈ʒu
    [SerializeField] private Vector2 chobinButtonSize; // �`���r����UI�̃T�C�Y
    [SerializeField] private SpriteSizeOption chobinButtonSizeOption = SpriteSizeOption.NonKeepAspect; // �`���r����UI�̃T�C�Y�I�v�V����
    [SerializeField] private bool buttonIsActiveInEditor = true; // �`���r����UI��\�����邩�ǂ���

    private UnityEvent checkAllSettings = new();
    private ShowCommandEvent showCommandEvent = new();

    private bool AllSettingAreCorrect = true;
    private int commandCount = 1;

    public ChobinBehaviour[] Chobins => chobins;

    public UnityEvent CheckAllSettings => checkAllSettings;
    public ShowCommandEvent ShowCommand => showCommandEvent;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // performingTimeLength�̒l�ɉ�����Slider���X�V
        if (performingTimeSlider != null)
        {
            // ��: performingTimeLength���ő�l�A0���ŏ��l
            performingTimeSlider.maxValue = performingTimeLength;
            // performingTimeLength�̐i�s�x��\���i�����ł͉��Ɍ������Ă�����j
            // ���ۂ�ChobinBehaviour����i�s�x���擾���Ĕ��f����̂����z
            performingTimeSlider.value = Mathf.Clamp(performingTimeLength, 0, performingTimeSlider.maxValue);
        }
    }

    public bool CheckSettings()
    {
        AllSettingAreCorrect = true;

        if (chobins.Length > 0)
        {
            for (int i = 0; i < chobins.Length; i++)
            {
                if (chobins[i] == null)
                {
                    AllSettingAreCorrect = false;
                    Debug.LogError($"ChobinBehaviour�̔z���null���܂܂�Ă��܂��BChobin {i} ��ݒ肵�Ă��������B");
                }
                else if (chobins[i].GetComponent<NavMeshAgent>() == null)
                {
                    AllSettingAreCorrect = false;
                    Debug.LogError($"ChobinBehaviour��NavMeshAgent��Chobin {i} �ɐݒ肳��Ă��܂���BNavMeshAgent��ǉ����Ă��������B");
                }
            }
        }
        else
        {
            AllSettingAreCorrect = false;
            Debug.LogError("ChobinBehaviour�̔z�񂪋�ł��BChobinBehaviour���A�^�b�`���Ă��������B");
        }

        if (WaitingSpot == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("�ҋ@�ꏊ��Transform���ݒ肳��Ă��܂���B");
        }
        if (ServingSpot == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("������񋟂���ꏊ��Transform���ݒ肳��Ă��܂���B");
        }

        if (chobinButtonCanvas == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("�`���r���I��p��Canvas�I�u�W�F�N�g���ݒ肳��Ă��܂���B");
        }
        if (chobinButtonPrefab == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("�`���r���I��p�̃{�^���v���n�u���ݒ肳��Ă��܂���B");
        }
        else if (chobinButtonPrefab.GetComponent<Button>() == null || chobinButtonPrefab.GetComponent<Image>() == null || chobinButtonPrefab.GetComponent<RectTransform>() == null)
        {
            AllSettingAreCorrect = false;
            Debug.LogError("�`���r���I��p�̃{�^���v���n�u�̃R���|�[�l���g���������ݒ肳��Ă��܂���B");
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
#else
        InitChobinButtonParent();
#endif

        Debug.Log("ChobinSetting�̏��������������܂����B");
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
            Button chobinButton = chobinButtonObject.GetComponent<Button>();
            RectTransform chobinButtonRect = chobinButtonObject.GetComponent<RectTransform>();
            chobinButtonRect.sizeDelta = chobinButtonSize;
            Image chobinImage = chobinButtonObject.GetComponent<Image>();
            if (chobinImage != null)
            {
                chobinImage.sprite = chobinButtonSprite;
            }
            if (chobinButton != null)
            {
                int chobinIndex = i; // ���[�J���ϐ����g�p���ăN���[�W���[�̖������
                chobinButton.onClick.RemoveAllListeners();
                chobinButton.onClick.AddListener(() => showCommandEvent.Invoke(chobinIndex));
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
                chobins[i].SetServingSpot(ServingSpot);
                chobins[i].SetSelectButton(chobinButtonObject, chobinButtonOffset);
                chobins[i].SetPerformingTimeLength(performingTimeLength);
                chobins[i].Init(i, commandCount);
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

    public void SetCommandCount(int _commandCount)
    {
        commandCount = _commandCount;
    }

    /// <summary>
    /// �G�f�B�^��Ńp�����[�^���ύX���ꂽ�Ƃ��ɐݒ���`�F�b�N���ď��������s��
    /// </summary>
#if UNITY_EDITOR
    private void OnValidate()
    {
        checkAllSettings.Invoke();
    }
#endif
}
