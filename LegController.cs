using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{

    // Foot references
    public GameObject leftFoot;
    IKFootSolver leftFootIK;
    Vector3 leftTarget;
    public GameObject rightFoot;
    IKFootSolver rightFootIK;
    Vector3 rightTarget;

    // Box size at rest
    public float stanceWidth;
    public float stanceLength;

    // base walk speed
    public float stepSpeed;

    public float stepHeight;

    // Determines how the box is scaled with speed
    public float strideScale;
    public float strafeScale;

    // Determines how far out the Center of Mass is projected
    public float strafeSize;
    public float strideSize;

    public LayerMask terrainLayer;

    // for checking we are inside the box
    float _tolerance = 0.01f;

    Vector3 _vel = Vector3.zero;
    Vector3 _oldVel = Vector3.zero;
    Vector3 _oldPos;

    // Corners of our box
    Vector3 _topLeft;
    Vector3 _bottomLeft;
    Vector3 _topRight;
    Vector3 _bottomRight;

    // Velocity-adjusted center-of-mass
    Vector3 _com;

    // Start is called before the first frame update
    void Start()
    {
        _oldPos = transform.position;
        _com = GetCenterOfMass();
        UpdateCorners();
        
        leftTarget = LeftFootMapping();
        rightTarget = RightFootMapping();

        // Initialize IK variables
        leftFootIK = leftFoot.GetComponent<IKFootSolver>();
        rightFootIK = rightFoot.GetComponent<IKFootSolver>();
        leftFootIK.SetBaseSpeed(stepSpeed);
        rightFootIK.SetBaseSpeed(stepSpeed);
        leftFootIK.SetStepHeight(stepHeight);
        rightFootIK.SetStepHeight(stepHeight);
        leftFootIK.UpdatePosition(leftTarget);
        rightFootIK.UpdatePosition(rightTarget);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // get previous velocity
        _oldVel = _vel;

        // get Velocity
        _vel = transform.position - _oldPos;
        _vel /= Time.fixedDeltaTime;
        _oldPos = transform.position;

        // set foot animation speed
        leftFootIK.SetSpeed(_vel.magnitude);
        rightFootIK.SetSpeed(_vel.magnitude);

        // get center of mass
        _com = GetCenterOfMass();

        // update the box corners
        UpdateCorners();

        // Move the feet
        if(!FootInsideBox(leftFoot) && rightFootIK.IsGrounded())
        {
            if(LeftFootFurther()){
                leftTarget = LeftFootMapping();
                leftFootIK.UpdatePosition(leftTarget);
            }
        }

        if(!FootInsideBox(rightFoot) && leftFootIK.IsGrounded())
        {
            rightTarget = RightFootMapping();
            // move left foot
            rightFootIK.UpdatePosition(rightTarget);
        }
    }

    // Update the corners of the box
    void UpdateCorners(){
        float _x = (stanceWidth + (Mathf.Abs(_vel.x) * strafeScale / 2));
        float _z = (stanceLength + (Mathf.Abs(_vel.z) * strideScale / 2));
        _topLeft = _com - Vector3.right * _x + Vector3.forward * _z;
        _topRight = _com + Vector3.right * _x + Vector3.forward * _z;
        _bottomLeft = _com - Vector3.right * _x - Vector3.forward * _z;
        _bottomRight = _com + Vector3.right * _x - Vector3.forward * _z;
    }

    void OnDrawGizmos(){

        // Draw footbox
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(_com, new Vector3((stanceWidth * 2) + Mathf.Abs(_vel.x) * strafeScale, 0.1f, (stanceLength * 2) + Mathf.Abs(_vel.z) * strideScale));

        
        // Center of mass
        Gizmos.color = new Color(1, 0, 0, 1f);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position - Vector3.up);

        // Draw corners
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_topLeft,transform.position);
        Gizmos.DrawLine(_topRight,transform.position);
        Gizmos.DrawLine(_bottomRight,transform.position);
        Gizmos.DrawLine(_bottomLeft,transform.position);
    }

    // Get future Center of mass based on instantaneous velocity
    Vector3 GetCenterOfMass(){
        Ray ray = new Ray(transform.position + new Vector3(_vel.x * strafeSize,0,_vel.z * strideSize), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit info,10,terrainLayer.value))
        {
           return info.point;
        }
        return transform.position + _vel;
    }

    // The furthest foot is the one getting moved
    bool LeftFootFurther(){
        float _leftDist = (_com - leftFoot.transform.position).magnitude;
        float _rightDist = (_com - rightFoot.transform.position).magnitude;
        if(_leftDist > _rightDist){
            return true;
        }
        return false;
    }

    // Checks if a foot is inside of the box
    bool FootInsideBox(GameObject foot){
        float x = foot.transform.position.x;
        float z = foot.transform.position.z;
        if(x >= _topLeft.x - _tolerance && x <= _topRight.x + _tolerance && z <= _topLeft.z + _tolerance && z >= _bottomLeft.z - _tolerance){
            return true;
        }
        return false;
    }
    
    // TODO: FIND A WAY TO NOT HARDCODE THESE
    // Tells us what the foot's destination should be
    Vector3 LeftFootMapping(){
        Vector3 footPos = leftFoot.transform.position;

        if(_vel.z >= 0){
            if(_vel.x > 0){
                return _topRight;
            }
            return _topLeft;
        }
        if(_vel.x > 0){
            return _bottomRight;
        }
        return _bottomLeft;
    }

    Vector3 RightFootMapping(){
        Vector3 footPos = rightFoot.transform.position;
        if(_vel.z >= 0){
            if(_vel.x < 0){
                return _topLeft;
            }
            return _topRight;
        }
        if(_vel.x < 0){
            return _bottomLeft;
        }
        return _bottomRight;
    }
}
