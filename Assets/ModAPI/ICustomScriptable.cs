using System.Collections.Generic;
using UnityEngine;

public interface ICustomScriptable {
    public abstract Dictionary<string, object> PersistentData();
    public abstract GameObject GetGameObject();
}