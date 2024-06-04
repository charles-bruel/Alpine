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

[System.Serializable]
public struct ServiceInformation {
    public Service[] Service;
    public float Volume;
    public string Name;
    public int MaxPatrons;
    public Sprite Icon;

    public ServiceInformation(Service[] service, float volume, string name, int maxPatrons, Sprite icon) {
        Service = service;
        Volume = volume;
        Name = name;
        MaxPatrons = maxPatrons;
        Icon = icon;
    }
}