using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RouletteFsmLoopSmoke : MonoBehaviour
{
    private readonly List<StateID> sequence = new List<StateID>();
    private StateID last = StateID.NullStateID;
    private int completedRounds;
    private float deadline;
    private bool finished;
    private bool playPending;

    public static bool Requested
    {
        get
        {
            return Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteFsmLoopSmoke") >= 0;
        }
    }

    private void Start()
    {
        deadline = Time.realtimeSinceStartup + 180f;
        Debug.Log("ROULETTE_FSM_LOOP_START: mode=" + Model.Instance.rouletteFlow);
        if (!Model.Instance.rouletteFlow)
        {
            Debug.LogError("ROULETTE_FSM_LOOP_FAILED: -noRouletteFlow must not be passed");
            Quit(1);
        }
    }

    private void Update()
    {
        if (finished) return;

        if (FSMSystem.Instance.CurrentStateID != last)
        {
            last = FSMSystem.Instance.CurrentStateID;
            sequence.Add(last);
            Debug.Log("ROULETTE_FSM_LOOP_STATE: " + last);

            if (last == StateID.Bet && RouletteFlowBridge.Instance != null)
                playPending = true;

            if (last == StateID.Prepare && sequence.Contains(StateID.EndRound))
            {
                ++completedRounds;
                if (completedRounds >= 2) Finish();
            }
        }

        // Bet 刚进入时押分窗口可能还没打开；逐帧重试直到 PLAY 被真正接受。
        if (playPending && !finished && RouletteFlowBridge.Instance != null)
        {
            if (RouletteFlowBridge.Instance.PlayOneRoundForSmoke(100))
                playPending = false;
        }

        if (Time.realtimeSinceStartup > deadline)
        {
            Debug.LogError("ROULETTE_FSM_LOOP_FAILED: timeout " +
                string.Join(">", sequence));
            Quit(1);
        }
    }

    private void Finish()
    {
        string[] expected =
        {
            "FirstStart", "Prepare", "Bet", "Reward", "EndRound", "Print", "Prepare"
        };
        int at = 0;
        for (int i = 0; i < sequence.Count && at < expected.Length; ++i)
        {
            if (sequence[i].ToString() == expected[at]) ++at;
        }

        if (at != expected.Length)
        {
            Debug.LogError("ROULETTE_FSM_LOOP_FAILED: " + string.Join(">", sequence));
            Quit(1);
            return;
        }

        Debug.Log("ROULETTE_FSM_LOOP_PASSED: " + string.Join(">", sequence));
        Quit(0);
    }

    private void Quit(int code)
    {
        if (finished) return;
        finished = true;
        Application.Quit(code);
    }
}
