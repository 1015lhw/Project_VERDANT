VAR repMar = 0
VAR repSie = 0

-> start

=== start ===
你好，这是一个声望测试。

+ [夸奖马库斯]
    ~ repMar += 5
    马库斯看起来更开心了。
    马库斯 = {repMar}
    西耶拉 = {repSie}
    -> END

+ [对马库斯无礼]
    ~ repMar -= 5
    马库斯看起来很恼火。
    马库斯 = {repMar}
    西耶拉 = {repSie}
    -> END

+ [离开]
    -> END
    


=== check ===
当前声望：
马库斯 = {repMar}
西耶拉 = {repSie}

+ [返回]
    -> start