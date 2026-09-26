using System;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Type dropdown for [SerializeReference, SubclassPicker] fields, followed
    // by the chosen instance's fields. Unity has no built-in picker for these.
    [CustomPropertyDrawer(typeof(SubclassPickerAttribute))]
    public class SubclassPickerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return height;
            }

            foreach (var child in Children(property))
            {
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(line, label.text, "Use [SubclassPicker] with [SerializeReference].");
                return;
            }

            UnshareDuplicate(property);

            EditorGUI.BeginProperty(position, label, property);
            var popup = label == GUIContent.none || string.IsNullOrEmpty(label.text) ? line : EditorGUI.PrefixLabel(line, label);
            if (EditorGUI.DropdownButton(popup, new GUIContent(TypeLabel(property.managedReferenceFullTypename)), FocusType.Keyboard))
            {
                ShowTypeMenu(property);
            }

            using (new EditorGUI.IndentLevelScope())
            {
                var y = line.yMax;
                foreach (var child in Children(property))
                {
                    var height = EditorGUI.GetPropertyHeight(child, true);
                    y += EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, true);
                    y += height;
                }
            }

            EditorGUI.EndProperty();
        }

        // Next, not NextVisible: under a [HideInInspector] parent (bars, drawn
        // by RhythmSkillCardEditor) every child counts as invisible.
        private static IEnumerable<SerializedProperty> Children(SerializedProperty property)
        {
            var child = property.Copy();
            var end = property.GetEndProperty(true);
            var depth = property.depth + 1;
            if (!child.Next(true))
            {
                yield break;
            }

            while (!SerializedProperty.EqualContents(child, end) && child.depth >= depth)
            {
                yield return child.Copy();
                if (!child.Next(false))
                {
                    yield break;
                }
            }
        }

        private static void ShowTypeMenu(SerializedProperty property)
        {
            var target = property.Copy();
            var current = property.managedReferenceFullTypename;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(current), () => Assign(target, null));
            foreach (var type in CandidateTypes(property.managedReferenceFieldTypename))
            {
                var isCurrent = current == $"{type.Assembly.GetName().Name} {type.FullName}";
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), isCurrent, () =>
                {
                    if (!isCurrent)
                    {
                        Assign(target, Activator.CreateInstance(type));
                    }
                });
            }

            menu.ShowAsContext();
        }

        private static void Assign(SerializedProperty property, object value)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        // Concrete, [Serializable], parameterless subclasses of the field type
        // ("Assembly Namespace.Type", as Unity reports it).
        private static IEnumerable<Type> CandidateTypes(string fieldTypename)
        {
            var fieldType = ParseType(fieldTypename);
            if (fieldType == null)
            {
                return Array.Empty<Type>();
            }

            return TypeCache.GetTypesDerivedFrom(fieldType)
                .Append(fieldType)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericType
                            && !typeof(UnityEngine.Object).IsAssignableFrom(t)
                            && t.IsDefined(typeof(SerializableAttribute), false)
                            && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name);
        }

        private static Type ParseType(string typename)
        {
            var split = typename.IndexOf(' ');
            return split < 0 ? null : Type.GetType($"{typename.Substring(split + 1)}, {typename.Substring(0, split)}");
        }

        private static string TypeLabel(string fullTypename)
        {
            if (string.IsNullOrEmpty(fullTypename))
            {
                return "None";
            }

            var name = fullTypename.Substring(fullTypename.LastIndexOfAny(new[] { ' ', '.' }) + 1);
            return ObjectNames.NicifyVariableName(name);
        }

        // Unity's array "+" / duplicate copies the reference, not the object,
        // so two elements would share one instance. Give the later one a copy.
        private static void UnshareDuplicate(SerializedProperty property)
        {
            var path = property.propertyPath;
            var arrayMarker = path.LastIndexOf(".Array.data[", StringComparison.Ordinal);
            if (arrayMarker < 0 || property.managedReferenceValue == null)
            {
                return;
            }

            var array = property.serializedObject.FindProperty(path.Substring(0, arrayMarker));
            var index = int.Parse(path.Substring(arrayMarker + ".Array.data[".Length).TrimEnd(']'));
            for (var i = 0; i < index; i++)
            {
                if (array.GetArrayElementAtIndex(i).managedReferenceId == property.managedReferenceId)
                {
                    var value = property.managedReferenceValue;
                    property.managedReferenceValue = JsonUtility.FromJson(JsonUtility.ToJson(value), value.GetType());
                    property.serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}
