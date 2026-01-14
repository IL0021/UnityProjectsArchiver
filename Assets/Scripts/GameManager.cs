using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<string> unityProjects = new List<string>();
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

    public void StartSearch(string sourcePath)
    {
        Debug.Log("Search started for path: " + sourcePath);
        unityProjects.Clear();

        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError($"Source path does not exist: {sourcePath}");
            return;
        }

        FindUnityProjectsRecursively(sourcePath);

        foreach (string projectPath in unityProjects)
        {
            UIHandler.Instance.AddToList(projectPath);
        }
    }

    private void FindUnityProjectsRecursively(string directory)
    {
        if (Directory.Exists(Path.Combine(directory, "Assets"))
            && Directory.Exists(Path.Combine(directory, "ProjectSettings"))
            && Directory.Exists(Path.Combine(directory, "Packages"))
            && Directory.Exists(Path.Combine(directory, "UserSettings")))
        {
            unityProjects.Add(directory);
            UIHandler.Instance.AddToList(directory);
            Debug.Log($"Found Unity project: {directory}");
            return;
        }

        try
        {
            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                FindUnityProjectsRecursively(subDirectory);
            }
        }
        catch (System.Exception e) { Debug.LogWarning($"Could not access directory: {directory}. Reason: {e.Message}"); }
    }
}
