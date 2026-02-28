/* 备注

与 Marcus 对话

*/

// 示例变量（测试用）
VAR rep = 20

VAR blanket = 1
VAR mushroom = 0
VAR wolfrep = 0

VAR MarcHasMush = 1
VAR compass = 0
VAR map = 1


{
- rep < 11:
    ...

- rep < 19:
    "你好……"

- rep > 19:
    + ["嘿 Tyler，我有东西要给你……"]
        -> MarcWolfrep
}


+ {mushroom} [把血蘑菇给 Marcus]

    "我……嗯……找到这个……"

    Marcus 的眼睛一下子亮了起来。

    "我前几天正好读到关于它的资料！"
    "我来这里有一半的原因就是为了这种东西！我一直知道这些森林藏着无数秘密，而你刚刚发现了其中一个！"

    "嗯……你的森林秘密现在全糊在我手上，看起来像我刚杀了人。"

        ** "不过现在不是研究森林秘密的时候，我们得先活下来。"
            "哦……对……是的。"

        ** 继续说下去……

    "哈哈，这是 Hydnellum peckii，学名血齿菌。"
    "它对健康完全没有危险，但绝对是我‘真菌人生清单’上的重要一项！"

    他懂得真的好多……

    ~ mushroom -= 1
    ~ MarcHasMush += 1
    ~ rep += 5



+ {MarcHasMush == 1} [把毯子给 Marcus]
    -> BlanketHappy


+ {MarcHasMush == 0} [把毯子给 Marcus]
    -> Blanket



=== MarcWolfrep ===

    "我读过不少关于这片森林的资料。"
    "一个人行动太危险了，拿着这个驱狼剂，这样你可以继续探索。"

    ~ wolfrep += 1

    "所以你是想让我当你的侦察小弟？"

    "不不不，我不是那个意思，对不起……"

    ...

    "其实我也不介意，Marcus。"
    "谢谢。"

    ~ rep += 3

    -> DONE



=== BlanketHappy ===

    "哇，谢谢你 Tyler！这样我和我的蘑菇都能保暖了。"

    "没事，兄弟。"

    -> DONE



=== Blanket ===

    "谢谢你……"

    ~ blanket -= 1

    -> DONE



-> END
// 下一步选项