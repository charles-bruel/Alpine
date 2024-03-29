using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PolygonTool {
    public PolygonConstructionData Data;
    public PolygonBuilder Builder;
    public PolygonBuilderToolGrab GrabTemplate;
    public List<PolygonBuilderToolGrab> Grabs = new List<PolygonBuilderToolGrab>();
    public Canvas Canvas;

    public void OnCancel() {
        for(int i = 0;i < Grabs.Count;i ++) {
            GameObject.Destroy(Grabs[i].gameObject);
        }
    }

    public void AddPoint(Vector2 pos) {
        // Universal setup
        PolygonBuilderToolGrab grab = GameObject.Instantiate(GrabTemplate);
        grab.transform.SetParent(Canvas.transform, false);
        grab.Data = Data;
        grab.Footprint = Builder.Result.Footprint;
        Grabs.Add(grab);

        Data.SlopePoints.Add(new PolygonConstructionData.SlopePoint(pos));
        grab.SlopePointIndex = Data.SlopePoints.Count - 1;

        // Universal finalization code
        grab.RectTransform.anchoredPosition = Data.SlopePoints[grab.SlopePointIndex].Pos;

        if(Data.SlopePoints.Count > 2) {
            if(!PolygonsController.Instance.PolygonObjects.Contains(Builder.Result.Footprint)) {
                PolygonsController.Instance.PolygonObjects.Add(Builder.Result.Footprint);
            }
            PolygonsController.Instance.MarkPolygonsDirty();
        }
    }

    public void RemovePoint(Vector2 pos) {
        //Find the closest station to the position and remove it
        float sqrMinDist = Mathf.Infinity;
        int index = -1;

        for(int i = 0;i < Data.SlopePoints.Count;i ++) {
            Vector2 hpos = Data.SlopePoints[i].Pos;
            float sqrMagnitude = (hpos - pos).sqrMagnitude;
            if(sqrMagnitude < sqrMinDist) {
                sqrMinDist = sqrMagnitude;
                index = i;
            }
        }
        if(index == -1) return;

        Data.SlopePoints.RemoveAt(index);

        // Fix the grabs
        for(int i = 0;i < Grabs.Count;i ++) {
            if(Grabs[i].SlopePointIndex == index) {
                GameObject.Destroy(Grabs[i].gameObject);
                Grabs.RemoveAt(i);
                i--;
            } else if(Grabs[i].SlopePointIndex > index) {
                Grabs[i].SlopePointIndex--;
            }
        }

        if(Data.SlopePoints.Count <= 2) {
            PolygonsController.Instance.PolygonObjects.Remove(Builder.Result.Footprint);
            // TODO: More elegant system
            if(Builder.Result.Footprint.Filter != null && Builder.Result.Footprint.Filter.gameObject != null) GameObject.Destroy(Builder.Result.Footprint.Filter.gameObject);
        }
        PolygonsController.Instance.MarkPolygonsDirty();
    }

    public void Start(PolygonBuilding polygonBuilding, PolygonFlags flags) {
        Builder = new PolygonBuilder();
        Builder.Initialize(polygonBuilding, flags);
        Data = Builder.Data;
    }
}