using UnityEngine;

/*
CODE WRITTEN BY LAUREN
*/
public class FixedCam : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // keep this offset please
    [SerializeField] private float smoothSpeed = 2f;
    
    [SerializeField, Range(0f, 90f)]
    private float pitchAngle = 30f; // angle to look down

    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        Vector3 lookAtPosition = target.position;
        
        //test start
        lookAtPosition.y += 1f;
        Quaternion rotation = Quaternion.Euler(pitchAngle, transform.eulerAngles.y, 0f);
        transform.rotation = rotation;
        //test end

        //lookAtPosition.y = transform.position.y;
        //transform.LookAt(lookAtPosition);
    }
}
