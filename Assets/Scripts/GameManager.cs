using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera MainCamera;
    public Transform Player;
    void Start()
    {
        MainCamera = Camera.main;
        Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        MainCamera.transform.position = new Vector3(Player.position.x, Player.position.y,-6);
    }
}
