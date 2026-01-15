using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.TestTools;
using System;
using System.IO.Compression;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<ProjectData> unityProjects = new List<ProjectData>();
    private Coroutine _searchCoroutine;
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

        if (_searchCoroutine != null) StopCoroutine(_searchCoroutine);
        _searchCoroutine = StartCoroutine(FindUnityProjectsRecursively(sourcePath));
    }

    private IEnumerator FindUnityProjectsRecursively(string rootPath)
    {
        Stack<SearchNode> stack = new Stack<SearchNode>();
        stack.Push(new SearchNode(rootPath, 0f, 1f));

        float timeBudget = 0.005f;
        float startTime = Time.realtimeSinceStartup;

        while (stack.Count > 0)
        {
            SearchNode currentNode = stack.Pop();
            string currentDir = currentNode.Path;

            // Update UI with current progress (0 to 100)
            UIHandler.Instance.UpdateProgress(currentNode.MinProgress * 100f);

            if (Directory.Exists(Path.Combine(currentDir, "Assets"))
                && Directory.Exists(Path.Combine(currentDir, "ProjectSettings"))
                && Directory.Exists(Path.Combine(currentDir, "Packages"))
                && Directory.Exists(Path.Combine(currentDir, "UserSettings")))
            {
                ProjectData projectData = new ProjectData(currentDir, ActionType.None);
                unityProjects.Add(projectData);
                UIHandler.Instance.AddToList(projectData);
                continue;
            }

            try
            {
                string[] subDirectories = Directory.GetDirectories(currentDir);
                int subDirCount = subDirectories.Length;

                if (subDirCount > 0)
                {
                    float step = (currentNode.MaxProgress - currentNode.MinProgress) / subDirCount;
                    // Iterate backwards to push to stack so we pop/process them in order
                    for (int i = subDirCount - 1; i >= 0; i--)
                    {
                        float childMin = currentNode.MinProgress + (i * step);
                        float childMax = currentNode.MinProgress + ((i + 1) * step);
                        stack.Push(new SearchNode(subDirectories[i], childMin, childMax));
                    }
                }
            }
            catch (System.Exception e) { Debug.LogWarning($"Could not access directory: {currentDir}. Reason: {e.Message}"); }

            if (Time.realtimeSinceStartup - startTime > timeBudget)
            {
                yield return null;
                startTime = Time.realtimeSinceStartup;
            }
        }

        // Reaching 100%
        UIHandler.Instance.UpdateProgress(100f);
        Debug.Log("Search Complete");
        UIHandler.Instance.continueButton.interactable = true;
    }

    public void PerformActionsOnProjects()
    {
        foreach (var project in unityProjects)
        {
            switch (project.actionType)
            {
                case ActionType.Archive:
                    Debug.Log($"Archiving project: {project.projectPath}");
                    StartCoroutine(ArchiveProject(project.projectPath));
                    break;
                case ActionType.Delete:
                    Debug.Log($"Deleting project: {project.projectPath}");
                    StartCoroutine(DeleteProject(project.projectPath));
                    break;
                case ActionType.None:
                    Debug.Log($"No action for project: {project.projectPath}");
                    break;
            }
        }
    }

    private IEnumerator DeleteProject(string projectPath)
    {
        Directory.Delete(projectPath, true);
        yield return null;
    }

    private IEnumerator ArchiveProject(string projectPath)
    {
        projectPath = projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string parentDir = Path.GetDirectoryName(projectPath);
        string archivePath = Path.Combine(parentDir, Path.GetFileName(projectPath) + ".zip");

        string[] foldersToInclude = { ".git", "Assets", "ProjectSettings", "Packages", "UserSettings" };

        using (FileStream zipToOpen = new FileStream(archivePath, FileMode.Create))
        {
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (string folder in foldersToInclude)
                {
                    string sourceDir = Path.Combine(projectPath, folder);
                    if (Directory.Exists(sourceDir))
                    {
                        string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
                        foreach (string file in files)
                        {
                            string relativePath = file.Substring(projectPath.Length + 1).Replace('\\', '/');
                            archive.CreateEntryFromFile(file, relativePath);
                        }
                    }
                }
            }
        }

        Directory.Delete(projectPath, true);
        yield return null;
    }

    private struct SearchNode
    {
        public string Path;
        public float MinProgress;
        public float MaxProgress;

        public SearchNode(string path, float min, float max)
        {
            Path = path;
            MinProgress = min;
            MaxProgress = max;
        }
    }
}
[Serializable]
public enum ActionType
{
    None,
    Archive,
    Delete
}

[System.Serializable]
public class ProjectData
{
    public string projectPath;
    public ActionType actionType;

    public ProjectData(string path, ActionType action)
    {
        projectPath = path;
        actionType = action;
    }
}
