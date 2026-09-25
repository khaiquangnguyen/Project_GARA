using System;
using GARA.Combat;
using GARA.Input;
using GARA.InputSets;
using GARA.Rhythm;
using GARA.ShakeBalance;
using MoreMountains.Feedbacks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GARA.EditorTools
{
    // One place for the combat scene's development aids. The visual-driver
    // tests don't reference any driver: like a character, they play a
    // generated minigame through a real RhythmSequencePlayer /
    // InputSetCollectionPlayer / ShakeBalancePlayer, whose static "any started/ended" events the
    // scene's overlay already forwards to whichever driver presents it.
    public class CombatSceneDevelopmentWindow : EditorWindow
    {
        private const string WindowTitle = "Combat Scene Development";
        private const string TestHostName = "[Dev] Combat Test Host";

        [SerializeField]
        private InputTokenMap rhythmInputMap;

        [Tooltip("Seconds from pressing Test until the first note's hit time.")]
        [SerializeField]
        private float rhythmLeadIn = 2f;

        [SerializeField]
        private int rhythmNoteCount = 8;

        [Tooltip("Seconds between consecutive notes' hit times.")]
        [SerializeField]
        private float rhythmNoteInterval = 0.5f;

        [SerializeField]
        private float rhythmTailOut = 0.5f;

        [SerializeField]
        private RhythmTimingWindows rhythmWindows = new RhythmTimingWindows
        {
            perfect = 0.05f,
            good = 0.1f,
            ok = 0.15f,
            holdReleaseTolerance = 0.1f
        };

        [SerializeField]
        private InputTokenMap inputSetInputMap;

        [SerializeField]
        private int setCount = 4;

        [SerializeField]
        private int minInputsPerSet = 3;

        [SerializeField]
        private int maxInputsPerSet = 5;

        [Tooltip("Seconds per set attempt. 0 = no set time limit.")]
        [SerializeField]
        private float setTimeLimit = 4f;

        [Tooltip("Seconds for the whole set collection. 0 = no set collection time limit.")]
        [SerializeField]
        private float setCollectionTimeLimit = 20f;

        [SerializeField]
        private InputSetRetryPolicy retryPolicy = new InputSetRetryPolicy
        {
            resetTimerOnRetry = true,
            maxAttemptsPerSet = 3,
            onExhausted = ExhaustedSetBehaviour.Skip
        };

        [SerializeField]
        private InputTokenMap shakeBalanceInputMap;

        [SerializeField]
        private InputToken shakeBalanceLeftToken = new InputToken(2);

        [SerializeField]
        private InputToken shakeBalanceRightToken = new InputToken(4);

        [SerializeField]
        private bool shakeBalanceUseCashOutToken = true;

        [SerializeField]
        private InputToken shakeBalanceCashOutToken = new InputToken(1);

        [Tooltip("Seconds until the run ends on its own. 0 = no limit.")]
        [SerializeField]
        private float shakeBalanceDuration;

        [SerializeField]
        private float shakeBalanceBaseInstability = 0.8f;

        [SerializeField]
        private float shakeBalanceInstabilityGrowth = 0.02f;

        [SerializeField]
        private float shakeBalanceBaseNoise = 0.3f;

        [SerializeField]
        private float shakeBalanceNoiseGrowth = 0.02f;

        [Range(0f, 1f)]
        [SerializeField]
        private float shakeBalancePerfectZone = 0.15f;

        [Range(0f, 1f)]
        [SerializeField]
        private float shakeBalanceGoodZone = 0.35f;

        [SerializeField]
        private float shakeBalanceBankTimeConstant = 8f;

        [Range(0f, 1f)]
        [SerializeField]
        private float shakeBalanceKeepOnFall = 0.5f;

        [Tooltip("Seconds the view's position shake lasts.")]
        [SerializeField]
        private float viewShakePositionDuration = 0.25f;

        [Tooltip("How fast the view goes back and forth. Higher = buzzier.")]
        [SerializeField]
        private float viewShakePositionSpeed = 20f;

        [Tooltip("How far the view moves, in UI pixels at the 1920x1080 reference resolution. 0 = no position shake.")]
        [SerializeField]
        private float viewShakePositionRange = 50f;

        [SerializeField]
        private Vector3 viewShakePositionMainDirection = Vector3.up;

        [Tooltip("When on, each shake picks a random direction between Main and Alt Direction.")]
        [SerializeField]
        private bool viewShakePositionRandomizeDirection;

        [SerializeField]
        private Vector3 viewShakePositionAltDirection = Vector3.up;

        [SerializeField]
        private bool viewShakePositionAddDirectionalNoise = true;

        [SerializeField]
        private Vector3 viewShakePositionNoiseStrengthMin = new Vector3(0f, 0.25f, 0f);

        [SerializeField]
        private Vector3 viewShakePositionNoiseStrengthMax = new Vector3(0f, 0.25f, 0f);

        [Tooltip("Seconds the view's Z rotation shake lasts.")]
        [SerializeField]
        private float viewShakeRotationDuration = 0.25f;

        [Tooltip("How fast the view tilts back and forth.")]
        [SerializeField]
        private float viewShakeRotationSpeed = 20f;

        [Tooltip("How far the view tilts around Z, in degrees. 0 = no rotation shake. Tilting uncovers the screen corners, so keep it small.")]
        [SerializeField]
        private float viewShakeRotationRange = 2f;

        [Tooltip("When on, both shakes' strength follows the attenuation curve over their duration.")]
        [SerializeField]
        private bool viewShakeUseAttenuation = true;

        [SerializeField]
        private AnimationCurve viewShakeAttenuationCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

        private SerializedObject _serializedWindow;
        private bool _rhythmExpanded = true;
        private bool _inputSetExpanded = true;
        private bool _shakeBalanceExpanded = true;
        private bool _viewShakeExpanded = true;
        private Vector2 _scroll;

        [MenuItem("Tools/Combat Scene Development")]
        public static void Open()
        {
            GetWindow<CombatSceneDevelopmentWindow>(WindowTitle);
        }

        private void OnEnable()
        {
            _serializedWindow = new SerializedObject(this);

            if (rhythmInputMap == null || inputSetInputMap == null || shakeBalanceInputMap == null)
            {
                var defaultMap = FindDefaultInputMap();
                rhythmInputMap = rhythmInputMap != null ? rhythmInputMap : defaultMap;
                inputSetInputMap = inputSetInputMap != null ? inputSetInputMap : defaultMap;
                shakeBalanceInputMap = shakeBalanceInputMap != null ? shakeBalanceInputMap : defaultMap;
            }
        }

        // Keeps the Test/Restart Test labels current while a test runs.
        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            _serializedWindow.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to run the visual driver tests. They play through the same players a character uses, so the scene's overlay presents them.", MessageType.Info);
            }

            _rhythmExpanded = DrawSection("Rhythm", _rhythmExpanded, new[]
            {
                nameof(rhythmInputMap), nameof(rhythmLeadIn), nameof(rhythmNoteCount), nameof(rhythmNoteInterval), nameof(rhythmTailOut), nameof(rhythmWindows)
            }, IsPlaying<RhythmSequencePlayer>(p => p.IsPlaying), RunRhythmTest);

            _inputSetExpanded = DrawSection("Input Sets", _inputSetExpanded, new[]
            {
                nameof(inputSetInputMap), nameof(setCount), nameof(minInputsPerSet), nameof(maxInputsPerSet), nameof(setTimeLimit), nameof(setCollectionTimeLimit), nameof(retryPolicy)
            }, IsPlaying<InputSetCollectionPlayer>(p => p.IsPlaying), RunInputSetTest);

            _shakeBalanceExpanded = DrawSection("Shake Balance", _shakeBalanceExpanded, new[]
            {
                nameof(shakeBalanceInputMap), nameof(shakeBalanceLeftToken), nameof(shakeBalanceRightToken), nameof(shakeBalanceUseCashOutToken), nameof(shakeBalanceCashOutToken),
                nameof(shakeBalanceDuration), nameof(shakeBalanceBaseInstability), nameof(shakeBalanceInstabilityGrowth), nameof(shakeBalanceBaseNoise), nameof(shakeBalanceNoiseGrowth),
                nameof(shakeBalancePerfectZone), nameof(shakeBalanceGoodZone), nameof(shakeBalanceBankTimeConstant), nameof(shakeBalanceKeepOnFall)
            }, IsPlaying<ShakeBalancePlayer>(p => p.IsPlaying), RunShakeBalanceTest);

            _viewShakeExpanded = DrawSection("View Shake", _viewShakeExpanded, new[]
            {
                nameof(viewShakePositionDuration), nameof(viewShakePositionSpeed), nameof(viewShakePositionRange), nameof(viewShakePositionMainDirection),
                nameof(viewShakePositionRandomizeDirection), nameof(viewShakePositionAltDirection), nameof(viewShakePositionAddDirectionalNoise),
                nameof(viewShakePositionNoiseStrengthMin), nameof(viewShakePositionNoiseStrengthMax),
                nameof(viewShakeRotationDuration), nameof(viewShakeRotationSpeed), nameof(viewShakeRotationRange),
                nameof(viewShakeUseAttenuation), nameof(viewShakeAttenuationCurve)
            }, false, RunViewShakeTest);

            EditorGUILayout.EndScrollView();
            _serializedWindow.ApplyModifiedProperties();
        }

        private bool DrawSection(string title, bool expanded, string[] propertyNames, bool isRunning, Action runTest)
        {
            EditorGUILayout.Space();
            expanded = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, title);
            if (expanded)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    foreach (var propertyName in propertyNames)
                    {
                        EditorGUILayout.PropertyField(_serializedWindow.FindProperty(propertyName), true);
                    }

                    EditorGUILayout.Space();
                    using (new EditorGUI.DisabledScope(!Application.isPlaying))
                    {
                        if (GUILayout.Button(isRunning ? "Restart Test" : "Test"))
                        {
                            // Deferred out of OnGUI: anything the test throws then can't
                            // leave this window's layout half-built.
                            EditorApplication.delayCall += () =>
                            {
                                runTest();
                                FocusGameView();
                            };
                        }
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            return expanded;
        }

        private void RunRhythmTest()
        {
            if (!TryGetInputMap(rhythmInputMap, "Rhythm"))
            {
                return;
            }

            var notes = new RhythmNote[Mathf.Max(1, rhythmNoteCount)];
            for (var i = 0; i < notes.Length; i++)
            {
                notes[i] = new RhythmNote
                {
                    input = RandomToken(rhythmInputMap),
                    kind = RhythmNoteKind.Tap,
                    time = i * rhythmNoteInterval
                };
            }

            var definition = RhythmSequenceDefinition.CreateRuntime(rhythmLeadIn, rhythmTailOut, rhythmWindows, notes);
            var player = GetHostPlayer<RhythmSequencePlayer>(rhythmInputMap);
            player.Play(definition, report =>
            {
                Destroy(definition);
                Debug.Log($"[RhythmTest] {report.HitNotes}/{report.TotalNotes} hit " +
                          $"(P={report.PerfectCount} G={report.GoodCount} O={report.OkCount} M={report.MissCount}), " +
                          $"accuracy={report.AccuracyScore:F2}, stray={report.StrayPresses}, aborted={report.WasAborted}");
            });
        }

        private void RunInputSetTest()
        {
            if (!TryGetInputMap(inputSetInputMap, "Input Sets"))
            {
                return;
            }

            var minInputs = Mathf.Max(1, minInputsPerSet);
            var maxInputs = Mathf.Max(minInputs, maxInputsPerSet);
            var sets = new InputSetDefinition[Mathf.Max(1, setCount)];
            for (var i = 0; i < sets.Length; i++)
            {
                var inputs = new InputToken[Random.Range(minInputs, maxInputs + 1)];
                for (var j = 0; j < inputs.Length; j++)
                {
                    inputs[j] = RandomToken(inputSetInputMap);
                }

                sets[i] = new InputSetDefinition
                {
                    inputs = inputs,
                    timeLimit = setTimeLimit
                };
            }

            var definition = InputSetCollectionDefinition.CreateRuntime(sets, setCollectionTimeLimit, retryPolicy);
            var player = GetHostPlayer<InputSetCollectionPlayer>(inputSetInputMap);
            player.Play(definition, report =>
            {
                Destroy(definition);
                Debug.Log($"[InputSetTest] cleared {report.ClearedSets}/{report.TotalSets} sets " +
                          $"({report.CompletionRate:P0}), firstTry={report.FirstTryRate:P0}, " +
                          $"attempts={report.TotalAttempts}, aborted={report.WasAborted}, " +
                          $"setCollectionTimedOut={report.SetCollectionTimedOut}, elapsed={report.TotalElapsed:F2}s");
            });
        }

        private void RunShakeBalanceTest()
        {
            if (!TryGetInputMap(shakeBalanceInputMap, "Shake Balance"))
            {
                return;
            }

            var cashOutToken = shakeBalanceUseCashOutToken ? shakeBalanceCashOutToken : (InputToken?)null;
            var definition = ShakeBalanceDefinition.CreateRuntime(shakeBalanceLeftToken, shakeBalanceRightToken, cashOutToken, shakeBalanceDuration);
            definition.SetDifficulty(shakeBalanceBaseInstability, shakeBalanceInstabilityGrowth, shakeBalanceBaseNoise, shakeBalanceNoiseGrowth);
            definition.SetScoring(shakeBalancePerfectZone, shakeBalanceGoodZone, shakeBalanceBankTimeConstant, shakeBalanceKeepOnFall);

            var player = GetHostPlayer<ShakeBalancePlayer>(shakeBalanceInputMap);
            player.Play(definition, report =>
            {
                Destroy(definition);
                Debug.Log($"[ShakeBalanceTest] {ShakeBalanceDebugLogger.Describe(report)}");
            });
        }

        /// <summary>
        /// Shakes the scene's view with this window's settings, sent straight to the view shakers
        /// CombatSceneManager references — the same call a Position / Rotation Shake feedback makes.
        /// The shakers store no settings of their own. Also used by the CombatSceneManager
        /// Inspector's Test View Shake button (opening this window if needed, for its settings).
        /// </summary>
        public static void TestViewShake()
        {
            GetWindow<CombatSceneDevelopmentWindow>(WindowTitle, false).RunViewShakeTest();
        }

        private void RunViewShakeTest()
        {
            var manager = Object.FindFirstObjectByType<CombatSceneManager>();
            if (manager == null)
            {
                Debug.LogWarning($"[{WindowTitle}] View Shake test needs a CombatSceneManager in the open scene.");
                return;
            }

            // Both are private serialized fields on CombatSceneManager.
            var serializedManager = new SerializedObject(manager);
            var positionShaker = serializedManager.FindProperty("viewPositionShaker").objectReferenceValue as MMPositionShaker;
            var rotationShaker = serializedManager.FindProperty("viewRotationShaker").objectReferenceValue as MMRotationShaker;
            if (positionShaker == null && rotationShaker == null)
            {
                Debug.LogWarning($"[{WindowTitle}] View Shake test needs CombatSceneManager's View Position Shaker or View Rotation Shaker assigned.");
                return;
            }

            if (positionShaker != null && viewShakePositionRange > 0f)
            {
                positionShaker.OnMMPositionShakeEvent(
                    viewShakePositionDuration, viewShakePositionSpeed, viewShakePositionRange, viewShakePositionMainDirection,
                    viewShakePositionRandomizeDirection, viewShakePositionAltDirection,
                    randomizeDirectionOnPlay: false, randomizeDirectionX: true, randomizeDirectionY: true, randomizeDirectionZ: true,
                    addDirectionalNoise: viewShakePositionAddDirectionalNoise,
                    directionalNoiseStrengthMin: viewShakePositionNoiseStrengthMin, directionalNoiseStrengthMax: viewShakePositionNoiseStrengthMax,
                    randomnessSeed: Vector3.zero, randomizeSeedOnShake: true,
                    useAttenuation: viewShakeUseAttenuation, attenuationCurve: viewShakeAttenuationCurve,
                    channelData: positionShaker.ChannelData);
            }

            if (rotationShaker != null && viewShakeRotationRange > 0f)
            {
                rotationShaker.OnMMRotationShakeEvent(
                    viewShakeRotationDuration, viewShakeRotationSpeed, viewShakeRotationRange, Vector3.forward,
                    randomizeDirection: false, shakeAltDirection: Vector3.forward, randomizeDirectionOnPlay: false,
                    addDirectionalNoise: false, directionalNoiseStrengthMin: Vector3.zero, directionalNoiseStrengthMax: Vector3.zero,
                    randomnessSeed: Vector3.zero, randomizeSeedOnShake: true,
                    useAttenuation: viewShakeUseAttenuation, attenuationCurve: viewShakeAttenuationCurve,
                    channelData: rotationShaker.ChannelData);
            }
        }

        /// <summary>The Play-mode host's player of type <typeparamref name="T"/>, created on first use and pointed at <paramref name="inputMap"/>.</summary>
        private static T GetHostPlayer<T>(InputTokenMap inputMap) where T : MonoBehaviour
        {
            // A plain scene object, so it goes away with the scene when Play mode ends.
            var host = GameObject.Find(TestHostName);
            if (host == null)
            {
                host = new GameObject(TestHostName);
            }

            var player = host.GetComponent<T>();
            if (player == null)
            {
                player = host.AddComponent<T>();
            }

            // inputMap is a private serialized field on every player.
            var serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("inputMap").objectReferenceValue = inputMap;
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();
            return player;
        }

        private static bool IsPlaying<T>(Func<T, bool> isPlaying) where T : MonoBehaviour
        {
            if (!Application.isPlaying)
            {
                return false;
            }

            var host = GameObject.Find(TestHostName);
            var player = host != null ? host.GetComponent<T>() : null;
            return player != null && isPlaying(player);
        }

        private static bool TryGetInputMap(InputTokenMap inputMap, string section)
        {
            if (inputMap != null && inputMap.Entries.Count > 0)
            {
                return true;
            }

            Debug.LogWarning($"[{WindowTitle}] {section} test needs an input map with at least one entry.");
            return false;
        }

        private static InputToken RandomToken(InputTokenMap inputMap)
        {
            return inputMap.Entries[Random.Range(0, inputMap.Entries.Count)].token;
        }

        private static InputTokenMap FindDefaultInputMap()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(InputTokenMap));
            return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<InputTokenMap>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        }

        // Clicking a button here leaves keyboard focus on this window, and the Input System
        // only feeds keyboard input to the game while the Game view is focused.
        private static void FocusGameView()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }
    }
}
