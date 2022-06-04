using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Carnivore_Agent : Agent
{
    public float HP;
    public Rigidbody rb;
    public GameObject Field;
    public float turnSpeed;
    public float moveSpeed;
    public bool isSafe;
    public GameObject safeFieldObject;
    public float safeDistance;

    public override void Initialize()
    {
        this.transform.position = new Vector3(Field.transform.position.x + Random.Range(-47f, 47f), Field.transform.position.y + 1, Field.transform.position.z + Random.Range(-47f, 47f));
        HP = 100;
        rb = GetComponent<Rigidbody>();
        turnSpeed = 120f;
        moveSpeed = 4f;
        isSafe = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(HP);
        sensor.AddObservation(isSafe);
        sensor.AddObservation(safeDistance);
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        AddReward(-0.0005f);

        MoveAgent(vectorAction);
        if (Vector3.Distance(Field.transform.position, this.gameObject.transform.position) > 100)
        {
            AddReward(-1);
            EndEpisode();
        }
        HP -= 5 * Time.deltaTime;
        if (HP <= 0)
        {
            HP = 0;
            death();
        }
        else if (HP >= 100)
        {
            HP = 100;
        }
        Monitor.Log("HP", HP * 0.01f, transform);
        Monitor.Log("reward", GetCumulativeReward() * 0.2f, transform);
    }

    public override void OnEpisodeBegin()
    {
        this.transform.position = new Vector3(Field.transform.position.x + Random.Range(-47f, 47f), Field.transform.position.y + 1, Field.transform.position.z + Random.Range(-47f, 47f));
        HP = 100;
        rb.velocity = Vector3.zero;
        Monitor.Log("HP", HP, transform);
        Monitor.Log("reward", GetCumulativeReward(), transform);
    }
    public void death()
    {
        AddReward(-1);
        EndEpisode();
    }
    public void safeField()
    {
        if (safeFieldObject != null)
        {
            safeDistance = Vector3.Distance(this.gameObject.transform.position, safeFieldObject.transform.position);
            if (safeDistance <= 10)
            {
                AddReward(-0.3f * Time.deltaTime);
                HP -= (10 * Time.deltaTime);
            }
        }
    }
    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = (int)act[0];
        var rightAxis = (int)act[1];
        var rotateAxis = (int)act[2];
        var MovetAxis = (int)act[3];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward;
                break;
            case 2:
                dirToGo = -transform.forward;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right;
                break;
            case 2:
                dirToGo = -transform.right;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = -transform.up;
                break;
            case 2:
                rotateDir = transform.up;
                break;
        }
        switch (MovetAxis)
        {
            case 1:
                dirToGo *= 0.5f;
                rb.velocity *= 0.75f;
                break;

        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
        rb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);

    }
    public void MoveAgentContinuous(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        dirToGo = this.transform.forward * act[0];
        rotateDir = this.transform.up * (act[1]);

        if (act[0] >= 0)
            //rb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            transform.Translate(dirToGo * moveSpeed * Time.deltaTime);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isSafe)
        {
            if (collision.gameObject.tag == "herbivore")
            {
                HP += 50;
                AddReward(1f);
            }
        }
        if (collision.gameObject.tag == "wall")
        {
            AddReward(-2);
            EndEpisode();
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("safefield"))
        {
            isSafe = true;
            safeFieldObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("safefield"))
        {
            isSafe = false;
            safeFieldObject = null;
        }
    }
}
