using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    /// <summary>
    /// DESCRIPTION: this script handles the games active camera. Follows center point of players and offset, zooms in and out accordingly
    /// TO BE ADDED: Screen shake, using perlin noise
    /// </summary>

    [Header("Modifiable variables")]
    [Tooltip("Offset to be added from the center of the players")]
    public Vector3 offset;
    [Tooltip("Speed that camera will follow at.")]
    public float follow_Speed;
    [Tooltip("Should the camera be looking at the center of the players?")]
    public bool look_At_Center_Point;
    [Tooltip("Should the camera be moving relative to the player's center point?")]
    public bool follow_Players_Center;
    [Tooltip("Maximum Field of view camera will go to if players are far from each other in perspective mode.")]
    public float max_FOV;
    [Tooltip("Minimum Field of view camera will go to if players are close to each other in perspective mode.")]
    public float min_FOV;
    [Tooltip("Maximum Field of view camera will go to if players are far from each other in orth mode.")]
    public float max_Orth_FOV;
    [Tooltip("Minimum Field of view camera will go to if players are close to each other in orth mode.")]
    public float min_Orth_FOV;
    [Tooltip("Field of view adjustment speed.")]
    public float FOV_speed = 8;
    [Tooltip("Field of view limiter")]
    public float FOV_Limiter;

    [SerializeField]
    [Tooltip("Is the camera in perspective mode")]
    private bool m_Is_In_Perpective;

    [HideInInspector]
    //amount that timescale will lower to when called.
    public float slowedTime = 0.05f;
    //duration of the shake effect
    private float effectDuration = 0.5f;
    //original timescale of the game
    private float original_Time_Scale;

    //magnitude of camera shake
    private float strength;
    //current timer for camera shake
    private float m_Camera_Shake_Timer;
    //max timer for camera shake
    private float m_Camera_Shake_Max;

    //reference to center point of the level itself.
    private Vector3 level_Center_Point = Vector3.zero;
    //reference vector used for smooth damp
    private Vector3 vel = Vector3.zero;
    //vector 3 used to look at centerpoint
    private Vector3 center_Depth;
    //vector 3 for relative offset position
    private Vector3 target_Position;
    //reference to camera component
    private Camera myCam;

    [Header("Player and POE positions")]
    [Tooltip("Only add things necissary for position calculation")]
    public List<GameObject> points_Of_Interest_Pos;

    //Action Camera
    private float stregnth;

    //singleton for this camera to be referenced by all players upon certain events
    public static Camera_Controller cam_Inst;


    // Start is called before the first frame update
    void Start()
    {
        cam_Inst = this;
        original_Time_Scale = Time.timeScale;
        myCam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale < original_Time_Scale)
        {
            Time.timeScale += (1f / effectDuration) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        }
    }

    private void LateUpdate()
    {
        if (follow_Players_Center) {
            target_Position = new Vector3(Center_Origin().x + offset.x, Center_Origin().y + offset.y, Center_Origin().z + offset.z);
        }else
        {
            target_Position = new Vector3(offset.x, offset.y, offset.z);
        }

        Vector3 smooth_Out_Pos = Vector3.SmoothDamp(transform.position,
            target_Position, ref vel, Time.deltaTime * follow_Speed);

        if (look_At_Center_Point)
        {
            center_Depth = new Vector3(Center_Origin().x, Center_Origin().y, Center_Origin().z);

            transform.LookAt(center_Depth);
        }else
        {
            transform.LookAt(Vector3.zero);
        }

        
        transform.position = smooth_Out_Pos;
        FieldOfView();
    }


    public void Remove_POI(GameObject _caller)
    {
        for (int i = 0; i < points_Of_Interest_Pos.Count; i++)
        {
            if (points_Of_Interest_Pos[i].GetInstanceID() == _caller.GetInstanceID())
            {
                points_Of_Interest_Pos.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Calculate center point of all points of interest.
    /// </summary>
    /// <returns>Center point.</returns>
    Vector3 Center_Origin()
    {
        if (points_Of_Interest_Pos.Count == 1)
        {
            return points_Of_Interest_Pos[0].transform.position;
        }

        var bounds = new Bounds(points_Of_Interest_Pos[0].transform.position, Vector3.zero);
        for (int i = 0; i < points_Of_Interest_Pos.Count; i++)
        {
            bounds.Encapsulate(points_Of_Interest_Pos[i].transform.position);


        }

        return bounds.center;
    }

    //The following was made with the help of Brackeys youtube video here https://www.youtube.com/watch?v=aLpixrPvlB8.
    void FieldOfView()
    {
        if (m_Is_In_Perpective) {
            float newFOV = Mathf.Lerp(min_FOV, max_FOV, greatestDistance() / FOV_Limiter);
            myCam.fieldOfView = Mathf.Lerp(myCam.fieldOfView, newFOV, Time.deltaTime * FOV_speed);
        }else
        {
            float new_orth_FOV = Mathf.Lerp(min_Orth_FOV, max_Orth_FOV, greatestDistance() / FOV_Limiter);
            myCam.orthographicSize = Mathf.Lerp(myCam.orthographicSize, new_orth_FOV, Time.deltaTime * FOV_speed);
        }
    }

    /// <summary>
    /// Find greatest distance between furthest points of interest.
    /// </summary>
    /// <returns>Greatest X distance or Z distance (greater number).</returns>
    float greatestDistance()
    {


        var bounds = new Bounds(points_Of_Interest_Pos[0].transform.position, Vector3.zero);
        for (int i = 0; i < points_Of_Interest_Pos.Count; i++)
        {

            bounds.Encapsulate(points_Of_Interest_Pos[i].transform.position);

        }

        //Mathf.Max returns the larger number of the two, this way the FOV will update either way.
        return Mathf.Max(bounds.size.x, bounds.size.z);
    }

    /// <summary>
    /// Cause camera to shake.
    /// </summary>
    /// <param name="_Max_Time">Amount of time to shake.</param>
    /// <param name="_magnitude">Shake stregnth.</param>
    /// <param name="_camera_Shake_Type">0 = X,Y : 1 = X : 2 = Y</param>
    public void Do_Camera_Shake(float _Max_Time, float _magnitude, int _camera_Shake_Type)
    {
        stregnth = _magnitude;
        effectDuration = _Max_Time;
        Time.timeScale = slowedTime;
        StartCoroutine(CameraShake(_camera_Shake_Type));
    }

    IEnumerator CameraShake(int _Type)
    {
      
        while (Time.timeScale < 0.8f)
        {
            float Xposition = 0;
            float Yposition = 0;

            if (_Type == 0)
            {
                Xposition = Random.Range(-1f, 1f) * stregnth;
                Yposition = Random.Range(-1f, 1f) * stregnth;
            }
            else if (_Type == 1)
            {
                Xposition = Random.Range(-1f, 1f) * stregnth;
                Yposition = 0;
            }
            else if (_Type == 2)
            {
                Xposition = 0;
                Yposition = Random.Range(-1f, 1f) * stregnth;
            }

            transform.localPosition = new Vector3(Xposition + target_Position.x, Yposition + target_Position.y, target_Position.z);
            yield return null;
        }

        transform.position = target_Position;
        

    }
}
