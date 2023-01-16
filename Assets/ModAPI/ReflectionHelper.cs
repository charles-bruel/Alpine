using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class ReflectionHelper
{
    private static bool Initialized = false;
    private static void Initialize() {
        if(Initialized) return;
        Initialized = true;

        var x = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var y in x) {
            assemblies.Add(y.GetName().Name, y);
        }
    }

	private static Assembly LoadIfNotLoaded(string name)
	{
		if (ReflectionHelper.assemblies.ContainsKey(name))
		{
			return ReflectionHelper.assemblies[name];
		}
		string text = string.Concat(new string[]
		{
			"ModData/Assemblies/",
			name,
			".dll"
		});
		if (File.Exists(text))
		{
			Debug.Log(string.Concat(new string[]
			{
				"Loading assembly ",
				name,
				" from disk for the first time at ",
				text,
				"."
			}));
            return Assembly.Load(File.ReadAllBytes(text));
		}
        throw new FileNotFoundException();
	}

	public static object GetInstance(Type fallback, APIDef def)
	{
        Initialize();
		try
		{
			if (def == null || !def.HasContent())
			{
				return fallback.GetConstructor(new Type[0]).Invoke(null);
			}
			if (def.Namespace == "")
			{
				return ReflectionHelper.LoadIfNotLoaded(def.AssemblyName).GetType(def.ClassName).GetConstructor(new Type[0]).Invoke(null);
			}
			return ReflectionHelper.LoadIfNotLoaded(def.AssemblyName).GetType(def.Namespace + "." + def.ClassName).GetConstructor(new Type[0]).Invoke(null);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			Console.WriteLine(ex.StackTrace);
            return null;
		}
	}

	public static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
}
