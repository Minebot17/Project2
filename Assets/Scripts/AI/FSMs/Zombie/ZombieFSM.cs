using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(AttackHandler))]
[RequireComponent(typeof(VisibleObserver))]
[RequireComponent(typeof(ZombieInfo))]
public class ZombieFSM : AbstractFSM<ZombieInfo> {

    protected override AbstractState<ZombieInfo>[] InitializeAllStates() {
        return new AbstractState<ZombieInfo>[] {
            // TODO
        };
    }

    protected override string GetDefaultStateName() {
        return "stand";
    }
}
