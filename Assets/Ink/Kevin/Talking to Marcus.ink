/* NOTES

TALKING TO MARCUS.

#SierraMAD
#SierraHAPPY
#SierraNERVOUS
#SierraSCARED
#SierraTALK
#SierraCRY

#MarcusHAPPY
#MarcusFLATTERED
#MarcusQUIET

#TylerTIRED
#TylerTALK
#TylerTALKBLOOD
#TylerDEFENSIVE
#TylerDEFENSIVEBLOOD

ENGINE BRIDGE:
- VARs below are written by DialogueManager.PushEngineStateIntoStory at story start.
  blanket = 1 default (starter item, not engine-pushed).
  mushroom comes from InventorySystem.Has("Mushroom").
  MarcHasMush / gaveBlanketToMarcus / gotWolfRepellent come from StoryFlags.
- Reputation / inventory / flag changes are emitted as tags:
    # rep:Mar:+5 / # rep:Sie:-3       -> ReputationManager.AddAffection
    # consume:Mushroom                -> InventorySystem.Remove
    # grant:Wolf Repellent             -> InventorySystem.AddTaskReward
    # flag:MarcHasMush:set            -> StoryFlags

GAMEPLAY FLOW:
1. Player gives Blood Mushroom -> "Sounds interesting, go on..." -> +5 Mar, mushroom stays
   (or "Anyway, we don't have time..." -> -3 Mar then +5 gather = +2, mushroom is consumed)
2. Player gives Blanket -> +3 Mar, blanket consumed via flag
3. Once MarcHasMush && gaveBlanketToMarcus, Marcus offers Wolf Repellent (grants Wolf Repellent item).

*/

VAR Srep = 0
VAR Mrep = 0

VAR blanket = 1

VAR mushroom = 0
VAR wolfrep = 0

VAR MarcHasMush = 0
VAR compass = 0
VAR map = 0
VAR gaveBlanketToMarcus = 0
VAR gotWolfRepellent = 0


{
- MarcHasMush && gaveBlanketToMarcus && !gotWolfRepellent:
    -> MarcWolfrep
- Mrep <= 0:
    <i> Marcus gives you the silent treatment. #narration
- else:
    "Hello..." #MarcusQUIET
}


+ [Leave]
    -> DONE


+{ mushroom && !MarcHasMush }[Give Marcus the blood mushroom]

    "I um... found this..." #TylerTALKBLOOD

    <i> Marcus' eyes sparked. #narration

    "I was reading about that the other day!" #MarcusHAPPY

    "Huh..." #TylerTALKBLOOD

    "I know these forests have many secrets and you just found one of them!" #MarcusHAPPY

    "Well, your forest secret is all over my hand. It looks like I killed someone." #TylerTALKBLOOD
        ** "Anyway, we don't have time when we need to survive."
            <i> You throw away the mushroom and wipe your hands. #narration
            "oh..." #MarcusQUIET # rep:Mar:-3 # consume:Mushroom
        ** Sounds interesting, go on...
    "You're holding a Hydnellum peckii, or more commonly a bleeding tooth mushroom. It's not dangerous to your health at all but it is one of many things i can tick off my fungi bucket list!" #MarcusQUIET

    <i>He knows so much... #narration #TylerTALK # flag:MarcHasMush:set # rep:Mar:+5
    -> DONE


+{ MarcHasMush && blanket && !gaveBlanketToMarcus }[Give blanket to Marcus]
        -> BlanketHappy

+{ !MarcHasMush && blanket && !gaveBlanketToMarcus }[Give blanket to Marcus]
        -> Blanket


=== MarcWolfrep ===

    "I've read enough about these woods, so I know travelling alone is dangerous, have this wolf repellent so you can continue exploring" #MarcusHAPPY

    "So you want me to be your little scout boy?" #TylerTALK

    "No no that's not what i mean, sorry.." #MarcusQUIET

    "hm..."  #TylerTALK
    "I wouldn't mind that, Marcus"   #TylerTALK
    "Thanks." #MarcusFLATTERED # rep:Mar:+3 # grant:Wolf Repellent # flag:gotWolfRepellent:set
    -> DONE


=== BlanketHappy ===

    "Wow, Thanks Tyler! This will keep both me and my mushroom safe."  #MarcusHAPPY

    "Sure buddy"  #TylerDEFENSIVE # rep:Mar:+3 # flag:gaveBlanketToMarcus:set
    -> DONE

=== Blanket ===

    "Thank you..." #MarcusQUIET # rep:Mar:+3 # flag:gaveBlanketToMarcus:set
    -> DONE

-> END
