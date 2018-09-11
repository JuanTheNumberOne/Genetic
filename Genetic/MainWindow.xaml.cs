using System;
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

        public MainWindow()
        {
            InitializeComponent();
            World.PopulationSize = 15;
            World.dCompletionTime = 725;
            World.iBreedingChance = 100;
            World.iPercentageOfElites = 10;
            World.dMutationChance = 50;
            World.bAllowInnBreeding = false;
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
                Results_Windows.Text = _Scanner.Write_Record(World.Population[i].DNA, i);
            }
            _Scanner.Set_Reset_Bool("bDataReceived", true);
        }

        private void Read_Click(object sender, RoutedEventArgs e)
        {
            bool _DataReadyToRead = false;
            _DataReadyToRead =_Scanner.Read_Bool("bWaitForNewData");
            while (_DataReadyToRead == false)
            {
                Thread.Sleep(10);
            }
            Results_Windows.Text = "Individuals tested, times for fitness function ready";
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
            //And there was light
            Results_Windows.Text = "AND THERE WAS LIGHT... ";
            int iNumberOfGenerations = 0;
     
            //Create the first population population
            World.CreateGeneration(World.PopulationSize);
            //Calculate the fitness of the current population individual
            World.CalculateFitnessPopulation();
            //Sort by fitness score
            World.Population.Sort((x,y) => y.dFitnessScore.CompareTo(x.dFitnessScore));
            Refresh_Old_List();
            //Choose the elite of the actual generation (The highrollers, the motherfuckers, la creme de la creme, the avengers)
            World.ChooseElite();
            World.decTotalFitness = 0;

            //Here should be a check if the actual highest fitness is enough. For later

            for (int i = 0; i < 10; i++)  //Let's say 5 generations
            {
                //Breed a generation
                World.Population = World.BreedNextGeneration_Roullete();
                World.CalculateFitnessPopulation();
                World.Population.Sort((x, y) => y.dFitnessScore.CompareTo(x.dFitnessScore));
                World.ChooseElite();
                World.decTotalFitness = 0;
                iNumberOfGenerations = iNumberOfGenerations + 1;
            }


            //Finally display the last generation
            Refresh_Actual_List();
            //Display the winner 
            Results_Windows.Text = "AND THERE WAS LIGHT... " + "The winner is: " + World.Population[0].DNA_Code;
        }

        private void Parameters_View_Old_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
