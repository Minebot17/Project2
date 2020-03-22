using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class MachineState<T> where T : EntityInfo {
    private AbstractFSM<T> fsm;
    private string stateName;
    private List<MachineTransition<T>> transitions = new List<MachineTransition<T>>(); 

    public string StateName => stateName;
    public List<MachineTransition<T>> Transitions => transitions; // Для визуализации переходов

    public MachineState(AbstractFSM<T> fsm, string stateName) {
        this.fsm = fsm;
        this.stateName = stateName;
    }

    // Переходы надо хранить самостоятельно после создания
    protected MachineTransition<T> CreateTransition(string toStateName, string description) {
        MachineTransition<T> transition = new MachineTransition<T>(fsm.AllStates.Find(s => s.stateName.Equals(toStateName)), description);
        transitions.Add(transition);
        return transition;
    }

    protected void MakeTransition(MachineTransition<T> transition) {
        fsm.PushTransition(transition);
    }

    // Вызывать если это действие закончено
    protected void PopState() {
        fsm.PopState();
    }

    public abstract void OnEnter();
    public abstract void OnTick();
    public abstract void OnOut(); // Вызывается при заканчивании действия или начале нового
}
