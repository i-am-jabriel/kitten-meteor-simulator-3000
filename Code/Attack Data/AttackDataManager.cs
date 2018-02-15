using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDataManager {
	public Combatant owner;
	AttackData currentAttack {
		get { return owner.currentAttack; }
		set { owner.currentAttack = value; }
	}
	public AttackDataManager( Combatant owner ) {
		this.owner = owner;
	}
	public virtual AttackData startAttack( string name ) {
        //owner.animx = 0;
		return currentAttack = new AttackData(owner, name);
	}
	public virtual AttackData startNextAttackInChain() {
		owner.animx++;
		return currentAttack = startAttack(currentAttack.name);
	}
}
