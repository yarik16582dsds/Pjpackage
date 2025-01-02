using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using PjPackageLibrary.DataStructures;
using PjPackageLibrary.Serialization;
using System;

public class PjPackageImporter : AssetPostprocessor
{
    void OnPreprocessAsset()
    {
        if (assetPath.EndsWith(".pjpackage"))
        {
            PjPackageImporterWindow.ShowWindow(assetPath);
            AssetDatabase.DeleteAsset(assetPath); // Prevent the .pjpackage file from being added to the project
        }
    }

    private void ImportPjPackage(string packagePath)
    {
        try
        {
            PjPackage package = PjPackageSerializer.Deserialize(packagePath);

            string projectAssetsPath = Application.dataPath;

            foreach (PjPackageFileEntry fileEntry in package.Files)
            {
                string relativePath = fileEntry.FileName.Replace("\\", "/");
                string fullPath = Path.Combine(projectAssetsPath, relativePath).Replace("\\", "/");
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

                    #if UNITY_EDITOR && DEBUG_LOGGING
                        Debug.Log($"Импортировано: {relativeToProjectPath}");
                    #endif
                }
                else
                {
                    Debug.LogWarning($"File {fullPath} already exists. Skipping.");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"PjPackage импортирован из: {packagePath}");

            // Удаляем файл .pjpackage после импорта
            File.Delete(packagePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка импорта: {e.Message}\n{e.StackTrace}");
        }
    }
}
