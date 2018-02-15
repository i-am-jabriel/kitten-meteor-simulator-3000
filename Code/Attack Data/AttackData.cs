using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackData {
	public string name;
	public Combatant owner;
	Transform skeleton;
	public List<SphereCollider> hitBoxes = new List<SphereCollider>();
    public List<FrameData> frameData = new List<FrameData>();
	delegate void attackDelegate();
	attackDelegate onStart, onInterrupt, onFinish, onEnterDamageFrames;
	delegate bool damagedDelegate( Combatant c );
	damagedDelegate onHit, onDamaged;
	public float cancelTime = 1,damage = 10;
	public uint maxChain = 0;
	public string[] chainableMoves = { };
	AttackData currentAttack {
		get { return owner.currentAttack; }
	}
	public AttackData() { }
	public AttackData( Combatant owner, string type ) {
		init(owner, type);
	}
	public virtual void init( Combatant owner, string type ) {
		this.owner = owner;
		name = type;
		skeleton = owner.getSkeleton();
	}
	public SphereCollider addHitBox( string tag ) {
		Transform parent = skeleton.FindComponentInChildWithTag<Transform>(tag);
		SphereCollider sc = parent.gameObject.addGetComponent<SphereCollider>();
		sc.isTrigger = true;
		hitBoxes.Add(sc);
		return sc;
	}
	public SphereCollider addHitBox( string tag, float radius ) {
		SphereCollider sc = addHitBox(tag);
		sc.radius = radius;
		return sc;
	}
    public void addFrameData(Vector2 window, Dictionary<string,object> data)
    {
        frameData.Add(new FrameData(window, data));
    }
    public void resetFrameData()
    {
        frameData = new List<FrameData>();
    }
    public List<Combatant> checkHitBoxes()
    {
        return checkHitBoxes(owner.enemies);
    }
    public List<Combatant> getCombatantsInHitboxes(List<Combatant> against)
    {
        List<Combatant> val = new List<Combatant>();
        foreach (Combatant c in against)
        {
            foreach (SphereCollider hitbox in hitBoxes)
            {
                if (hitbox.bounds.Intersects(c.hitBox.bounds))
                {
                    val.Add(c);
                }
            }
            break;
        }
        return val;
    }
    public List<Combatant> checkHitBoxes(List<Combatant> against)
    {
        //(owner as Player).debug(hitBoxes.Count.ToString());
        List<Combatant> hitTargets = getCombatantsInHitboxes(against);
        foreach (FrameData data in frameData)
        {
            if (owner.animationTime.between(data.activeWindow.x, data.activeWindow.y))
            {
                if((string)data.data["target"]=="enemy")foreach(Combatant target in hitTargets)
                {
                    data.effect(owner, target);
                    owner.StartCoroutine(removeFrameData(data));
                }
                if ((string)data.data["target"] == "owner")
                {
                    data.effect(owner, owner);
                    owner.StartCoroutine(removeFrameData(data));
                }
            }
        }
        return hitTargets;
    }
    IEnumerator removeFrameData(FrameData data)
    {
        yield return new WaitForEndOfFrame();
        if(frameData.Contains(data))frameData.Remove(data);
    }
	public bool canBeCancelled() {
		//owner.debug("Wilbur: " + (cancelTime <= owner.currentAnimationFrameRatio));
		return cancelTime <= owner.currentAnimationFrameRatio;
	}
	public bool canChain() {
		return 
			// If the current attack is part of a string
			(maxChain != 0 && (owner.animx+1) < maxChain && currentAttack.name == name) 
			|| System.Array.IndexOf<string>(currentAttack.chainableMoves,name) != -1;
	}

}

public struct FrameData {

    public Vector2 activeWindow;
    public Dictionary<string,object> data;

    public FrameData(Vector2 activeWindow, Dictionary<string, object> data)
    {
        this.activeWindow = activeWindow;
        this.data = data;
        if (!this.data.ContainsKey("target")) this.data["target"] = "enemy";
    }
    public void effect(Combatant owner,Combatant target)
    {
        if(target!=owner)target.startAction("Hit");
        if (data.ContainsKey("knockback"))
        {
            bool[] noStun = new bool[0];
            if (target == owner) noStun = new bool[] { true };
            Quaternion theta = owner.transform.getXZTheta(target.transform);
            float knockbackDampRate = 0.94f;
            if (data.ContainsKey("knockbackDampRate")) knockbackDampRate = (float)data["knockbackDampRate"];
            Vector3 force = ((theta * (Vector3)data["knockback"])) + Vector3.up * ((Vector3)data["knockback"]).y;
            target.startKnockback(owner, force * 10000 * 15, knockbackDampRate, noStun);
        }
        if (data.ContainsKey("dash"))
        {
            bool[] canAimDash = new bool[0];
            if (data.ContainsKey("canAimDash")) canAimDash = new bool[] { true };
            owner.startDash((Vector3)data["dash"],canAimDash);
        }
        if (data.ContainsKey("stun"))
        {
            target.stun((float)data["stun"]);
        }
    }
}