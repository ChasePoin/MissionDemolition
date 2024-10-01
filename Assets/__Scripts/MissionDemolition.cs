using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameMode {
    idle,
    playing,
    levelEnd
}

public class MissionDemolition : MonoBehaviour
{
    static private MissionDemolition S;
    [Header("Inscribed")]
    public TMP_Text     uitLevel; // level text
    public TMP_Text     uitShots; // shots text
    public TMP_Text     uitLives; // lives text
    public TMP_Text     uitScore; // score text
    public Vector3      castlePos; // where to put the castle
    public GameObject[] castles;
    public int          livesCount = 5;

    [Header("Dynamic")]
    public int          level;
    public int          levelMax;
    public static int   remainingLives;
    public int          shotsTaken;
    public int          shotsTakenTracker; // hidden counter to prevent spam
    public int          score;
    public GameObject   castle;
    public GameMode     mode = GameMode.idle;
    public string       showing = "Show Slingshot";
    // Start is called before the first frame update
    void Start()
    {
        S = this;
        level = 0;
        shotsTaken = 0;
        shotsTakenTracker = 0;
        score = 0;
        levelMax = castles.Length;
        StartLevel();
    }
    void StartLevel() {
        if (castle != null) {
            Destroy(castle);
        }
        if (remainingLives > 0) {
        score = score + (remainingLives * 100 );
        }
        shotsTakenTracker = 0;
        remainingLives = livesCount;
        Projectile.DESTROY_PROJECTILES();

        // new castle
        castle = Instantiate<GameObject>(castles[level]);
        castle.transform.position = castlePos;

        Goal.goalMet = false;

        UpdateGUI();

        mode = GameMode.playing;

        FollowCam.SWITCH_VIEW(FollowCam.eView.both);
    }

    void UpdateGUI() {
        // show data in gui texts
        uitLevel.text = "Level: " +(level+1)+" of "+levelMax;
        uitShots.text = "Shots Taken: "+shotsTaken;
        uitLives.text = "Remaining Lives: " + (remainingLives-1);
        uitScore.text = "Score: " + score;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGUI();
        // check for game over
        if (remainingLives == 0 && Goal.goalMet == false) {
        
            SceneManager.LoadScene("EvilGameOver");
        }

        if (shotsTakenTracker > livesCount) {
            SceneManager.LoadScene("EvilGameOver");
        }

        // check for level end
        if ((mode == GameMode.playing) && Goal.goalMet) {
            mode = GameMode.levelEnd;

            // show both at the start
            FollowCam.SWITCH_VIEW(FollowCam.eView.both);

            Invoke("NextLevel", 2f);
        }
    }
    void NextLevel() {
        level++;
        if (level == levelMax) {
            level = 0;
            shotsTaken = 0;
            SceneManager.LoadScene("GameOver");
        }
        StartLevel();
    }
    static public void SHOT_FIRED() {
        S.shotsTaken++;
        S.shotsTakenTracker++;
        if (remainingLives != 1) {
            remainingLives--;
        }
    }
    static public GameObject GET_CASTLE() {
        return S.castle;
    }
}
