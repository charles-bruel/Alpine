using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class SaveManager {
    public static Save GetSave() {
        SaveV1 data = SaveV1.CreateSaveData();
        Save save = new Save();
        save.version = 1;
        save.data = data;
        return save;
    }

    // TODO: Second thread
    public static void QueueSaveJob(Save save, String name) {
        string json = JsonConvert.SerializeObject(save, Formatting.Indented, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Error
        });
        System.IO.File.WriteAllText(name + ".json", json);
    }

    public static void LoadSave(String name) {
        string json = System.IO.File.ReadAllText(name + ".json");
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
}