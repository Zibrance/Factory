using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class Conveyor : MonoBehaviour
{
    public GameObject trackItem,start,end;
    [Range(.1f,1f)]
    public float spacing =.5f;
    
    [Range(.5f,10f)]
    public float speed = 1f;
    
    Vector3 startPos,endPos;
    float setSpacing ,setSpeed;
    BoxCollider boxCollider ; 
    SplineContainer path;
    Rigidbody rb ;
    SplineAnimate[] tracks = {};
    
    void Start()
    {
        Vector3[] knotPos = { 
            new Vector3 { x = 0,    y = 1,     z = -.5f }, 
            new Vector3 { x = 0,    y = 1,     z = .5f }, 
            new Vector3 { x = 0,    y = .5f,   z = 1 }, 
            new Vector3 { x = 0,    y = 0,     z = .5f }, 
            new Vector3 { x = 0,    y = 0,     z = -.5f }, 
            new Vector3 { x = 0,    y = .5f,   z = -1},
        };

        Vector3[] knotTan = { 
            new Vector3 { x = 0, y = 0, z = .1f}, 
            new Vector3 { x = 0, y = 0, z = .1f}, 
            new Vector3 { x = 0, y = 0, z = .5f}, 
            new Vector3 { x = 0, y = 0, z = .1f}, 
            new Vector3 { x = 0, y = 0, z = .1f}, 
            new Vector3 { x = 0, y = 0, z = .5f}, 
        };

        Quaternion[] knotRot = { 
            Quaternion.Euler(0,0,0), 
            Quaternion.Euler(0,0,0), 
            Quaternion.Euler(90,0,0), 
            Quaternion.Euler(0,180,180), 
            Quaternion.Euler(0,180,180), 
            Quaternion.Euler(270,0,0), 
        };

        boxCollider = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
        boxCollider.center = new Vector3(0, .5f,0);

        rb = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
        rb.isKinematic = true;

        path = gameObject.AddComponent(typeof(SplineContainer)) as SplineContainer;
        for (int i = 0; i < knotPos.Length; i++)
        {
            var knot = new BezierKnot(knotPos[i],knotTan[i],knotTan[i],knotRot[i]);
            path.Spline.Add(knot,TangentMode.Mirrored);
        }           

        path.Spline.Closed = true;        
        Resize();
    }

    void Resize(){
        for (int i = 0; i < tracks.Length; i++)
        {
            Destroy(tracks[i].gameObject);
        }

        startPos = start.transform.position;
        endPos =  end.transform.position;
        setSpacing = spacing;
        setSpeed = speed;

        var pathVector = endPos - startPos;        
        transform.LookAt(endPos);
        transform.position = start.transform.position + (pathVector / 2);        
        var length =  Mathf.RoundToInt(pathVector.magnitude);

        var knots = path.Spline.Knots.ToArray();
        knots[0].Position.z = ((length /2f ) -.5f) *-1f ;
        knots[1].Position.z = (length /2f ) -.5f ;
        knots[2].Position.z = length /2f   ;
        knots[3].Position.z = (length /2f ) -.5f ;
        knots[4].Position.z = ((length /2f ) -.5f) *-1f ;
        knots[5].Position.z = length /2f * -1f;

        for (int i = 0; i < knots.Length; i++)
        {
            path.Spline.SetKnot(i,knots[i]);
        }
        
        boxCollider.size = new Vector3(1, 1, length);

        var trackCount = Mathf.RoundToInt(path.Spline.GetLength()/setSpacing);
        
        tracks = new SplineAnimate[trackCount];
        
        float offsetIncreament = 1f / trackCount;
        
        for (int i = 0; i < trackCount; i++)
        {
            var newTrack = Instantiate(trackItem, new Vector3(0, 0, 0), Quaternion.identity);        
            newTrack.transform.parent = transform;
            tracks[i] = newTrack.AddComponent(typeof(SplineAnimate)) as SplineAnimate;
            tracks[i].Container = path;
            tracks[i].StartOffset = offsetIncreament * i;
            tracks[i].AnimationMethod = SplineAnimate.Method.Speed;
            tracks[i].MaxSpeed = speed;
        }
    }

    void Update()
    {
        if (startPos != start.transform.position || endPos != end.transform.position || setSpacing != spacing || setSpeed != speed){
            Resize();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var rb = collision.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        collision.gameObject.transform.LookAt(collision.gameObject.transform.position + transform.forward);
        rb.isKinematic = false;
        rb.velocity = transform.forward * setSpeed;
    }
    void OnCollisionExit(Collision collision)
    {
        var rb = collision.gameObject.GetComponent<Rigidbody>();
        rb.useGravity = true;
    }
}