using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardSwordAttackData : AttackDataManager {

	public StandardSwordAttackData( Combatant owner ) : base(owner) { }

	public override AttackData startAttack( string name ) {
		AttackData ad = base.startAttack(name);
		switch (name) {
            case "Heavy":
                owner.startAction("Uppercut");
                float startFrame = 0.3f;
                ad.addHitBox("RHand",3f);
                owner.startDash(new Vector3(0.6f, 0.001f, 1.2f), true);
                ad.addFrameData(new Vector2(startFrame+0.1f, 1),new Dictionary<string, object>()
                {
                    { "target","owner" },
                    { "knockback", new Vector3(0, 1.8f,-1.8f) },
                    { "dash", new Vector3(1.1f,0.0001f,1.2f) },
                    { "canAimDash", true }
                });
                ad.addFrameData(new Vector2(startFrame, 1), new Dictionary<string, object>()
                {
                    { "knockback", new Vector3(0,2.3f,5f) },
                    { "knockbackDampRate", 0.97f }
                });
                break;
            case "Grab":
                owner.startAction("Grab");
                ad.addHitBox("RHand",2f);
                ad.addHitBox("LHand", 2f);
                float startFrames = 0.3f;
                ad.addFrameData(new Vector2(startFrames, 0.7f), new Dictionary<string, object>()
                {
                    { "knockback", new Vector3(0,0.8f,6f) },
                });
                ad.addFrameData(new Vector2(0.3f, 1f), new Dictionary<string, object>()
                {
                    {"target","owner" },
                    {"dash", new Vector3(1.2f,0.001f,1.2f) }
                });
                ad.cancelTime = startFrames;
                owner.startDash(new Vector3(0.8f, 0.0001f, 4.5f), true);
                break;
            case "Jumping Light":
                owner.startAction("Light Jump Attack");
                ad.addHitBox("RFoot");
                ad.addHitBox("LFoot");
                ad.addFrameData(new Vector2(0.3f, 0.7f), new Dictionary<string, object>()
                {
                    {"knockback", new Vector3(0,0.1F,3f) }
                });
                owner.startDash(new Vector3(1.8f, 0.0001f, 1.2f), true);
                break;
            case "Jumping Heavy":
                owner.startAction("Heavy Jump Attack");
                ad.addHitBox("RFoot",4f);
                ad.addFrameData(new Vector2(0, 1.4f), new Dictionary<string, object>()
                {
                    {"knockback", new Vector3(0,1.5f,10) }
                });
                owner.StartCoroutine(owner.dash(new Vector3(0.6f, 2.2f, 2.2f), true));
                ((Player)owner).gravity += 50;
                break;
            case "Light Flurry":
                ad.cancelTime = 0.55f;
                owner.startAction(name);
                
                
                ad.maxChain = 4;
                ad.addFrameData(new Vector2(0.2f, 0.4f), new Dictionary<string, object>()
                {
                    {"knockback", new Vector3(0,0.1f,2) },
                    {"knockbackDampRate",0.9f }
                });
                switch ((int)owner.animx)
                {
                    case 0:
                        owner.startDash(new Vector3(0.1f, 0.4f, 1.2f),true);
                        ad.addHitBox("LHand", 1f);
                        break;
                    case 1:
                        owner.startDash(new Vector3(0.1f, 0.4f, 1.2f), true);
                        ad.addHitBox("RHand", 1f);
                        ad.cancelTime = 0.75f;
                        break;
                    case 2:
                        owner.startDash(new Vector3(0.1f, 0.4f, 1.2f), true);
                        ad.cancelTime = 0.7f;
                        ad.addHitBox("LHand", 1f);
                        break;
                    case 3:
                        owner.startDash(new Vector3(0.45f, 0.00001f, 10f), true);
                        ad.resetFrameData();
                        ad.cancelTime = 1;
                        ad.addHitBox("RHand", 1.5f);
                        ad.addFrameData(new Vector2(0.45f, 0.6f), new Dictionary<string, object>()
                        {
                            {"knockback", new Vector3(0,1,8) },
                            //{"stun", 3f }
                        });
                        break;
                }
                //Debug.Log(ad.frameData.Count);
                break;

                
			/*case "Heavy":
				ad.maxChain = 4;
				owner.startAction("Standard Sword Heavy");
				ad.addHitBox("Weapon");
                ad.resetFrameData();
                switch ((int)owner.animx) {
                    case 0:
                        ad.addFrameData(new Vector2(0.1f, 0.3f), new Dictionary<string, object>() {
                            { "knockback", new Vector3(0, 1f,10) }
                        });
                        ad.cancelTime = 0.45f;
						owner.StartCoroutine(owner.dash(new Vector3(0.5f, 5.5f, 16f), true));
						break;
					case 1:
                        ad.addFrameData(new Vector2(0.1f, 0.3f), new Dictionary<string, object>() {
                            { "knockback", new Vector3(0,0.3f,5) }
                        });
                        ad.cancelTime = 1.0f;
						owner.StartCoroutine(owner.dash(new Vector3(0.1f, 0.6f, 14f), true));
						break;
					case 2:
                        ad.addFrameData(new Vector2(0.3f, 0.5f), new Dictionary<string, object>() {
                            { "knockback", new Vector3(0,0.3f,5) }
                        });
                        ad.cancelTime = 0.8f;
						owner.StartCoroutine(owner.dash(new Vector3(0.1f, 0.5f, 10f), true));
						break;
					case 3:
                        ad.addFrameData(new Vector2(0.4f,2f), new Dictionary<string, object>() {
                            { "knockback", new Vector3(0,10000,10) }
                        });
                        owner.StartCoroutine(owner.dash(new Vector3(0.3f, 1.4f, 12f), true));
						break;
				}
				
				break;*/
		}
		//owner.action = name;
		return ad;
	}
}
