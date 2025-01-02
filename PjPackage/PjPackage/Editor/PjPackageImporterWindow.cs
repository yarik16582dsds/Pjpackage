using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using PjPackageLibrary.DataStructures;
using PjPackageLibrary.Serialization;
using System;
using System.Linq;

public class PjPackageImporterWindow : EditorWindow
{
    private string importPath = "";
    private string targetDirectory = "";
    private PjPackage package;
    private List<bool> selectedFiles = new List<bool>();
    private Vector2 scrollPosition;

    public static void ShowWindow(string importPath)
    {
        PjPackageImporterWindow window = GetWindow<PjPackageImporterWindow>("PjPackage Importer");
        window.importPath = importPath;
        window.LoadPackage();
    }

    private void OnEnable()
    {
        // Automatically set the target directory to the project's Assets folder
        targetDirectory = Application.dataPath;
    }

    private void OnGUI()
    {
        if (package == null)
        {
            EditorGUILayout.LabelField("No package loaded.");
            return;
        }

        EditorGUILayout.LabelField("Import Path:", GUILayout.Width(100));
        EditorGUILayout.TextField(importPath);

        EditorGUILayout.BeginVertical();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < package.Files.Count; i++)
        {
            selectedFiles[i] = EditorGUILayout.ToggleLeft(package.Files[i].FileName, selectedFiles[i]);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Import Selected Files"))
        {
            ImportSelectedFiles();
        }
    }

    private void LoadPackage()
    {
        try
        {
            package = PjPackageSerializer.Deserialize(importPath);
            selectedFiles = new List<bool>(new bool[package.Files.Count]);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading package: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ImportSelectedFiles()
    {
        try
        {
            for (int i = 0; i < package.Files.Count; i++)
            {
                if (selectedFiles[i])
                {
                    PjPackageFileEntry fileEntry = package.Files[i];
                    string relativePath = fileEntry.FileName.Replace("\\", "/");
                    string fullPath = Path.Combine(targetDirectory, relativePath).Replace("\\", "/");
                    string directory = Path.GetDirectoryName(fullPath);

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(directory);

                    // Check if the file already exists
                    if (!File.Exists(fullPath))
                    {
                        File.WriteAllBytes(fullPath, fileEntry.Data);

                        // Convert the full path to a relative path
                        string relativeToProjectPath = "Assets" + fullPath.Substring(Application.dataPath.Length).Replace("\\", "/");
                        AssetDatabase.ImportAsset(relativeToProjectPath); // Important for Unity to recognize the asset
                    }
                    else
                    {
                        Debug.LogWarning($"File {fullPath} already exists. Skipping.");
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Selected files imported to: {targetDirectory}");
            this.Close(); // Close the window after import

            // Delete the .pjpackage file after import
            File.Delete(importPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Import failed: {e.Message}\n{e.StackTrace}");
        }
    }
}
