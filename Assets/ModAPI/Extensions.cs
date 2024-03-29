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

using UnityEngine;

public static class Extensions {
    //Writing this as an extension method allows it to be called on a null
    //This is very convenient as we already have nice neat null behavior
    public static T Fetch<T>(this APIDef APIDef) where T : APIBase {
        object temp = ReflectionHelper.GetInstance(typeof(T), APIDef);
        if(!(temp is T)) {
            Debug.Log(typeof(T));
        }
        T toReturn = (T) ReflectionHelper.GetInstance(typeof(T), APIDef);

        if(APIDef == null) return toReturn;

        toReturn.FloatParameters      = APIDef.Params.FloatParameters;
        toReturn.IntParameters        = APIDef.Params.IntParameters;

        return toReturn;
    }
}