using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections;
public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance;
    GameObject sourcePanel, listPanel, dialogPanel, loadingPanel;
    TMP_InputField sourceInputField, destinationInputField;
    Button startButton, backButton, yes, no;
    public Button continueButton;
    GameObject card;
    Transform listContent, processingImage;
    Slider scanSlider;
    public Coroutine rotateImageCoroutine;

    public string destinationPath = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        card = Resources.Load<GameObject>("Card");
        GetReferences();
        AssignListeners();
    }

    private void GetReferences()
    {
        sourceInputField = transform.GetChildWithName("SourceInputField").GetComponent<TMP_InputField>();
        destinationInputField = transform.GetChildWithName("DestinationInputField").GetComponent<TMP_InputField>();
        startButton = transform.GetChildWithName("StartButton").GetComponent<Button>();

        sourcePanel = transform.GetChildWithName("SourcePanel").gameObject;
        listPanel = transform.GetChildWithName("ListPanel").gameObject;

        listContent = listPanel.transform.GetChildWithName("ListContent");
        continueButton = listPanel.transform.GetChildWithName("ContinueButton").GetComponent<Button>();
        continueButton.interactable = false;
        backButton = listPanel.transform.GetChildWithName("BackButton").GetComponent<Button>();
        scanSlider = listPanel.transform.GetChildWithName("ScanSlider").GetComponent<Slider>();


        dialogPanel = transform.GetChildWithName("DialogPanel").gameObject;
        yes = dialogPanel.transform.GetChildWithName("YesButton").GetComponent<Button>();
        no = dialogPanel.transform.GetChildWithName("NoButton").GetComponent<Button>();

        loadingPanel = transform.GetChildWithName("LoadingPanel").gameObject;
        loadingPanel.SetActive(false);
        processingImage = loadingPanel.transform.GetChildWithName("ProcessingImage");
    }

    private void AssignListeners()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        backButton.onClick.AddListener(() =>
        {
            listPanel.SetActive(false);
            sourcePanel.SetActive(true);
            CleanProjects();
        });
        yes.onClick.AddListener(OnYesButtonClicked);
        no.onClick.AddListener(OnNoButtonClicked);
    }

    private void CleanProjects()
    {
        GameManager.Instance.unityProjects.Clear();
        foreach (Transform child in listContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnYesButtonClicked()
    {
        "Yes button clicked.".Print();
        dialogPanel.SetActive(false);
        listPanel.SetActive(false);
        loadingPanel.SetActive(true);
        rotateImageCoroutine = StartCoroutine(RotateProcessingImage());
        GameManager.Instance.PerformActionsOnProjects(() =>
        {
            if (rotateImageCoroutine != null) StopCoroutine(rotateImageCoroutine);
            loadingPanel.SetActive(false);
            sourcePanel.SetActive(true);
            Debug.Log("Processing finished.");
        });
    }

    private IEnumerator RotateProcessingImage()
    {
        float rotationSpeed = 100f;
        while (true)
        {
            processingImage.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnNoButtonClicked()
    {
        "No button clicked.".Print();
        dialogPanel.SetActive(false);
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked.");
        dialogPanel.SetActive(true);
    }
    private void OnStartButtonClicked()
    {
        CleanProjects();
        GameManager.Instance.StartSearch(sourceInputField.text);
        destinationPath = destinationInputField.text;
        sourcePanel.SetActive(false);
        listPanel.SetActive(true);
    }

    public void AddToList(ProjectData projectData)
    {
        GameObject newCard = Instantiate(card, listContent);

        Button cardButton = newCard.GetComponentInChildren<Button>();

        TextMeshProUGUI projectNameText = newCard.transform.GetChildWithName("ProjectName").GetComponent<TextMeshProUGUI>();
        projectNameText.text = Path.GetFileName(projectData.projectPath);

        cardButton.onClick.AddListener(() =>
        {
            $"Clicked on project: {projectData.projectPath}".Print();
        });

        TMP_Dropdown dropdown = newCard.GetComponentInChildren<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener((int index) =>
        {
            $"Selected option {index} for project: {projectData.projectPath}".Print();
            projectData.actionType = (ActionType)index;
        });
    }

    public void UpdateProgress(float value)
    {
        if (scanSlider != null)
            scanSlider.value = value;
    }

}
