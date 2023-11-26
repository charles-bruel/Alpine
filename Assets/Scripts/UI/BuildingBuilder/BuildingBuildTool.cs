using UnityEngine.EventSystems;
using UnityEngine;

public class BuildingBuilderTool : ITool {
    private bool done = false;

    public SimpleBuildingTemplate Template;
    public BuildingBuilder Builder;

    public void Start()
    {
        Builder = new BuildingBuilder();
        Builder.Initialize();
        Builder.Template = Template;
    }

    public void Cancel(bool confirm)
    {
        if(confirm) {
            Builder.Build();
            Builder.Finish();
        } else {
            Builder.Cancel();
        }
    }

    public bool IsDone()
    {
        return done;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right) {
            InterfaceController.Instance.SelectedTool = null;
        }
        if (eventData.button == PointerEventData.InputButton.Left) {
            Builder.Build();
            Builder.Finish();
            InterfaceController.Instance.SelectedTool = null;
        }
    }

    public bool Require2D()
    {
        return true;
    }

    public void Update()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition).ToHorizontal();
        Builder.UpdatePos(pos);
        Builder.LightBuild();
    }
}