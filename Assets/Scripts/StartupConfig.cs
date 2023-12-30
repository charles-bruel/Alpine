using System.IO;
using Mono.Cecil;
using UnityEngine;

public class StartupConfig {
    public static readonly string DEFAULT_CONFIG = "# 0: Very Low, 1: Low, 2: Medium, 3: High, 4: Very High, 5: Ultra\nint quality 5\n\nfloat tree_mul 1\nfloat rock_mul 1\nint max_visitor 1200\nfloat tree_lod_distance 1000";

    private static bool Initialized = false;
    public static void Initialize() {
        if (Initialized) return;
        Initialized = true;

        string configPath = Path.Combine(Application.persistentDataPath, ConfigHelper.CONFIG_NAME);
        if (!File.Exists(configPath)) {
            Debug.Log("Creating config file");
            File.WriteAllText(configPath, DEFAULT_CONFIG);
        }
        QualitySettings.SetQualityLevel(ConfigHelper.GetFile(ConfigHelper.CONFIG_NAME).GetInt("quality"));
    }
}