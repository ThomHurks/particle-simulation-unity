# C\# Particle Simulation
This project is a particle simulation written in C\# based on the SIGGRAPH 2001 Physically Based Modeling course notes by Andrew Witkin and David Baraff.

## Online Demo
A WebGL demo can be found at http://thomhurks.com/particles/
Note that you may need a recent version of Google Chrome, and the initial loading time can be somewhat long.

## Reusability
This particlar project was created within the Unity game engine to make easy cross-platform deployment possible, but the core framework does not depend on Unity and should be portable. We only use Unity for front-end matters such as:
* User input handling (both mouse and touch)
* OpenGL drawing (very straightforward to replace by any other OpenGL drawing library)
* The update loop
* Drawing user interface components

We do use Unity's built-in Vector2 class, but again that should be very straightforward to replace by any typical Vector class.

## Authors
This project was created by Thom Hurks and Jeffrey Aben for the course "Simulation in Computer Graphics" taught by Dr. Andrei Jalba at the Eindhoven University of Technology.

## Manual:
There is only one screen. You can use the various dropdowns to select different scenarios, solvers, integrators and types of constraints. There are also buttons to reset the simulation and the simulation speed. There is a slider to change the simulation speed and a toggle to reverse the simulation. The user interface should be very self-explanatory.
The simulation automatically resets itself when it encounters an error.
### User Input:
Users can click on particles to attach a spring between the particle and the mouse pointer. The mouse can then be moved (you do not need to keep the mouse button pressed) to influence the particle. A right-click destroys the spring and releases the particle.
On touch-based devices (mobile, tablet) you can influence particles by touching and dragging in an intuitive way and multi-touch is also possible.

## Building it yourself
Download the Unity game engine and install it. Navigate to this project's folder and open it. There is only one scene called "Main.unity" in the Scenes folder which you need to open. You can then press the play button to test the project.
We also created and tested standalone builds for:
* Windows (see Links section for a link to the latest standalone Windows build)
* MacOS (see Links section for a link to the latest standalone MacOS build)
* iOS
* WebGL (see online demo)

These can be built from within Unity if you have the necessary (optional) components installed and all related settings should already be correctly set. The only thing you may need to change is the application's bundle identifier in the Player Settings.

## Links:
* http://www.pixar.com/companyinfo/research/pbm2001/
* http://education.tue.nl/Activiteiten/Pages/Informatie.aspx?coursecode=2IMV15&educationyear=2015
* http://www.win.tue.nl/~ajalba/edu/2IV15_Simulation/2ivm15_description.pdf
* http://thomhurks.com/particles/
* http://thomhurks.com/particles/builds/ParticleSimulationMac.zip
* http://thomhurks.com/particles/builds/ParticleSimulationWindows.zip
* http://unity3d.com
