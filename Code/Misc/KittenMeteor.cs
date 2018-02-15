using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class KittenMeteor : MonoBehaviour {

        const float speed = 22f;
        const float floor = -1.9f;
        public GameObject warning;
        public GameObject crate;
        GameObject targetLoc;
        Bounds bounds;
    bool dead = false;
	    // Use this for initialization
	    void Start () {
            targetLoc = GameObject.Instantiate(warning, transform.position.addToY(-transform.position.y+0.1f),Quaternion.identity);
        }
	
	    // Update is called once per frame
	void FixedUpdate () {
        if (dead || Main.paused) return;
        transform.addToY(-speed * Time.deltaTime);
        if (this.transform.position.y < floor) explode();
    }

    void explode()
    {
        dead = true;
        bounds = GetComponent<SphereCollider>().bounds;
        new Particles("Meteor", transform.position.addToY(-floor*2));
        foreach (Combatant target in Combatant.Combatants)
        {
            if (bounds.Intersects(target.hitBox.bounds))
            {
                target.transform.addToY(10);
                target.startKnockback(this, new Vector3(0, 1, 7) * 80000, 0.96f);
                target.stun(1f);
                ((Player)target).gravity = 150;
            }
        }
        for (int i = 0; i < Random.Range((int)0, 4); i++)
        {
            Crate c = (GameObject.Instantiate(crate, transform.position.addToY(-floor*2), Quaternion.identity)).GetComponent<Crate>();
            c.gravity.gravity = Random.Range(100,300);
            c.rb.AddForce(Quaternion.Euler(0, Random.Range(0, 360),0) * Vector3.forward * 1000000 * Random.Range(0.8f,2.5f));

        }

        GameObject.Destroy(this.targetLoc);
        GameObject.Destroy(this.gameObject);   
    }
}
