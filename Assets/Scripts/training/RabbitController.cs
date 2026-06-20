using UnityEngine;
using UnityEngine.AI;

public class RabbitController : MonoBehaviour
{
    [Header("Referências")]
    public Transform alice;   
    public Transform finalDoor; 

    [Header("Parâmetros")]
    public float speed = 3f;
    public float lookAheadDistance = 4f; 
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Running away from Alice
        Vector3 dirAwayFromAlice = (transform.position - alice.position).normalized;

        // Runnning towards the door
        Vector3 dirToDoor = (finalDoor.position - transform.position).normalized;

        // Combining the two directions to create a movement that is both away from Alice and towards the door
        Vector3 moveDirection = (dirAwayFromAlice + (dirToDoor * 2f)).normalized;

        // it's not a flying rabbit
        moveDirection.y = 0;

        agent.SetDestination(transform.position + moveDirection * lookAheadDistance);

    }
}