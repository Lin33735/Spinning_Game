using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Entity
{
    Vector2 PlayerInput, RawInput;
    Vector2 FaceDirection;
    public enum State { 
        Idle,
        Walking
    }
    public State currentState;
    public void FixedUpdate()
    {
        StateUpdate(currentState);
    }
    public void Update()
    {
        GetInput();
        FaceTo((Vector2)transform.position+ FaceDirection);
    }
    public void GetInput()
    {
        PlayerInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        RawInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (RawInput != Vector2.zero)
        {
            FaceDirection = RawInput.normalized;
            ChangeState(State.Walking);
        }
        else if (currentState == State.Walking)
        {
            ChangeState(State.Idle);
        }
    }
    public void ChangeState(State state)
    {
        if (currentState != state)
        {
            ExitState(currentState);
            currentState = state;
            EnterState(currentState);
        }
    }
    public void ExitState(State state)
    {
        if(state == State.Walking)
        {
            animator.SetBool("walking", false);
        }
    }
    public void EnterState(State state)
    {
        if (state == State.Walking)
        {
            animator.SetBool("walking", true);
        }
    }
    public void StateUpdate(State state)
    {
        if (state == State.Walking)
        {
            rb.velocity = FaceDirection * speed;
        }
    }
}
