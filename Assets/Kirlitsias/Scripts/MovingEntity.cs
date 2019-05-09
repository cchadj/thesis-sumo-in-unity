using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEntity : MonoBehaviour {

    private Vector3 velocity=new Vector3(5,0,0);

    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }

        set
        {
            velocity = value;
        }
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        //transform.Translate(velocity*Time.fixedDeltaTime);

        //if(Input.GetKey(KeyCode.Space)){

        //    velocity = new Vector3(20, 0, 0);
        //}
        //else velocity = new Vector3(5, 0, 0);
    }



    public float Gap(Vector3 otherPosition)
    {
        return (otherPosition - transform.position).magnitude / Velocity.magnitude;
    }
}
