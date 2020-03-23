using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractTransition<T> where T : EntityInfo {
    
    private AbstractState<T> stateTransition;
    private string description;

    public AbstractState<T> StateTransition => stateTransition;
    public string Description => description;

    public AbstractTransition(AbstractState<T> stateTransition, string description) {
        this.stateTransition = stateTransition;
        this.description = description;
    }
}
