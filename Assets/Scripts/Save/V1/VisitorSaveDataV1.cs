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
public struct VisitorSaveDataV1 {
    public SlopeDifficulty Ability;
    public int TemplateIndex;
    public float RemainingTime;
    public float TraverseSpeed;
    public float SkiSpeed;
    public int PosID;
    public PosRef posRefType;
    public float Progress;

    public static VisitorSaveDataV1 FromVisitor(Visitor visitor, SavingContextV1 context) {
        VisitorSaveDataV1 result = new VisitorSaveDataV1();
        result.Ability = visitor.Ability;
        result.RemainingTime = visitor.RemainingTime;
        result.TraverseSpeed = visitor.TraverseSpeed;
        result.SkiSpeed = visitor.SkiSpeed;
        result.Progress = visitor.Progress;

        if(visitor.CurrentLink != null) {
            result.PosID = context.linkIds[visitor.CurrentLink];
            result.posRefType = PosRef.Link;
        } else {
            result.PosID = context.nodeIds[visitor.StationaryPos];
            result.posRefType = PosRef.Pos;
        }

        for(int i = 0;i < VisitorController.Instance.Templates.Length;i ++) {
            // TODO: Get better template equality check
            if(VisitorController.Instance.Templates[i].Ability == visitor.Ability) {
                result.TemplateIndex = i;
                break;
            }
        }

        return result;
    }

    public enum PosRef {
        Link, Pos
    }
}