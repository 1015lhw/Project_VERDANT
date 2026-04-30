/* NOTES

 TALKING TO SIERRA

26/02/22 21:34 - accidently deleted this file T-T

the mushroom is a potentially a little bit of bonding with Sierra and Tyler.
If you show Sierra you have the mushroom before Marcus, then she'll become paranoid that you've harmed yourself, losing reputation
If you show it after Marcus, then Tyler can reassure her that Marcus knows it's safe, gaining some trust

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
  map / mushroom / compass come from InventorySystem.Has(...).
  MarcHasMush / gaveMapToSierra / gaveBlanketToSierra come from StoryFlags.
- Reputation / inventory / flag changes are emitted as tags, handled by DialogueManager:
    # rep:Sie:+15 / # rep:Mar:-1     -> ReputationManager.AddAffection
    # grant:Compass                  -> InventorySystem.AddTaskReward
    # flag:gaveMapToSierra:set       -> StoryFlags

REPUTATION FLOW:
- Cold start: Srep = 0, blanket = 1, no map. Choices: Leave / Give blanket.
- Player gets map, returns: choice "Show your map" appears, +15 Srep on choose.
- After giving both map (+15) and blanket (+3), Srep = 18. Next conversation routes to Compass.

*/

VAR Mrep = 0
VAR Srep = 0

VAR blanket = 1

VAR mushroom = 0
VAR wolfrep = 0

VAR MarcHasMush = 0
VAR compass = 0
VAR map = 0
VAR gaveMapToSierra = 0
VAR gaveBlanketToSierra = 0

// Greeting / rep gate. Rep >= 18 with compass not yet given routes straight to Compass knot.
{
- compass == 0 && Srep >= 18:
    "Maybe you do have some kind of use.."
    -> Compass
- Srep <= 10:
    "You came back? It's easier to let us rot, idiot." #SierraNERVOUS
- Srep <= 17:
    ... #SierraNERVOUS
- else:
    ... #SierraNERVOUS
}

+ [Leave]
    -> DONE

*{ map && !gaveMapToSierra }[Show your map to her]
    "You found this...?" #SierraTALK
    "we might have a chance here..." #SierraHAPPY # rep:Sie:+15 # flag:gaveMapToSierra:set
    -> DONE


*{ blanket && !gaveBlanketToSierra }[Give a blanket to her]
    "Thanks... It's not as advanced as my Pantagoatia one though..." #SierraTALK # rep:Sie:+3 # flag:gaveBlanketToSierra:set
    -> DONE


//Minus reputation, if we have a visual rep bar, splitting the reputation into 4 makes the dialogue more dramatic.
*{!MarcHasMush && mushroom}[Give mushroom to her]
    "JESUS WHAT THE FUCK IS THAT?" #SierraMAD # rep:Sie:-1

    "Well... don't know really, I thought you'd have some thoughts on it?" #TylerTALK # rep:Sie:-1

    "YOU DON'T KNOW??? STOP HOLDING IT YOU'RE GOING TO DIE YOU FUCKING MORON." #SierraMAD # rep:Sie:-1

    "Hey, calm down." #TylerTALK # rep:Sie:-1

    "GO AWAY." #SierraMAD

    ->DONE

//After you give marcus the mushroom, you can tell Sierra that Marcus is an excellent Forager and can help us out too.
*{MarcHasMush}[Tell her about the the blood mushroom]

    "Hey Sier-  " #TylerTALKBLOOD

   "YOUR HANDS? WHERE'S MARCUS? DID YOU KILL HIM?" #SierraSCARED

    "Sierra calm down." #TylerDEFENSIVEBLOOD

    ... #SierraSCARED

    "My hands are red because apparentely i found this... <i>Bleeding Mushroom</i> thing. Marcus knew exactly what I was holding." #TylerTALKBLOOD

       ...  #SierraSCARED

    "We're are safer than you think. Marcus is like a <i>chubby Bear Grylls</i>." #TylerTALKBLOOD

   ...  #SierraNERVOUS

    "You keep bringing the most outrageous things..." #SierraTALK

        ** "Hey, I'm doing other things too."

            "Yeah well, you're fairly useless by yourself." #SierraTALK

            *** "You're even more useless!"

                !!! #SierraMAD
                ... #SierraNERVOUS
                "Do you think..."  #SierraCRY
                "we'll..." #SierraCRY

                ->DONE

            *** -> BarelyKnow

        ** -> BarelyKnow


=== BarelyKnow ===
"I barely know what I'm doing.." #TylerTALKBLOOD

            "Well at least you can admit to it" #SierraTALK

            "Everyone can admit to things" #TylerTALKBLOOD

            "..." #SierraNERVOUS

            "Let's go and help out Marcus" #SierraTALK # rep:Sie:+4

            ->DONE

=== Compass ===
    "This is my dads Compass... plot twist of the century, I'm giving it to you." #SierraTALK
    "So please... Don't be a moron with it." #SierraTALK # grant:Compass
-> DONE


-> END
