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
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UIToolbar : MonoBehaviour {
    public GameObject UIToolbarCategoryPrefab;
    public GameObject UIToolbarItemPrefab;
    public GameObject LeftSideButtonPrefab;
    public Sprite[] Sprites;
    public UIReferences UIReferences;

    public Sprite SnowfrontSprite;
    public Sprite SlopeSprite;


    [NonSerialized]
    public UIToolbarCategoryButton[] UIToolbarCategoryButtons;

    [NonSerialized]
    private UIToolbarCategory ActiveUIToolbarCategory;

    public void Start() {
        Inflate();
    }

    public void Inflate() {
        UIToolbarCategoryButtons = new UIToolbarCategoryButton[3];
        for(int i = 0; i < UIToolbarCategoryButtons.Length; i++) {
            GameObject temp = Instantiate(LeftSideButtonPrefab);
            temp.transform.SetParent(transform);
            RectTransform rect = temp.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(37.5f, - (i + 1) * 100 + 50);
            rect.localScale = new Vector3(1, 1, 1);
            UIToolbarCategoryButtons[i] = temp.AddComponent<UIToolbarCategoryButton>();

            UIToolbarCategoryButtons[i].Sprite = Sprites[i];
            UIToolbarCategoryButtons[i].UIToolbarCategoryPrefab = UIToolbarCategoryPrefab;
            UIToolbarCategoryButtons[i].UIToolbarItemPrefab = UIToolbarItemPrefab;

            UIToolbarCategoryButtons[i].UIReferences = UIReferences;
        }

        UIToolbarCategoryButtons[0].UIToolbarItemProviders.AddRange(from x in BuildingsController.Instance.BuildingTemplates select x as IUIToolbarItemProvider);

        UIToolbarCategoryButtons[1].UIToolbarItemProviders.AddRange(from x in BuildingsController.Instance.LiftTemplates select x as IUIToolbarItemProvider);

        UIToolbarCategoryButtons[2].UIToolbarItemProviders.Add(new SnowfrontToolbarItemProvider(SnowfrontSprite));
        UIToolbarCategoryButtons[2].UIToolbarItemProviders.Add(new SlopeToolbarItemProvider(SlopeSprite));

        foreach (UIToolbarCategoryButton button in UIToolbarCategoryButtons) {
            button.Inflate();
        }

        for(int i = 0; i < UIToolbarCategoryButtons.Length; i++) {
            int index = i; // Declaring a new variable to avoid closure issues
            UIToolbarCategoryButtons[index].transform.Find("Panel").GetComponent<Button>().onClick.AddListener(() => {
                if (UIToolbarCategoryButtons[index].UIToolbarCategory == ActiveUIToolbarCategory) {
                    ActiveUIToolbarCategory.gameObject.SetActive(false);
                    ActiveUIToolbarCategory = null;
                } else {
                    if (ActiveUIToolbarCategory != null) {
                        ActiveUIToolbarCategory.gameObject.SetActive(false);
                    }
                    ActiveUIToolbarCategory = UIToolbarCategoryButtons[index].UIToolbarCategory;
                    ActiveUIToolbarCategory.gameObject.SetActive(true);
                }
            });
        }
    }

    private class SlopeToolbarItemProvider : IUIToolbarItemProvider {
        Sprite sprite;

        public SlopeToolbarItemProvider(Sprite sprite) {
            this.sprite = sprite;
        }

        public Sprite GetSprite() {
            return sprite;
        }

        public void OnToolEnabled(UIReferences uiReferences) {
            SlopeBuilderTool tool = new SlopeBuilderTool();
            tool.PolygonTool.GrabTemplate = uiReferences.GrabTemplate;
            tool.PolygonTool.Canvas = uiReferences.WorldCanvas;
            InterfaceController.Instance.SelectedTool = tool;
            var UI = uiReferences.SlopeBuilderUI; 
            tool.UI = UI;
            UI.gameObject.SetActive(true);
        }
    }
    private class SnowfrontToolbarItemProvider : IUIToolbarItemProvider {
        Sprite sprite;

        public SnowfrontToolbarItemProvider(Sprite sprite) {
            this.sprite = sprite;
        }

        public Sprite GetSprite() {
            return sprite;
        }

        public void OnToolEnabled(UIReferences uiReferences) {
            SnowfrontBuilderTool tool = new SnowfrontBuilderTool();
            tool.PolygonTool.GrabTemplate = uiReferences.GrabTemplate;
            tool.PolygonTool.Canvas = uiReferences.WorldCanvas;
            InterfaceController.Instance.SelectedTool = tool;
            var UI = uiReferences.SlopeBuilderUI; 
            tool.UI = UI;
            UI.gameObject.SetActive(true);
        }
    }

}