using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractFSM<T> : MonoBehaviour where T : EntityInfo {
    protected T info;
    private List<MachineState<T>> allStates = new List<MachineState<T>>(); 
    private MachineState<T> defaultState = null;
    private Stack<MachineState<T>> currentStates = new Stack<MachineState<T>>();

    public List<MachineState<T>> AllStates => allStates;// Для визуализации состояний

    protected virtual void Start() {
        info = (T) GetComponent<EntityInfo>();
        allStates.AddRange(InitializeAllStates());
        defaultState = allStates.Find(s => s.StateName.Equals(GetDefaultStateName()));
    }

    protected virtual void FixedUpdate() {
        if (!info.EnableAI)
            return;
        
        if (currentStates.Count == 0)
            currentStates.Push(defaultState);
        
        currentStates.Peek().OnTick();
    }

    public void PushTransition(MachineTransition<T> transition) {
        if (currentStates.Count != 0)
            currentStates.Peek().OnOut();

        MachineState<T> newState = transition.StateTransition;
        newState.OnEnter();
        currentStates.Push(newState);
    }

    public void PopState() {
        if (currentStates.Count != 0)
            currentStates.Pop().OnOut();
    }

    protected abstract MachineState<T>[] InitializeAllStates();
    protected abstract string GetDefaultStateName();
}
