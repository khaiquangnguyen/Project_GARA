using System;
using System.Collections.Generic;
using GARA.Input;
using UnityEngine;

namespace GARA.Rhythm
{
    /// <summary>
    /// Drives a <see cref="RhythmSequenceDefinition"/> through time, judging notes as
    /// presses/releases arrive. Plain C# so it can be unit tested and driven by any host.
    /// </summary>
    public sealed class RhythmSequenceRunner
    {
        private readonly RhythmSequenceDefinition _definition;
        private readonly List<RhythmNoteResult> _results = new List<RhythmNoteResult>();
        private readonly bool[] _judged;
        private readonly bool[] _windowOpened;

        private int _holdingNoteIndex = -1;
        private int _holdResultPosition = -1;
        private float _holdStartElapsed;
        private int _strayPresses;
        private bool _isRunning;
        private bool _completed;
        private float _elapsedTime;

        public RhythmSequenceRunner(RhythmSequenceDefinition definition)
        {
            _definition = definition;
            var noteCount = definition.Notes.Count;
            _judged = new bool[noteCount];
            _windowOpened = new bool[noteCount];
        }

        public bool IsRunning => _isRunning;
        public float ElapsedTime => _elapsedTime;
        public RhythmSequenceDefinition Definition => _definition;
        public IReadOnlyList<RhythmNoteResult> ResultsSoFar => _results;

        public event Action<int> NoteWindowOpened;
        public event Action<RhythmNoteResult> NoteJudged;
        public event Action<RhythmCompletionReport> Completed;

        public void Start()
        {
            _elapsedTime = 0f;
            _results.Clear();
            _strayPresses = 0;
            _holdingNoteIndex = -1;
            _holdResultPosition = -1;
            _holdStartElapsed = 0f;
            _completed = false;
            _isRunning = true;
            Array.Clear(_judged, 0, _judged.Length);
            Array.Clear(_windowOpened, 0, _windowOpened.Length);
        }

        public void Tick(float deltaSeconds)
        {
            if (!_isRunning || _completed)
            {
                return;
            }

            _elapsedTime += deltaSeconds;

            var notes = _definition.Notes;
            var windows = _definition.Windows;

            for (var i = 0; i < notes.Count; i++)
            {
                if (_judged[i])
                {
                    continue;
                }

                var noteTime = _definition.LeadIn + notes[i].time;
                var delta = _elapsedTime - noteTime;

                if (!_windowOpened[i] && Mathf.Abs(delta) <= windows.ok)
                {
                    _windowOpened[i] = true;
                    NoteWindowOpened?.Invoke(i);
                }

                if (_windowOpened[i] && delta > windows.ok)
                {
                    JudgeNote(i, RhythmJudgement.Miss, float.NaN, 0f);
                }
            }

            if (_holdingNoteIndex >= 0)
            {
                var holdNote = notes[_holdingNoteIndex];
                var heldDuration = _elapsedTime - _holdStartElapsed;
                if (heldDuration >= holdNote.holdDuration)
                {
                    FinalizeHold(1f);
                }
            }

            if (_elapsedTime >= _definition.Duration)
            {
                CompleteInternal(false);
            }
        }

        public void Press(InputToken token)
        {
            if (!_isRunning || _completed)
            {
                return;
            }

            var matchIndex = FindOpenNoteIndex(token);
            if (matchIndex < 0)
            {
                _strayPresses++;
                return;
            }

            var note = _definition.Notes[matchIndex];
            var delta = _elapsedTime - (_definition.LeadIn + note.time);
            var judgement = _definition.Windows.Judge(Mathf.Abs(delta));

            if (note.kind == RhythmNoteKind.Hold)
            {
                BeginHold(matchIndex, judgement, delta);
            }
            else
            {
                JudgeNote(matchIndex, judgement, delta, 1f);
            }
        }

        public void Release(InputToken token)
        {
            if (!_isRunning || _completed || _holdingNoteIndex < 0)
            {
                return;
            }

            var note = _definition.Notes[_holdingNoteIndex];
            if (!(note.input == token))
            {
                return;
            }

            var heldDuration = _elapsedTime - _holdStartElapsed;
            var completion = note.holdDuration > 0f ? Mathf.Clamp01(heldDuration / note.holdDuration) : 1f;
            FinalizeHold(completion);
        }

        public void Abort()
        {
            if (!_isRunning || _completed)
            {
                return;
            }

            CompleteInternal(true);
        }

        private int FindOpenNoteIndex(InputToken token)
        {
            var notes = _definition.Notes;
            var best = -1;
            var bestTime = float.MaxValue;

            for (var i = 0; i < notes.Count; i++)
            {
                if (_judged[i] || !_windowOpened[i])
                {
                    continue;
                }

                if (!(notes[i].input == token))
                {
                    continue;
                }

                if (notes[i].time < bestTime)
                {
                    bestTime = notes[i].time;
                    best = i;
                }
            }

            return best;
        }

        private void BeginHold(int index, RhythmJudgement judgement, float deltaSeconds)
        {
            _judged[index] = true;
            var note = _definition.Notes[index];
            var result = new RhythmNoteResult(index, note.input, judgement, deltaSeconds, 0f);
            _holdResultPosition = _results.Count;
            _results.Add(result);
            _holdingNoteIndex = index;
            _holdStartElapsed = _elapsedTime;
            NoteJudged?.Invoke(result);
        }

        private void FinalizeHold(float holdCompletion)
        {
            if (_holdingNoteIndex < 0)
            {
                return;
            }

            var previous = _results[_holdResultPosition];
            _results[_holdResultPosition] = new RhythmNoteResult(previous.NoteIndex, previous.Input, previous.Judgement, previous.DeltaSeconds, holdCompletion);
            _holdingNoteIndex = -1;
            _holdResultPosition = -1;
        }

        private void JudgeNote(int index, RhythmJudgement judgement, float deltaSeconds, float holdCompletion)
        {
            _judged[index] = true;
            var note = _definition.Notes[index];
            var result = new RhythmNoteResult(index, note.input, judgement, deltaSeconds, holdCompletion);
            _results.Add(result);
            NoteJudged?.Invoke(result);
        }

        private void CompleteInternal(bool wasAborted)
        {
            if (_completed)
            {
                return;
            }

            if (_holdingNoteIndex >= 0)
            {
                FinalizeHold(1f);
            }

            var notes = _definition.Notes;
            for (var i = 0; i < notes.Count; i++)
            {
                if (!_judged[i])
                {
                    JudgeNote(i, RhythmJudgement.Miss, float.NaN, 0f);
                }
            }

            _completed = true;
            _isRunning = false;

            var report = new RhythmCompletionReport(_results, _strayPresses, wasAborted);
            Completed?.Invoke(report);
        }
    }
}
