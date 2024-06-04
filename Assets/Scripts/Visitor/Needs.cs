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

[System.Serializable]
public struct Needs {
    public float warmth;
    public float rest;
    public float bathroom;
    public float food;
    public float drink;

    public Needs(float warmth, float rest, float bathroom, float food, float drink) {
        this.warmth = warmth;
        this.rest = rest;
        this.bathroom = bathroom;
        this.food = food;
        this.drink = drink;
    }

    public Needs(Needs other) {
        this.warmth = other.warmth;
        this.rest = other.rest;
        this.bathroom = other.bathroom;
        this.food = other.food;
        this.drink = other.drink;
    }

    public float this[Need index] {
        get {
            switch (index) {
                case Need.WARMTH:
                    return warmth;
                case Need.REST:
                    return rest;
                case Need.BATHROOM:
                    return bathroom;
                case Need.FOOD:
                    return food;
                case Need.DRINK:
                    return drink;
            }
            throw new ArgumentException();
        }
        set {
            if(value < 0) value = 0;
            if(value > 1) value = 1;
            switch (index) {
                case Need.WARMTH:
                    warmth = value;
                    break;
                case Need.REST:
                    rest = value;
                    break;
                case Need.BATHROOM:
                    bathroom = value;
                    break;
                case Need.FOOD:
                    food = value;
                    break;
                case Need.DRINK:
                    drink = value;
                    break;
                throw new ArgumentException();
            }
        }
    }
}