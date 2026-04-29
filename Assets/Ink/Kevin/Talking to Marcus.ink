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

Does the reputation system work?

*/

// EXAMPLE VALUES RESET TO 0

VAR Srep = 15
VAR Mrep = 0 

VAR blanket = 1

VAR mushroom = 1
VAR wolfrep = 0

VAR MarcHasMush = 0
VAR compass = 0
VAR map = 1


{ 
- Mrep < 11:
    <i> Marcus gives you the silent treatment.
- Mrep < 19:
    "Hello..."
- Mrep > 19:
    +["Hey Tyler, I have something for you..."]
    -> MarcWolfrep
}


+{mushroom}[Give Marcus the blood mushroom]

    "I um... found this..." #TylerTALKBLOOD
    
    <i> Marcus' eyes sparked.

    "I was reading about that the other day!" #MarcusHAPPY
    
    "Huh..." #TylerTALKBLOOD
    
    "I know these forests have many secrets and you just found one of them!" #MarcusHAPPY

    "Well, your forest secret is all over my hand. It looks like I killed someone." #TylerTALKBLOOD
        ** "Anyway, we don't have time when we need to survive."
            <i> You throw away the mushroom and wipe your hands. 
            "oh..."
            ~ Mrep -= 3
            ~ mushroom -= 1
        ** Sounds interesting, go on...
    #Marcus
    "You're holding a Hydnellum peckii, or more commonly a bleeding tooth mushroom. It's not dangerous to your health at all but it is one of many things i can tick off my fungi bucket list!"
    #Tyler
    <i>He knows so much...
    
    ~ mushroom -= 1
    ~ MarcHasMush += 1
    ~ Mrep += 5

 
+{ MarcHasMush == 1}[Give blanket to Marcus]
        ->BlanketHappy

+{ MarcHasMush == 0}[Give blanket to Marcus]
        ->Blanket
    
=== MarcWolfrep ===

    I've read enough about these woods, so I know travelling alone is dangerous, have this wolf repellent so you can continue exploring" #MarcusHAPPY
    
    "So you want me to be your little scout boy?" #TylerTALK

    "No no that's not what i mean, sorry.." #MarcusQUIET
 
    "hm..."  #TylerTALK
    "I wouldn't mind that, Marcus"   #TylerTALK
    "Thanks." #MarcusFLATTERED
    
    ~ Mrep = Mrep + 3
    ~ wolfrep = wolfrep + 1
    -> DONE
    
    
=== BlanketHappy ===
  
    "Wow, Thanks Tyler! This will keep both me and my mushroom safe."  #MarcusHAPPY
  
    "Sure buddy"  #TylerDEFENSIVE
    -> DONE
    
=== Blanket ===
   
    "Thank you..." #MarcusQUIET
    ~ blanket -= 1
    -> DONE
    
-> END

