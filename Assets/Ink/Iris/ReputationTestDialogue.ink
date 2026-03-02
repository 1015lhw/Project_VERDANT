VAR repMar = 0
VAR repSie = 0

-> start

=== start ===
Hello, this is a reputation test.

+ [Compliment Marcus]
    ~ repMar += 5
    Marcus seems happier.
    Mar = {repMar}
    Sie = {repSie}
    -> END

+ [Be rude to Marcus]
    ~ repMar -= 5
    Marcus looks annoyed.
    Mar = {repMar}
    Sie = {repSie}
    -> END

+ [Leave]
    -> END
    


=== check ===
Current rep:
Mar = {repMar}
Sie = {repSie}

+ [Back]
    -> start