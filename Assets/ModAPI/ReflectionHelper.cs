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
