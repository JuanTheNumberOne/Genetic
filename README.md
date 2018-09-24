# Genetic
This application (app) is used to implement a genetic algorithm to find the optimal parameters of an ABB'S Robot trajectoy.
The app let's you scann for avaible controllers in your local area network. Once a controller or controllers have been found, you will
have to double click to get the controller's info needed to write/read the controller's data. The program in the robot's controllers must be
prepared a priori to receive and transfer the data (variable names and such that match those used in the app).

After that use the button "Let there be light", then a first generation of individuals (each containing the parameters in their DNA)
that are send to the robot's controller. The controller then executes the trajectory according to the parameters of each individual.
Each of these trajectorie's time should be monitored by the robot in order to get a measure of the time elapsed for each trajectory.
When the robot has measured all the trajectories it asks the app for new input data. The app then reads the measured times, and according to them
calculates the fitness score of each individual. Then it executes the genetic algorithm to build the next generation of individuals, which should again 
be tested as described before in the first generation.

The process will iterate until it reaches the number of generations specified by the user. Then at the end it will select the fittest of the last generation
as the optimum solution.

The process occurs in the class "World". Many worlds can be initialized depending on how many points the trajectory has.
For n points, the number of worlds will be n-1. Each world has a population of individuals that undergo the same evolutionary process described above.

Disclaimer:
The aim of this app is to test whether it is possible to apply such algorithm to an indsutrial robot. Therefore it is not meant for commercial use or any
other professional applications. It is only for research purposses and the results should be treated accordingly.

The code is open source, so use it as you like. The creator (me, Jan Zalewski :D) does not take resposibility of the results or any damage that can arise from 
the use of this software. Remember that industrial robots are dangerous machines and should be treated with caution and ensuring that common sense and neccesary 
safety measures are applied.

More info:
This application is my master's thesis/final project. Therefore for a more specific documentation you should look for my work in the Silesian University
of Technology, MT Faculty, located in Gliwice, Poland.  http://mt.polsl.pl/en/
