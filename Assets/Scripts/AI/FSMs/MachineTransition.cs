using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineTransition<T> where T : EntityInfo {
    
    private MachineState<T> stateTransition;
    private string description;

    public MachineState<T> StateTransition => stateTransition;
    public string Description => description;

    public MachineTransition(MachineState<T> stateTransition, string description) {
        this.stateTransition = stateTransition;
        this.description = description;
    }
}
