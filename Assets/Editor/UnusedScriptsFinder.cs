using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Simple Unity Editor window to find MonoBehaviour scripts that are not referenced by scenes/prefabs/assets.
/// It performs a conservative scan: a script is considered "unused" only if there are no references to the MonoScript asset
/// in other serialized assets (scenes, prefabs, scriptable objects, materials, etc.).
///
/// NOTE: This tool is conservative but not perfect. It cannot detect dynamic usages via Reflection or AddComponent("TypeName").
/// Always review results and keep backups (or move files instead of deleting).
/// </summary>
public class UnusedScriptsFinder : EditorWindow
{
    private Vector2 scroll;
    private List<string> candidates = new List<string>();
    private List<string> allMonoScriptPaths = new List<string>();
    private string statusMessage = "Ready";

    [MenuItem("Tools/Unused Scripts Finder")]
    public static void ShowWindow()
    {
        var w = GetWindow<UnusedScriptsFinder>("Unused Scripts Finder");
        w.minSize = new Vector2(600, 300);
    }

    void OnGUI()
    {
        GUILayout.Label("Unused Scripts Finder", EditorStyles.boldLabel);

        if (GUILayout.Button("Scan project for unused MonoBehaviour scripts"))
        {
            ScanProject();
        }

        GUILayout.Space(6);
        EditorGUILayout.LabelField("Status:", statusMessage);

        GUILayout.Space(6);
        if (candidates != null && candidates.Count > 0)
        {
            EditorGUILayout.LabelField($"Candidates found: {candidates.Count}");
            if (GUILayout.Button("Export CSV"))
            {
                ExportCsv();
            }

            if (GUILayout.Button("Move selected to Assets/UnusedScripts (non-destructive)"))
            {
                MoveCandidatesToFolder();
            }

            GUILayout.Space(6);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < candidates.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(candidates[i]);
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<MonoScript>(candidates[i]);
                    Selection.activeObject = obj;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("No candidates yet. Run a scan to populate the list.");
        }
    }

    private void ScanProject()
    {
        statusMessage = "Scanning...";
        Repaint();

        candidates.Clear();
        allMonoScriptPaths.Clear();

        // Find all .cs files that are Unity MonoScripts in Assets
        string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });
        foreach (var g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                allMonoScriptPaths.Add(path);
            }
        }

        // Build a set of referenced asset paths by searching serialized assets for GUID occurrences
        // We'll collect all text assets (scenes, prefabs, .asset, materials, etc.) and search their text for the GUID of each MonoScript.
        // To speed up, build a mapping from path->text and a reverse lookup.

        // Gather candidate containers
        string[] containerGuids = AssetDatabase.FindAssets("", new[] { "Assets" });
        var containerPaths = containerGuids.Select(AssetDatabase.GUIDToAssetPath).Where(p => p.StartsWith("Assets/")).ToArray();

        // Read file contents for text-based assets
        var pathToText = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in containerPaths)
        {
            try
            {
                // Binary files (fbx, png, etc.) will produce garbage; for performance skip large binary files by extension
                string ext = Path.GetExtension(p).ToLowerInvariant();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".fbx" || ext == ".dll" || ext == ".exe" || ext == ".mp3" || ext == ".wav" || ext == ".ogg")
                    continue;

                string text = File.ReadAllText(p);
                pathToText[p] = text;
            }
            catch
            {
                // ignore read errors
            }
        }

        // For each MonoScript, check if its GUID appears in any serialized asset text.
        foreach (var scriptPath in allMonoScriptPaths)
        {
            string guid = AssetDatabase.AssetPathToGUID(scriptPath);
            bool referenced = false;

            foreach (var kv in pathToText)
            {
                if (kv.Value.IndexOf(guid, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    referenced = true;
                    break;
                }
            }

            // Additionally, check scenes references via SceneManager (safer to check text found in .unity files already above)

            if (!referenced)
            {
                // Extra check: some scripts are referenced by name in text (AddComponent "Namespace.ClassName"), check type name occurrences
                var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                if (ms != null)
                {
                    var type = ms.GetClass();
                    if (type != null)
                    {
                        string typeName = type.FullName;
                        foreach (var kv in pathToText)
                        {
                            if (kv.Value.IndexOf(typeName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                referenced = true;
                                break;
                            }
                        }
                    }
                }
            }

            // If still not referenced, add to candidates
            if (!referenced)
            {
                candidates.Add(scriptPath);
            }
        }

        statusMessage = $"Scan complete. Found {candidates.Count} candidates (out of {allMonoScriptPaths.Count} MonoScripts).";
        Repaint();
    }

    private void ExportCsv()
    {
        string path = EditorUtility.SaveFilePanel("Export unused scripts CSV", "", "unused-scripts.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            using (var sw = new StreamWriter(path))
            {
                sw.WriteLine("AssetPath,GUID");
                foreach (var p in candidates)
                {
                    string guid = AssetDatabase.AssetPathToGUID(p);
                    sw.WriteLine($"\"{p}\",{guid}");
                }
            }
            EditorUtility.RevealInFinder(path);
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to export CSV: " + ex.Message);
        }
    }

    private void MoveCandidatesToFolder()
    {
        if (candidates == null || candidates.Count == 0) return;

        string destFolder = "Assets/UnusedScripts";
        if (!AssetDatabase.IsValidFolder(destFolder))
        {
            AssetDatabase.CreateFolder("Assets", "UnusedScripts");
        }

        int moved = 0;
        foreach (var p in candidates)
        {
            try
            {
                string fileName = Path.GetFileName(p);
                string destPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(destFolder, fileName));
                AssetDatabase.MoveAsset(p, destPath);
                // Move .meta is automatic when moving asset via AssetDatabase
                moved++;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to move {p}: {ex.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        statusMessage = $"Moved {moved} assets to {destFolder}.";
        // Clear list as they moved
        candidates.Clear();
    }
}
