/*
 * Written by Rua M. Williams
 * This script causes a player to follow, by gaze and by distance, an object transform.
 * This comment is for showing how to commit to GIT
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPoint : MonoBehaviour
{
    public Transform followThis;            //this is the object controlled by gaze
    public Transform immediateFollow;       //this is an empty transform that we use to interpolate to in local space
    private Rigidbody rb;                   //the rigid body that is on the character, if any

    public float minDistance = 15f;         //minimum distance between player and surface
    public float maxAngle = 60f;            //max angle to turn towards (prevents fast turning)
    public float maxVertical = 10f;         //max up down angle for looking (prevents upward turn around)
    public float walkSpeed = 2f;            //walk speed multiplier
    public float turnSpeed = 1f;            //turn speed multiplier

    public bool isWalking = false;
    public bool isLooking = true;

    // Start is called before the first frame update
    void Start()
    {

        if (followThis == null)
        {
            Debug.Log("No follow object assinged");
        }

        rb = this.transform.parent.gameObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = this.GetComponent<Rigidbody>();
        if (rb == null)
            Debug.Log("No rigid body found");
    }

    // Update is called once per frame
    void Update()
    {

        if (followThis != null)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isWalking = !isWalking;

                if (!isWalking)
                {
                    if (rb != null)
                    {
                        rb.AddForce(this.gameObject.transform.forward * rb.velocity.magnitude *-5f);
                    }
                }
            }

            if (isLooking)
            {
                immediateFollow.LookAt(followThis, Vector3.up);     //give a rotation proxy with no z rotation

                float angleBetween = Quaternion.Angle(immediateFollow.rotation, this.transform.rotation);
                float distanceBetween = Vector3.Distance(followThis.position, this.transform.position);

                // Debug.Log("Angle between: " + angleBetween);
                // Debug.Log("Distance between: " + distanceBetween);

                float turnAmount = turnSpeed * Mathf.Min(angleBetween, maxAngle) / maxAngle;
                float walkAmount = walkSpeed * Mathf.Min(distanceBetween, minDistance) / minDistance;

                // Debug.Log("The dot is " + angleBetween + " angle away and we are turning toward it at " + turnAmount + ".");
                //turn camera towards goal
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, immediateFollow.rotation, turnAmount);
                //clamp camera x and y
                this.transform.rotation = Quaternion.Euler(ClampAngle(this.transform.rotation.eulerAngles.x, -maxVertical, maxVertical), this.transform.rotation.eulerAngles.y, 0f);

                if (isWalking)
                {
                    // Debug.Log("Looking to walk.");

                    if (Vector3.Distance(this.transform.position, followThis.position) > minDistance && rb != null)
                    {
                        if(rb.velocity.magnitude < walkSpeed)
                        {
                            rb.AddForce(Vector3.Min(this.gameObject.transform.forward * walkAmount, this.gameObject.transform.forward * walkSpeed / 2f));
                        }
                        // Debug.Log("The dot is " + distanceBetween + " far away and we are walking toward it at " + rb.velocity.magnitude + ".");

                    }
                    else
                    {
                        Debug.Log("The dot is too close or there is no rigid body");
                    }
                }
            }
        }
    }

    public static float ClampAngle(float current, float min, float max)
    {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float hdtAngle = dtAngle * 0.5f;
        float midAngle = min + hdtAngle;

        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }
}
