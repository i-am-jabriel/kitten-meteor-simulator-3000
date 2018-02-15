using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectroWall : MonoBehaviour {

    public Material leftWallMaterial;
    public Material rightWallMaterial;
    GameObject[] walls;
    bool wallsUp = true;
    float wallHeight = 100;
	// Use this for initialization
	void Start () {
        wallHeight = Main.WALLHEIGHT;
        int i = 0;
        foreach (Transform t in transform)
        {
            t.localScale = new Vector3(Main.MAP_SIZE.x, 0.1f, wallHeight);
            float getSign = (i++) % 2 == 0 ? -0.5f : 0.5f;
            if (i < 3)
            {
                t.position = new Vector3(0, wallHeight / 2, getSign * Main.MAP_SIZE.x);
                t.localRotation = Quaternion.Euler(90, 0, 0);
            }
            else
            {
                t.position = new Vector3(getSign * Main.MAP_SIZE.x, wallHeight / 2, 0);
                t.localRotation = Quaternion.Euler(90, 90, 0);
            }
        }

    }
	// Update is called once per frame
	void FixedUpdate () {
        bool anyStun = false;
		foreach(Combatant target in Combatant.Combatants) { anyStun = anyStun || target.isStunned; }
        if (anyStun) dropWalls();
        else raiseWalls();
        if (wallsUp) rotateWalls();
    }

    void dropWalls()
    {
        if (!wallsUp) return;
        wallsUp = !wallsUp;
        
        foreach(Transform t in transform)
        {
            StartCoroutine(WallAnimation(t.gameObject,true));
        }
    }

    void raiseWalls()
    {
        if (wallsUp) return;
        wallsUp = !wallsUp;
        foreach (Transform t in transform)
        {
            StartCoroutine(WallAnimation(t.gameObject));
        }
    }

    IEnumerator WallAnimation(GameObject go,params bool[] dropping)
    {
        bool dropMode = dropping.Length != 0;
        float scale = dropMode ? Main.WALLHEIGHT : 0;
        float deltaTime = Time.deltaTime * (dropMode ? 1 : -1);
        Vector3 s = go.transform.localScale;
        Vector3 p = go.transform.localPosition;
        do
        {
            go.transform.localScale = new Vector3(s.x, s.y, scale -= deltaTime * Main.WALLDROPSPEED);
            go.transform.localPosition = new Vector3(p.x, scale * 0.5f, p.z);
            yield return new WaitForEndOfFrame();
        }
        while (scale.between(0, Main.WALLHEIGHT));

    }

    float offset = 0;
    Vector2 wallSpeed = new Vector2(5,8);
    void rotateWalls()
    {
        int i = 0;
        offset += Time.deltaTime * wallSpeed.x;
        if (offset > wallSpeed.y) offset %= wallSpeed.y;
        leftWallMaterial.mainTextureOffset = Vector2.left * offset;
        rightWallMaterial.mainTextureOffset = Vector2.right * offset;
    }
}
