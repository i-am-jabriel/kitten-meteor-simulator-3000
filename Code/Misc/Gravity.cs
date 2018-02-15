using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Gravity : MonoBehaviour {
    float groundCheckDistance = 0.25f;
    float gravityRate = -225;
    [NonSerialized]
    public float gravity = 0;
    [NonSerialized]
    public bool isGrounded = true;
    [NonSerialized]
    public bool isActive = true;
    Vector3 groundNormal;
    Rigidbody rb;
    List<Action> landingHandlers = new List<Action>();
    // Use this for initialization
    void Start () {
        groundCheckDistance = Main.GROUNDCHECKDISTANCE;
        gravityRate = Main.GRAVITYRATE;
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
    public void addLandingHandler(Action a)
    {
        landingHandlers.Add(a);
    }
	void FixedUpdate () {
        if(isActive)checkGroundStatus();
	}
    public void checkGroundStatus()
    {
        RaycastHit hitInfo;
        Vector3 root = transform.position;
        //#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        //Debug.DrawLine(root + (Vector3.up * 0.1f), root + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
        //#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        int layerMask = ~(1 << 8);
        if (Physics.Raycast(root + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance, layerMask))
        {
            //print("here");
            groundNormal = hitInfo.normal;
            land();
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            gravity += gravityRate * Time.deltaTime;
            rb.velocity += Vector3.up * gravity;
        }
    }
    void land()
    {
        isGrounded = true;
        gravity = 0;
        rb.velocity = rb.velocity.addToY(-rb.velocity.y);
        //rb.velocity = rb.velocity * 0.5f;
        rb.angularVelocity = Vector3.zero;
        foreach (Action a in landingHandlers) a();
    }
}
