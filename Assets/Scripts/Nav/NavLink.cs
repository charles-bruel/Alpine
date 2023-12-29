using System;

public class NavLink {
    public INavNode A;
    public INavNode B;
    public float Cost;
    public SlopeDifficulty Difficulty;
    public INavLinkImplementation Implementation;
    public string Marker;

    private bool Dead = false;
    public bool IsDead() {
        if (A.IsDead() || B.IsDead()) {
            Dead = true;
        }
        return Dead;
    }

    public void Destroy() {
        Implementation.OnRemove();
        Dead = true;
    }
}