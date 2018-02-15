using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : Combatant {

    public bool mainCharacter = false;
    Vector2 cameraRotSpeed = new Vector2(360, 20);
    float cameraRotSpeedMovementPenalty = 0.4f;
    float crateMovementSpeedPenalty = 0.75f;
    float maxSpeed = 28;
    Vector2 cameraFOVRange = new Vector2(60, 90);
    Vector2 cameraRotRange = new Vector2(10, 20);
    float cameraFOVLerpSpeed = 1.3f;//2.25f;

    //current, max , min don't ask why
    Vector3 jumpFrames = new Vector3(0, 20 , 9);
    float jumpPower = 100 * 80;
    /*
	Factors
	 * x - min %
	 * y - max %
	 * z - how fast it damps
	 */
    Vector3 jumpDamping = new Vector3(0.00005f, 0.38f, 1.85f);

    
    Vector3 dashFactor = new Vector3(0.3f, 4f, 5.2f);

    Dictionary<string, Transform> taggedChildren = new Dictionary<string, Transform>();

    bool isGuarding;
    bool continuousJump;
    string currentAnimation = "Unsheathed";
    string style = "Standard";

    Camera mainCamera;

    float backpedalMoveSpeedPenalty = 0.85f;
    float guardMoveSpeedPenalty = 0.6f;

    [NonSerialized]
    public Crate activeCrate;

    // Use this for initialization
    public override void Awake() {

        base.Awake();
        gravityManager.addLandingHandler(land);
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        mainCamera = GetComponentInChildren<Camera>();
        attackDataManager = new StandardSwordAttackData(this);
        rb.mass *= 7.3f;
        rb.drag *= 3.8f;
        moveSpeed *= 83f;//60f;
        Input = new PlayerInput(new Dictionary<string, string>()
        {
            { "axis 1","Horizontal" },
            { "axis 2","Vertical"},
            { "axis 4", "RightX" },
            {"axis 5","RightY" },
            {"button 0", "Submit" },
            {"button 4", "Grab" },
            /*{"axis 10","Heavy Attack" },
            {"axis 9", "Guard" },*/
            {"axis 3", "Triggers" },
            {"button 5", "Light Attack" },
            {"button 3", "Interact" },
            {"button 7", "Start" }
        }, id);
        Input.setDeadZones(new Dictionary<string, float>()
        {
            {"Horizontal",0.25f },
            {"Vertical",0.25f },
            {"RightX",0.6f },
            {"RightY",0.6f },
        });

    }
    public override void Start()
    {
        base.Start();
        foreach (string tag in new List<string>() { "LHand", "RHand", "LFoot", "RFoot" })
        {
            taggedChildren.Add(tag, GameObject.FindGameObjectsWithTag(tag)[id-1].transform);
        }
    }

    // Update is called once per frame
    public override void FixedUpdate() {
        Input.Update();
        if (Main.gameState < Main.SANDBOX_MODE || Main.paused) return;
        base.FixedUpdate();
        if (!mainCharacter) return;
        checkInput();
        handleAnimation();
        moveCharacter();
        checkHitBoxes();
    }

    void checkInput() {
        //isGuarding = Input.GetAxis("Guard") != 0;
        isGuarding = Input.GetAxis("Triggers") > 0;
        bool heavyAttack = Input.GetAxis("Triggers") < 0;
        float targetFOV = isGuarding ? cameraFOVRange.x : cameraFOVRange.y;
        float targetRot = isGuarding ? cameraRotRange.x : cameraRotRange.y;
        float newTheta = Mathf.Lerp(transform.rotation.eulerAngles.x,targetRot,Time.deltaTime*cameraFOVLerpSpeed);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * cameraFOVLerpSpeed);
        mainCamera.transform.localRotation = Quaternion.Euler(newTheta, 0, 0);

        if (isStunned) return;

        if (action == null && activeCrate != null && (Input.GetButton("Interact") || Input.GetButton("Light Attack") || heavyAttack))
        {
            throwCrate();
            return;
        }
        if (Input.GetButton("Interact") && action == null && Crate.Interact(this)) return;
        if (ACTION && Input.GetButton("Submit") && isGuarding) {
            finishAction();
            startAction("Dash");
            StartCoroutine(dash(dashFactor));
        }

        if (ACTION && Input.GetButton("Light Attack"))
        {
            startAttack("Light Flurry");
            return;
        }

        if (ACTION && heavyAttack && isGuarding || Input.GetButton("Grab"))
        {
            startAttack("Grab");
            return;
        }

        if (ACTION && heavyAttack) {
            startAttack("Heavy");
        }

        if (action == "Jump" && heavyAttack)
        {
            startAttack("Jumping Heavy");
        }
        if (action == "Jump" && Input.GetButton("Light Attack"))
        {
            startAttack("Jumping Light");
        }

        if (ACTION && Input.GetButton("Submit") && isGrounded)
        {
            startJump();
        }
        else if (action == "Jump" && jumpFrames.x < jumpFrames.y)
        {
            if ((Input.GetButton("Submit") && continuousJump) || jumpFrames.x < jumpFrames.z) continueJump();
            if (!Input.GetButton("Submit")) continuousJump = false;
        }


    }

    void handleAnimation() {
        if (canMove()) {
            string oldAnimation = currentAnimation;
            Vector2 speed = new Vector2(-Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical"));
            //if (getSpeedVector().magnitude > speed.magnitude) speed = getSpeedVector();
            animator.SetFloat("speedx", speed.x, 0.05f, Time.deltaTime);
            animator.SetFloat("speedy", speed.y, 0.05f, Time.deltaTime);

            if (activeCrate != null) currentAnimation = "Overhead";
            else if (isGuarding) currentAnimation = style + " Guard";
            else currentAnimation = "Idle";

            if (oldAnimation != currentAnimation && action == null  ) {
                playAnimation(currentAnimation);
            }
        }
    }
    void startJump()
    {
        print(jumpFrames);
        startAction("Jump");
        jumpFrames.x = 0;
        continuousJump = true;
        continueJump();
    }

    void continueJump()
    {
        float lerp = Mathf.Lerp(jumpDamping.x, jumpDamping.y, jumpDamping.getRatio());
        rb.AddForce(Vector3.up * Mathf.Pow(jumpPower * lerp, jumpDamping.z));
        if (++jumpFrames.x >= jumpFrames.y) gravity = 0;
    }
    Transform getRoot()
    {
        return transform;
    }

    public void startSplitScreen(int i)
    {
        float x = i == 1 ? 0 : 0.5f;
        if (i == 0) mainCamera.clearFlags = CameraClearFlags.Nothing;
        mainCamera.rect = new Rect(x, 0, 0.5f, 1);
    }
    public void reset()
    {
        startAction(null);
        unstun();
        animator.SetFloat("speedx", 0);
        animator.SetFloat("speedy", 0);
    }
    void moveCharacter() {

        if (rb.velocity.magnitude < 100)
        {
            rb.velocity = Vector3.zero;
        }

        if (currentAttack != null || !canMove()) return;
        //Move Camera
        //if (!isGuarding) {
        float rightX = Input.GetAxis("RightX");
        Vector3 thetax = Time.deltaTime * new Vector3(0, rightX * cameraRotSpeed.x * ((Mathf.Pow(cameraRotSpeedMovementPenalty, rb.velocity.magnitude / maxSpeed))));
        Vector3 thetay = Time.deltaTime * new Vector3(Input.GetAxis("RightY") * cameraRotSpeed.y, 0);
        transform.Rotate(thetax);
        //mainCamera.transform.Rotate(thetay);
        //}

        float cp = activeCrate == null ? 1 : crateMovementSpeedPenalty;
        if (getSpeedVector().normalized.magnitude > 0.3f) {
            if (!isGuarding) {
                float bp = speedy >= 0 ? 1 : backpedalMoveSpeedPenalty;

                rb.AddForce(transform.rotation * Vector3.forward * speedy * moveSpeed * bp * cp);
                rb.AddForce(transform.rotation * Vector3.left * speedx * moveSpeed * backpedalMoveSpeedPenalty * cp);
            } else {
                rb.AddForce(transform.rotation * Vector3.forward * speedy * moveSpeed * guardMoveSpeedPenalty * cp);
                rb.AddForce(transform.rotation * Vector3.left * speedx * moveSpeed * guardMoveSpeedPenalty * cp);
            }
        }
    }
    void checkHitBoxes()
    {
        if (currentAttack == null) return;
        List<Combatant> targets = currentAttack.checkHitBoxes();
    }
	/*public override void debug(string txt ) {
		
	}*/
    public void land()
    {
        List<string> landing = new List<string> { "Land", "Fall", "Jump", "Jumping Heavy", "Jumping Light"};
        if (landing.Contains(action))
        {
            new Particles("Smoke", transform.position);
            if(jumpFrames.x>1)startAction("Land");
        }
        jumpFrames.x = 0;
        //transform.position.Set(transform.position.x, groundNormal.y - 1, transform.position.z);
    }
    public override void onFinishedAnimation()
    {
        if ((new List<string>() { "Knockback", "Stunned" }).Contains(action)) return;
        if ((new List<string>() { "Heavy Jumping Attack", "Light Jumping Attack" }).Contains(action) && !isGrounded) action = "Fall";
        if(action != "Jump" || isGrounded)base.onFinishedAnimation();

    }
    public void pickupCrate(Crate c)
    {
        activeCrate = c;
        startAction("Pickup");
        Vector3 euler = transform.rotation.eulerAngles;
        //c.transform.rotation = Quaternion.identity;
        c.transform.SetParent(taggedChildren["RHand"],false);
        c.transform.localPosition = Quaternion.Euler(0,0, 0) * new Vector3(-1,0,-1f);
        c.pickedUp();
    }
    public void throwCrate()
    {
        startAction("Throw");
        activeCrate.StartCoroutine(activeCrate.throwCrate(this));
    }
    public override bool canStartNewAction()
    {
        return base.canStartNewAction() && activeCrate == null;
    }
    bool canWallJump = false;
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.transform.tag)
        {
            case "wall":
                new Particles("Electric",transform.position + Vector3.up * 0.5f);
                if ((new List<string>() { "Knockback" }).Contains(action)) {
                    Vector3 d = rb.velocity;
                    Vector3 n = rb.velocity.normalized;

                    //rb.velocity = Vector3.Reflect(rb.velocity,Vector3.forward);
                    rb.velocity *= 0.75f;
                    gravity *= 0.75f;
                    reverseKnockBack();
                    stun(1.5f);
                    //transform.Rotate(0, 180, 0);
                }
                else canWallJump = true;
                break;
        }

    }

    //Animation Events I dont use but good to know
    void FootL() { }
    void FootR() { }
    void Land() { }
    void Hit() { }
    void Shoot() { }
}