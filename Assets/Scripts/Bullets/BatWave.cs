using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatWave : Bullets
{
    // Start is called before the first frame update
    public float ScaleValue=0.1f;
    void Start()
    {

    }
    protected override void Awake()
    {
        base.Awake();
        transform.localScale = transform.localScale/2;

    }
    // Update is called once per frame
    protected override void Update()
    {

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        transform.localScale += Vector3.one * ScaleValue;
    }
    

}
