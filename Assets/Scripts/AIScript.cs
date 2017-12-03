using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AIScript : MonoBehaviour {

    public float MAX_SPEED = 10;
    public float MAX_FORCE = 7;
    public float fleeThreshold = 2;
    public float arrivalThreshold = 8;
    public GameObject seekTarget = null;
    public GameObject fleeTarget = null;

    Rigidbody2D rgb;
    Vector2 acceleration = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    Vector2 steering_force = Vector2.zero;

    //DateTime time;
    //Thread timing;
    Thread arrival;
    Thread flee;
    //float deltaTime;
    //bool isDTdirty;
    bool isStopped;
    //AutoResetEvent resetEventForTime;
    AutoResetEvent resetEventForThreads;
    Vector2 currentPos;
    Vector2 arrivalTargetPos;
    Vector2 arrivalHeading;
    Vector2 fleeTargetPos;
    Vector2 fleeHeading;

    enum MovementState
    {
        Seek = 0,
        Flee
    }

    MovementState currentState = MovementState.Seek;

    //float DeltaTime 
    //{
    //    get 
    //    {
    //        if (isDTdirty)
    //        {
    //            resetEventForThreads.WaitOne();
    //        }

    //        isDTdirty = true;
    //        return deltaTime;
    //    }
    //}

	// Use this for initialization
	void Start () {

        rgb = GetComponent<Rigidbody2D>();

        //resetEventForTime = new AutoResetEvent(false);
        resetEventForThreads = new AutoResetEvent(false);

        //timing = new Thread(TimingRoutine);
        //timing.Start();

        arrival = new Thread(ArrivalRoutine);
        arrival.Start();

        flee = new Thread(FleeRoutine);
        flee.Start();
	}
	
	// Update is called once per frame
	void Update () {
        currentPos = transform.position;
        arrivalTargetPos = seekTarget.transform.position;
        fleeTargetPos = fleeTarget.transform.position;

        //resetEventForTime.Set();
        resetEventForThreads.Set();
        MoveAI();
	}

    /// <summary>
    /// Moves the actual rigidbody using the steering_force
    /// </summary>
    void MoveAI()
    {
        if ((fleeTargetPos - currentPos).magnitude < fleeThreshold)
        {
            currentState = MovementState.Flee;
            Vector2 awayVec = currentPos - fleeTargetPos;
            transform.up = awayVec.normalized;
            rgb.velocity *= 0.9f;
        }

        if ((arrivalTargetPos - currentPos).magnitude > arrivalThreshold)
        {
            currentState = MovementState.Seek;
            rgb.velocity *= 0.9f;
        }

        switch(currentState)
        {
            case MovementState.Seek:
                steering_force = arrivalHeading;
                break;

            case MovementState.Flee:
                steering_force = fleeHeading;
                break;
        }
        
        if (steering_force.magnitude > MAX_FORCE)
            steering_force = steering_force.normalized * MAX_FORCE;

        acceleration = steering_force / rgb.mass;

        velocity += acceleration;

        if (velocity.magnitude > MAX_SPEED)
            velocity = velocity.normalized * MAX_SPEED;

        rgb.velocity = velocity;
        transform.up = rgb.velocity.normalized;
    }

    //void TimingRoutine()
    //{
    //    while(!isStopped)
    //    {
    //        time = DateTime.Now;
    //        CalculateDeltaTime();
    //    }
    //}

    //void CalculateDeltaTime()
    //{
    //    resetEventForTime.WaitOne();

    //    DateTime now = DateTime.Now;

    //    TimeSpan dT = now - time;
    //    time = now;

    //    deltaTime = (float)dT.TotalSeconds;
    //    //isDTdirty = false;

    //    resetEventForThreads.Set();
    //}

    void ArrivalRoutine()
    {
        while(!isStopped)
        {
            CalculateArrivalHeading();
        }
    }

    void CalculateArrivalHeading()
    {
        resetEventForThreads.WaitOne();

        Vector2 target_offset = arrivalTargetPos - currentPos;
        float distance = target_offset.magnitude;
        float ramped_speed = MAX_SPEED * (distance / 1.5f);
        float clipped_speed = Mathf.Min(ramped_speed, MAX_SPEED);

        Vector2 desired_velocity = (clipped_speed / distance) * target_offset;

        Vector2 steer = desired_velocity - velocity;

        arrivalHeading = steer;
    }

    void FleeRoutine()
    {
        while (!isStopped)
        {
            CalculateFleeHeading();
        }
    }

    void CalculateFleeHeading()
    {
        resetEventForThreads.WaitOne();

        Vector2 awayVec = currentPos - fleeTargetPos;

        Vector2 desired_velocity = awayVec.normalized * MAX_SPEED;

        Vector2 steer = desired_velocity - velocity;

        fleeHeading = steer;
    }

    private void OnDestroy()
    {
        //timing.Abort();
        arrival.Abort();
        flee.Abort();
        isStopped = true;
    }

    private void OnApplicationQuit()
    {
        //timing.Abort();
        arrival.Abort();
        flee.Abort();
        isStopped = true;
    }
}
