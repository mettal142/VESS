using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class Herbivore_Agent : Agent
{
    public float HP;
    public Rigidbody rb;
    public GameObject Field;
    public float turnSpeed;
    public float moveSpeed;
    public GameObject target;
    public float distance = 0;
    public bool isSafe;
    public GameObject safeFieldObject;
    public float safeDistance;
    public override void Initialize()
    {
        this.transform.position = new Vector3(Field.transform.position.x + Random.Range(-47f, 47f), Field.transform.position.y + 1, Field.transform.position.z + Random.Range(-47f, 47f));
        HP = 100;
        rb = GetComponent<Rigidbody>();
        turnSpeed = 120f;
        moveSpeed = 3f;
        isSafe = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(HP);
        sensor.AddObservation(distance);
        sensor.AddObservation(isSafe);
        sensor.AddObservation(safeDistance);
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        AddReward(-0.0005f);
        MoveAgent(vectorAction);
        predation();
        safeField();
        if (Vector3.Distance(Field.transform.position, this.gameObject.transform.position) > 100)
        {
            AddReward(-1);
            EndEpisode();
        }
        if (!isSafe)
            HP -= 5 * Time.deltaTime;
        else
            HP -= 10 * Time.deltaTime;
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
        AddReward(-2);
        EndEpisode();
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
            case 1://감속
                dirToGo *= 0.5f;
                rb.velocity *= 0.75f;
                break;

        }
        rb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

    }

    public void MoveAgentContinuous(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        dirToGo = this.transform.forward * act[0];
        rotateDir = this.transform.up * act[1];

        if (act[0] >= 0)
            //rb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            transform.Translate(dirToGo * moveSpeed * Time.deltaTime);
        transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
    }

    public void predation()
    {
        if (target != null)
        {
            distance = Vector3.Distance(this.transform.position, target.gameObject.transform.position);
            if (distance <= 2)
            {
                target.gameObject.GetComponent<Plant>().HP -= (30 * Time.deltaTime);
                HP += (30 * Time.deltaTime);
                if(HP<=80)
                    AddReward(1f * Time.deltaTime);
            }
            else
            {
                target = null;
                distance = 0;
            }
        }
    }
    public void safeField()
    {
        if (safeFieldObject != null)
        {
            safeDistance = Vector3.Distance(this.gameObject.transform.position, safeFieldObject.transform.position);
            if (safeDistance <= 10)
            {
                if(HP<=50)
                    AddReward(-0.2f*Time.deltaTime);
                else
                    AddReward(0.5f * Time.deltaTime);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "wall")
        {
            AddReward(-2);
            EndEpisode();
        }
        if (!isSafe)
        {
            if (collision.gameObject.tag == "carnivore")
            {
                AddReward(-1.0f);
                EndEpisode();
            }
        }
        if (collision.gameObject.tag == "plant")
        {
            target = collision.gameObject;
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
            safeDistance = 0.0f;
        }
    }
}
