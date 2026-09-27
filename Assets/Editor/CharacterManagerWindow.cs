using System;
using System.Collections.Generic;
using System.Linq;
using GARA.Characters;
using GARA.Combat;
using GARA.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GARA.EditorTools
{
    // One window for a character: every CharacterState under it, with a
    // Preview button for those that can preview a clip (SpineAnimationState),
    // and — in play mode — a test bench that plays any of a battle
    // participant's skill cards right now (turn order, costs, loadout and
    // side ignored), normally, perfectly or with random misses, and swaps
    // any roster slot for another character (restarting the battle), and
    // previews the noir looks (one character noirified, or the Noir World).
    public class CharacterManagerWindow : EditorWindow
    {
        private CharacterDefinition _character;
        private Vector2 _scroll;
        private string _testMessage;
        [NonSerialized] private CharacterDefinition[] _characterPrefabs;

        [MenuItem("GARA/Characters/Character Manager")]
        public static void Open()
        {
            GetWindow<CharacterManagerWindow>("Character Manager");
        }

        public static void OpenFor(CharacterDefinition character)
        {
            var window = GetWindow<CharacterManagerWindow>("Character Manager");
            window._character = character;
            window.Repaint();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            EditorApplication.projectChanged += ClearCharacterPrefabs;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            EditorApplication.projectChanged -= ClearCharacterPrefabs;
        }

        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }

        private void OnSelectionChanged()
        {
            if (_character != null || Selection.activeGameObject == null)
            {
                return;
            }

            var found = Selection.activeGameObject.GetComponentInParent<CharacterDefinition>();
            if (found != null)
            {
                _character = found;
                Repaint();
            }
        }

        private void OnGUI()
        {
            _character = (CharacterDefinition)EditorGUILayout.ObjectField(
                "Character", _character, typeof(CharacterDefinition), true);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (EditorApplication.isPlaying)
            {
                DrawRoster();
                DrawBattleTest();
            }

            if (_character == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a character (prefab or scene instance with a CharacterDefinition) to see its states.",
                    MessageType.Info);
            }
            else
            {
                DrawStates();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStates()
        {
            var states = _character.GetComponentsInChildren<CharacterState>(true);
            EditorGUILayout.LabelField($"States ({states.Length})", EditorStyles.boldLabel);

            foreach (var state in states)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.ObjectField(state, typeof(CharacterState), true);

                using (new EditorGUI.DisabledScope(state is not SpineAnimationState))
                {
                    if (GUILayout.Button("Preview", GUILayout.Width(80)))
                    {
                        ((SpineAnimationState)state).PreviewAnimation();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        // Play mode: swap a slot's character, then reload the scene so the
        // battle restarts with it.
        private void DrawRoster()
        {
            var run = FindAnyObjectByType<RogueRunManager>();
            if (run == null)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Roster (swap restarts the battle)", EditorStyles.boldLabel);

                // Fresh participants (HP, death, statuses); swaps are kept.
                if (GUILayout.Button("Restart Battle", GUILayout.Width(110)))
                {
                    RestartBattle(run);
                    GUIUtility.ExitGUI();
                }
            }

            // Rebuilt when empty too: a lookup during an import can come back
            // empty, and that must not stick.
            if (_characterPrefabs == null || _characterPrefabs.Length == 0 || _characterPrefabs.Any(prefab => prefab == null))
            {
                _characterPrefabs = FindCharacterPrefabs();
            }

            var options = new[] { "(empty)" }.Concat(_characterPrefabs.Select(prefab => prefab.gameObject.name)).ToArray();

            for (var slot = 0; slot < RogueRunManager.SlotCount; slot++)
            {
                var label = slot < BattleParty.Size ? $"Player {slot + 1}" : $"Enemy {slot - BattleParty.Size + 1}";
                var current = Array.IndexOf(_characterPrefabs, run.CharacterAt(slot)) + 1;
                var picked = EditorGUILayout.Popup(label, current, options);
                if (picked != current)
                {
                    var character = picked > 0 ? _characterPrefabs[picked - 1] : null;
                    RogueRunManager.SetTestOverride(slot, character);
                    _character = character;
                    RestartBattle(run);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.Space();
        }

        private static void RestartBattle(RogueRunManager run)
        {
            EditorSceneManager.LoadSceneInPlayMode(run.gameObject.scene.path, new LoadSceneParameters(LoadSceneMode.Single));
        }

        private void ClearCharacterPrefabs()
        {
            _characterPrefabs = null;
        }

        private static CharacterDefinition[] FindCharacterPrefabs()
        {
            return AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/MainGame/Characters" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Select(prefab => prefab.GetComponent<CharacterDefinition>())
                .Where(definition => definition != null)
                .OrderBy(definition => definition.gameObject.name)
                .ToArray();
        }

        // Play mode: pick a participant (it becomes the Character above, as
        // its live instance) and play any of its cards through real combat.
        private void DrawBattleTest()
        {
            EditorGUILayout.LabelField("Test in battle", EditorStyles.boldLabel);

            var manager = FindAnyObjectByType<CombatSceneManager>();
            var participants = manager != null ? manager.DebugParticipants.ToList() : new List<CombatParticipant>();
            if (participants.Count == 0)
            {
                EditorGUILayout.HelpBox("No battle running yet.", MessageType.Info);
                EditorGUILayout.Space();
                return;
            }

            DrawNoirWorldLook();

            var current = participants.FindIndex(IsSelected);
            var picked = EditorGUILayout.Popup("Participant", current, participants.Select(Describe).ToArray());
            if (picked != current && picked >= 0)
            {
                var root = participants[picked].SceneRoot;
                _character = root != null ? root.GetComponent<CharacterDefinition>() : participants[picked].definition;
                current = picked;
            }

            if (current < 0)
            {
                EditorGUILayout.HelpBox("Pick a participant to test its cards.", MessageType.Info);
                EditorGUILayout.Space();
                return;
            }

            var participant = participants[current];
            DrawNoirifiedLook(participant);

            var blocker = manager.DebugPlayBlocker;
            if (blocker != null)
            {
                EditorGUILayout.HelpBox(blocker, MessageType.None);
            }

            using (new EditorGUI.DisabledScope(blocker != null))
            {
                foreach (var card in CardsOf(participant))
                {
                    DrawCard(manager, participant, card);
                }
            }

            if (!string.IsNullOrEmpty(_testMessage))
            {
                EditorGUILayout.HelpBox(_testMessage, MessageType.Warning);
            }

            EditorGUILayout.Space();
        }

        // Look only: no Noir World is entered, nothing ticks or ends it.
        private static void DrawNoirWorldLook()
        {
            var screenEffect = FindAnyObjectByType<NoirWorldScreenEffect>();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Noir World look (whole screen)");
                using (new EditorGUI.DisabledScope(screenEffect == null))
                {
                    if (GUILayout.Button("Show", GUILayout.Width(70)))
                    {
                        screenEffect.Show();
                    }

                    if (GUILayout.Button("Hide", GUILayout.Width(70)))
                    {
                        screenEffect.Hide();
                    }
                }
            }

            if (screenEffect == null)
            {
                EditorGUILayout.HelpBox("No NoirWorldScreenEffect in the scene.", MessageType.None);
            }
        }

        // Look only: the participant isn't given the Noirified status.
        private static void DrawNoirifiedLook(CombatParticipant participant)
        {
            var root = participant.SceneRoot;
            var effect = root != null ? root.GetComponentInChildren<OnNoirifiedEffect>(true) : null;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Noirified look (this character)");
                using (new EditorGUI.DisabledScope(effect == null))
                {
                    if (GUILayout.Button("Noirify", GUILayout.Width(70)))
                    {
                        effect.OnTrigger();
                    }

                    if (GUILayout.Button("Restore", GUILayout.Width(70)))
                    {
                        effect.OnDone();
                    }
                }
            }

            if (effect == null)
            {
                EditorGUILayout.HelpBox("No OnNoirifiedEffect on this character (is it on its side's effect dummy?).", MessageType.None);
            }
        }

        // The Character field names this participant (its live instance, or
        // the prefab it was spawned from).
        private bool IsSelected(CombatParticipant participant)
        {
            if (_character == null)
            {
                return false;
            }

            return participant.definition == _character
                   || (participant.SceneRoot != null && participant.SceneRoot == _character.gameObject);
        }

        private void DrawCard(CombatSceneManager manager, CombatParticipant participant, SkillCardDefinition card)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var inHand = participant.SkillCards.Contains(card);
                EditorGUILayout.LabelField(new GUIContent(card.displayName + (inHand ? "" : "  (not in hand)"), card.description), GUILayout.MinWidth(120));
                if (GUILayout.Button("Play", GUILayout.Width(60)))
                {
                    Play(manager, participant, card, AutoPlayMode.Off);
                }

                if (GUILayout.Button("Perfect", GUILayout.Width(70)))
                {
                    Play(manager, participant, card, AutoPlayMode.Perfect);
                }

                if (GUILayout.Button("Random Fail", GUILayout.Width(90)))
                {
                    Play(manager, participant, card, AutoPlayMode.RandomFail);
                }
            }
        }

        private void Play(CombatSceneManager manager, CombatParticipant participant, SkillCardDefinition card, AutoPlayMode mode)
        {
            _testMessage = manager.DebugPlaySkillCard(participant, card, mode, out var error) ? null : error;
        }

        // Every card the participant can play: its own pool, plus anything
        // added to its hand in battle (copies, replays).
        private static IEnumerable<SkillCardDefinition> CardsOf(CombatParticipant participant)
        {
            return participant.definition.SkillCards.Concat(participant.SkillCards).Where(card => card != null).Distinct();
        }

        private static string Describe(CombatParticipant participant)
        {
            var side = participant.Allegiance != participant.faction ? $"{participant.faction}→{participant.Allegiance}" : participant.faction.ToString();
            var state = participant.IsDefeated ? "defeated" : $"HP {participant.currentHp}";
            var prefab = participant.definition.gameObject.name;
            var name = prefab == participant.definition.displayName ? prefab : $"{participant.definition.displayName} [{prefab}]";
            return $"{name} ({side}, {state})";
        }
    }
}
