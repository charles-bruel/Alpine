//>============================================================================<
//
//    Alpine, Ski Resort Tycoon Game
//    Copyright (C) 2024  Charles Bruel
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//>============================================================================<

using System.IO;
using Mono.Cecil;
using UnityEngine;

public class StartupConfig {
    public static readonly string DEFAULT_CONFIG = "# 0: Very Low, 1: Low, 2: Medium, 3: High, 4: Very High, 5: Ultra\nint quality 5\n\nfloat tree_mul 1\nfloat rock_mul 1\nint max_visitors 1200\nfloat tree_lod_distance 1000";

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