using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface iState
{
    void OnEnter();
    void HandleInput(ref Vector2 input);
    void LogicTick();
    void PhysicsTick();
    void OnExit();
}

// https://unity3d.college/2017/05/26/unity3d-design-patterns-state-basic-state-machine/
public class StateMachine : MonoBehaviour
{
    public List<iState> states = new List<iState>();
    public iState currentState = null;

    public void Initialize(iState startingState)
    {
        currentState = startingState;
        startingState.OnEnter();
    }   

    public void ChangeState(iState newState)
    {
        currentState.OnExit();

        currentState = newState;
        newState.OnEnter();
    }

    private void Update() {
        if(currentState != null)
            currentState.LogicTick();
    }

    private void LateUpdate() {
        if(currentState != null)
            currentState.LogicTick();
    }
}
