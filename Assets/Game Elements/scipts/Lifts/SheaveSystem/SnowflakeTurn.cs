using System.Collections.Generic;
using UnityEngine;

class SnowflakeTurn : GenericTurn
{

    [Header("Parameters")]
    public float TargetAngleTurnAmount;
    public float Heading;

    [Header("Configuration")]
    public float LineSpacing;
    public float TurnWheelDistance;
    public float VerticalSpacing;

    [Header("Linking")]
    public Transform CablePointsUpParent;
    public Transform CablePointsDownParent;

    [Header("Prefab")]
    public BullWheel BullWheel;
    public TurnTowerA AsymTower1;
    public TurnTowerA AsymTower2;
    public TurnTowerB AsymTower3;
    public TurnTowerB AsymTower4;

    [Header("Runtime Data")]
    public bool Initialized;
    public Vector3 MainWheelLocalEdgePoint;

    public List<Transform> BullWheelCablePointsUp;
    public List<Transform> BullWheelCablePointsDown;

    public bool Reversed;

    public bool Debug;

    [Header("Runtime Elements")]
    public TurnTowerA Tower1;
    public TurnTowerA Tower2;
    public TurnTowerB Tower3;
    public TurnTowerB Tower4;
    public BullWheel MainWheel;
    public BullWheel TurnStage1;
    public BullWheel TurnStage2;
    public Transform MainWheelEdgePoint;

    void Update()
    {
        if (Debug) Reset();
    }

    public override void Reset()
    {
        if (!Initialized)
        {
            Initialize();
            Initialized = true;
        }

        UpdateParameters();
        PlaceBullwheels();
        PlaceTowers();
        DoSheaves();
        AssignCablePoints();
    }

    private void UpdateParameters()
    {
        Vector3 dif = transform.position - IncomingCablePoint.transform.position;
        float IncomingHeading = Mathf.Atan2(dif.x, dif.z) * Mathf.Rad2Deg;

        dif = OutgoingCablePoint.transform.position - transform.position;
        float OutgoingHeading = Mathf.Atan2(dif.x, dif.z) * Mathf.Rad2Deg;

        if (OutgoingHeading - IncomingHeading > 0)
        {
            Reversed = true;
            var x = IncomingHeading;
            IncomingHeading = OutgoingHeading + 180;
            OutgoingHeading = x + 180;
        }
        else
        {
            Reversed = false;
        }

        Heading = IncomingHeading;
        TargetAngleTurnAmount = OutgoingHeading - IncomingHeading;


        transform.localEulerAngles = new Vector3(0, Heading, 0);
    }

    void OnDrawGizmos()
    {
        if (!Debug) return;
        Gizmos.color = Color.black;
        for (int i = 0; i < CablePointsUp.Count - 1; i++)
        {
            Gizmos.DrawLine(CablePointsUp[i].position, CablePointsUp[i + 1].position);
        }
        for (int i = 0; i < CablePointsDown.Count - 1; i++)
        {
            Gizmos.DrawLine(CablePointsDown[i].position, CablePointsDown[i + 1].position);
        }
    }


    private void PlaceBullwheels()
    {

        //Main wheel
        float radius = BullWheel.Radius;
        float HalfWidth = LineSpacing * 0.5f;
        float theta = Mathf.Deg2Rad * TargetAngleTurnAmount * 0.5f;

        MainWheelLocalEdgePoint = new Vector3(HalfWidth, 0, HalfWidth * Mathf.Tan(theta));

        MainWheelEdgePoint.transform.localPosition = MainWheelLocalEdgePoint;
        theta = Mathf.Deg2Rad * TargetAngleTurnAmount * 0.5f;
        float distance = radius / Mathf.Cos(theta);
        Vector3 MainWheelLocation;


        MainWheelLocation = MainWheelLocalEdgePoint + new Vector3(-distance * Mathf.Cos(theta), 0, -distance * Mathf.Sin(theta));
        MainWheel.transform.localEulerAngles = new Vector3(0, TargetAngleTurnAmount * 0.5f + 180, 0);

        MainWheel.transform.localPosition = MainWheelLocation;


        //Turn stage 1
        //Easier cause it's aligned with the coordinate system
        Vector3 TurnStage1Location;

        TurnStage1Location = new Vector3(-HalfWidth + radius, VerticalSpacing * 1.5f, TurnWheelDistance);

        TurnStage1.transform.localPosition = TurnStage1Location;
        TurnStage1.transform.localEulerAngles = new Vector3(0, 90 + TargetAngleTurnAmount * 0.25f, 0);

        TurnStage1.Right.transform.localEulerAngles = new Vector3(0, TargetAngleTurnAmount * 0.25f, 0);
        TurnStage1.Left.transform.localEulerAngles = new Vector3(0, -TargetAngleTurnAmount * 0.25f, 0);

        theta = Mathf.Abs(Mathf.Deg2Rad * 0.25f * TargetAngleTurnAmount);
        float scaleMul = TurnStage1.TowerDist * Mathf.Sin(theta * 0.5f) + TurnStage1.Radius / Mathf.Cos(theta);
        scaleMul /= TurnStage1.BaseTowerLength;
        TurnStage1.Right.transform.localScale = new Vector3(scaleMul, 1, 1);
        TurnStage1.Left.transform.localScale = new Vector3(scaleMul, 1, 1);

        //Turn stage 2
        //Much harder
        //Reflection across the line through the origin.
        theta = Mathf.Deg2Rad * TargetAngleTurnAmount * 0.5f;
        float m = -1 / Mathf.Tan(theta);
        //b is 0 because it goes through the origin.
        float A = -m;
        float B = 1;
        float C = 0;

        Vector2 output = ReflectAcrossArbitraryLine(new Vector2(TurnStage1Location.x, TurnStage1Location.z), A, B, C);
        Vector3 TurnStage2Location = new Vector3(output.x, VerticalSpacing * 0.5f, output.y);
        TurnStage2.transform.localPosition = TurnStage2Location;
        TurnStage2.transform.localEulerAngles = new Vector3(0, -90 + TargetAngleTurnAmount * 0.75f, 0);

        TurnStage2.Right.transform.localEulerAngles = new Vector3(0, TargetAngleTurnAmount * 0.25f, 0);
        TurnStage2.Left.transform.localEulerAngles = new Vector3(0, -TargetAngleTurnAmount * 0.25f, 0);

        TurnStage2.Right.transform.localScale = new Vector3(scaleMul, 1, 1);
        TurnStage2.Left.transform.localScale = new Vector3(scaleMul, 1, 1);
    }

    private void PlaceTowers()
    {
        Tower1.transform.localPosition = new Vector3(0, -10.25f, -TurnWheelDistance * 0.25f);
        Tower3.transform.localPosition = new Vector3(0, -10.25f, TurnWheelDistance * 0.3333f);

        float theta = (90 - TargetAngleTurnAmount) * Mathf.Deg2Rad;
        float s = Mathf.Sin(theta);
        float c = Mathf.Cos(theta);

        Tower2.transform.localPosition = new Vector3(c * TurnWheelDistance * 0.25f, -10.25f, s * TurnWheelDistance * 0.25f);
        Tower2.transform.localEulerAngles = new Vector3(0, TargetAngleTurnAmount, 0);

        Tower4.transform.localPosition = new Vector3(-c * TurnWheelDistance * 0.33f, -10.25f, -s * TurnWheelDistance * 0.33f);
        Tower4.transform.localEulerAngles = new Vector3(0, TargetAngleTurnAmount, 0);
    }

    private void DoSheaves()
    {
        TurnStage1.ControllerLeft.End = Tower3.Upper.transform;
        TurnStage1.ControllerLeft.PushUpdate();

        TurnStage2.ControllerLeft.End = Tower4.Upper.transform;
        TurnStage2.ControllerLeft.PushUpdate();

        TurnStage2.ControllerRight.Start = TurnStage1.ControllerLeft.transform;
        TurnStage2.ControllerRight.PushUpdate();

        TurnStage1.ControllerRight.Start = TurnStage2.ControllerRight.transform;
        TurnStage1.ControllerRight.PushUpdate();

        Tower2.ControllerUpper.End = Tower4.ControllerUpper.transform;
        Tower2.ControllerUpper.Start = Reversed ? IncomingCablePoint.transform : OutgoingCablePoint.transform;
        Tower2.ControllerUpper.PushUpdate();

        Tower4.ControllerUpper.End = TurnStage2.ControllerRight.transform;
        Tower4.ControllerUpper.Start = Tower2.ControllerUpper.transform;
        Tower4.ControllerUpper.PushUpdate();

        Tower1.ControllerUpper.Start = Tower3.ControllerUpper.transform;
        Tower1.ControllerUpper.End = Reversed ? OutgoingCablePoint.transform : IncomingCablePoint.transform;
        Tower1.ControllerUpper.PushUpdate();

        Tower3.ControllerUpper.Start = TurnStage1.ControllerRight.transform;
        Tower3.ControllerUpper.End = Tower1.ControllerUpper.transform;
        Tower3.ControllerUpper.PushUpdate();

        Tower2.ControllerLower.Start = MainWheelEdgePoint;
        Tower2.ControllerLower.End = Reversed ? IncomingCablePoint.transform : OutgoingCablePoint.transform;
        Tower2.ControllerLower.PushUpdate();

        Tower1.ControllerLower.End = MainWheelEdgePoint;
        Tower1.ControllerLower.Start = Reversed ? OutgoingCablePoint.transform : IncomingCablePoint.transform;
        Tower1.ControllerLower.PushUpdate();
    }

    private void AssignCablePoints()
    {
        float HalfWidth = LineSpacing * 0.5f;

        CablePointsUp.Clear();
        CablePointsDown.Clear();

        List<Vector3> MainWheelCPs;
        List<Vector3> TurnWheel1CPs;
        List<Vector3> TurnWheel2CPs;



        MainWheelCPs = MainWheel.GetCablePoints(8, 0, -TargetAngleTurnAmount);
        TurnWheel1CPs = TurnStage1.GetCablePoints(8, 180, 0 - TargetAngleTurnAmount * 0.5f);
        TurnWheel2CPs = TurnStage2.GetCablePoints(8, -TargetAngleTurnAmount * 0.5f, 180 - TargetAngleTurnAmount);

        for (int i = 0; i < BullWheelCablePointsUp.Count; i++)
        {

            BullWheelCablePointsUp[i].localPosition = MainWheelCPs[i];
        }

        CablePointsUp.AddRange(Tower1.ControllerLower.Target.GetAllCablePoints(!Tower1.ControllerLower.Target.Uphill));
        CablePointsUp.AddRange(BullWheelCablePointsUp);
        CablePointsUp.AddRange(Tower2.ControllerLower.Target.GetAllCablePoints(!Tower2.ControllerLower.Target.Uphill));

        for (int i = 0; i < BullWheelCablePointsDown.Count; i++)
        {
            if (i <= 7)
            {
                BullWheelCablePointsDown[i].localPosition = TurnWheel1CPs[i];
            }
            else if (i <= 15)
            {
                BullWheelCablePointsDown[i].localPosition = TurnWheel2CPs[i - 8];
            }
        }

        List<Transform> temp = Tower1.ControllerUpper.Target.GetAllCablePoints(!Tower1.ControllerUpper.Target.Uphill);
        temp.Reverse();
        CablePointsDown.AddRange(temp);
        temp = Tower3.ControllerUpper.Target.GetAllCablePoints(!Tower3.ControllerUpper.Target.Uphill);
        temp.Reverse();
        CablePointsDown.AddRange(temp);


        temp = TurnStage1.ControllerLeft.Target.GetAllCablePoints(!TurnStage1.ControllerLeft.Target.Uphill);
        temp.Reverse();

        CablePointsDown.AddRange(temp);

        for (int i = 0; i < 8; i++)
        {
            CablePointsDown.Add(BullWheelCablePointsDown[i]);
        }


        temp = TurnStage1.ControllerRight.Target.GetAllCablePoints(!TurnStage1.ControllerRight.Target.Uphill);
        temp.Reverse();

        CablePointsDown.AddRange(temp);

        temp = TurnStage2.ControllerLeft.Target.GetAllCablePoints(!TurnStage2.ControllerLeft.Target.Uphill);
        temp.Reverse();

        CablePointsDown.AddRange(temp);

        for (int i = 8; i < 16; i++)
        {
            CablePointsDown.Add(BullWheelCablePointsDown[i]);
        }


        temp = TurnStage2.ControllerRight.Target.GetAllCablePoints(!TurnStage2.ControllerRight.Target.Uphill);
        temp.Reverse();

        CablePointsDown.AddRange(temp);
        temp = Tower4.ControllerUpper.Target.GetAllCablePoints(!Tower4.ControllerUpper.Target.Uphill);
        temp.Reverse();
        CablePointsDown.AddRange(temp);
        temp = Tower2.ControllerUpper.Target.GetAllCablePoints(!Tower2.ControllerUpper.Target.Uphill);
        temp.Reverse();
        CablePointsDown.AddRange(temp);

        if (Reversed)
        {
            var x = CablePointsDown;
            CablePointsDown = CablePointsUp;
            CablePointsUp = x;
            CablePointsUp.Reverse();
            CablePointsDown.Reverse();
        }

    }

    //Thanks to https://stackoverflow.com/questions/2259476/rotating-a-point-about-another-point-2d
    private Vector2 RotatePointByAngle(Vector2 pointIn, float theta)
    {
        float s = Mathf.Sin(theta);
        float c = Mathf.Cos(theta);
        return new Vector2(pointIn.x * c - pointIn.y * s, pointIn.x * s + pointIn.y * c);
    }

    //Thanks to https://math.stackexchange.com/questions/1013230/how-to-find-coordinates-of-reflected-point
    private Vector2 ReflectAcrossArbitraryLine(Vector2 pointIn, float A, float B, float C)
    {
        float numx = pointIn.x * (A * A - B * B) - 2 * B * (A * pointIn.y + C);
        float numy = pointIn.y * (B * B - A * A) - 2 * A * (B * pointIn.x + C);
        float dom = A * A + B * B;
        return new Vector2(numx / dom, numy / dom);
    }

    private void Initialize()
    {
        MainWheel = Instantiate(BullWheel, transform);
        TurnStage1 = Instantiate(BullWheel, transform);
        TurnStage2 = Instantiate(BullWheel, transform);

        TurnStage1.LeftEnabled = true;
        TurnStage1.RightEnabled = true;
        TurnStage2.LeftEnabled = true;
        TurnStage2.RightEnabled = true;


        Tower1 = Instantiate(AsymTower1, transform);
        Tower2 = Instantiate(AsymTower2, transform);
        Tower3 = Instantiate(AsymTower3, transform);
        Tower4 = Instantiate(AsymTower4, transform);

        MainWheelEdgePoint = new GameObject("Main Wheel Edge Point").transform;
        MainWheelEdgePoint.transform.parent = transform;

        BullWheelCablePointsUp = new List<Transform>(8);
        BullWheelCablePointsDown = new List<Transform>(16);
        for (int i = 0; i < BullWheelCablePointsUp.Capacity; i++)
        {
            GameObject temp = new GameObject();
            temp.transform.parent = CablePointsUpParent;
            temp.transform.localScale = new Vector3(1, 0.1f, 1);
            temp.name = "CP";
            BullWheelCablePointsUp.Add(temp.transform);
        }
        for (int i = 0; i < BullWheelCablePointsDown.Capacity; i++)
        {
            GameObject temp = new GameObject();
            temp.transform.parent = CablePointsDownParent;
            temp.name = "CP";
            temp.transform.localScale = new Vector3(1, 0.1f, 1);
            BullWheelCablePointsDown.Add(temp.transform);
        }

        CablePointsUp = new List<Transform>();
        CablePointsDown = new List<Transform>();
    }

    public override float GetLength()
    {
        return TurnWheelDistance * 0.25f;
    }
}