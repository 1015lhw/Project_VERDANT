using UnityEngine;

public class RepTester : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T pressed");
            ReputationManager.Instance.AddAffection(NpcId.A, 5);
        }
        //1: Marcus+5
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ReputationManager.Instance.AddAffection(NpcId.A, 5, "Tester Key 1");
            Debug.Log("[Reputation] Marcus+5");
        }

        //2: Marcus-5
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ReputationManager.Instance.AddAffection(NpcId.A, -5, "Tester Key 2");
            Debug.Log("[Reputation] Marcus-5");
        }


        //3£ºSierra+5
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ReputationManager.Instance.AddAffection(NpcId.B, 5, "Tester Key 3");
            Debug.Log("[Reputation] Sierra+5");
        }

        //4£ºSierra-5
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ReputationManager.Instance.AddAffection(NpcId.B, -5, "Tester Key 4");
            Debug.Log("[Reputation] Sierra-5");
        }

        //5£ºPrint current value
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log($"Marcus={ReputationManager.Instance.GetAffection(NpcId.A)}, " +
                      $"Sierra={ReputationManager.Instance.GetAffection(NpcId.B)}");
        }
    }
    void Start()
    {
        Debug.Log("RepTester started");
    }

}