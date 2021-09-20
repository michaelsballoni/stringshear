StringShear is a Simple Harmonic Motion on a String simulation

![Screenshot](/assets/screen.png)

It consists of C++ and C# components:
- app: C# WinForms program for visualizing and controlling the simulation
- sealib: portable C++ class library that implements the simulation and serialization to the app
- seasvr: C# program that interfaces between the app and sealib
- seadll: Windows C++ DLL project that wraps sealib for communication with seasvr

The project is built using Visual Studio 2019.  The C# components use .NET 5.0.

To run a simulation, execute the seasvr program, then run the app program.

Add a frequency to the "left enabled" list of oscillators.  Try 20.

Then press the Run button, and observe the wave pattern.

Next add 30 to the "right enabled" list, press the Reset button, and press the Run button.

Observe a more interesting pattern appear.