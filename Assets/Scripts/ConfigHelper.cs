// Taken from https://raw.githubusercontent.com/charles-bruel/avalanche-github/master/src/ConfigHelper.cs
// Modified slightly
// THIS IS A TEMPORARY SOLUTION

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class ConfigHelper
{
    public static readonly string CONFIG_NAME = "settings.cfg";

	public static ConfigHelper.ConfigFile GetFile(string fileName)
	{
        fileName = Path.Combine(Application.persistentDataPath, fileName);
		if (ConfigHelper.configs.ContainsKey(fileName))
		{
			return ConfigHelper.configs[fileName];
		}
		return ConfigHelper.InitializeFile(fileName);
	}

	public static ConfigHelper.ConfigFile InitializeFile(string fileName)
	{
		Debug.Log("Loading " + fileName + " for the first time");
		CultureInfo.CurrentCulture = new CultureInfo("en-US");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		Dictionary<string, float> dictionary3 = new Dictionary<string, float>();
		Dictionary<string, Vector3> dictionary4 = new Dictionary<string, Vector3>();
		StreamReader streamReader = new StreamReader(fileName);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			if (text.Length != 0 && text[0] != '#')
			{
				string[] array = text.Split(new char[]
				{
					' '
				});
				if (array.Length != 3)
				{
					Debug.Log(string.Concat(new string[]
					{
						"Invalid number of parameter on line ",
						text,
						" in file ",
						fileName,
						"."
					}));
				}
				else if (array[0] == "string")
				{
					dictionary.Add(array[1], array[2]);
				}
				else
				{
					if (array[0] == "int")
					{
						try
						{
							dictionary2.Add(array[1], int.Parse(array[2]));
							continue;
						}
						catch (Exception ex)
						{
							Debug.Log(ex.Message);
							Debug.Log("On " + array[2]);
							dictionary2.Add(array[1], 0);
							continue;
						}
					}
					if (array[0] == "float")
					{
						try
						{
							dictionary3.Add(array[1], float.Parse(array[2]));
							continue;
						}
						catch (Exception ex2)
						{
							Debug.Log(ex2.Message);
							Debug.Log("On " + array[2]);
							dictionary3.Add(array[1], 0f);
							continue;
						}
					}
					if (array[0] == "vec3")
					{
						try
						{
							string[] array2 = array[2].Split(new char[]
							{
								','
							});
							if (array2.Length != 3)
							{
								Debug.Log(string.Concat(new string[]
								{
									"Invalid number of values on line ",
									text,
									" in file ",
									fileName,
									"."
								}));
							}
							else
							{
								array2[0].Trim();
								array2[1].Trim();
								array2[2].Trim();
								dictionary4.Add(array[1], new Vector3(float.Parse(array2[0]), float.Parse(array2[1]), float.Parse(array2[2])));
							}
							continue;
						}
						catch (Exception ex3)
						{
							Debug.Log(ex3.Message);
							Debug.Log("On " + array[2]);
							dictionary4.Add(array[1], default(Vector3));
							continue;
						}
					}
					Debug.Log(string.Concat(new string[]
					{
						"Data type ",
						array[0],
						" not found in file ",
						fileName,
						"."
					}));
				}
			}
		}
		ConfigHelper.ConfigFile configFile = new ConfigHelper.ConfigFile(dictionary, dictionary2, dictionary3, dictionary4);
		ConfigHelper.configs.Add(fileName, configFile);
		return configFile;
	}

	public static string GetString(string configName, string valName)
	{
		string result;
		try
		{
			result = ConfigHelper.GetFile(configName).GetString(valName);
		}
		catch
		{
			Debug.Log("(str)Unable to get config value (or could be bad assignment)" + valName.ToString() + " from " + configName.ToString());
			result = null;
		}
		return result;
	}

	public static int GetInt(string configName, string valName)
	{
		int result;
		try
		{
			result = ConfigHelper.GetFile(configName).GetInt(valName);
		}
		catch
		{
			Debug.Log("(int)Unable to get config value (or could be bad assignment)" + valName.ToString() + " from " + configName.ToString());
			result = 0;
		}
		return result;
	}

	public static float GetFloat(string configName, string valName)
	{
		float result;
		try
		{
			result = ConfigHelper.GetFile(configName).GetFloat(valName);
		}
		catch
		{
			Debug.Log("(float)Unable to get config value (or could be bad assignment) " + valName.ToString() + " from " + configName.ToString());
			result = 0f;
		}
		return result;
	}

	public static Vector3 GetVec3(string configName, string valName)
	{
		return ConfigHelper.GetFile(configName).GetVec3(valName);
	}

	public static void Reset()
	{
		ConfigHelper.configs = new Dictionary<string, ConfigHelper.ConfigFile>();
	}

	private static Dictionary<string, ConfigHelper.ConfigFile> configs = new Dictionary<string, ConfigHelper.ConfigFile>();

	public class ConfigFile
	{
		public ConfigFile()
		{
		}

		public ConfigFile(Dictionary<string, string> sStrings, Dictionary<string, int> sInts, Dictionary<string, float> sFloats, Dictionary<string, Vector3> sVecs)
		{
			this.sStrings = sStrings;
			this.sInts = sInts;
			this.sFloats = sFloats;
			this.sVecs = sVecs;
		}

		public string GetString(string valName)
		{
			return this.sStrings[valName];
		}

		public int GetInt(string valName)
		{
			return this.sInts[valName];
		}

		public float GetFloat(string valName)
		{
			return this.sFloats[valName];
		}

		public Vector3 GetVec3(string valName)
		{
			return this.sVecs[valName];
		}

		public void AddExternalValue(string valName, string value)
		{
			if (this.sStrings.ContainsKey(valName))
			{
				return;
			}
			this.sStrings.Add(valName, value);
		}

		public void AddExternalValue(string valName, int value)
		{
			if (this.sInts.ContainsKey(valName))
			{
				return;
			}
			this.sInts.Add(valName, value);
		}

		public void AddExternalValue(string valName, float value)
		{
			if (this.sFloats.ContainsKey(valName))
			{
				return;
			}
			this.sFloats.Add(valName, value);
		}

		public void AddExternalValue(string valName, Vector3 value)
		{
			if (this.sVecs.ContainsKey(valName))
			{
				return;
			}
			this.sVecs.Add(valName, value);
		}

		private Dictionary<string, string> sStrings = new Dictionary<string, string>();

		private Dictionary<string, int> sInts = new Dictionary<string, int>();

		private Dictionary<string, float> sFloats = new Dictionary<string, float>();

		private Dictionary<string, Vector3> sVecs = new Dictionary<string, Vector3>();
	}
}
