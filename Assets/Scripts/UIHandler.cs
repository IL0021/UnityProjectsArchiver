using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance;
    GameObject sourcePanel, listPanel;
    TMP_InputField sourceInputField;
    Button startButton;

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
        GetReferences();
        AssignListeners();
    }

    private void GetReferences()
    {
        sourceInputField = transform.GetChildWithName("SourceInputField").GetComponent<TMP_InputField>();
        startButton = transform.GetChildWithName("StartButton").GetComponent<Button>();

        sourcePanel = transform.GetChildWithName("SourcePanel").gameObject;
        listPanel = transform.GetChildWithName("ListPanel").gameObject;
    }

    private void AssignListeners()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        GameManager.Instance.StartSearch(sourceInputField.text);
        sourcePanel.SetActive(false);
        listPanel.SetActive(true);
    }

    public void AddToList(string projectName)
    {
        
    }
}
