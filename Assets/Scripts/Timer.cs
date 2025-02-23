using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class Timer : MonoBehaviour
    {
        private List<TimerProcess> _timers = new List<TimerProcess>();
        private void Start()
        {
            StartCoroutine(ProcessTimers());
        }
        public TimerProcess StartNewTimer(float timer)
        {
            if(timer <= 0)
            {
                throw new ArgumentException("Timer must be greater than 0");
            }
            TimerProcess timerProcess = new TimerProcess(timer);
            _timers.Add(timerProcess);
            timerProcess.OnTimerEnds += () => _timers.Remove(timerProcess);
            timerProcess.ProcessStopped += () => _timers.Remove(timerProcess);
            return timerProcess;
        }
        public TimerProcess StartEndlessTimer()
        {
            TimerProcess timerProcess = new TimerProcess();
            _timers.Add(timerProcess);
            timerProcess.ProcessStopped += () => _timers.Remove(timerProcess);
            return timerProcess;
        }
        private IEnumerator ProcessTimers()
        {
            while(true)
            {
                yield return null;
                for(int i = 0; i < _timers.Count; i++)
                {
                    var timer = _timers[i];
                    if(timer == null) continue;
                    timer.Process();
                }
            }
        }

        public class TimerProcess
        {
            public bool EndlessTimer { get; private set; }
            public Action OnTimerEnds;
            internal Action ProcessStopped;
            private float _secondsPassed;
            public float SecondsPassed => _secondsPassed;

            public TimerProcess(float timer = 0)
            {
                EndlessTimer = timer == 0;
                _secondsPassed = timer;
            }
            public void Stop()
            {
                ProcessStopped?.Invoke();
            }
            public void Reset(float timer)
            {
                if(EndlessTimer)
                {
                    throw new InvalidOperationException("Endless timer cannot be reset");
                }
                _secondsPassed = timer;
            }
            internal void Process()
            {
                if(EndlessTimer) 
                    _secondsPassed += Time.deltaTime;
                else
                {
                    _secondsPassed -= Time.deltaTime;
                    if(_secondsPassed <= 0)
                    {
                        OnTimerEnds?.Invoke();
                    }
                }
            }

            public override string ToString()
            {
                return string.Format("{0}:{1:00}", _secondsPassed / 60, _secondsPassed % 60);
            }
        }
    }
}
