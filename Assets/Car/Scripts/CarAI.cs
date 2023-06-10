using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    public Transform carBody;
    public Transform wheelsEntity;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public Terrain terrain;
    public List<GameObject> targetObjects = new List<GameObject>(); // List of target game objects

    private int currentTargetIndex = 0;
    private bool isCreatureDetected = false;

    private void FixedUpdate()
    {
        MoveToTarget();
        ApplyInverseKinematics();
    }

    private void MoveToTarget()
    {
        if (currentTargetIndex >= targetObjects.Count)
        {
            // All targets reached, stop moving
            return;
        }

        GameObject currentTarget = targetObjects[currentTargetIndex];
        Vector3 targetPosition = currentTarget.transform.position;
        Vector3 moveDirection = targetPosition - transform.position;
        moveDirection.y = 0f;

        if (moveDirection.magnitude < 0.1f)
        {
            // Reached current target, move to the next one
            currentTargetIndex++;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        carBody.rotation = Quaternion.Lerp(carBody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        if (!isCreatureDetected)
        {
            transform.position += moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;
        }
    }

    private void ApplyInverseKinematics()
    {
        Vector3 wheelPosition = wheelsEntity.position;
        Vector3 targetPosition = wheelPosition + (transform.forward * 100f); // Adjust the target position as needed

        // Raycast to the terrain to adjust the target position based on its height
        float terrainHeight = terrain.SampleHeight(targetPosition);
        targetPosition.y = terrainHeight + terrain.transform.position.y;

        // Calculate the rotation needed for the wheel entity to reach the target position
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - wheelPosition);
        wheelsEntity.rotation = Quaternion.Lerp(wheelsEntity.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Dino"))
    //     {
    //         // Object with the tag "creature" entered the trigger
    //         isCreatureDetected = true;
    //     }
    // }
    //
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Dino"))
    //     {
    //         // Object with the tag "creature" exited the trigger
    //         isCreatureDetected = false;
    //     }
    // }
}
