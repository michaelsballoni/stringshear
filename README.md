StringShear is a Simple Harmonic Motion on a String simulation

![Screenshot](/assets/screen.png)

It consists of C++ and C# components:
- app: C# WinForms program for visualizing and controlling the simulation
- sealib: portable C++ class library that implements the simulation and serialization to the app
- seasvr: C# program that interfaces between the app and sealib
- seadll: Windows C++ DLL project that wraps sealib for communication with seasvr

The project is built using Visual Studio 2019.  The C# components use .NET 5.0.