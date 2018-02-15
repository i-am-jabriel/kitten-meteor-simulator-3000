using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Combatant : Interactable {

	// Use this for initialization
	public Transform skeleton;

	[NonSerialized]
	public Animator animator;
	[NonSerialized]
	public CapsuleCollider hitBox;
	[NonSerialized]
	public Rigidbody rb;
	[NonSerialized]
	public AttackData currentAttack;

	[NonSerialized]
	public AttackDataManager attackDataManager;


	[NonSerialized]
	public float animationTime = 0;
	[NonSerialized]
	public string action = null;
	[NonSerialized]
	public float moveSpeed = 3500;
    [NonSerialized]
    float stunTime = 0;

    [NonSerialized]
    public List<Combatant> enemies = new List<Combatant>();
    public static List<Combatant> Combatants = new List<Combatant>();
    [NonSerialized]
    public int id = 0;
    [NonSerialized]
    public Gravity gravityManager;

    public PlayerInput Input;

    public UnityEngine.UI.Text DebugText;

    public float gravity
    {
        get { return gravityManager.gravity; }
        set { gravityManager.gravity = value; }
    }
    public bool isGrounded
    {
        get { return gravityManager.isGrounded; }
        set { gravityManager.isGrounded = value; }
    }

    public float currentAnimationFrameRatio {
		get { return animationTime / animator.GetCurrentAnimatorStateInfo(0).length; }
	}
	public float animx {
		get { return (float)animator.GetFloat("animx"); }
		set { animator.SetFloat("animx", value); }
	}
	public float speedx {
		get { return (float)animator.GetFloat("speedx"); }
		set { animator.SetFloat("speedx", value); }
	}
	public float speedy {
		get { return (float)animator.GetFloat("speedy"); }
		set { animator.SetFloat("speedy", value); }
	}
    public bool isStunned
    {
        get { return stunTime != 0; }
    }
	public bool CURRENTATTACK {
		get { return currentAttack == null; }
	}
	public bool ACTION {
		get { return (action == null || actionIsCancellable()) && canStartNewAction(); }
	}
	public virtual void Awake() {
		animator = gameObject.addGetComponent<Animator>();
		hitBox = gameObject.addGetComponent<CapsuleCollider>();
		rb = gameObject.addGetComponent<Rigidbody>();
        gravityManager = gameObject.addGetComponent<Gravity>();
        Combatants.Add(this);
        id = Combatants.Count;

    }
    public virtual void Start()
    {
        enemies = new List<Combatant>(Combatants);
        enemies.Remove(this);
    }
	// Update is called once per frame
	virtual public void FixedUpdate() {
        if (stunTime > 0)
        {
            if((stunTime -= Time.deltaTime) <= 0)unstun();
        }
        animationTime += Time.deltaTime; //* animator.GetCurrentAnimatorStateInfo(0).speed;
		if (currentAnimationFrameRatio >= 1) {
            animationTime %= 1;
            onFinishedAnimation();
		}
	}
    virtual public void unstun()
    {
        stunTime = 0;
        if(action == "Stunned")finishAction();
    }
    virtual public void stun(float time)
    {
        stunTime = Math.Max(time, stunTime);
        startAction("Stunned");
    }
	virtual public Transform getSkeleton() {
		return skeleton;
	}
    virtual public bool canStartNewAction()
    {
        return true;
    }
	bool killDash;
	public IEnumerator dash( Vector3 v, params bool[] canBeAimed ) {
		yield return new WaitForEndOfFrame();
		string oldAction = action;
		Vector2 dashVector = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical")).normalized;
        bool canInflunce = canBeAimed.Length != 0;
		if (canBeAimed.Length != 0) dashVector = Vector2.up;
		else {
			speedx = dashVector.x;
			speedy = dashVector.y;
		}
		float force = 0;
		killDash = false;
		do {
            if (canInflunce)
            {
                transform.Rotate(0, Input.GetAxis("Horizontal") * 180 * Time.deltaTime, 0);
            }
			Vector3 dashDirection = transform.rotation * new Vector3(dashVector.x, 0, dashVector.y) * moveSpeed;
			force = Mathf.Pow(1 - currentAnimationFrameRatio, v.z) * Main.DASHINGFACTOR;
			force = Mathf.Max(force, 0);
            if (force < 0.01f) force = 0;
			rb.AddForce(dashDirection * Mathf.Lerp(v.x, v.y, force));
			//debug("Current Force : "+force.ToString());
			yield return new WaitForEndOfFrame();
			//Debug.Log(++i);
		} while (oldAction == action && force != 0 && !killDash);
        new Particles("Smoke", transform.position);
    }
    int lastKnockback;
    Quaternion knockBackTheta;

    public void reverseKnockBack()
    {
        knockBackTheta = Quaternion.Euler(0, 180, 0);
    }
    public IEnumerator knockback(Vector3 v,float dampRate,params bool[] noStun)
    {
        if(noStun.Length==0)startAction("Knockback");
        yield return new WaitForEndOfFrame();
        Vector3 dashVector = v.normalized + new Vector3(0,0.1f,0);
        float force = v.magnitude * Main.KNOCKBACKFACTOR;
        int oldKnockback = lastKnockback = Time.frameCount;
        knockBackTheta = Quaternion.identity;
        do
        {
            //Vector3 dashDirection = transform.rotation * new Vector3(dashVector.x, 0, dashVector.y) * moveSpeed;
            force *= dampRate;
            if (force <= 5000f) force = 0;
            //debug("force " + force);
            rb.AddForce(knockBackTheta * dashVector * force);
            yield return new WaitForEndOfFrame();
        } while (force != 0);
        if(action == "Knockback" && oldKnockback == lastKnockback)finishAction();
    }
    public void playAnimation( string s ) {
        //animator.CrossFade(s, 0.2f, -1, 0f);
        animator.Play(s, -1, 0f);
        animationTime = 0;
	}
	public void startAction( string s ) {
		action = s;
		animationTime = 0;
		killDash = true;
		if (s != null) playAnimation(s);
		else finishAction();
	}
    public bool canMove()
    {
        if (isStunned) return false;
        if (action == null) return true;
        return (new List<string>(){ "Jump", "Land", "Throw"}).Contains(action);
    }
	public void finishAction() {
		//Debug.Log("SHIFT GEAR");
		action = null;
		animx = 0;
		currentAttack = null;
		animator.Play("Idle", -1, 0f);
	}
	public virtual void onFinishedAnimation() {
        if (action == "Stunned" && isStunned) return;
		if (action != null) startAction(null);
	}
	public void startAttack( string name ) {
		if (currentAttack != null && currentAttack.name == name) {
			if(currentAttack.canChain())attackDataManager.startNextAttackInChain();
		} else attackDataManager.startAttack(name);
	}
	public virtual void debug( params object[] stuffs ) {
        string text = "";
        foreach (object s in stuffs)
        {
            text += s + "\n";
        }
        DebugText.text = text;
    }

	public virtual bool actionIsCancellable() {
		if (currentAttack != null && currentAttack.canBeCancelled()) return true;

		if (action == "Dash" && currentAnimationFrameRatio >= 0.8f) return true;
		return false;
	}
	public Vector2 getSpeedVector() {
		return new Vector2(speedx, speedy);
	}
    public virtual void startDash(Vector3 force, params bool[] canAim)
    {
        StartCoroutine(dash(force, canAim));
    }
    public virtual void startKnockback(Component from, Vector3 Force, float dampRate,params bool[] noStun)
    {
        transform.rotation = transform.getXZTheta(from.transform);
        StartCoroutine(knockback(Force, dampRate,noStun));
    }
}

public class PlayerInput
{
    public PlayerInput(Dictionary<string, string> inputToCheck)
    {
        this.inputToCheck = inputToCheck;
    }
    public PlayerInput(Dictionary<string, string> inputToCheck, int controllerNumber)
    {
        this.inputToCheck = inputToCheck;
        this.controllerNumber = controllerNumber;
    }
    int controllerNumber = 0;
    Dictionary<string, string> inputToCheck;
    List<string> keysDown = new List<string>();
    Dictionary<string, float> axisData = new Dictionary<string, float>();
    Dictionary<string, float> deadZones = new Dictionary<string, float>();
    string controllerPrefix
    {
        get { return controllerNumber != 0 ? "joystick " + controllerNumber + " " : ""; }
    }
    void setKeyDown(string key) { if (!keysDown.Contains(key)) keysDown.Add(key); }
    void setKeyUp(string key) { if (keysDown.Contains(key)) keysDown.Remove(key); }
    void setAxis(string key, float val)
    {
        if (!axisData.ContainsKey(key)) axisData.Add(key, val);
        else axisData[key] = val;
    }
    public void setDeadZones(Dictionary<string, float> deadZones)
    {
        this.deadZones = deadZones;
    }
    public bool GetButton(string key) { return keysDown.Contains(key); }
    public float GetAxis(string key) {
        float currentVal = axisData[key];
        if (deadZones.ContainsKey(key) && deadZones[key] > Math.Abs(currentVal)) return 0;
        return currentVal;
    }
    public void Update()
    {
        foreach (string input in inputToCheck.Keys)
        {
            if (!input.StartsWith("axis"))
            {
                if (UnityEngine.Input.GetKey(controllerPrefix + input)) setKeyDown(inputToCheck[input]);
                else setKeyUp(inputToCheck[input]);
            }
            else
            {
                setAxis(inputToCheck[input], UnityEngine.Input.GetAxisRaw(controllerPrefix + input));
            }
        }
    }
    public override string ToString()
    {
        string val = controllerPrefix+"\nKeys Down: \n";
        foreach (string s in keysDown) { val += s + ", "; }
        val += "\nAxises: \n";
        foreach (string s in axisData.Keys)
        {
            val += s + ":" + GetAxis(s) + ", ";
        }
        return val;
    }
}