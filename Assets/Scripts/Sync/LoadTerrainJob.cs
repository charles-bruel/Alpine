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
using System.Threading;
using UnityEngine;

public class LoadTerrainJob : Job {
    public Color[] InputData;
    public float[,] OutputData;
    public int Width;
    public TerrainData TerrainData;
	public TerrainTile Reference;

	public static int ActiveJobs = 0;

    public void Run() {
		Interlocked.Increment(ref ActiveJobs);

        for (int i = 0; i < InputData.Length; i++)
		{
			Color color = InputData[i];
			int b = (int)(color.b * 255f);
			int g = (int)(color.g * 255f);
			if(g == 0) b++;
			int r = (int)(color.r * 255f);
			if(r == 0) g++;
			int num = b << 16 | g << 8 | r;
			int x = Width - 1 - (i / Width);
			int y = Width - 1 - (i % Width);
			OutputData[y, x] = (float)num / 16777215f;
		}

		Reference.HeightData = OutputData;

		Reference.DirtyStates |= TerrainTile.TerrainTileDirtyStates.CONTOURS;
		
		lock(ASyncJobManager.completedJobsLock) {
        	ASyncJobManager.Instance.completedJobs.Enqueue(this);
		}
    }

    public override void Complete() {
        try {
			TerrainData.SetHeights(0, 0, OutputData);
		} catch (Exception ex) {
			Debug.Log(ex.Message);
			Debug.Log(ex.StackTrace);
			Debug.Log("Error setting size. Make sure your heightmap is square and 2^n or 2^n+1 in size.");
		}

		Interlocked.Decrement(ref ActiveJobs);

		LoadingScreen.INSTANCE.LoadingTasks--;
    }

    public void Initialize() {
        LoadingScreen.INSTANCE.LoadingTasks++;
    }
}