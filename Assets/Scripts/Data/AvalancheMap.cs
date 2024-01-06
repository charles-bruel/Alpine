// Taken from https://github.com/charles-bruel/avalanche-github/blob/master/src/ModdedMap.cs, heavily modified
// Loads and stores information for an avalanche map from a meta file.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class AvalancheMap : IMap {
	public static AvalancheMap Load(string path) {
		CultureInfo.CurrentCulture = new CultureInfo("en-US");
		AvalancheMap result;
		try {
			float trees_rel = 50000f;
            float rocks_rel = 5000f;
			AvalancheMap moddedMap = new AvalancheMap();
			moddedMap.MapPath = path;
            string diskPath = Path.Combine(Application.persistentDataPath, "CustomMaps", path);
			if (!File.Exists(Path.Combine(diskPath, "meta"))) {
				Debug.Log("Couldn't find map at " + diskPath + ", assuming its been uninstalled");
				return null;
			}
			string[] lines = File.ReadAllLines(Path.Combine(diskPath, "meta"));
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i][0] != '#')
				{
					string[] line_contents = lines[i].Split(new char[]
					{
						'='
					});
					string param_name = line_contents[0].ToLower();
					if (param_name == "difficulty")
					{
						moddedMap.Difficulty = float.Parse(line_contents[1]);
					}
					else if (param_name == "name")
					{
						moddedMap.Name = line_contents[1];
					}

					else if (param_name == "thumb")
					{
						Texture2D texture2D = AvalancheMap.LoadPNG(Path.Combine(diskPath, line_contents[1]));
						moddedMap.Thumb = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
					}
					else if (param_name == "heightmap")
					{
						moddedMap.HeightMapPath = Path.Combine(diskPath, line_contents[1]);
					}
					else if (param_name == "decoration")
					{
						moddedMap.DecorationMapsPath = Path.Combine(diskPath, line_contents[1]);
					}
					else if (param_name == "height")
					{
						moddedMap.Height = float.Parse(line_contents[1]);
					}
					else if (param_name == "size")
					{
						moddedMap.Size = int.Parse(line_contents[1]);
					}
					else if (param_name == "treelower")
					{
						moddedMap.TreeLowerBound = float.Parse(line_contents[1]);
					}
					else if (param_name == "treeupper")
					{
						moddedMap.TreeUpperBound = float.Parse(line_contents[1]);
					}
					else if (param_name == "treeupperslope")
					{
						moddedMap.TreeUpperSlopeBound = float.Parse(line_contents[1]);
					}
					else if (param_name == "treelowerslope")
					{
						moddedMap.TreeLowerSlopeBound = float.Parse(line_contents[1]);
					}
					else if (param_name == "treesrel")
					{
						trees_rel = float.Parse(line_contents[1]);
					}
                    else if (param_name == "treesrel")
					{
						rocks_rel = float.Parse(line_contents[1]);
					}
					else if (param_name == "treesraw")
					{
						moddedMap.Trees = int.Parse(line_contents[1]);
					}
					else if (param_name == "rocksraw" || param_name == "rocks")
					{
						moddedMap.Rocks = int.Parse(line_contents[1]);
					}
					else if (param_name == "smoothmap")
					{
						moddedMap.SmoothMap = bool.Parse(line_contents[1]);
					}
					else if (param_name == "rockstyle")
					{
						moddedMap.RocksStyle = int.Parse(line_contents[1]);
					}
					else if (param_name == "rocklowerslope" || param_name == "rocklowerbound" || param_name == "rockslowerbound")
					{
						moddedMap.RocksLowerSlopeBound = float.Parse(line_contents[1]);
					}
					else if (param_name == "rockupperslope" || param_name == "rockupperbound" || param_name == "rocksupperbound")
					{
						moddedMap.RocksUpperSlopeBound = float.Parse(line_contents[1]);
					}
					else if (param_name == "blurradius")
					{
						moddedMap.BlurRadius = int.Parse(line_contents[1]);
					}
					else
					{
						Debug.Log("Cannot find parameter type: " + param_name);
					}
				}
			}
			if (moddedMap.Trees == -1)
			{
				float num3 = trees_rel / 2560000f;
				moddedMap.Trees = (int)(num3 * (float)moddedMap.Size * (float)moddedMap.Size);
			}
			if (moddedMap.Rocks == -1)
			{
				float num3 = rocks_rel / 2560000f;
				moddedMap.Rocks = (int)(num3 * (float)moddedMap.Size * (float)moddedMap.Size);
			}
			result = moddedMap;
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
			Debug.Log(ex.StackTrace);
			result = null;
		}
		return result;
	}

	public static Texture2D LoadPNG(string filePath) {
		Texture2D texture2D = null;
		if (File.Exists(filePath))
		{
			byte[] data = File.ReadAllBytes(filePath);
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(data);
		}
		return texture2D;
	}

    public Texture2D GenerateWeatherMap() {
        // Take the height map, invert it, and only save the blue channel
        Texture2D weatherMap = new Texture2D(this.HeightMap.width, this.HeightMap.height);
        Color[] pixels = this.HeightMap.GetPixels();
        for(int i = 0; i < pixels.Length; i++) {
            pixels[i].r = 0;
            pixels[i].g = 0;
            pixels[i].b = 1 - pixels[i].b;
            pixels[i].a = 1;
        }
        weatherMap.SetPixels(pixels);
        weatherMap.Apply(false);
        return weatherMap;
    }

	public void LoadTextures() {
		HeightMap = LoadPNG(HeightMapPath);
		if(DecorationMapsPath != null) { 
			DecorationMaps = LoadPNG(this.DecorationMapsPath); 
		} else {
			DecorationMaps = GenerateDecorationsMap();
		}
	}

    private Texture2D GenerateDecorationsMap() {
        // We have parameters for where trees and rocks are placed based on height, so use them
		// to generate a map of where trees and rocks should be placed
		Texture2D decorationsMap = new Texture2D(this.HeightMap.width, this.HeightMap.height);
		Color[] pixels = this.HeightMap.GetPixels();
		Color[] result = new Color[pixels.Length];

		float pixelHeight = this.Height;
		float pixelWidth = (float) this.Size / this.HeightMap.width;

		int lookahead_dist = 16;

		// green for trees, blue for rocks
		for(int i = 0; i < pixels.Length; i++) {
			int x = i % this.HeightMap.width;
			int y = i / this.HeightMap.width;

			float val = pixels[i].b;
			// Only look at partials in the positive direction.
			// This is fine as an approximation
			float delhdelx = 0;
			if(x < this.HeightMap.width - lookahead_dist) {
				delhdelx = pixels[i + lookahead_dist].b - val;
			}
			float delhdely = 0;
			if(y < this.HeightMap.height - lookahead_dist) {
				delhdely = pixels[i + lookahead_dist * this.HeightMap.width].b - val;
			}
			delhdelx *= pixelHeight / pixelWidth / lookahead_dist;
			delhdely *= pixelHeight / pixelWidth / lookahead_dist;
			
			// Find magnitude of grad(h) 
			float slope = new Vector2(delhdelx, delhdely).magnitude;
			slope = Mathf.Atan(slope) / (Mathf.PI / 2);
			// Trees
			float treeHeightFactor;
			if(val < TreeLowerBound) {
				treeHeightFactor = 1;
			} else if(val > TreeUpperBound) {
				treeHeightFactor = 0;
			} else {
				treeHeightFactor = (val - TreeLowerBound) / (TreeUpperBound - TreeLowerBound);
			}

			float treeSlopeFactor;
			if(slope < TreeLowerSlopeBound) {
				treeSlopeFactor = 1;
			} else if(slope > TreeUpperSlopeBound) {
				treeSlopeFactor = 0;
			} else {
				treeSlopeFactor = (slope - TreeLowerSlopeBound) / (TreeUpperSlopeBound - TreeLowerSlopeBound);
			}

			float g = treeHeightFactor * treeSlopeFactor;
			
			float rockFactor;
			if(val < RocksLowerSlopeBound) {
				rockFactor = 0;
			} else if(val > RocksUpperSlopeBound) {
				rockFactor = 1;
			} else {
				rockFactor = (val - RocksUpperSlopeBound) / (RocksLowerSlopeBound - RocksUpperSlopeBound);
			}

			float b = rockFactor;

			result[i] = new Color(0, g, b);
		}

		decorationsMap.SetPixels(result);
		decorationsMap.Apply(false);

		return decorationsMap;
    }

    public Texture2D[] SplitIntoTiles(int divisions) {
        Assert.AreEqual(this.HeightMap.width, this.HeightMap.height);

        int outsize = (this.HeightMap.width) / divisions + 1;

        Color[] tempResult = new Color[outsize * outsize];
        Color[] rawData = this.HeightMap.GetPixels();
        Texture2D[] tiles = new Texture2D[divisions * divisions];
        for (int texture_index = 0; texture_index < divisions * divisions; texture_index++) {
            int imageY = (texture_index % divisions);
            // int imageX = divisions - (texture_index / divisions) - 1;
            int imageX = texture_index / divisions;

            for(int lookup_index = 0; lookup_index < outsize * outsize; lookup_index++) {
                int x = (lookup_index % outsize);
                int y = (lookup_index / outsize);

                int absX = x + imageX * (outsize - 1);
                int absY = y + imageY * (outsize - 1);

                x = outsize - x - 1;
                y = outsize - y - 1;
                
                if(SmoothMap) {
                    float val = Smooth(absX, absY, rawData, this.HeightMap.width);
                    tempResult[x + y * outsize] = FromVal(val);
                } else {
                    tempResult[x + y * outsize] = Retrieve(absX, absY, rawData, this.HeightMap.width).Item1;
                }
            }

            tiles[texture_index] = new Texture2D(outsize, outsize);
            tiles[texture_index].SetPixels(0, 0, outsize, outsize, tempResult);
            tiles[texture_index].Apply(false);
        }

        return tiles;
    }

    private float Smooth(int absX, int absY, Color[] rawData, int imgSize) {
        float sum = 0;
        int count = 0;

        for(int x = absX - BlurRadius; x <= absX + BlurRadius; x++) {
            for(int y = absY - BlurRadius; y <= absY + BlurRadius; y++) {
                Tuple<Color, int> result = Retrieve(x, y, rawData, imgSize);
                sum += result.Item1.b * result.Item2;
                count += result.Item2;
            }
        }

        return sum / count;
    }

    private Tuple<Color, int> Retrieve(int absX, int absY, Color[] rawData, int imgSize) {
        if(absX < 0 || absY < 0 || absX >= imgSize || absY >= imgSize) {
            return new Tuple<Color, int>(Color.black, 0);
        }

        int raw_data_index = absY + absX * imgSize;
        return new Tuple<Color, int>(rawData[raw_data_index], 1);
    }

    private Color FromVal(float val) {
        // Take float from 0-1, scale it to 0-16777215, then convert to int econded in the RGB channels
        // return new Color(val, val, val);
        int intVal = (int)(val * 0x00FFFFFE);
        int rInt = (intVal & 0x0000FF) >> 0;
        int gInt = (intVal & 0x00FF00) >>  8;
        int bInt = (intVal & 0xFF0000) >> 16;
        return new Color(rInt/255f, gInt/255f, bInt/255f);
    }

    public void Load(TerrainManager terrainManager) {
        terrainManager.CopyMapData(this);
    }

	public static List<AvalancheMap> GetMaps() {
		string path = Path.Combine(Application.persistentDataPath, "CustomMaps");
		List<AvalancheMap> list = new List<AvalancheMap>();
		if (!Directory.Exists(path)) {
			Debug.Log("Unable to find maps directory");
			return new List<AvalancheMap>();
		}
		string[] directories = Directory.GetDirectories(path);
		for (int i = 0; i < directories.Length; i++) {
			AvalancheMap moddedMap = AvalancheMap.Load(directories[i]);
			if (moddedMap != null) {
				list.Add(moddedMap);
			}
		}
		return list;
	}

    public string GetName() {
        return Name;
    }

    public string GetID() {
		return "avl-" + MapPath;
    }

    public float Difficulty = 3f;

	public float Height = 350f;

	public string Name = "Unknown";

	public Sprite Thumb;

	public int SnowfrontCount;

	public Texture2D HeightMap;

	public string MapPath;

	public int Size = 1600;

	public float TreeLowerBound;

	public float TreeUpperBound = 1f;

	public float TreeUpperSlopeBound = 0.5f;

	public int Trees = -1;

	public float TreeLowerSlopeBound = 0.1f;

	public int Rocks = -1;

	public bool SmoothMap = true;

	public Texture2D DecorationMaps;

	public int RocksStyle = 1;

	public float RocksUpperSlopeBound = 0.9f;

	public float RocksLowerSlopeBound = 0.2f;

	public string HeightMapPath;

	public string DecorationMapsPath;

	public int BlurRadius = 2;
}