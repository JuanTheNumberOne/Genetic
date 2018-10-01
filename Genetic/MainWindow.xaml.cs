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

        //Allows to/not repeat the inputdatacheck
        private bool Input_Checked;

        //Delegate to raise the event of data ready to read instead of waiting in a loop
        public delegate void RobotHasTestedIndividuals(); //This is the pointer class to the method
        public event RobotHasTestedIndividuals DataTested;   //Define the event that calls the delegate

        //Scribe to record and read the evolution history. He is the sole and unique witness of this universe.
        private Evolution_History_Scribe Royal_Scribe;

        //List of worlds in the unvierse
        private List<World> _Worlds = new List<World>();
        private Universe NewUniverse;

        //Byre that indicates if the input parameters are right. // 0 Wrong parameter ; 1 right parameter
        private bool[] arboAlgorithmParams_OK = new bool[8];
        //--// BIT VALUES //--//
        //--// Bit 0: Generation number
        //--// Bit 1: Selected artificial selection method
        //--// Bit 2: Mutation rate
        //--// Bit 3: Breeding rate
        //--// Bit 4: Poupulation number
        //--// Bit 5: Primuses number
        //--// Bit 6: Reserve
        //--// Bit 7: Reserve

        //Structure for the combobox
        public struct Selection_Method_Struct
        {
            public string MethodName { get; set; }
            public int MethodIndex { get; set; }

            public Selection_Method_Struct(string Name, int Index)
            {
                MethodName = Name;
                MethodIndex = Index;
            }     
        }

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


        // END OF GLOBAL VARIABLES DECLARATION----------------------------------------------------------------------------------------------------------------------------------------

        public MainWindow()
        {
            InitializeComponent();

            //Create new universe
            NewUniverse = new Universe(1, _Worlds); //Create a new unvierse
            //Initialize some numbers for the world
            World.PopulationSize = 15;
            World.dCompletionTime = (decimal)0.10; // Distance d= 924 mm, velocity v= 9000 mm/s , ignoring acceleration and deceleration, massless point (no energy loss)
            World.iBreedingChance = 100;
            World.iPercentageOfElites = 10;
            World.dMutationChance = 50;
            World.bAllowInnBreeding = false;
            World.iWolrdIndex = 1;
            World.iWorldGenerations = 5;
            World.INumberOfPrimuses = 1;
            World.iNumberOfParameters = 6;

            Conn_Text_Status.Content = "Connectivity status: Not connected";
            Conn_Status.Fill = Brushes.Red;

            //Reset flags and set GUI values
            RefreshNeeded = false;
            Input_Checked = true;

            //Combo boxes values 
            List<Selection_Method_Struct> Combobox_SelectionMethods = new List<Selection_Method_Struct>();
            Combobox_SelectionMethods.Add(new Selection_Method_Struct("Biased Roulette [BR]",1));
            Combobox_SelectionMethods.Add(new Selection_Method_Struct("Tournament [T]", 2));
            Combobox_SelectionMethods.Add(new Selection_Method_Struct("Mixed selection [BR + T]", 3));
            Selection_Method.ItemsSource = Combobox_SelectionMethods;

            //Create a subscription
            DataTested += HandleDataTested;
        }

        //ABB INTERFACE AND DATA EXCHANGE AREA
        //////////////////////////////////////--------------------------------------------------------------------------------------------------------------------

        //Creates a list of all controllers avaible in the network adn displays their info
        private void Scanner_Click(object sender, RoutedEventArgs e)
        {
            Controllers_In_Network_List.ItemsSource = _Scanner.Scann_For_Controllers();
        }

        //Creates an instance of the selected controller
        private void Controllers_In_Network_List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Controller_Properties _SelectedController = null;
            string Output;

            if (Controllers_In_Network_List.SelectedItems.Count == 1)
            {
                _SelectedController = (Controller_Properties)Controllers_In_Network_List.SelectedItems[0];
                Output = _Scanner.Network_Connect_To_Cotroller(_SelectedController.ControllerInfo);
                Conn_Text_Status.Content = "Connectivity status: " + Output;

                if (Output == "Selected controller not available / ready")
                {
                    Conn_Status.Fill = Brushes.Red;
                }
                else
                {
                    Conn_Status.Fill = Brushes.Green;
                }
                
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
                        DataReadyToRead = false;
                        RefreshNeeded = true;                  
                    }
                }
                else
                {
                    //do nothing
                }
            }
        }

        //GENETIC ALGORITH ML AREA
        //////////////////////////////////////--------------------------------------------------------------------------------------------------------------------
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

            //Check if a controller is selected for the algorithm and the parameters are all correct
            if (_Scanner.controller !=null && Check_Input_Parameters(arboAlgorithmParams_OK,World.iNumberOfParameters))
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

                //Here begin looping for x generations defined in the world
                for (int i = 1; i < World.iWorldGenerations; i++)
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
                    if (i == 1)
                    {
                        Refresh_Old_List();
                    }

                    //Choose the elite of the actual generation (The highrollers, the motherfuckers, la creme de la creme, the avengers)
                    World.ChooseElite();

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
            else if (_Scanner.controller == null && Check_Input_Parameters(arboAlgorithmParams_OK, World.iNumberOfParameters))
            {
                Results_Windows.Text = "No controller selected, parameters in order";
            }
            else if (_Scanner.controller != null && !Check_Input_Parameters(arboAlgorithmParams_OK, World.iNumberOfParameters))
            {
                Results_Windows.Text = "Controller selected, parameters not in order";
            }
            else
            {
                Results_Windows.Text = "No controller selected and parameters not in order";
            }
 
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


        //INPUT PARAMETERS FILTER
        private void Generation_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            string MessageOutput = "Wrong number of generations. Must be a natural number";
            Check_Numeric_Input(sender, e, 1, MessageOutput);
        }

        private void Mutation_Rate_TextChanged(object sender, TextChangedEventArgs e)
        {
            string MessageOutput = "Wrong mutation rate. Must be between 0 and 100 (decimals with max. 2 digits after the coma: example => 1,23 or 1.23)";
            Check_Numeric_Input(sender, e, 2, MessageOutput);
        }

        private void Breeding_Rate_TextChanged(object sender, TextChangedEventArgs e)
        {
            string MessageOutput = MessageOutput = "Wrong breeding rate. Must be between 0 and 100 (natural numbers)";
            Check_Numeric_Input(sender, e, 3, MessageOutput);
        }

        private void Population_Size_TextChanged(object sender, TextChangedEventArgs e)
        {
            string MessageOutput = "Wrong population size. Must be a natural number";
            Check_Numeric_Input(sender, e, 4, MessageOutput);
        }

        private void Primuses_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            string MessageOutput = "Wrong Primuses number. Must be a natural number excluding 0";
            Check_Numeric_Input(sender, e, 5, MessageOutput);
        }

        private void Check_Numeric_Input(object sender, TextChangedEventArgs e, int Input_Index, string MessageOutput)
        {
            //Get the object
            var textBox = sender as TextBox;
            //Regex token for integers
            bool bNotSomeNotNumbers = Regex.IsMatch(textBox.Text, @"^\d+$");
            //Regex token for decimal x.xx
            bool IsDecimal_2digits = Regex.IsMatch(textBox.Text, @"^\d{1,3}[\.\,]\d{1,2}$");
            bool IsDecimal_0digits = Regex.IsMatch(textBox.Text, @"^\d{1,3}[\.\,]$");
            string text = textBox.Text;

            int TryParse_Out = 0;
            decimal TryParse_Out_d = 0;

            int _Input_Index = Input_Index;
            string _MessageOutput = MessageOutput;

            if (Input_Checked == true && text.Length >= 1)
            {

                if (bNotSomeNotNumbers && (_Input_Index == 1 || _Input_Index == 3 || _Input_Index == 4 || _Input_Index == 5))
                {
                    int.TryParse(textBox.Text, out TryParse_Out);
                    if (_Input_Index == 1)
                    {
                        World.iWorldGenerations = TryParse_Out;
                        Number_Generations_Used.Content = TryParse_Out;
                        MessageOutput = "";
                        arboAlgorithmParams_OK[0] = true;
                        Input_Checked = true;
                    }
                    else if (_Input_Index == 3)
                    {
                       MessageOutput = TryParse_Out > 100 ? "Rate input greater than 100, set at 100" : "";
                       TryParse_Out = TryParse_Out > 100 ? 100 : TryParse_Out;
                       Breeding_Rate_Used.Content = TryParse_Out;
                       World.iBreedingChance = TryParse_Out;
                       arboAlgorithmParams_OK[3] = true;
                       Input_Checked = true;
                    }
                    else if (_Input_Index == 4)
                    {
                        MessageOutput = TryParse_Out == 0 ? "Population size cannot be 0, set at 1" : "";
                        TryParse_Out = TryParse_Out == 0 ? 1 : TryParse_Out;
                        World.PopulationSize = TryParse_Out;
                        Population_Size_Used.Content = TryParse_Out;
                        arboAlgorithmParams_OK[4] = true;
                        Input_Checked = true;
                    }
                    else
                    {
                        MessageOutput = TryParse_Out == 0 ? "Primuses number cannot be 0, set at 1" : "";
                        TryParse_Out = TryParse_Out == 0 ? 1 : TryParse_Out;
                        World.INumberOfPrimuses = TryParse_Out;
                        Primuses_Number_Used.Content = TryParse_Out;
                        arboAlgorithmParams_OK[5] = true;
                        Input_Checked = true;
                    }

                }

                else if ((IsDecimal_2digits || bNotSomeNotNumbers || IsDecimal_0digits) && _Input_Index == 2)
                {
                    MessageOutput = "";
                    decimal.TryParse(textBox.Text, out TryParse_Out_d);
                    MessageOutput = TryParse_Out_d > 100 ? "Rate input greater than 100, set at 100" : "";
                    TryParse_Out_d = TryParse_Out_d > 100 ? 100 : TryParse_Out_d;
                    Mutation_Rate_Used.Content = (double)TryParse_Out_d;
                    World.dMutationChance = (double)TryParse_Out_d;
                    arboAlgorithmParams_OK[2] = true;
                    Input_Checked = true;
                }
                else
                {
                    
                    text = text.Remove(text.Length - 1);
                    switch (_Input_Index)
                    {
                        case 1:
                            Number_Generations_Used.Content = "Error";
                            arboAlgorithmParams_OK[0] = false;
                            break;
                        case 2:
                            Mutation_Rate_Used.Content = "Error";
                            arboAlgorithmParams_OK[2] = false;
                            break;
                        case 3:
                            Breeding_Rate_Used.Content = "Error";
                            arboAlgorithmParams_OK[3] = false;
                            break;
                        case 4:
                            Population_Size_Used.Content = "Error";
                            arboAlgorithmParams_OK[4] = false;
                            break;
                        case 5:
                            Primuses_Number_Used.Content = "Error";
                            arboAlgorithmParams_OK[5] = false;
                            break;
                        default:
                            break;
                    }
                    Input_Checked = false;
                    textBox.Text = text;
                }
            }
            else
            {
                Input_Checked = true;
                switch (_Input_Index)
                {
                    case 1:
                        Number_Generations_Used.Content = "Error";
                        break;
                    case 2:
                        Mutation_Rate_Used.Content = "Error";
                        break;
                    case 3:
                        Breeding_Rate_Used.Content = "Error";
                        break;    
                    default:
                        break;
                }
            }

            Results_Windows.Text = MessageOutput;         
        }

        private bool Check_Input_Parameters(bool[] input_table, int parameters_used)
        {
            bool[] _input_table = input_table;
            bool Parameters_Ok = false;
            int _parameters_used = parameters_used;
            int _parameters_right_count = 0;

            for (int i = 0; i < _input_table.Length; i++)
            {
                if (_input_table[i])
                {
                    _parameters_right_count++;
                }
            }

            return Parameters_Ok = (_parameters_right_count == _parameters_used) ? true : false;

        }

        private void Check_Inputs_Click(object sender, RoutedEventArgs e)
        {
            bool hue;
            string NotInOrder = "Parameters not in order: ";
            hue = Check_Input_Parameters(arboAlgorithmParams_OK, World.iNumberOfParameters);

            if (hue)
            {
                Results_Windows.Text = "Parameters in order";
            }
            else
            {
                NotInOrder = arboAlgorithmParams_OK[0] == false?  NotInOrder + "Wrong generation number; ": NotInOrder;
                NotInOrder = arboAlgorithmParams_OK[1] == false ? NotInOrder + "No method chosen; "       : NotInOrder;
                NotInOrder = arboAlgorithmParams_OK[2] == false ? NotInOrder + "Wrong mutation rate; "    : NotInOrder;
                NotInOrder = arboAlgorithmParams_OK[3] == false ? NotInOrder + "Wrong breeding rate; "    : NotInOrder;
                NotInOrder = arboAlgorithmParams_OK[4] == false ? NotInOrder + "Wrong population size; "  : NotInOrder;
                NotInOrder = arboAlgorithmParams_OK[5] == false ? NotInOrder + "Wrong primuses number; "  : NotInOrder;
                Results_Windows.Text = NotInOrder;
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selection_Method_Struct Method_Selected = (Selection_Method_Struct)Selection_Method.SelectedItem;
            World.iSelectedMethod = Method_Selected.MethodIndex;
            //Method selected
            arboAlgorithmParams_OK[1] = true;

        }

        private void Elitism_used_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox _Elitism = (CheckBox)sender;

            if (_Elitism.IsChecked == true )
            {
                World.iPercentageOfElites = 10;
            }
            else
            {
                World.iPercentageOfElites = 0;
            }
        }

        //DATABASE AREA
        ///////////////////////////////////
        private void RecordIndividual(Individual Idividual_Recorded, int World_Numer, int Generation_Number)
        {
            Royal_Scribe = new Evolution_History_Scribe();
            Individual _Idividual_Recorded = Idividual_Recorded;
            Royal_Scribe.Add_Individual_Record(_Idividual_Recorded, Generation_Number, World_Numer, NewUniverse.Session_Number);
        }


        //  MISCELANEOUS AREA
        ///////////////////////////////////

    }
}
