using UnityEngine;

public class InteractionDebugger : MonoBehaviour
{
    private BoxCollider col;
    private bool playerInTrigger = false;

    void Start()
    {
        col = GetComponent<BoxCollider>();
        
        // 1. 检查是否有 Collider
        if (col == null)
            Debug.LogError($"[Debug] {gameObject.name} 上找不到 BoxCollider！");
        else if (!col.isTrigger)
            Debug.LogWarning($"[Debug] {gameObject.name} 的 BoxCollider 没有勾选 Is Trigger！");

        // 2. 检查 Collider 尺寸是否太小 (参考你截图中的 0.04)
        if (col != null && (col.size.x < 0.1f || col.size.y < 0.1f))
            Debug.LogError($"[Debug] 警告：你的 Collider 尺寸非常小 ({col.size})，玩家很难碰到它！请调大 Size。");

        // 3. 检查脚本冲突
        var scripts = GetComponents<MonoBehaviour>();
        int count = 0;
        foreach (var s in scripts)
        {
            if (s is Berries_Interaction || s is PressE_Interact) count++;
        }
        if (count > 1)
            Debug.LogWarning($"[Debug] 检测到该物体上同时挂载了多个交互脚本，可能会导致 UI 冲突！");
    }

    void Update()
    {
        // 实时监控状态
        if (Input.GetKeyDown(KeyCode.P)) // 按 P 键打印当前环境状态
        {
            Debug.Log($"--- 实时状态报告 ---");
            Debug.Log($"玩家是否在范围内: {playerInTrigger}");
            Debug.Log($"游戏全局状态 (IsNormal): {GameStateManager.IsNormal}");
            Debug.Log($"当前状态枚举: {GameStateManager.CurrentState}");
            
            var interaction = GetComponent<Berries_Interaction>();
            if (interaction != null)
            {
                Debug.Log($"UI 引用是否存在: {interaction.pressEUI != null}");
                if (interaction.pressEUI != null) 
                    Debug.Log($"UI 当前激活状态: {interaction.pressEUI.activeSelf}");
            }
        }
    }

    // 监测触发器事件
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Debug] 碰撞触发！物体名称: {other.name}, Tag: {other.tag}");
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log("<color=green>[Debug] 玩家成功进入触发区域！</color>");
        }
        else
        {
            Debug.Log($"[Debug] 有东西进来了，但它的 Tag 不是 'Player'，而是 '{other.tag}'");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log("<color=red>[Debug] 玩家离开了触发区域。</color>");
        }
    }

    // 在 Scene 窗口画出一个红框，方便你肉眼观察感应区
    void OnDrawGizmos()
    {
        col = GetComponent<BoxCollider>();
        if (col == null) return;

        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(col.center, col.size);
    }
}