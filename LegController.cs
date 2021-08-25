using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public float tolerance = 0.05f;

    // angle determining what is considered walking "diagonal"
    public float sideWalkThreshold = 25f;

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
            // make sure that we are alternating steps
            if(LeftFootFurther()){
                leftTarget = LeftFootMapping();
                leftFootIK.UpdatePosition(leftTarget);
            }
        }

        if(!FootInsideBox(rightFoot) && leftFootIK.IsGrounded())
        {
            rightTarget = RightFootMapping();
            rightFootIK.UpdatePosition(rightTarget);
        }
    }

    // Update the corners of the box
    void UpdateCorners(){

        // get an un-rotated velocity vector
        Quaternion unRotate = Quaternion.Euler(0,-transform.rotation.eulerAngles.y,0);
        Vector3 _rotVel = Rotators.Rotated(_vel,unRotate,transform.up);

        // get width and length of the box
        float _x = (stanceWidth + (Mathf.Abs(_rotVel.x) * strafeScale / 2));
        float _z = (stanceLength + (Mathf.Abs(_rotVel.z) * strideScale / 2));

        _topLeft = -Vector3.right * _x + Vector3.forward * _z;
        _topRight = Vector3.right * _x + Vector3.forward * _z;
        _bottomLeft = -Vector3.right * _x - Vector3.forward * _z;
        _bottomRight = Vector3.right * _x - Vector3.forward * _z;

        _topLeft = Rotators.Rotated(_topLeft,transform.rotation,transform.up);
        _topRight = Rotators.Rotated(_topRight,transform.rotation,transform.up);
        _bottomLeft = Rotators.Rotated(_bottomLeft,transform.rotation,transform.up);
        _bottomRight = Rotators.Rotated(_bottomRight,transform.rotation,transform.up);

        _topLeft += _com;
        _topRight += _com;
        _bottomLeft += _com;
        _bottomRight += _com;
    }

    void OnDrawGizmos(){

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

        // get an unrotation Quaternion
        Quaternion unRotate = Quaternion.Euler(0,-transform.rotation.eulerAngles.y,0);

        Vector3 _tl = _topLeft - _com;
        Vector3 _tr = _topRight - _com;
        Vector3 _bl = _bottomLeft - _com;

        _tl = Rotators.Rotated(_tl,unRotate,transform.up);
        _tr = Rotators.Rotated(_tr,unRotate,transform.up);
        _bl = Rotators.Rotated(_bl,unRotate,transform.up);

        Vector3 _rotFoot = foot.transform.position - _com;
        _rotFoot = Rotators.Rotated(_rotFoot,unRotate,transform.up);

        float x = _rotFoot.x;
        float z = _rotFoot.z;

        if(x >= _tl.x - tolerance && x <= _tr.x + tolerance && z <= _tl.z + tolerance && z >= _bl.z - tolerance){
            return true;
        }
        return false;
    }
    
    // TODO: FIND A WAY TO NOT HARDCODE THESE
    // Tells us what the foot's destination should be
    Vector3 LeftFootMapping(){
        Vector3 footPos = leftFoot.transform.position;

        // get an unrotation Quaternion
        Quaternion unRotate = Quaternion.Euler(0,-transform.rotation.eulerAngles.y,0);
        Vector3 _rotVel = Rotators.Rotated(_vel,unRotate,transform.up);

        float thresh = _rotVel.magnitude * Mathf.Sin(Mathf.Deg2Rad * sideWalkThreshold);

        if(_rotVel.z >= 0){
            if(_rotVel.x > thresh){
                return _topRight;
            }
            if(_rotVel.x < -thresh){
                return _bottomLeft;
            }
            return _topLeft;
        }
        if(_rotVel.x > thresh){
            return _bottomRight;
        }
        return _bottomLeft;
    }

    Vector3 RightFootMapping(){
        Vector3 footPos = rightFoot.transform.position;

        // get an unrotation Quaternion
        Quaternion unRotate = Quaternion.Euler(0,-transform.rotation.eulerAngles.y,0);
        Vector3 _rotVel = Rotators.Rotated(_vel,unRotate,transform.up);

        float thresh = _vel.magnitude * Mathf.Sin(Mathf.Deg2Rad * sideWalkThreshold);

        if(_rotVel.z >= 0){
            if(_rotVel.x < -thresh){
                return _topLeft;
            }
            if(_rotVel.x > thresh){
                return _bottomRight;
            }
            return _topRight;
        }
        if(_rotVel.x < -thresh){
            return _bottomLeft;
        }
        return _bottomRight;
    }

    // just return the second-furthest possible foot position lol
    // Vector3 FootMapping(GameObject foot){
    //     Vector3 footPos = foot.transform.position;

    //     float tl = (footPos - _topLeft).magnitude;
    //     float tr = (footPos - _topRight).magnitude;
    //     float bl = (footPos - _bottomLeft).magnitude;
    //     float br = (footPos - _bottomRight).magnitude;

    //     List<Tuple<float,Vector3>> cList = new List<Tuple<float,Vector3>>();
    //     cList.Add(new Tuple<float,Vector3>(tl, _topLeft));
    //     cList.Add(new Tuple<float,Vector3>(tr, _topRight));
    //     cList.Add(new Tuple<float,Vector3>(bl, _bottomLeft));
    //     cList.Add(new Tuple<float,Vector3>(br, _bottomRight));

    //     cList.Sort((x,y) => y.Item1.CompareTo(x.Item1));

    //     return cList[1].Item2;

    // }
}
