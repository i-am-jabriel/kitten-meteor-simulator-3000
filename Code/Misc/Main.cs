using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    // Use this for initialization
    Vector2 kittenTimer = new Vector2(0, 5f);
    public Vector2 mapSize;
    public float wallHeight;
    public static Main me;
    public float groundCheckDistance;
    public float gravityRate;

    const float kittenOffset = 15f;
    const float kittenHeight = 45f;
    public bool debug;
    public GameObject kittenMeteor;

    Player player1;
    Player player2;

    public static bool paused = false;

    public GameObject startScreen;
    public const int TITLE_SCREEN = 0;
    public const int SANDBOX_MODE = 1;
    public const int SPLIT_SCREEN = 2;
    public static int gameState = TITLE_SCREEN;


    public float wallDropSpeed;

    public float dashingFactor;
    public float knockBackFactor;

    public static float WALLDROPSPEED 
    {
        get { return me.wallDropSpeed; }
    }
    public static float KNOCKBACKFACTOR
    {
        get { return me.knockBackFactor; }
    }
    public static float DASHINGFACTOR
    {
        get { return me.dashingFactor; }
    }

    public static Terrain terrain;
    public static Vector2 MAP_SIZE
    {
        get { return me.mapSize; }
    }
    public static float WALLHEIGHT 
    {
        get { return me.wallHeight; }
    }
    public static float GROUNDCHECKDISTANCE
    {
        get { return me.groundCheckDistance; }
    }
    public static float GRAVITYRATE
    {
        get { return me.gravityRate; }
    }
	void Awake () {
        me = this;
        if(!debug)startScreen.SetActive(true);
        kittenTimer.x = kittenTimer.y;
        terrain = GameObject.FindGameObjectsWithTag("Terrain")[0].GetComponent<Terrain>();
        terrain.terrainData.size = new Vector3(MAP_SIZE.x, 600, MAP_SIZE.y);
        terrain.transform.position = new Vector3(-MAP_SIZE.x / 2, 0, -MAP_SIZE.y / 2);
        player1 = (Player)Combatant.Combatants[0];
        player2 = (Player)Combatant.Combatants[1];
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (gameState)
        {
            case TITLE_SCREEN:
                if (player1.Input.GetButton("Start") || debug)
                {
                    gameState = SANDBOX_MODE;
                    startScreen.SetActive(false);
                    player2.GetComponentInChildren<Camera>().enabled = false;
                    player2.GetComponent<CapsuleCollider>().enabled = false;
                    player2.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                }
                break;
            case SANDBOX_MODE:
                if (player2.Input.GetButton("Start")||debug)
                {
                    gameState = SPLIT_SCREEN;
                    player2.GetComponentInChildren<Camera>().enabled = true;
                    player2.GetComponent<CapsuleCollider>().enabled = true;
                    player2.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                    player1.startSplitScreen(1);
                    player2.startSplitScreen(2);
                    resetPlayers(true);
                }
                break;
            case SPLIT_SCREEN:
                if (!paused && (kittenTimer.x -= Time.deltaTime) <= 0)
                {
                    fireNewKitten();
                }

                if(player1.transform.position.y < -10)
                {
                    player1.debug("Player 1 Wins!");
                    resetPlayers();
                }
                if (player2.transform.position.y < -10)
                {
                    player1.debug("Player 2 Wins!");
                    resetPlayers();
                }
                break;
        }

        

        
	}
    void resetPlayers(params bool[] first)
    {
        paused = true;
        player1.reset();
        player2.reset();
        player1.transform.position = new Vector3(-mapSize.x * 0.125f, 0, -mapSize.y * 0.125f);
        player2.transform.position = new Vector3(mapSize.x * 0.125f, 0, mapSize.y * 0.125f);
        player1.transform.LookAt(player2.transform);
        player2.transform.LookAt(player1.transform);
        StartCoroutine(StartCountdown(first));
    }
    IEnumerator StartCountdown(params bool[] first)
    {
        if (first.Length == 0) yield return new WaitForSeconds(2);
        for (int i = 3; i > 0; i--)
        {
            player1.debug(i);
            yield return new WaitForSeconds(1);
        }
        player1.debug("Go");
        paused = false;
        yield return new WaitForSeconds(1);
        player1.debug("");
    }
    void fireNewKitten()
    {
        kittenTimer.x = kittenTimer.y;
        GameObject.Instantiate(kittenMeteor, findRandomStartPosition(), Quaternion.identity);
    }
    Vector3 findRandomStartPosition()
    {
        float halfX = mapSize.x / 2;
        float halfZ = mapSize.y / 2;
        return new Vector3(Random.Range(-halfX + kittenOffset, halfX - kittenOffset), kittenHeight, Random.Range(-halfZ + kittenOffset, halfZ - kittenOffset));
    }
}
