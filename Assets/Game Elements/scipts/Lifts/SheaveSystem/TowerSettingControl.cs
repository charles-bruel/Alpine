using UnityEngine;

class TowerSettingControl : SettingControl
{
    public float AngleThreshold1 = 2.5f;
    public float AngleThreshold2 = 15f;
    public float AngleThreshold3 = 25f;
    public float TiltThreshold = 5f;
    public float HalfTiltThreshold = 20f;
    public int AllTiltThreshold = 6;
    public TowerAssemblyScript tower;
    private int BaseIndex;
    private bool Initialized = false;
    private void Initialize()
    {
        BaseIndex = tower.SheaveLayout;
        Initialized = true;
    }
    public override void Run()
    {
        if (!Initialized) Initialize();
        float avg = Mathf.Abs(tower.StartAngle + tower.EndAngle - 180) / 2;
        float overall = Mathf.Abs(tower.StartAngle - tower.EndAngle - 180);
        int index = BaseIndex;
        if (overall > AngleThreshold1) index++;
        if (overall > AngleThreshold2) index++;
        if (overall > AngleThreshold3) index++;
        tower.SheaveLayout = index;
        if (index >= AllTiltThreshold)
        {
            tower.TiltTowers = true;
            tower.HalfTilt = false;
        } 
        else
        {
            if (avg > TiltThreshold)
            {
                tower.TiltTowers = true;
                if (avg > HalfTiltThreshold)
                {
                    tower.HalfTilt = true;
                }
                else 
                {
                    tower.HalfTilt = false;
                }
            } 
            else
            {
                tower.TiltTowers = false;
                tower.HalfTilt = false;
            }
        }
    }
}