using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IKFootSolver : MonoBehaviour
{
    public LayerMask terrainLayer;

    float stepHeight = 0.2f;
    float stepSpeed = 3;

    private Vector3 target;

    private float lerp;
    private Vector3 currentPosition;
    Vector3 oldPos;

    private float bodySpeed;
    public bool grounded;

    void Start(){
        bodySpeed = 0;
        grounded = true;
        oldPos = transform.position;
    }

    void Update()
    {
        transform.position = currentPosition;

        if(lerp < 1){
            Vector3 footPos = Vector3.Slerp(oldPos,target,lerp);
            footPos.y = Mathf.Sin(lerp * Mathf.PI) * stepHeight;
            currentPosition = footPos;
            lerp += Time.deltaTime * (stepSpeed + (1 + bodySpeed));
            grounded = false;
        }else{
            currentPosition = target;
            grounded = true;
        }
    }

    public void UpdatePosition(Vector3 targetPosition){
        target = GetNewPosition(targetPosition);
        oldPos = transform.position;
    }

    Vector3 GetNewPosition(Vector3 target){
        lerp = 0;
        Ray ray = new Ray(target + Vector3.up * 5f,Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit info,10,terrainLayer.value))
        {
            return info.point;
        }
        return target;
    }

    public bool IsGrounded(){
        return grounded;
    }

    public void SetStepHeight(float sh){
        stepHeight = sh;
    }
    public void SetBaseSpeed(float sp){
        stepSpeed = sp;
    }
    public void SetSpeed(float sp){
        bodySpeed = sp;
    }

    void OnDrawGizmos(){
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(target,0.15f);

        if(grounded){
            Handles.Label(transform.position,"Grounded");
        }
    }

}
