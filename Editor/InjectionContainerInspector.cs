using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.Shared.DependencyInjection
{
	[CustomEditor(typeof(InjectionContainer))]
	public class InjectionContainerInspector : Editor
	{
		public InjectionContainer Container => (InjectionContainer)target;

		private readonly List<Tuple<Type, object>> m_ProcessedMapping = new List<Tuple<Type, object>>();
		
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
		
		private void OnContainerMappingChanged(Type type, object instance)
		{
			RefreshContainerMappingList();
		}

		private void RefreshContainerMappingList()
		{
			m_ProcessedMapping.Clear();

			var mapping = Container.Mapping;
			foreach (var mappingPair in mapping)
			{
				m_ProcessedMapping.Add(new Tuple<Type, object>(mappingPair.Key, mappingPair.Value));
			}
			
			m_ProcessedMapping.Sort((lhs, rhs) => EditorUtility.NaturalCompare(lhs.Item1.Name, rhs.Item1.Name));

		}


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			foreach (var tuple in m_ProcessedMapping)
			{
				if (tuple.Item2 is UnityEngine.Object unityObjectValue)
				{
					EditorGUILayout.ObjectField(tuple.Item1.Name, unityObjectValue, typeof(UnityEngine.Object));
				}
				else
				{
					EditorGUILayout.LabelField(tuple.Item1.Name, tuple.Item2.GetType().Name);
				}
			}
			
		}
	}
}