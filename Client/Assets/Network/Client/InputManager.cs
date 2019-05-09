using core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager 
{
    static readonly float kTimeBetweenInputSamples = 0.03f;

    /// <summary>
    /// Global instance of NetworkManagerClient
    /// </summary>
    public static InputManager sInstance = new InputManager();

    core.InputState mCurrentState = new core.InputState();

    InputManager()
    {
        mNextTimeToSampleInput = 0.0f;
        mPendingMove = null;

    }

    bool IsTimeToSampleInput()
    {
        float time = core.Timing.sInstance.GetFrameStartTime();
        if (time > mNextTimeToSampleInput)
        {
            mNextTimeToSampleInput = mNextTimeToSampleInput + kTimeBetweenInputSamples;
            return true;
        }

        return false;
    }
    Move SampleInputAsMove()
    {
        return mMoveList.AddMove(GetState(), core.Timing.sInstance.GetFrameStartTime());

    }

    MoveList mMoveList = new MoveList();
    float mNextTimeToSampleInput;
    Move mPendingMove = new Move();

    public void HandleInput(core.EInputAction inInputAction, KeyCode inKeyCode)
    {
#if USE_INPUT_STATE_OLD
        switch (inKeyCode)
        {
            case KeyCode.A:
                UpdateDesireFloatFromKey(inInputAction, out mCurrentState.mDesiredLeftAmount);
                break;
            case KeyCode.D:
                UpdateDesireFloatFromKey(inInputAction, out mCurrentState.mDesiredRightAmount);
                break;
            case KeyCode.W:
                UpdateDesireFloatFromKey(inInputAction, out mCurrentState.mDesiredForwardAmount);
                break;
            case KeyCode.S:
                UpdateDesireFloatFromKey(inInputAction, out mCurrentState.mDesiredBackAmount);
                break;
            case KeyCode.K:
                UpdateDesireVariableFromKey(inInputAction, out mCurrentState.mIsShooting);
                break;
        }
#else
        switch (inKeyCode)
        {
            case KeyCode.A:
                UpdateDesireVariableFromKey(inInputAction, out mCurrentState.mIsLeft);
                break;
            case KeyCode.D:
                UpdateDesireVariableFromKey(inInputAction, out mCurrentState.mIsRight);
                break;
            case KeyCode.W:
                UpdateDesireVariableFromKey(inInputAction, out mCurrentState.mIsForward);
                break;
            case KeyCode.S:
                UpdateDesireVariableFromKey(inInputAction, out mCurrentState.mIsBack);
                break;
            case KeyCode.K:
                UpdateDesireVariableFromKey(inInputAction, out mCurrentState.mIsShooting);
                break;
        }
#endif
    }

    public core.InputState GetState() { return mCurrentState; }

    public MoveList GetMoveList() { return mMoveList; }

    public Move GetAndClearPendingMove() { var toRet = mPendingMove; mPendingMove = null; return toRet; }

    public void Update()
    {
        if (IsTimeToSampleInput())
        {
            mPendingMove = SampleInputAsMove();
        }
    }


    void UpdateDesireVariableFromKey(core.EInputAction inInputAction, out bool ioVariable )
    {
        ioVariable = false;
        if (inInputAction == core.EInputAction.EIA_Pressed)
        {
            ioVariable = true;
        }
        else if (inInputAction == core.EInputAction.EIA_Released)
        {
            ioVariable = false;
        }
    }

    void UpdateDesireFloatFromKey(core.EInputAction inInputAction, out float ioVariable )
    {
        ioVariable = 0.0f;
        if (inInputAction == core.EInputAction.EIA_Pressed)
        {
            ioVariable = 1.0f;
        }
        else if (inInputAction == core.EInputAction.EIA_Released)
        {
            ioVariable = 0.0f;
        }
    }
}
