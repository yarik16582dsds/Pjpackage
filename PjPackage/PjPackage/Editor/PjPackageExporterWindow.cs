using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using PjPackageLibrary.DataStructures;
using PjPackageLibrary.Serialization;
using System;
using System.Linq;

public class PjPackageExporterWindow : EditorWindow
{
    private List<string> selectedFiles = new List<string>();
    private List<FileSystemInfo> allFilesAndFolders = new List<FileSystemInfo>();
    private Vector2 scrollPosition;
    private string rootPath = "Assets";

    [MenuItem("PjPackage/Export PjPackage")]
    public static void ShowWindow()
    {
        GetWindow<PjPackageExporterWindow>("PjPackage Exporter");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Root Path:", GUILayout.Width(80));
        rootPath = EditorGUILayout.TextField(rootPath);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Select Files/Folders"))
        {
            PopulateFileList(rootPath);
            selectedFiles.Clear();
        }

        if (allFilesAndFolders.Count > 0)
        {
            EditorGUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (FileSystemInfo fileInfo in allFilesAndFolders)
            {
                if (fileInfo.Attributes.HasFlag(FileAttributes.Directory)) continue;

                string filePath = fileInfo.FullName;
                bool isSelected = selectedFiles.Contains(filePath);
                isSelected = EditorGUILayout.ToggleLeft(fileInfo.Name, isSelected);
                if (isSelected && !selectedFiles.Contains(filePath))
                {
                    selectedFiles.Add(filePath);
                }
                else if (!isSelected && selectedFiles.Contains(filePath))
                {
                    selectedFiles.Remove(filePath);
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.LabelField("Please select a root path and use 'Select Files/Folders'.");
        }

        if (GUILayout.Button("Export PjPackage") && selectedFiles.Count > 0)
        {
            string packageName = "MyPackage";
            string packagePath = EditorUtility.SaveFilePanel("Save PjPackage", "Assets", packageName, "pjpackage");
            if (!string.IsNullOrEmpty(packagePath))
            {
                ExportPjPackage(packagePath);
            }
            else
            {
                Debug.LogWarning("Export cancelled.");
            }
        }
    }

    private void PopulateFileList(string path)
    {
        allFilesAndFolders.Clear();
        try
        {
            var filesAndFolders = new DirectoryInfo(path).GetFileSystemInfos("*", SearchOption.AllDirectories)
                .Where(f => !f.FullName.EndsWith(".meta"));
            allFilesAndFolders.AddRange(filesAndFolders);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error accessing path '{path}': {e.Message}");
        }
    }

    private void ExportPjPackage(string packagePath)
    {
        try
        {
            PjPackage package = new PjPackage();
            package.Metadata = new PjPackageMetadata { Name = "MyPackage", Version = "1.0" };
            package.Files = new List<PjPackageFileEntry>();

            foreach (string filePath in selectedFiles)
            {
                if (!File.Exists(filePath)) continue;

                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    using (var reader = new BinaryReader(fileStream))
                    {
                        long fileSize = fileStream.Length;
                        byte[] fileData = reader.ReadBytes((int)fileSize);

                        package.Files.Add(new PjPackageFileEntry
                        {
                            FileName = filePath, // Now using full path
                            FileSize = fileSize,
                            Data = fileData
                        });
                    }
                }
                catch (IOException e)
                {
                    Debug.LogError($"Error reading file {filePath}: {e.Message}");
                }
                catch (OutOfMemoryException e)
                {
                    Debug.LogError($"Out of memory error reading {filePath}: {e.Message}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Unexpected error reading file {filePath}: {e.Message}\n{e.StackTrace}");
                }
            }

            PjPackageSerializer.Serialize(package, packagePath);
            AssetDatabase.Refresh();
            Debug.Log($"PjPackage exported to: {packagePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Error exporting PjPackage (IO Error): {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Critical error during export: {e.Message}\n{e.StackTrace}");
        }
    }
}
