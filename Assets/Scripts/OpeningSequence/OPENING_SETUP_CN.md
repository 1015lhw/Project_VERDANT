# Opening Sequence 手把手配置（中文）

> 目标：点击 **Start** 才播放 6 张漫画开场；开场期间锁住移动/背包/交互/对话；播完显示世界并进入后续剧情。

## 你需要新建东西吗？

- **`OpeningLock.cs` 不需要你手动新建**：代码里已经加好了。
- **`gameWorldRoot` 推荐你在场景里指定一个对象**（通常是场景里承载可见世界的总父物体）。
  - 不是必须：你留空也能播开场。
  - 但留空时，开场期间世界不会被自动隐藏。

---

## 一、场景里要有的对象

1. 一个 `OpeningSequenceManager` 挂载对象（可建空物体命名为 `OpeningManager`）。
2. 一套开场 UI：
   - `Image`（显示漫画图）
   - `TMP_Text`（显示字幕）
   - `AudioSource`（播放每页语音）
3. Start 按钮（UI Button）。
4. （推荐）一个 `gameWorldRoot`，把场景中“正式可玩世界”都作为它子物体。

---

## 二、Inspector 逐项配置

选中 `OpeningManager`（挂了 `OpeningSequenceManager` 的对象）：

1. `Slides` 设置为 `6`（或你的页数）。
2. 每个 `Element` 都填：
   - `Image`（Sprite）
   - `Subtitle`（字幕）
   - `Voice`（AudioClip，**可选**）
   - `Duration If No Voice`（没配语音时这一页停留秒数）
3. 绑定引用：
   - `Comic Image` → 你的漫画 Image
   - `Subtitle Text` → 你的 TMP_Text
   - `Audio Source` → 你的 AudioSource
   - `Opening Canvas Root` → 整个开场 UI 的根节点（例如 `OpeningCanvas`）
   - `Game World Root` → 你的世界根节点（推荐填）

---

## 三、把 Start 按钮接上播放函数

选中 Start 按钮：

1. 在 `Button -> OnClick()` 点击 `+`
2. 拖入 `OpeningManager` 对象
3. 下拉选择：`OpeningSequenceManager -> BeginOpening()`

> 这样就不是进场自动播，而是点击 Start 后才播。

---

## 四、当前版本的行为（你会看到什么）

1. 点击 Start
2. `BeginOpening()` 执行：
   - `OpeningLock.IsLocked = true`
   - 隐藏 `gameWorldRoot`（如果你配置了）
3. Slide1~Slide6 逐页播放（每页等语音播放完才进下一页）
4. 全部播完后：
   - 显示 `gameWorldRoot`
   - `OpeningLock.IsLocked = false`
   - 预留了“强制 Ink 对话”挂点（在 `EndOpening()` 里）

---

## 五、为什么你会感觉“不会用”——最常见 4 个坑

1. **没把 `BeginOpening()` 绑到按钮** → 点击 Start 没反应。
2. **某页没配 `Voice` 但也没配时长** → 会使用 `defaultNoVoiceDuration` 默认秒数。
3. **有语音页但 `AudioSource` 没配** → 管理器会报未配置错误，无法开始。
4. **没配 `gameWorldRoot`** → 开场期间世界不会自动隐藏（但流程仍可跑）。

---

## 六、你下一步只要做这 3 件事

1. 有语音的页就配 `Voice`；没语音的页就配 `Duration If No Voice`。
2. 把 `gameWorldRoot` 指到你的世界总父物体（推荐）。
3. Start 按钮 `OnClick` 绑定 `BeginOpening()`。

> 如果你走的是主菜单切场景流程，通常不用再绑第二个按钮，只要保留 `Auto Begin On Scene Load = true`。

---

## 七、你这次问的关键：第一幕没配音频能不能自动切？

可以。现在逻辑是：

- 有 `Voice`：等语音播完再切下一页。
- 没 `Voice`：按该页 `Duration If No Voice` 秒数自动切页。
- 如果该页时长写成 `0` 或忘填：使用 `OpeningSequenceManager.defaultNoVoiceDuration`。

---

## 八、你现在这个问题：点 StartGame 切场景后没反应，怎么配？

你是「主菜单按钮 -> LoadScene(Playtest Scene)」这条链路，所以通常**不会再点一次 BeginOpening 按钮**。

现在脚本已支持自动触发：

1. 选中 `OpeningManager`
2. 勾选 `Auto Begin On Scene Load`（默认就是开）
3. 运行后从主菜单点 StartGame，进入 Playtest Scene 就会自动播

如果还没反应，按这个顺序查：

1. `OpeningManager` 物体是否是 Active（Hierarchy 左侧勾选）
2. `OpeningSequenceManager` 组件是否 enabled
3. `Slides` 数组长度是否 > 0
4. `Comic Image` / `Subtitle Text` 是否都拖了引用
5. Console 是否有红字：`OpeningSequenceManager:` 开头报错

你截图里 `Audio Source`、`ComicImage`、`SubtitleText` 已经有引用，重点再确认第 1、2、5 条。

完成后就能按你要的流程工作。

---

## 九、Slide 6 播完还卡住怎么办？

新版本里，开场结束会执行：

1. 停止语音（如果还在播）
2. 隐藏 `Opening Canvas Root`
3. 显示 `gameWorldRoot`（如果配置了）
4. 解锁 `OpeningLock`

所以请务必把 `Opening Canvas Root` 指到你的开场 UI 根节点，否则可能视觉上还停在最后一页。

---

## 十、`gameWorldRoot` 下面到底要放什么？（最重要）

建议你新建一个空物体：`GameWorldRoot`，把“正式可玩世界”都拖进去：

- `Background`
- `Env Assets`
- `Terrain`
- `Player`
- `NPC`
- `BerryShrub`
- 其他场景内可见模型 / 可交互物体

通常不要放进去：

- `OpeningSequence`（开场 UI 系统）
- `EventSystem`
- 常驻管理器（如 `DontDestroyOnLoad`）

这样做不会影响你现有功能：只是把已有对象改父子层级，不改脚本引用逻辑。

---

## 十一、长按 ESC 3 秒跳过（已实现）

`OpeningSequenceManager` 新增：

- `Allow Hold Esc To Skip`（开关）
- `Skip Hold Duration`（默认 3 秒）
- `Skip Hint Root`（可选：提示 UI 根节点）
- `Skip Hint Text`（可选：显示长按进度文本）
- `Skip Progress Fill`（可选：Image Filled 进度条）

行为：

- 开场时按住 ESC 累计时间；松开会清零。
- 按满 3 秒立即结束开场，进入正常游戏流程。
