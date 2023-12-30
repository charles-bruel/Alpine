using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class SaveManager {
    public static Save GetSave() {
        SaveV1 data = SaveV1.CreateSaveData();
        Save save = new Save();
        save.version = 1;
        save.data = data;
        return save;
    }

    // TODO: Second thread
    public static void QueueSaveJob(Save save, String name, SaveLoadScreen screen) {
        CreateSaveGameDirectory();
        string json = JsonConvert.SerializeObject(save, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Error
        });
        System.IO.File.WriteAllText(GetSaveGameFilePath(name), json);

        if(screen != null) screen.Hide();
    }

    public static void LoadSave(String name) {
        CreateSaveGameDirectory();
        string json = System.IO.File.ReadAllText(GetSaveGameFilePath(name));
        Save save = JsonConvert.DeserializeObject<Save>(json);
        JObject jobject = (JObject) save.data;
        switch(save.version) {
            case 1:
                SaveV1 data = jobject.ToObject<SaveV1>();
                data.Restore();
                break;
        }
    }

    public class Save {
        public int version;
        public object data;
    }

    public static string GetSaveGameFilePath(string name) {
        return Path.Combine(Application.persistentDataPath, "Saves", name + ".json");
    }

    public static void CreateSaveGameDirectory() {
        string path = Path.Combine(Application.persistentDataPath, "Saves");
        if(!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    public static List<string> GetSaves() {
        string saveGameDirectory = Path.Combine(Application.persistentDataPath, "Saves");
        if(!Directory.Exists(saveGameDirectory)) {
            return new List<string>();
        }
        string[] files = Directory.GetFiles(saveGameDirectory);
        List<string> saves = new List<string>();
        foreach(string file in files) {
            if(file.EndsWith(".json")) {
                saves.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        return saves;
    }
}