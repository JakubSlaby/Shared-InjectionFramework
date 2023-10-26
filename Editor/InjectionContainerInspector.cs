using System;
using System.Collections.Generic;
using UnityEditor;
using WhiteSparrow.Shared.DependencyInjection.Containers;

namespace WhiteSparrow.Shared.DependencyInjection
{
	[CustomEditor(typeof(InjectionContainer))]
	public class InjectionContainerInspector : Editor
	{
		public InjectionContainer Container => (InjectionContainer)target;

		private readonly List<Tuple<object, object>> m_ProcessedMapping = new List<Tuple<object, object>>();
		
		private void OnEnable()
		{
			RefreshContainerMappingList();
			
			Container.OnMappingAdded -= OnContainerMappingChanged;
			Container.OnMappingRemoved -= OnContainerMappingChanged;
			Container.OnMappingAdded += OnContainerMappingChanged;
			Container.OnMappingRemoved += OnContainerMappingChanged;
		}

		private void OnDisable()
		{
			Container.OnMappingAdded -= OnContainerMappingChanged;
			Container.OnMappingRemoved -= OnContainerMappingChanged;
		}
		
		private void OnContainerMappingChanged(object o, object instance)
		{
			RefreshContainerMappingList();
		}

		private void RefreshContainerMappingList()
		{
			m_ProcessedMapping.Clear();

			var mapping = Container.Mapping;
			foreach (var mappingPair in mapping)
			{
				m_ProcessedMapping.Add(new Tuple<object, object>(mappingPair.Key, mappingPair.Value));
			}
			
			m_ProcessedMapping.Sort((lhs, rhs) => EditorUtility.NaturalCompare(lhs.Item1.ToString(), rhs.Item1.ToString()));
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			foreach (var tuple in m_ProcessedMapping)
			{
				var label = EditorGUIUtility.TrTempContent(GetLabelString(tuple.Item1));
				if (tuple.Item2 is MonoBehaviourInstanceBinding mb)
				{
					EditorGUILayout.ObjectField(label, mb.Instance, typeof(UnityEngine.MonoBehaviour), false);
				}
				else if (tuple.Item2 is ScriptableObjectInstanceBinding so)
				{
					EditorGUILayout.ObjectField(label, so.Instance, typeof(UnityEngine.ScriptableObject), false);
				}
				else
				{
					EditorGUILayout.LabelField(GetLabelString(tuple.Item1), tuple.Item2.GetType().Name);
				}
			}
		}

		private string GetLabelString(object o)
		{
			if (o is Type t)
				return t.Name;
			return o.ToString();
		}
	}
}