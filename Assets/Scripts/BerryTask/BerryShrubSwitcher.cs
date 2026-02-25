using UnityEngine;

public class BerryShrubSwitcher : MonoBehaviour
{
    [Header("Scene children (NOT prefabs)")]
    public GameObject withFruitObj;   // 拖 Hierarchy 里的 WithFruit
    public GameObject emptyObj;       // 拖 Hierarchy 里的 Empty

    void Awake()
    {
        // 初始状态：有果子
        if (withFruitObj != null) withFruitObj.SetActive(true);
        if (emptyObj != null) emptyObj.SetActive(false);
    }

    public void SwitchToEmpty()
    {
        if (withFruitObj != null) withFruitObj.SetActive(false);
        if (emptyObj != null) emptyObj.SetActive(true);
    }

    public void SwitchToWithFruit()
    {
        if (withFruitObj != null) withFruitObj.SetActive(true);
        if (emptyObj != null) emptyObj.SetActive(false);
    }
}