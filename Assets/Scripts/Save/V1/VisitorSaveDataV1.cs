using System;

[System.Serializable]
public struct VisitorSaveDataV1 {
    public SlopeDifficulty Ability;
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

        return result;
    }

    public enum PosRef {
        Link, Pos
    }
}