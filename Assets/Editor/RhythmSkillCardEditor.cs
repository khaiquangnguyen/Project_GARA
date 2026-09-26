using System;
using System.Collections.Generic;
using GARA.Characters;
using GARA.Input;
using GARA.Rhythm;
using GARA.SkillCards.Rhythm;
using NaughtyAttributes;
using NaughtyAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace GARA.EditorTools
{
    // Draws a rhythm card's bars as flat note rows plus a timeline (one lane
    // per input), instead of nested foldouts. Each bar shows its timing and
    // move on the left and its effects on the right. Inputs are labelled from
    // an InputTokenMap.
    [CustomEditor(typeof(RhythmSkillCard), true)]
    public class RhythmSkillCardEditor : NaughtyInspector
    {
        private const string TokenMapPrefKey = "GARA.RhythmSkillCardEditor.TokenMap";
        private const float DefaultStep = 0.5f;
        private const float LaneHeight = 14f;
        private const float AxisHeight = 14f;
        private const float LaneGutter = 56f;

        private const float OrderWidth = 22f;
        private const float TimeWidth = 42f;
        private const float DeltaWidth = 38f;
        private const float InputWidth = 84f;
        private const float KindWidth = 44f;
        private const float HoldWidth = 36f;
        private const float BarLabelWidth = 40f;
        private const float BarRangeWidth = 64f;
        private const float RemoveWidth = 20f;
        private const float TimingColumnWidth = 318f;
        private const float MoveMinWidth = 150f;
        private const float EffectsWidth = 224f;
        private const float EffectsLabelWidth = 70f;
        private const float ColumnGap = 6f;

        private struct NoteRef
        {
            public int bar;
            public int note;
            public int tokenId;
            public float time;
            public float end;
        }

        private enum RowAction
        {
            None,
            TimeChanged,
            Remove
        }

        private SerializedProperty _bars;
        private InputTokenMap _tokenMap;
        private int _selectedBar = -1;
        private int _selectedNote = -1;

        // Inner width of a bar box, measured on repaint. Layout groups here
        // don't stretch reliably, so the timing column is sized from it.
        private float _barContentWidth;

        protected override void OnEnable()
        {
            base.OnEnable();
            _bars = serializedObject.FindProperty("bars");
            _tokenMap = LoadTokenMap();
        }

        // Card fields, then the bars, then box groups (the finale) last.
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var groups = new List<(string name, SerializedProperty property)>();
            using (var iterator = serializedObject.GetIterator())
            {
                for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                {
                    var property = serializedObject.FindProperty(iterator.name);
                    var group = PropertyUtility.GetAttribute<BoxGroupAttribute>(property);
                    if (group != null)
                    {
                        groups.Add((group.Name, property));
                    }
                    else if (property.name == "m_Script")
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.PropertyField(property);
                        }
                    }
                    else
                    {
                        NaughtyEditorGUI.PropertyField_Layout(property, true);
                    }
                }
            }

            DrawBarsSection();
            DrawBoxGroups(groups);

            serializedObject.ApplyModifiedProperties();

            DrawNonSerializedFields();
            DrawNativeProperties();
            DrawButtons();
        }

        private void DrawBarsSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bars", EditorStyles.boldLabel);
            if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox("Select a single card to edit its bars.", MessageType.Info);
                return;
            }

            DrawTokenMapField();

            var ordered = CollectNotes();
            DrawTimeline(ordered);
            DrawBarList(ordered);
            DrawWarnings(ordered);
        }

        private static void DrawBoxGroups(List<(string name, SerializedProperty property)> groups)
        {
            var names = new List<string>();
            foreach (var entry in groups)
            {
                if (!names.Contains(entry.name))
                {
                    names.Add(entry.name);
                }
            }

            foreach (var groupName in names)
            {
                var visible = groups.FindAll(entry => entry.name == groupName && PropertyUtility.IsVisible(entry.property));
                if (visible.Count == 0)
                {
                    continue;
                }

                EditorGUILayout.Space();
                NaughtyEditorGUI.BeginBoxGroup_Layout(groupName);
                foreach (var entry in visible)
                {
                    NaughtyEditorGUI.PropertyField_Layout(entry.property, true);
                }

                NaughtyEditorGUI.EndBoxGroup_Layout();
            }
        }

        private void DrawTokenMapField()
        {
            EditorGUI.BeginChangeCheck();
            _tokenMap = (InputTokenMap)EditorGUILayout.ObjectField("Token Map", _tokenMap, typeof(InputTokenMap), false);
            if (EditorGUI.EndChangeCheck())
            {
                var path = _tokenMap != null ? AssetDatabase.GetAssetPath(_tokenMap) : string.Empty;
                EditorPrefs.SetString(TokenMapPrefKey, AssetDatabase.AssetPathToGUID(path));
            }
        }

        private void DrawBarList(List<NoteRef> ordered)
        {
            var orderOf = new Dictionary<(int, int), int>();
            for (var i = 0; i < ordered.Count; i++)
            {
                orderOf[(ordered[i].bar, ordered[i].note)] = i;
            }

            DrawColumnHeader(TimingWidth);

            var removeBar = -1;
            var removeNote = (bar: -1, note: -1);
            var addNoteToBar = -1;
            var sortBar = -1;

            for (var b = 0; b < _bars.arraySize; b++)
            {
                var bar = _bars.GetArrayElementAtIndex(b);
                var notes = bar.FindPropertyRelative("notes");

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(TimingWidth)))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"Bar {b + 1}", EditorStyles.boldLabel, GUILayout.Width(BarLabelWidth));
                            EditorGUILayout.LabelField(BarRangeLabel(ordered, b), EditorStyles.miniLabel, GUILayout.Width(BarRangeWidth));
                            EditorGUILayout.ObjectField(bar.FindPropertyRelative("move"), typeof(AttackAnimationSpec), GUIContent.none, GUILayout.MinWidth(MoveMinWidth), GUILayout.ExpandWidth(true));
                            if (GUILayout.Button(new GUIContent("+", "Add a note to this bar"), GUILayout.Width(RemoveWidth)))
                            {
                                addNoteToBar = b;
                            }

                            if (GUILayout.Button(new GUIContent("×", "Remove this bar"), GUILayout.Width(RemoveWidth)))
                            {
                                removeBar = b;
                            }
                        }

                        for (var n = 0; n < notes.arraySize; n++)
                        {
                            var order = orderOf[(b, n)];
                            var delta = order > 0 ? ordered[order].time - ordered[order - 1].time : (float?)null;
                            var action = DrawNoteRow(notes.GetArrayElementAtIndex(n), b, n, order, delta);
                            if (action == RowAction.Remove)
                            {
                                removeNote = (b, n);
                            }
                            else if (action == RowAction.TimeChanged)
                            {
                                sortBar = b;
                            }
                        }
                    }

                    GUILayout.Space(ColumnGap);
                    DrawEffects(bar.FindPropertyRelative("effects"));
                }

                if (b == 0)
                {
                    MeasureBarContentWidth();
                }
            }

            if (GUILayout.Button("+ Bar"))
            {
                AddBar(ordered);
            }

            if (addNoteToBar >= 0)
            {
                AddNote(addNoteToBar, ordered);
            }

            if (sortBar >= 0)
            {
                SortNotes(_bars.GetArrayElementAtIndex(sortBar).FindPropertyRelative("notes"));
                ClearSelection();
            }

            if (removeNote.bar >= 0)
            {
                _bars.GetArrayElementAtIndex(removeNote.bar).FindPropertyRelative("notes").DeleteArrayElementAtIndex(removeNote.note);
                ClearSelection();
            }

            if (removeBar >= 0)
            {
                _bars.DeleteArrayElementAtIndex(removeBar);
                ClearSelection();
            }
        }

        // The bar's effects, each picked by type (SubclassPickerDrawer).
        private static void DrawEffects(SerializedProperty effects)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(EffectsWidth)))
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = EffectsLabelWidth;

                var remove = -1;
                for (var i = 0; i < effects.arraySize; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(effects.GetArrayElementAtIndex(i), GUIContent.none, true);
                        if (GUILayout.Button(new GUIContent("×", "Remove this effect"), GUILayout.Width(RemoveWidth)))
                        {
                            remove = i;
                        }
                    }
                }

                EditorGUIUtility.labelWidth = labelWidth;

                if (GUILayout.Button("+ Effect"))
                {
                    effects.arraySize++;
                    effects.GetArrayElementAtIndex(effects.arraySize - 1).managedReferenceValue = null;
                }

                if (remove >= 0)
                {
                    effects.DeleteArrayElementAtIndex(remove);
                }
            }
        }

        // The timing column takes whatever the fixed effects column leaves.
        private float TimingWidth => Mathf.Max(TimingColumnWidth, _barContentWidth - ColumnGap - EffectsWidth - 1f);

        private void MeasureBarContentWidth()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            var width = GUILayoutUtility.GetLastRect().width - EditorStyles.helpBox.padding.horizontal;
            if (Mathf.Abs(width - _barContentWidth) > 0.5f)
            {
                _barContentWidth = width;
                Repaint();
            }
        }

        private static void DrawColumnHeader(float timingWidth)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorStyles.helpBox.margin.left + EditorStyles.helpBox.padding.left);
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(timingWidth)))
                {
                    EditorGUILayout.LabelField("#", EditorStyles.miniLabel, GUILayout.Width(OrderWidth));
                    EditorGUILayout.LabelField("Time", EditorStyles.miniLabel, GUILayout.Width(TimeWidth));
                    EditorGUILayout.LabelField(new GUIContent("Δ", "Seconds since the previous note in play order"), EditorStyles.miniLabel, GUILayout.Width(DeltaWidth));
                    EditorGUILayout.LabelField("Input", EditorStyles.miniLabel, GUILayout.Width(InputWidth));
                    EditorGUILayout.LabelField("Kind", EditorStyles.miniLabel, GUILayout.Width(KindWidth));
                    EditorGUILayout.LabelField("Hold", EditorStyles.miniLabel, GUILayout.Width(HoldWidth));
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(ColumnGap);
                EditorGUILayout.LabelField("Effects", EditorStyles.miniLabel, GUILayout.Width(EffectsWidth));
            }
        }

        private RowAction DrawNoteRow(SerializedProperty note, int bar, int index, int order, float? delta)
        {
            var action = RowAction.None;
            var time = note.FindPropertyRelative("time");
            var tokenId = note.FindPropertyRelative("input").FindPropertyRelative("id");
            var kind = note.FindPropertyRelative("kind");
            var hold = note.FindPropertyRelative("holdDuration");

            var row = EditorGUILayout.BeginHorizontal();
            if (bar == _selectedBar && index == _selectedNote && Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(row, new Color(0.24f, 0.49f, 0.9f, 0.25f));
            }

            if (GUILayout.Button($"{order + 1}", EditorStyles.miniLabel, GUILayout.Width(OrderWidth)))
            {
                Select(bar, index);
            }

            EditorGUI.BeginChangeCheck();
            var newTime = EditorGUILayout.DelayedFloatField(time.floatValue, GUILayout.Width(TimeWidth));
            if (EditorGUI.EndChangeCheck())
            {
                time.floatValue = Mathf.Max(0f, newTime);
                action = RowAction.TimeChanged;
            }

            EditorGUILayout.LabelField(delta.HasValue ? $"+{delta.Value:0.###}" : "—", EditorStyles.miniLabel, GUILayout.Width(DeltaWidth));

            BuildTokenOptions(tokenId.intValue, out var labels, out var ids);
            tokenId.intValue = EditorGUILayout.IntPopup(tokenId.intValue, labels, ids, GUILayout.Width(InputWidth));

            EditorGUILayout.PropertyField(kind, GUIContent.none, GUILayout.Width(KindWidth));

            using (new EditorGUI.DisabledScope(kind.enumValueIndex != (int)RhythmNoteKind.Hold))
            {
                EditorGUI.BeginChangeCheck();
                var newHold = EditorGUILayout.FloatField(hold.floatValue, GUILayout.Width(HoldWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    hold.floatValue = Mathf.Max(0f, newHold);
                }
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("×", "Remove this note"), GUILayout.Width(RemoveWidth)))
            {
                action = RowAction.Remove;
            }

            EditorGUILayout.EndHorizontal();
            return action;
        }

        private void DrawTimeline(List<NoteRef> ordered)
        {
            var lanes = LaneIds(ordered);
            var height = AxisHeight + Mathf.Max(1, lanes.Count) * LaneHeight + 4f;
            var rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            var track = new Rect(rect.x + LaneGutter, rect.y, rect.width - LaneGutter - 8f, rect.height);

            var duration = DefaultStep;
            foreach (var note in ordered)
            {
                duration = Mathf.Max(duration, note.end);
            }

            duration += 0.25f;
            float X(float t) => track.x + t / duration * track.width;
            float LaneY(int lane) => rect.y + AxisHeight + lane * LaneHeight;

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.18f));
                DrawBarBands(ordered, rect, X);
                DrawGrid(rect, track, duration, X);

                for (var lane = 0; lane < lanes.Count; lane++)
                {
                    var labelRect = new Rect(rect.x + 4f, LaneY(lane), LaneGutter - 6f, LaneHeight);
                    GUI.Label(labelRect, TokenLabel(lanes[lane]), EditorStyles.miniLabel);
                }
            }

            foreach (var note in ordered)
            {
                var lane = lanes.IndexOf(note.tokenId);
                var y = LaneY(lane) + 2f;
                var color = TokenColor(lane);
                var marker = new Rect(X(note.time) - 5f, y, 10f, LaneHeight - 4f);

                if (Event.current.type == EventType.Repaint)
                {
                    if (note.end > note.time)
                    {
                        EditorGUI.DrawRect(new Rect(X(note.time), y + 3f, X(note.end) - X(note.time), LaneHeight - 10f), color * new Color(1f, 1f, 1f, 0.6f));
                    }

                    var selected = note.bar == _selectedBar && note.note == _selectedNote;
                    if (selected)
                    {
                        EditorGUI.DrawRect(new Rect(marker.x - 2f, marker.y - 2f, marker.width + 4f, marker.height + 4f), Color.white);
                    }

                    EditorGUI.DrawRect(marker, color);
                }
                else if (Event.current.type == EventType.MouseDown && marker.Contains(Event.current.mousePosition))
                {
                    Select(note.bar, note.note);
                    Event.current.Use();
                }
            }
        }

        private void DrawBarBands(List<NoteRef> ordered, Rect rect, Func<float, float> x)
        {
            for (var b = 0; b < _bars.arraySize; b++)
            {
                if (!TryGetBarRange(ordered, b, out var start, out var end))
                {
                    continue;
                }

                var band = new Rect(x(start) - 6f, rect.y + AxisHeight, x(end) - x(start) + 12f, rect.height - AxisHeight);
                EditorGUI.DrawRect(band, b % 2 == 0 ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 1f, 1f, 0.1f));
                GUI.Label(new Rect(band.x, rect.yMax - 12f, 40f, 12f), $"B{b + 1}", EditorStyles.miniLabel);
            }
        }

        private static void DrawGrid(Rect rect, Rect track, float duration, Func<float, float> x)
        {
            const float step = 0.25f;
            for (var i = 0; i * step <= duration; i++)
            {
                var t = i * step;
                var major = i % 2 == 0;
                EditorGUI.DrawRect(new Rect(x(t), rect.y + AxisHeight, 1f, rect.height - AxisHeight), new Color(1f, 1f, 1f, major ? 0.15f : 0.06f));
                if (major)
                {
                    GUI.Label(new Rect(x(t) - 2f, rect.y, 40f, AxisHeight), $"{t:0.#}", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawWarnings(List<NoteRef> ordered)
        {
            if (_tokenMap == null)
            {
                EditorGUILayout.HelpBox("No InputTokenMap found — inputs show as ids only.", MessageType.Warning);
            }
            else
            {
                var unmapped = new HashSet<int>();
                foreach (var note in ordered)
                {
                    if (!_tokenMap.TryGetDisplayName(new InputToken(note.tokenId), out _))
                    {
                        unmapped.Add(note.tokenId);
                    }
                }

                if (unmapped.Count > 0)
                {
                    EditorGUILayout.HelpBox($"Input ids not in {_tokenMap.name}: {string.Join(", ", unmapped)}", MessageType.Warning);
                }
            }

            for (var a = 0; a < _bars.arraySize; a++)
            {
                if (!TryGetBarRange(ordered, a, out var aStart, out var aEnd))
                {
                    EditorGUILayout.HelpBox($"Bar {a + 1} has no notes.", MessageType.Warning);
                    continue;
                }

                for (var b = a + 1; b < _bars.arraySize; b++)
                {
                    if (TryGetBarRange(ordered, b, out var bStart, out var bEnd) && aStart <= bEnd && bStart <= aEnd)
                    {
                        EditorGUILayout.HelpBox($"Bars {a + 1} and {b + 1} overlap in time — their notes interleave.", MessageType.Info);
                    }
                }
            }
        }

        private List<NoteRef> CollectNotes()
        {
            var result = new List<NoteRef>();
            for (var b = 0; b < _bars.arraySize; b++)
            {
                var notes = _bars.GetArrayElementAtIndex(b).FindPropertyRelative("notes");
                for (var n = 0; n < notes.arraySize; n++)
                {
                    var note = notes.GetArrayElementAtIndex(n);
                    var time = note.FindPropertyRelative("time").floatValue;
                    var isHold = note.FindPropertyRelative("kind").enumValueIndex == (int)RhythmNoteKind.Hold;
                    result.Add(new NoteRef
                    {
                        bar = b,
                        note = n,
                        tokenId = note.FindPropertyRelative("input").FindPropertyRelative("id").intValue,
                        time = time,
                        end = time + (isHold ? note.FindPropertyRelative("holdDuration").floatValue : 0f)
                    });
                }
            }

            result.Sort((x, y) =>
            {
                var byTime = x.time.CompareTo(y.time);
                return byTime != 0 ? byTime : x.bar != y.bar ? x.bar.CompareTo(y.bar) : x.note.CompareTo(y.note);
            });
            return result;
        }

        private void AddNote(int barIndex, List<NoteRef> ordered)
        {
            var notes = _bars.GetArrayElementAtIndex(barIndex).FindPropertyRelative("notes");
            var hasOwn = TryGetBarRange(ordered, barIndex, out _, out var barEnd);
            var after = hasOwn ? barEnd : LastEnd(ordered);
            var tokenId = ordered.Count > 0 ? ordered[ordered.Count - 1].tokenId : FirstTokenId();

            notes.arraySize++;
            WriteNote(notes.GetArrayElementAtIndex(notes.arraySize - 1), tokenId, ordered.Count > 0 ? after + Step(ordered) : 0f);
            SortNotes(notes);
            ClearSelection();
        }

        private void AddBar(List<NoteRef> ordered)
        {
            var tokenId = ordered.Count > 0 ? ordered[ordered.Count - 1].tokenId : FirstTokenId();
            _bars.arraySize++;
            var bar = _bars.GetArrayElementAtIndex(_bars.arraySize - 1);
            bar.FindPropertyRelative("move").objectReferenceValue = null;
            bar.FindPropertyRelative("effects").arraySize = 0;
            var notes = bar.FindPropertyRelative("notes");
            notes.arraySize = 1;
            WriteNote(notes.GetArrayElementAtIndex(0), tokenId, ordered.Count > 0 ? LastEnd(ordered) + Step(ordered) : 0f);
        }

        private static void WriteNote(SerializedProperty note, int tokenId, float time)
        {
            note.FindPropertyRelative("input").FindPropertyRelative("id").intValue = tokenId;
            note.FindPropertyRelative("kind").enumValueIndex = (int)RhythmNoteKind.Tap;
            note.FindPropertyRelative("time").floatValue = time;
            note.FindPropertyRelative("holdDuration").floatValue = 0f;
        }

        // The last gap between notes, so appended notes keep the card's beat.
        private static float Step(List<NoteRef> ordered)
        {
            for (var i = ordered.Count - 1; i > 0; i--)
            {
                var gap = ordered[i].time - ordered[i - 1].time;
                if (gap > 0f)
                {
                    return gap;
                }
            }

            return DefaultStep;
        }

        private static float LastEnd(List<NoteRef> ordered)
        {
            var end = 0f;
            foreach (var note in ordered)
            {
                end = Mathf.Max(end, note.end);
            }

            return end;
        }

        private static void SortNotes(SerializedProperty notes)
        {
            for (var i = 1; i < notes.arraySize; i++)
            {
                for (var j = i; j > 0 && TimeAt(notes, j - 1) > TimeAt(notes, j); j--)
                {
                    notes.MoveArrayElement(j, j - 1);
                }
            }
        }

        private static float TimeAt(SerializedProperty notes, int index)
        {
            return notes.GetArrayElementAtIndex(index).FindPropertyRelative("time").floatValue;
        }

        private static bool TryGetBarRange(List<NoteRef> ordered, int bar, out float start, out float end)
        {
            start = float.MaxValue;
            end = float.MinValue;
            foreach (var note in ordered)
            {
                if (note.bar == bar)
                {
                    start = Mathf.Min(start, note.time);
                    end = Mathf.Max(end, note.end);
                }
            }

            return end >= start;
        }

        private static string BarRangeLabel(List<NoteRef> ordered, int bar)
        {
            return TryGetBarRange(ordered, bar, out var start, out var end) ? $"{start:0.##}–{end:0.##}s" : "empty";
        }

        private List<int> LaneIds(List<NoteRef> ordered)
        {
            var ids = new List<int>();
            if (_tokenMap != null)
            {
                foreach (var entry in _tokenMap.Entries)
                {
                    if (!ids.Contains(entry.token.Id))
                    {
                        ids.Add(entry.token.Id);
                    }
                }
            }

            foreach (var note in ordered)
            {
                if (!ids.Contains(note.tokenId))
                {
                    ids.Add(note.tokenId);
                }
            }

            ids.Sort();
            return ids;
        }

        private void BuildTokenOptions(int current, out GUIContent[] labels, out int[] ids)
        {
            var idList = new List<int>();
            if (_tokenMap != null)
            {
                foreach (var entry in _tokenMap.Entries)
                {
                    idList.Add(entry.token.Id);
                }
            }

            if (!idList.Contains(current))
            {
                idList.Add(current);
            }

            idList.Sort();
            ids = idList.ToArray();
            labels = new GUIContent[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                labels[i] = new GUIContent(TokenLabel(ids[i]));
            }
        }

        private string TokenLabel(int id)
        {
            if (_tokenMap != null && _tokenMap.TryGetDisplayName(new InputToken(id), out var displayName))
            {
                return $"{id} · {displayName}";
            }

            return $"{id} · ?";
        }

        private int FirstTokenId()
        {
            return _tokenMap != null && _tokenMap.Entries.Count > 0 ? _tokenMap.Entries[0].token.Id : 0;
        }

        private static Color TokenColor(int lane)
        {
            return Color.HSVToRGB(lane * 0.618f % 1f, 0.55f, 0.95f);
        }

        private void Select(int bar, int note)
        {
            _selectedBar = bar;
            _selectedNote = note;
            Repaint();
        }

        private void ClearSelection()
        {
            _selectedBar = -1;
            _selectedNote = -1;
        }

        private static InputTokenMap LoadTokenMap()
        {
            var savedPath = AssetDatabase.GUIDToAssetPath(EditorPrefs.GetString(TokenMapPrefKey, string.Empty));
            var saved = string.IsNullOrEmpty(savedPath) ? null : AssetDatabase.LoadAssetAtPath<InputTokenMap>(savedPath);
            if (saved != null)
            {
                return saved;
            }

            var guids = AssetDatabase.FindAssets($"t:{nameof(InputTokenMap)}");
            return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<InputTokenMap>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        }
    }
}
