using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour {
    public static List<Crate> Crates = new List<Crate>();

    BoxCollider hitBox;
    public Gravity gravity;
    public Rigidbody rb;
    const float interactRange = 5;
	// Use this for initialization
	void Awake () {
        Crates.Add(this);
        gravity = GetComponent<Gravity>();
        rb = GetComponent<Rigidbody>();
        hitBox = GetComponent<BoxCollider>();
	}
    public static bool Interact(Player source)
    {
        foreach(Crate crate in Crates)
        {
            if (source.getDistance(crate) <= interactRange)
            {
                source.pickupCrate(crate);
                return true;
            }
        }
        return false;
    }
    public void OnTriggerEnter (Collider collider)
    {
        Combatant target = collider.gameObject.GetComponent<Combatant>();
        if (target == thrower || !target || !thrower) return;
        target.gameObject.transform.addToY(10);
        target.startKnockback(this, new Vector3(0, 1, 65) * 10000, 0.975f);
        target.stun(1);
        new Particles("Crate Explosion", target.transform.position);
        ((Player)target).gravity = 30;
    }
    public void pickedUp()
    {
        hitBox.isTrigger = true;
        gravity.isActive = false;
        rb.isKinematic = true;
    }
    Combatant thrower;
    public void thrown(Player source)
    {
        if (!hitBox.isTrigger) return;
        thrower = source;
        gravity.isActive = true;
        source.activeCrate = null;
        hitBox.isTrigger = false;
        rb.isKinematic = false;
        transform.localScale = Vector3.one * 1.5f;
    }
    public IEnumerator throwCrate(Player source)
    {
        yield return new WaitForEndOfFrame();
        while (source.currentAnimationFrameRatio < 0.55f)
        {
            yield return new WaitForEndOfFrame();
        }
        Vector3 throwVector = new Vector3(0, 3, 220) * 10000;
        Quaternion throwRot = Quaternion.Euler(0, transform.root.rotation.eulerAngles.y, 0);
        transform.SetParent(transform.root.parent);
        Vector3 dashVector = throwVector.normalized;
        float dampRate = 0.99f;
        gravity.gravity = -20;
        float force = throwVector.magnitude;
        int frames = 0;
        //rb.isKinematic = false;
        yield return new WaitForEndOfFrame();
        do
        {
            force *= dampRate;
            dampRate -= frames * 0.00002f;
            if (++frames > 5)
            {
                thrown(source);
                if (force <= 10000f) force = 0;
                rb.AddForce(throwRot * dashVector * force);
            }
            else transform.position += (throwRot * Vector3.forward * 5f + Vector3.up * 1f) * Time.deltaTime * 15f;
            //for(int i=0;i<2;i++)
            yield return new WaitForEndOfFrame();
        } while (force != 0);
    }
}
