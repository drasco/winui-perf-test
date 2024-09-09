### Introduction

These tests are desgined to expose what seems to be a flaw on recent Xeons, particularily Sapphire Rappids (2022) - but 2020 chips also. 

The results are apparent on older CPUs, however not as significant as the threading performance keeps up with the demand. 

### Lazurus GDI Test
ListView tester created in Lazurus (https://www.lazarus-ide.org/), in order to get closer to Win32 ComCTL32 "CListView"

Compile using Lazurus. 

Run the app with nothing else running and get a timing. Usually aroung .5 to 2sec

Launch edge chrome and test again - perhaps now getting 3 - 10 seconds

Launch two of the test apps and kick them off at the same time - 20+ seconds.

### Resolve lag scripts

Run ResolveLagOpt2 to set all EXE's in your session to use the same core, and run Lazurus GUI Test's above.

When all the apps that are hooked into the what seems to be windows message queue, perhaps centred around a single kernel critical section, the context switching demand of the UI operations drops.

This mitigates the impact of bad-threading CPUs. Obvioulsy, as everything is bound to one core now, performance in that regard is lost. 

Chrome and edge in particular would much prefer at least two cores, but they have hooks into the message queue. And if you give them 2 cores, the lag issue is apparent again.
