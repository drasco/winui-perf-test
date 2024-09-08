### Lazurus GDI Test
ListView tester created in Lazurus (https://www.lazarus-ide.org/), in order to get closer to Win32 ComCTL32 "CListView"

Compile using Lazurus. 

Run the app with nothing else running and get a timing. Usually aroung .5 to 2sec

Launch edge chrome and test again - perhaps now getting 3 - 10 seconds

Launch two of the test apps and kick them off at the same time - 20+ seconds.

### Resolve lag scripts

Run ResolveLagOpt2 to set all EXE's in your session to use the same core, and run Lazurus GUI Test's above.
