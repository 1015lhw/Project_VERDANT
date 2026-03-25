using UnityEngine;

public class PlayerListener:MonoBehaviour
{
    public GameObject mainCam, player;

    private void Update()
    {
        transform.position = player.transform.position;
        transform.rotation = mainCam.transform.rotation;
    }
}
