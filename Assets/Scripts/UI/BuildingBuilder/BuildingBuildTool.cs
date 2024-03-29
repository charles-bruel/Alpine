using UnityEngine.EventSystems;
using UnityEngine;
using Codice.Client.BaseCommands.BranchExplorer.Layout;

public class BuildingBuilderTool : ITool {
    private bool done = false;
    
    public Canvas WorldUICanvas;
    public SimpleBuildingTemplate Template;
    public BuildingBuilder Builder;

    public void Start()
    {
        Builder = new BuildingBuilder();
        Builder.Template = Template;
        Builder.WorldUICanvas = WorldUICanvas;
        Builder.Initialize();
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
            InterfaceController.Instance.Finish();
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

        int dir = 0;

        if(Input.GetKey(KeyCode.M)) {
            dir = 1;
        }
        if(Input.GetKey(KeyCode.N)) {
            dir = -1;
        }

        if(Input.GetKey(KeyCode.LeftShift)) {
            Builder.Rotation += dir * Time.deltaTime * 5;
        } else {
            Builder.Rotation += dir * Time.deltaTime * 90;
        }
    }
}