﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Evolution_History_Scribe_Space;

//Domain of interface and communication with ABB controllers
using Network_Scanner_Domain;

//ControllerInfo domain
using Controller_Props;

namespace Genetic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Create a scanner to look for the controller in the local web.
        //This class also serves as the interface point between the ABB controller and the GA app
        private Network_Scanner _Scanner = new Network_Scanner();

        //Delegate to raise the event of data ready to read instead of waiting in a loop
        public delegate void RobotHasTestedIndividuals(); //This is the pointer class to the method
        public event RobotHasTestedIndividuals DataTested;   //Define the event that calls the delegate

        //Scribe to record and read the evolution history. He is the sole and unique witness of this universe.
        private Evolution_History_Scribe Royal_Scribe;

        //See if there are any subscribers and fire the event
        protected virtual void OnDataTested()
        {
            DataTested?.Invoke();

            /*
             Or 
             if (DataTested !=Null)
             {
               DataTested(this, EventArgs.Empty);
             }
             */
        }

        //Event handler
        public void HandleDataTested()
        {
            //The dispatcher allows us to throw the action to the UI thread?
            Thread.Sleep(2000);
            Dispatcher.Invoke(() =>
            {
                Refresh_Actual_List();
            });
        }

        //Variable to have green light once data is ready to read
        private bool DataReadyToRead = false;
        private bool DataWritten = false;

        private bool _RefreshedNeeded = false;

        //Here fire the event if the bool is true
        public bool RefreshNeeded
        {
            get { return _RefreshedNeeded; }

            set
            {
                _RefreshedNeeded = value;
                if (_RefreshedNeeded == true)
                {
                    //Here raise the event
                    //OnDataTested();
                }

            }
        }
        public Task ReadTimes;

        public MainWindow()
        {
            InitializeComponent();
            World.PopulationSize = 15;
            World.dCompletionTime = (decimal)0.10; // Distance d= 924 mm, velocity v= 9000 mm/s , ignoring acceleration and deceleration, massless point (no energy loss)
            World.iBreedingChance = 100;
            World.iPercentageOfElites = 10;
            World.dMutationChance = 50;
            World.bAllowInnBreeding = false;
            World.iWolrdIndex = 1;
            RefreshNeeded = false;

            //Create a subscription
            DataTested += HandleDataTested;

            //Start reading in the background when the times from the robot is ready to read
            //Task ReadTimes = new Task(new Action(IsDataReady));
            //ReadTimes = new Task(new Action(IsDataReady));
            //ReadTimes.Start();
        }

        //ABB INTERFACE AND DATA EXCHANGE AREA
        //////////////////////////////////////
        //Creates a list of all controllers avaible in the network adn displays their info
        private void Scanner_Click(object sender, RoutedEventArgs e)
        {
            Controllers_In_Network_List.ItemsSource = _Scanner.Scann_For_Controllers();
        }

        //Creates an instance of the selected controller
        private void Controllers_In_Network_List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Controller_Properties _SelectedController = null;
            

            if (Controllers_In_Network_List.SelectedItems.Count == 1)
            {
                _SelectedController = (Controller_Properties)Controllers_In_Network_List.SelectedItems[0];
                Results_Windows.Text = _Scanner.Network_View_DoubleClick(_SelectedController.ControllerInfo);
            }

        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Results_Windows.Text = _Scanner.Program_Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Results_Windows.Text = _Scanner.Program_Stop();
        }

        private void Write_Click(object sender, RoutedEventArgs e)
        {
            World.CreateGeneration(World.PopulationSize);
            Parameters_View.ItemsSource = World.Population;
            for (int i = 0; i < World.Population.Count; i++)
            {
                Results_Windows.Text = _Scanner.Write_Record_In_Array(World.Population[i].DNA, i);
            }
            _Scanner.Set_Reset_Bool("bDataReceived", true);
            DataWritten = true;
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            Refresh_Actual_List();
        }

        private void IsDataReady()
        {
            while (RefreshNeeded == false)
            { 
                if (DataReadyToRead == false & World.Population != null & DataWritten == true)
                {
                    DataReadyToRead = _Scanner.Read_Bool("bWaitForNewData");
                    if (DataReadyToRead)
                    {
                        DataWritten = false;
                        decimal TryParse_Out = 0;
                        string _Individual_Time_Elapsed = "";

                        for (int i = 0; i < World.Population.Count; i++)
                        {
                            //Read the elapsed time for each indivdual and update it in the population
                            _Individual_Time_Elapsed = _Scanner.Read_Record_FromArray(i);
                            Decimal.TryParse(_Individual_Time_Elapsed, out TryParse_Out);
                            World.Population[i].dTime = TryParse_Out;
                        }
                        //World.CalculateFitnessPopulation();
                        //Sort by fitness score
                        //World.Population.Sort((x, y) => y.dFitnessScore.CompareTo(x.dFitnessScore));
                        DataReadyToRead = false;
                        RefreshNeeded = true;
                        
                    }
                }
                else
                {
                    //do nothing
                }
            }
            Thread.Sleep(0);
        }

        //GENETIC ALGORITH ML AREA
        ///////////////////////////////////
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            //Create a population and display it
            World.CreateGeneration(World.PopulationSize);
            Parameters_View.ItemsSource = World.Population;

        }

        private void Kill_Individual_Click(object sender, RoutedEventArgs e)
        {
            World.KillIndividual(Parameters_View.SelectedIndex);
            //Refresh the list
            Parameters_View.ItemsSource = null;
            Parameters_View.ItemsSource = World.Population;
        }

        private void CheckValue(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");

        }

        private void CheckPopulation_Click(object sender, RoutedEventArgs e)
        {
            int iCheckResult;
            iCheckResult =  World.CheckIfPopulationisComplete();

            switch (iCheckResult)
            {
                case 0:
                    Results_Windows.Text = "The population count is right";
                    break;
                case 1:
                    Results_Windows.Text = "There is an overpopulation, " + (World.Population.Count - World.PopulationSize) + " individual/s less needed";
                    break;
                case 2:
                    Results_Windows.Text = "There is an underpopulation, " + (World.PopulationSize - World.Population.Count) + " individual/s more needed";
                    break;
                case 3:
                    Results_Windows.Text = "There is no population";
                    break;

                default:
                break;
            }

        }

        private void Calculate_fitness_of_population_Click(object sender, RoutedEventArgs e)
        {

            if (World.Population != null)
            {
                //Calculate the fitness score of each individual in the population
                foreach (Individual item in World.Population)
                {
                    //item.dFitnessScore = World.CalculateFitness(item);
                }

                //Refresh the list
                Parameters_View.ItemsSource = null;
                Parameters_View.ItemsSource = World.Population;

            }
            else
            {
                Results_Windows.Text = "There is no population ";
            }

        }

        private void Breed_Individuals_Click(object sender, RoutedEventArgs e)
        {
            Individual _Breeded = new Individual();

            if (World.Population != null)
            {
                _Breeded = _Breeded.SwapDna(World.Population[0], World.Population[1], 2);
                World.Population.Add(_Breeded);

                _Breeded = _Breeded.SwapDna(World.Population[0], World.Population[1], 1);
                World.Population.Add(_Breeded);

                Parameters_View.ItemsSource = null;
                Parameters_View.ItemsSource = World.Population;
            }

            else
            {
                Results_Windows.Text = "There is no population ";
            }

        }

        private void Mutate_Individual_Click(object sender, RoutedEventArgs e)
        {
            Individual _Mutated = new Individual();
            double _MutationChance = 100;

            if (World.Population != null)
            {
                if (Parameters_View.SelectedIndex >= 0 & Parameters_View.SelectedIndex < World.Population.Count)
                {
                    _Mutated = _Mutated.Mutate(World.Population[Parameters_View.SelectedIndex], _MutationChance);
                    World.Population[Parameters_View.SelectedIndex] = _Mutated;

                    Parameters_View.ItemsSource = null;
                    Parameters_View.ItemsSource = World.Population;
                }
                else
                {
                    Results_Windows.Text = "No individual selected";
                }
                
            }

            else
            {
                Results_Windows.Text = "There is no population ";
            }

        }

        private void Refresh_Actual_List()
        {
            Parameters_View.ItemsSource = null;
            Parameters_View.ItemsSource = World.Population;
        }

        private void Refresh_Old_List()
        {
            Parameters_View_Old.ItemsSource = null;
            Parameters_View_Old.ItemsSource = World.Population;
        }


        private void Let_There_Be_Light_Click(object sender, RoutedEventArgs e)
        {
            //Check if a controller is selected for the algorithm
            if (_Scanner.controller !=null)
            {
                //And there was light
                Results_Windows.Text = "AND THERE WAS LIGHT... ";
                int iNumberOfGenerations = 1;
                RefreshNeeded = false;
                DataReadyToRead = false;

                //Create the first population population
                World.CreateGeneration(World.PopulationSize);

                //Reset the total fitness
                World.decTotalFitness = 0;

                //Write the first data to robot
                WriteDataToRobot();

                ReadTimes = new Task(new Action(IsDataReady));
                ReadTimes.Start();

                //Here begin looping for x generations
                for (int i = 1; i < 5; i++)
                {

                    //Wait until paralell task has detected and read avaible data
                    ReadTimes.Wait();

                    //Calculate the fitness of the current population individual
                    World.CalculateFitnessPopulation();

                    //Sort by fitness score
                    World.Population.Sort((x, y) => y.dFitnessScore.CompareTo(x.dFitnessScore));
                    World.Population.Sort((x, y) => x.dTime.CompareTo(y.dTime));

                    //Register the current generation in the database
                    for (int j = 0; j < World.PopulationSize; j++)
                    {
                        RecordIndividual(World.Population[j],World.iWolrdIndex,iNumberOfGenerations);
                    }

                    //Display first generation in the window
                    if (i == 0)
                    {
                        Refresh_Old_List();
                    }

                    //Choose the elite of the actual generation (The highrollers, the motherfuckers, la creme de la creme, the avengers)
                    World.ChooseElite();

                    //Here should be a check if the actual highest fitness is enough. For later

                    //Breed a generation
                    World.Population = World.BreedNextGeneration_Roullete();

                    //Update the number of the generation
                    iNumberOfGenerations = iNumberOfGenerations + 1;

                    //Write the new generation to the robot
                    WriteDataToRobot();

                    //Reset the total fitness
                    World.decTotalFitness = 0;

                    //Set the paralell task to detect when new data from the robot is ready
                    DataReadyToRead = false;
                    RefreshNeeded = false;
                    ReadTimes = new Task(new Action(IsDataReady));
                    ReadTimes.Start();
                }

                //Wait for the last  population to be read
                ReadTimes.Wait();

                //Calculate the fitness of the last population
                World.CalculateFitnessPopulation();
                World.Population.Sort((x, y) => y.dFitnessScore.CompareTo(x.dFitnessScore));
                World.Population.Sort((x, y) => x.dTime.CompareTo(y.dTime));

                //Register the last generation in the database
                for (int j = 0; j < World.PopulationSize; j++)
                {
                    RecordIndividual(World.Population[j], World.iWolrdIndex, iNumberOfGenerations);
                }

                //Finally display the last generation
                Refresh_Actual_List();
                //Display the winner 
                Results_Windows.Text = "And on the seventh day God had finished his work of creation, so he rested from all " + "The winner is: " + World.Population[0].DNA_Code;
            }
            else
            {
                Results_Windows.Text = "No controller selected";
            }
 
        }

        private void Parameters_View_Old_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void WriteDataToRobot()
        {
            //Write data to robot
            for (int i = 0; i < World.Population.Count; i++)
            {
                Results_Windows.Text = _Scanner.Write_Record_In_Array(World.Population[i].DNA, i);
            }
            _Scanner.Set_Reset_Bool("bDataReceived", true);
            DataWritten = true;
        }



        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            Royal_Scribe = new Evolution_History_Scribe();
            Individual _Ind = new Individual()
            {
                DNA_Code = "1785,14,5,9",
                dFitnessScore = (decimal)0.56,
                dWeightedFitnessValue = (decimal)0.6,
                dTime = (decimal)0.67

            };
            Royal_Scribe.Add_Individual_Record(_Ind,1,1);
            

        }

        private void RecordIndividual(Individual Idividual_Recorded, int World_Numer, int Generation_Number)
        {
            Royal_Scribe = new Evolution_History_Scribe();
            Individual _Idividual_Recorded = Idividual_Recorded;
            Royal_Scribe.Add_Individual_Record(_Idividual_Recorded, Generation_Number, World_Numer);
        }
    }
}
