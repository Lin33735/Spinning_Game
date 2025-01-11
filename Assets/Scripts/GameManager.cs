using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameManager Instance;
    public Camera MainCamera;
    public Transform PlayerTarget,Cursor;
    public PlayerMovement Player;
    public Vector2 MousePosition;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        MainCamera = Camera.main;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
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

        Vector2 camposition = Player.transform.position;
        if (Player.Target)
        {
            camposition += ((Vector2)Player.Target.transform.position - camposition) * 0.8f;
        }
        MainCamera.transform.position += (new Vector3(camposition.x, camposition.y, MainCamera.transform.position.z) - MainCamera.transform.position) * 0.1f;

    }
}
