using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject   projectilePrefab;
    public GameObject   heavyProjectilePrefab;
    private LineRenderer lineRenderer;
    public GameObject   projLinePrefab;

    [SerializeField] private AudioClip whipCrack;
    private AudioSource audioSource;

    [Header("Dynamic")]
    public GameObject   launchPoint;
    public float        velocityMult;
    public Vector3      launchPos;
    public GameObject   projectile;
    public bool         aimingMode;
    // Start is called before the first frame update
    void Awake() 
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        velocityMult = 10f;

        // audio source
        audioSource = GetComponent<AudioSource>();

        // linerenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f; 
        lineRenderer.endWidth = 0.1f;   
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red; 
        lineRenderer.endColor = Color.red;   
        lineRenderer.positionCount = 2;       
    }

    void OnMouseEnter()
    {
        // print("Slingshot: OnMouseEnter()");
        if (MissionDemolition.remainingLives > 0) {
        launchPoint.SetActive(true);
        }
    }

    // Update is called once per frame
    void OnMouseExit()
    {
        // print("Slingshot: OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        if (Random.value <= .2) {
            velocityMult = 8f;
            projectile = Instantiate(heavyProjectilePrefab) as GameObject;
        } else {
            velocityMult = 10f;
            projectile = Instantiate(projectilePrefab) as GameObject;
        }
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update()
    {
        if (!aimingMode) {
            lineRenderer.enabled = true; // re-enable line after fired for next shot
            return;
        }
        // get current mousepos
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        

        // delta of launchPos to mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;

        //normalize if larger than radius
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude) {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        //move projectile to new pos
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // Update LineRenderer positions
        lineRenderer.SetPosition(0, launchPos);
        lineRenderer.SetPosition(1, projPos);

        if (Input.GetMouseButtonUp(0)) {
            audioSource.PlayOneShot(whipCrack);
            
            lineRenderer.SetPosition(0, new Vector3(0,0,0));
            lineRenderer.SetPosition(1, new Vector3(0,0,0));


            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic=false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);

            FollowCam.POI = projectile; // camera follow
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;

            lineRenderer.enabled = false; // Hide line after launch

            MissionDemolition.SHOT_FIRED();
        }
    }
}
