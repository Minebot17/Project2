using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class AbstractState<T> where T : EntityInfo {
    private AbstractFSM<T> fsm;
    private string stateName;
    private List<AbstractTransition<T>> transitions = new List<AbstractTransition<T>>(); 

    public string StateName => stateName;
    public List<AbstractTransition<T>> Transitions => transitions; // Для визуализации переходов

    public AbstractState(AbstractFSM<T> fsm, string stateName) {
        this.fsm = fsm;
        this.stateName = stateName;
    }

    // Переходы надо хранить самостоятельно после создания и создавать в конструкторе
    protected AbstractTransition<T> CreateTransition(string toStateName, string description) {
        AbstractTransition<T> transition = new AbstractTransition<T>(fsm.AllStates.Find(s => s.stateName.Equals(toStateName)), description);
        transitions.Add(transition);
        return transition;
    }

    protected void MakeTransition(AbstractTransition<T> transition) {
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
