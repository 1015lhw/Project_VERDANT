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

Changes: 
Line 87 no longer has an incorrect conditional
Line 99 was rewritten

*/

VAR Mrep = 0
VAR Srep = 20

VAR blanket = 1

VAR mushroom = 1
VAR wolfrep = 0

VAR MarcHasMush = 1
VAR compass = 0
VAR map = 1

//Reputation 
{
- Srep < 11:
    "You came back? It's easier to let us rot, idiot." #SierraNERVOUS
- Srep < 19:
    ... #SierraNERVOUS
- Srep > 19:
    * "Maybe you do have some kind of use.."
        -> Compass
}


*{ map }[Give your map to her]
    "You found this...? #SierraTALK
    we might have a chance here..." #SierraHAPPY
    ~ Srep = Srep + 15


*{blanket}[Give a blanket to her]
    "Thanks... It's not as advanced as my Pantagoatia one though..." #SierraTALK
    ~ Srep = Srep + 3
    ~ blanket = blanket - 1
    

//Minus reputation, if we have a visual rep bar, splitting the reputation into 4 makes the dialogue more dramatic.
*{!MarcHasMush && mushroom}[Give mushroom to her]
      ~ Srep -= 1
    "JESUS WHAT THE FUCK IS THAT?" #SierraMAD
    
    "Well... don't know really, I thought you'd have some thoughts on it?" #TylerTALK
        ~ Srep -= 1
    
    "YOU DON'T KNOW??? STOP HOLDING IT YOU'RE GOING TO DIE YOU FUCKING MORON." #SierraMAD
        ~ Srep -= 1
        
    "Hey, calm down." #TylerTALK
        ~ Srep -= 1
        
    "GO AWAY." #SierraMAD
    
    ->DONE

//After you give marcus the mushroom, you can tell Sierra that Marcus is an excellent Forager and can help us out too.
*{MarcHasMush}[Tell her about the the blood mushroom]
    
    "Hey Sier-  "#TylerTALKBLOOD
    
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
            
            "Let's go and help out Marcus" #SierraTALK
            ~ Srep += 4
            
            ->DONE

=== Compass ===
    "This is my dads Compass... plot twist of the century, I'm giving it to you." #SierraTALK
    "So please... Don't be a moron with it." #SierraTALK
        ~ compass += 1 
-> DONE


-> END
