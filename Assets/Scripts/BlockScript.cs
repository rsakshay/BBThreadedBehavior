﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Reference: http://blog.universityofgames.net/using-threads-with-unity/
public class BlockScript : MonoBehaviour {

    public float moveSpeed = 1f;

    Vector3 lastPos = Vector3.zero;
    bool goingLeft;
    bool isStopped = false;

    DateTime time;
    Thread thread;
    AutoResetEvent resetEvent;

	// Use this for initialization
	void Start () {
        lastPos = transform.position;

        thread = new Thread(Run);
        thread.Start();

        resetEvent = new AutoResetEvent(false);
	}
	
	// Update is called once per frame
	void Update () {
        resetEvent.Set();
        transform.position = lastPos;
	}

    void Run()
    {
        while(!isStopped)
        {
            time = DateTime.Now;
            MoveObject();
        }
    }

    void MoveObject()
    {
        resetEvent.WaitOne();

        DateTime now = DateTime.Now;

        TimeSpan deltaTime = now - time;
        time = now;

        lastPos += Vector3.right * (goingLeft ? -1f : 1f) * moveSpeed
            * (float)deltaTime.TotalSeconds;

        if ((goingLeft && lastPos.x < -3) || (!goingLeft && lastPos.x > 3))
            goingLeft = !goingLeft;
    }

    private void OnDestroy()
    {
        thread.Abort();
        isStopped = true;
    }

    private void OnApplicationQuit()
    {
        thread.Abort();
        isStopped = true;
    }
}
