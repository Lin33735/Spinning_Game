using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager Instance;
    public Camera MainCamera;
    public Transform PlayerTarget,Cursor;
    public PlayerMovement Player;
    public Vector2 MousePosition;
    public List<float> CameraSize;
    public Vector2 LockPosition;
    public Slider Slider,PlayerHealthBar,ChargeBar;
    public Entity Boss;
    bool pausing;
    private void Awake()
    {
        Instance = this;
        MainCamera = Camera.main;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        Application.targetFrameRate = 60;
    }
    void Start()
    {


        CameraSize.Insert(0,MainCamera.orthographicSize);
        Slider.gameObject.SetActive(false);
        if (Player)
        {
            PlayerHealthBar.maxValue = Player.health;
            ChargeBar.maxValue = 5;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Getting Mouse Position
        Vector3 mouseScreenPosition = Input.mousePosition;
        MousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0));
        if (PlayerTarget)
            Cursor.position = PlayerTarget.position;
        else if (Player.Target)
            Cursor.position = Player.Target.position;
        else
            Cursor.position = MousePosition;
        if (!Player.Target&&Player.currentState!=PlayerMovement.State.Attacking && PlayerTarget && Input.GetKeyDown(KeyCode.Mouse0))
        {
            Player.Target = PlayerTarget;
            Player.ChangeState(PlayerMovement.State.Attacking);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
           
            Player.Target = null;
        }
        

    }
    private void FixedUpdate()
    {
        if (pausing||!Player)
        {
            return;
        }
        Vector2 camposition = Player.transform.position;
        if (Player)
        {

            if (Player.Target)
            {
                camposition += ((Vector2)Player.Target.transform.position - camposition) * 0.8f;
            }
        }
        MainCamera.orthographicSize += (CameraSize[0] - MainCamera.orthographicSize) * 0.1f;
        if (LockPosition != Vector2.zero)
        {
            camposition = LockPosition;
        }
        MainCamera.transform.position += (new Vector3(camposition.x, camposition.y, MainCamera.transform.position.z) - MainCamera.transform.position) * 0.1f;
        if (Boss)
        {
            Slider.value = Boss.health;
        }
        else
        {
            Slider.gameObject.SetActive(false);
        }
        if (Player)
        {
            PlayerHealthBar.value = Player.health;
            ChargeBar.value = Player.ChargeLevel;
        }
    }
    public void SetBoss(Entity boss)
    {
        Boss = boss;
        if(boss)
        Slider.maxValue = Boss.health;
        if (boss != null)
        {
            Slider.gameObject.SetActive(true);
        }
        else
        {
            Slider.gameObject.SetActive(false);
        }
    }
    public void PauseTime(float time, float charge)
    {
        if(time!=0)
        StartCoroutine(StopTime(time, charge));
    }
    IEnumerator StopTime(float time,float charge)
    {
        float count=0;
        Time.timeScale = 0f;
        pausing = true;

        while (count < time)
        {
            count += Time.unscaledDeltaTime*8;
            Vector2 camposition = (Vector2)Player.transform.position+(new Vector2(Random.Range(1,-1), Random.Range(1, -1))* charge * (1-count/ time));
            MainCamera.transform.position += (new Vector3(camposition.x, camposition.y, MainCamera.transform.position.z) - MainCamera.transform.position) * 0.3f;
            MainCamera.orthographicSize += (8 - MainCamera.orthographicSize) * 0.3f;
            yield return null;
        }
        Time.timeScale = 1f;
        pausing=false;
        yield return null;
    }
}
