using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WhiteSparrow.Shared.DependencyInjection.Containers;

namespace WhiteSparrow.Shared.DependencyInjection
{
    public class InjectionContextWindow : EditorWindow
    {
        [MenuItem("Tools/White Sparrow/Injection")]
        private static void ShowInjectionWindow()
        {
            InjectionContextWindow window = EditorWindow.GetWindow<InjectionContextWindow>();
            window.Show();
        }

        private ScrollView m_Cotnainer;

        private void CreateGUI()
        {
            m_Cotnainer = new ScrollView(ScrollViewMode.Vertical);
            rootVisualElement.Add(m_Cotnainer);
            
            RefreshList();
        }

        private void OnFocus()
        {
            RefreshList();
        }

        private void RefreshList()
        {
            m_Cotnainer.Clear();
            var allInjectionContainers = Resources.FindObjectsOfTypeAll<InjectionContainer>();
            if (allInjectionContainers.Length == 0)
            {
                m_Cotnainer.Add(new Label("No injection containers"));
                return;
            }

            foreach (var container in allInjectionContainers)
            {
                var btn = new Button();
                btn.text = container.name;
                btn.clickable.clickedWithEventInfo += OnContainerClicked;
                m_Cotnainer.Add(btn);
                btn.userData = container;
            }
        }

        private void OnContainerClicked(EventBase clickEvent)
        {
            if(clickEvent.target is not Button btn || btn.userData is not InjectionContainer container)
                return;

            Selection.activeObject = container;
        }
    }
}