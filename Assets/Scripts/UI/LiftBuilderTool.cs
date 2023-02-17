using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class LiftBuilderTool : ITool {
    private bool done = false;

    public LiftConstructionData Data;
    public LiftBuilderUI UI;
    public LiftBuilder Builder;
    public LiftBuilderToolGrab GrabTemplate;
    public List<LiftBuilderToolGrab> Grabs = new List<LiftBuilderToolGrab>();
    public Canvas Canvas;

    public bool Require2D() {
        return true;
    }

    public void Cancel(bool confirm) {
        if(Data.RoutingSegments.Count >= 2) {
            Builder.Build();
            Builder.Finish();
        }
        for(int i = 0;i < Grabs.Count;i ++) {
            GameObject.Destroy(Grabs[i].gameObject);
        }
        UI.gameObject.SetActive(false);
    }

    public bool IsDone() {
        return done;
    }

    public void Start() {
        Builder = new LiftBuilder();
        Builder.Data = Data;
        Builder.Initialize();

        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            LiftBuilderToolGrab grab = GameObject.Instantiate(GrabTemplate);
            grab.transform.SetParent(Canvas.transform, false);
            grab.RectTransform.anchoredPosition = Data.RoutingSegments[i].Position.ToHorizontal();
            grab.RoutingSegmentIndex = i;
            grab.Data = Data;
            Grabs.Add(grab);
        }
    }

    public void AddStation(Vector2 pos) {
        // Universal setup
        LiftConstructionData.RoutingSegment segment = new LiftConstructionData.RoutingSegment();
        segment.Position = new Vector3(pos.x, 0, pos.y);
        segment.TemplateIndex = 0;

        LiftBuilderToolGrab grab = GameObject.Instantiate(GrabTemplate);
        grab.transform.SetParent(Canvas.transform, false);
        grab.Data = Data;
        Grabs.Add(grab);

        // It is trivial to insert the new station if we have no other stations
        // or only one other; there is only one place to put it. It becomes
        // significantly more complex to make that desicion with more segments
        if(Data.RoutingSegments.Count == 0 || Data.RoutingSegments.Count == 1) {
            segment.RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.STATION;
            Data.RoutingSegments.Add(segment);
            grab.RoutingSegmentIndex = Data.RoutingSegments.Count - 1;
        } else {
            // Find the closest line segment
            float minDist = Mathf.Infinity;
            int closestIndex = -1;
            for(int i = 1;i < Data.RoutingSegments.Count;i ++) {
                float dist = Utils.LineSegmentPoint(
                    Data.RoutingSegments[i - 1].Position.ToHorizontal(),
                    Data.RoutingSegments[i].Position.ToHorizontal(),
                    pos
                );
                if(dist < minDist) {
                    minDist = dist;
                    closestIndex = i;
                }
            }
            // Project the point onto the line segment. If t < 0, place
            // the station before, if t > 1, place it after, otherwise
            // in the middle
            // Portions of this code copied from Utils.LineSegmentPoint()
            Vector2 v = Data.RoutingSegments[closestIndex - 1].Position.ToHorizontal();
            Vector2 w = Data.RoutingSegments[closestIndex].Position.ToHorizontal();
            float l2 = (v - w).sqrMagnitude;
            // if(l2 == 0) return;
            float t = Vector2.Dot(pos - v, w - v) / l2;
            int indexToInsertAt;
            if (t < 0) {
                indexToInsertAt = closestIndex - 1;
            } else if (t > 1) {
                indexToInsertAt = closestIndex + 1;
            } else {
                indexToInsertAt = closestIndex;
            }
            
            if(indexToInsertAt != 0 && indexToInsertAt != Data.RoutingSegments.Count) {
                segment.RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.MIDSTATION;
            } else {
                segment.RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.STATION;
            }

            Data.RoutingSegments.Insert(indexToInsertAt, segment);

            // We need to relink the grabs
            for(int i = 0;i < Grabs.Count;i ++) {
                if(Grabs[i].RoutingSegmentIndex >= indexToInsertAt) Grabs[i].RoutingSegmentIndex++;
            }
            
            // Now we can add our grab (we wait until here so it doesn't get
            // changed by the above loop)
            grab.RoutingSegmentIndex = indexToInsertAt;

            // We also need to potentially make the adjacent stations into midstations
            if(indexToInsertAt == 0) {
                // We inserted at the beginning of the list
                // The list is garaunteed to be longer than 2 if we got here
                // so it should be a midstation
                Data.RoutingSegments[1].RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.MIDSTATION;
            }
            if(indexToInsertAt == Data.RoutingSegments.Count - 1) {
                // We inserted at the end of the list
                // The list is garaunteed to be longer than 2 if we got here
                // so it should be a midstation
                Data.RoutingSegments[Data.RoutingSegments.Count - 2].RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.MIDSTATION;
            }
        }

        // Universal finalization code
        grab.RectTransform.anchoredPosition = Data.RoutingSegments[grab.RoutingSegmentIndex].Position.ToHorizontal();
    }

    public void RemoveStation(Vector2 pos) {
        //Find the closest station to the position and remove it
        float sqrMinDist = Mathf.Infinity;
        int index = -1;

        for(int i = 0;i < Data.RoutingSegments.Count;i ++) {
            Vector2 hpos = Data.RoutingSegments[i].Position.ToHorizontal();
            float sqrMagnitude = (hpos - pos).sqrMagnitude;
            if(sqrMagnitude < sqrMinDist) {
                sqrMinDist = sqrMagnitude;
                index = i;
            }
        }
        if(index == -1) return;

        Data.RoutingSegments.RemoveAt(index);

        // Fix the grabs
        for(int i = 0;i < Grabs.Count;i ++) {
            if(Grabs[i].RoutingSegmentIndex == index) {
                GameObject.Destroy(Grabs[i].gameObject);
                Grabs.RemoveAt(i);
                i--;
            } else if(Grabs[i].RoutingSegmentIndex > index) {
                Grabs[i].RoutingSegmentIndex--;
            }
        }
        // Fix station types
        if(index == 0) {
            // We removed the start so there will be a new start
            Data.RoutingSegments[0].RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.STATION;
        }
        if(index == Data.RoutingSegments.Count) {
            // We removed the end so there will be a new end
            Data.RoutingSegments[Data.RoutingSegments.Count - 1].RoutingSegmentType = LiftRoutingSegmentTemplate.RoutingSegmentType.STATION;
        }
    }

    public void Update() {
        Builder.LightBuild();
    }

    public void OnPointerClick(PointerEventData eventData) {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal();
        if(eventData.button == PointerEventData.InputButton.Left) {
            AddStation(pos);
        } else if(eventData.button == PointerEventData.InputButton.Right) {
            RemoveStation(pos);
        }
    }
}