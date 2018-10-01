using System;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography;

namespace Genetic
{
    class World
    {
        //List containing the population
        public static List<Individual> Population { get; set;}
        //The size the population should be at the beggining of each new generation
        public static int PopulationSize {get; set;}
        //Reference completion time
        public static decimal dCompletionTime { get; set;}
        //Chance a breeding will be succesful (between 0 and 100)
        public static int iBreedingChance { get; set;}
        //Chance a breeding will be succesful (between 0 and 100)
        public static double dMutationChance { get; set; }
        //Percentage of elites in a population
        public static float iPercentageOfElites { get; set; }
        //Total fitness of the current population
        public static decimal decTotalFitness { get; set; }
        //Inbreeding allowance
        public static bool bAllowInnBreeding { get; set; }
        //Wolrd index
        public static int iWolrdIndex { get; set; }
        //Number of generations in the world
        public static int iWorldGenerations { get; set; }
        //Method of selection (1 = biased roulette, 2 = Tournament, 3 = Combined Biased+Tournament)
        public static int iSelectedMethod { get; set; }
        //Number of Primuses used
        public static int INumberOfPrimuses { get; set; }
        //Number of parameters used
        public static int iNumberOfParameters { get; set; }

        //Random numbers
        static Random r = new Random();
        static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        //Creates a population of the specified size
        public static void CreateGeneration(int iPopulationSizeOfGeneration)
        {
            int _iGenerationPopulationSize = iPopulationSizeOfGeneration;
            List<Individual> _Population = new List<Individual>();

            for (int i = 0; i < _iGenerationPopulationSize; i++)
            {
                _Population.Add(new Individual(1));
                Thread.Sleep(100);
            }

            Population = _Population;

        }


        //Adds the desired number of individuals to the list
        public static void AddIndividuals(int NumberOfNewcomers)
        {
            int _BatchSize = NumberOfNewcomers;
            List<Individual> _NewComers = new List<Individual>();
            _NewComers = Population;

            for (int i = 0; i < _BatchSize; i++)
            {
                _NewComers.Add(new Individual(1));
                Thread.Sleep(100);
            }

            Population = _NewComers;
            Thread.Sleep(0);

        }

        //Calculate total fitness of a population and of an individual. Return -1 if no population is present
        private static decimal CalculateFitness (Individual Test)
        {
            decimal FitnessScore = 0;
            

            if (Population != null)
            {


                //Check if the completion time is lower or 0. That cannot be. Return 0 to signalize an error
                if (dCompletionTime <= 0)
                {
                    return -1;
                }

                FitnessScore = Math.Round(dCompletionTime / Test.dTime, 2);
                decTotalFitness = Math.Round(decTotalFitness + FitnessScore, 2);
                return FitnessScore;
            }
            else
            {
                return -1;
            }
        }

        //Calculate total fitness of a population . Return -1 if no population is present
        public static void CalculateFitnessPopulation ()
        {
            //Calculate fitness of the population
            foreach (Individual item in World.Population)
            {
                    item.dFitnessScore = World.CalculateFitness(item);
            }

            //Calculate the weighted fitness
            foreach (Individual item in World.Population)
            {
                item.dWeightedFitnessValue = Math.Round((item.dFitnessScore / World.decTotalFitness) * 100, 2);
            }
        }

        //Method for checking if the population size is correct at the beginning of a generation
        // Returns 3 if there is no population, 2 if there is underpopulation, 1 if there is overpopulation and 0 if it is just right
        public static int CheckIfPopulationisComplete()
        {
            if (Population == null)
            {
                return 3;
            }

            int iActualPopulationSize = Population.Count;

            if (iActualPopulationSize < PopulationSize)
            {
                return 2;
            }
            else if (iActualPopulationSize > PopulationSize)
            {
                return 1;
            }
            else return 0;

        }

        //Return true if the individual has been eliminated, if not false
        //Used to kill an individual in the actual generation
        public static bool KillIndividual (int IndividualPointer)
        {
            //Check if a population exists
            if (Population == null )
            {
                return false;
            }

            if (Population.Count <= 0 | IndividualPointer > Population.Count - 1)
            {
                return false;
            }

            if (IndividualPointer < 0)
            {
                return false;
            }

            //Check if the individual exists
            if (Population.Contains(Population[IndividualPointer]))
            {
                Population.RemoveAt(IndividualPointer);
                return true;
            }
            else return false;
        }


        //Kills a random number of individuals. Each individual rolls a dice.
        //Depending on what it is rolled and the fitness of the individual it can either
        //survive or perish. This method represents random situations (famine, diseases, accidents) that can occur
        //to each individual in an era. Returns 1 if an individual has survived, 2 if it has perished, 0 if there is an error
        public static int SurvivalTest(decimal dThreshold, Individual IndividualTested)
        {
            decimal _dThreshold = dThreshold;

            //Check if a population exists
            if (IndividualTested == null)
            {
                return 0;
            }

            if (dThreshold <= 0)
            {
                return 0;
            }

            if (IndividualTested.dFitnessScore >= dThreshold)
            {
                return 1;
            }
            else
            {
                return 2;
            }

        }

        //Determines if an individual is chosen for breeding
        //Returns 0 if the individual has failed the test, returns 1 if it succeded
        public static int MatingTest(Individual IndividualTested)
        {

            Individual _IndividualTested = IndividualTested;
            decimal _IndividualBreedChance = _IndividualTested.dWeightedFitnessValue;
            decimal _dRandomRoll = r.Next(1, 100);
            _IndividualBreedChance = _IndividualBreedChance + 30;
            Thread.Sleep(100);

            _dRandomRoll = Math.Round(_dRandomRoll, 2);

            if (_IndividualBreedChance >= _dRandomRoll)
            {
                return 1;

            }
            else
            {
                return 0;
            }  

        }
        
        //Determines the winner of the single-elimination tournament
        //Returns the survivor of the tournament (audaces fortuna iuvat!)
        public static Individual Tournament_Round(List<Individual> Tournament_Group)
        {

            List<Individual> _Tournament_Group = Tournament_Group;
            Individual _Gladiator_1, _Gladiator_2, _Victor_Gladiator;
            int Rounds = _Tournament_Group.Count - 1;  // In a knockout torunament is R = n-1
            int _iRandomRoll_1, _iRandomRoll_2, Winning_Chance , _Combat_Result;
            int _Fight_Concluded = 0;

            for (int i = 1; i <= Rounds; i++)
            {
                //Check for final round
                if (i == Rounds)
                {
                    _iRandomRoll_1 = 0;
                    _iRandomRoll_2 = 1;
                    _Gladiator_1 = _Tournament_Group[_iRandomRoll_1];
                    _Gladiator_2 = _Tournament_Group[_iRandomRoll_2];
                }
                else
                {
                    //Roll a number to select first oponent
                    _iRandomRoll_1 = r.Next(0, _Tournament_Group.Count - 1);
                    _Gladiator_1 = _Tournament_Group[_iRandomRoll_1];
                    Thread.Sleep(100);

                    //Roll a number to select second oponent (make sure the same oponent is not selected)
                    do
                    {
                        _iRandomRoll_2 = r.Next(0, _Tournament_Group.Count - 1);
                        Thread.Sleep(100);
                    } while (_iRandomRoll_2 == _iRandomRoll_1);

                    _Gladiator_2 = _Tournament_Group[_iRandomRoll_2];
                }
                

                //Set the probabilities of winning
                if (_Gladiator_1.dFitnessScore > _Gladiator_2.dFitnessScore)
                {
                    Winning_Chance = 80;
  
                }
                else if (_Gladiator_1.dFitnessScore < _Gladiator_2.dFitnessScore)
                {
                    Winning_Chance = 20;
                }
                //Equal probability of winning
                else
                {
                    Winning_Chance = 0;
                }

                //Here the two individuals fight
                while (_Fight_Concluded == 0)
                {
                    _Combat_Result = r.Next(0, 100);

                    if (Winning_Chance != 0)
                    {
                        _Fight_Concluded = _Combat_Result >= Winning_Chance ? 1 : 2;
                    }
                    else
                    {
                        _Fight_Concluded = _Combat_Result < 50 ? 1 : _Combat_Result == 50 ? 0 : 2;
                    }
                    Thread.Sleep(100);
                }

                //Check who survived the fight
                if (_Fight_Concluded == 1)
                {
                    _Tournament_Group.RemoveAt(_iRandomRoll_1);
                }

                else
                {
                    _Tournament_Group.RemoveAt(_iRandomRoll_2);
                }
                //Set a new fight
                _Fight_Concluded = 0;

            }

            //Return the survivor
            _Victor_Gladiator = _Tournament_Group[0];
            return _Victor_Gladiator;

        }

        //The breeding method creates/breeds a new individual. This new individual's DNA is made out
        //of two separate individuals (the parents), that enter the method as its parameters (well the pointers/indexes to them).
        //The breeding does not always occur. The probability of happening is defined by the variable iBreedingChance.
        //This is supposed to represent different situations that can occur during a breeding (miscarriage, premature death of the newborn, etc).

        public static Individual BreedIndividuals (int Individual_1_index, int Individual_2_index, List<Individual> _BreedingPool)
        {
            int _iRandomNumber = r.Next(1,100);
            Thread.Sleep(100);
            int _DNASwapPercentage = r.Next(0,4);
            Individual _Individual_1;
            Individual _Individual_2;
            Individual _Newborn = new Individual();

            if (Individual_1_index < 0 | Individual_1_index > _BreedingPool.Count - 1)
            {
                return _Newborn;
            }

            if (Individual_2_index < 0 | Individual_2_index > _BreedingPool.Count - 1)
            {
                return _Newborn;
            }

            if (iBreedingChance > 0 && _iRandomNumber <= iBreedingChance)
            {
                _Individual_1 = _BreedingPool[Individual_1_index];
                _Individual_2 = _BreedingPool[Individual_2_index];
                _Newborn = _Newborn.SwapDna(_Individual_1, _Individual_2, _DNASwapPercentage);
                return _Newborn;
            }
            else
            {
                return _Newborn;
            }

        }

        //This method chooses the elite from the population
        public static void ChooseElite()
        {
            
            int _PopulationCount = World.Population.Count;
            double _fNumberOfElites;

            _fNumberOfElites = _PopulationCount * (iPercentageOfElites / 100);
            _fNumberOfElites = Math.Round(_fNumberOfElites,0);

            for (int i = 0; i < _PopulationCount; i++)
            {
                if (i < _fNumberOfElites)
                {
                    World.Population[i].bIsElite = true;
                }
                else
                {
                    World.Population[i].bIsElite = false;
                }
                
            }

        }

        //Method to create the next generation. Only elites and breeded individuals are allowed to the next generation.
        //The breeding in this case is a stochastic method, the biased roulette. In short every member, even the weak ones, can
        //be selected to breed, it is just that they have a lesser chance of doing so (stronger individuals are more likely to breed)
        public static List<Individual> BreedNextGeneration_Roullete()
        {

            List<Individual> _OldGeneration = Population;
            List<Individual> _NewGeneration = new List<Individual>();
            List<Individual> _BreedingRoom = new List<Individual>();
            Individual _Breeded = new Individual();

            int iRandomPickIndex = 0;
            int iIndividualPicked = 0;
            
            int _EliteCount;
   
            //Add the elites to the new generation. Elites are carried trough generations to preserve the good traits
            foreach (Individual item in _OldGeneration)
            {
                if (item.bIsElite)
                {
                    _NewGeneration.Add(item);
                }
                
            }

            _EliteCount = _NewGeneration.Count;

            //Now breed until a new generation is born
            //For breeding each iteration a random 
            
            do
            {
                do
                {
                    iRandomPickIndex = r.Next(0, (_OldGeneration.Count - 1));
                    Thread.Sleep(100);
                    iIndividualPicked = World.MatingTest(_OldGeneration[iRandomPickIndex]);

                    if (iIndividualPicked == 1)
                    {
                        _BreedingRoom.Add(_OldGeneration[iRandomPickIndex]);
                        _OldGeneration.RemoveAt(iRandomPickIndex);
                    }

                } while (_BreedingRoom.Count < 2);

                if (_BreedingRoom[0].DNA_Code != _BreedingRoom[1].DNA_Code | bAllowInnBreeding == true)
                {
                    //Mutate and Add breeded newborn to new generation
                    _Breeded = World.BreedIndividuals(0, 1, _BreedingRoom);
                    _Breeded = _Breeded.Mutate(_Breeded, dMutationChance);
                    _NewGeneration.Add(_Breeded);

                    //Return Parents to old generation
                    _OldGeneration.Add(_BreedingRoom[0]);
                    _OldGeneration.Add(_BreedingRoom[1]);
                    _BreedingRoom.RemoveAt(1);
                    _BreedingRoom.RemoveAt(0);

                    //Re-sort the old generation
                    _OldGeneration.Sort((x, y) => y.dFitnessScore.CompareTo(x.dFitnessScore));

                }
                else
                {
                    //Return Parents to old generation 
                    _OldGeneration.Add(_BreedingRoom[0]);
                    _OldGeneration.Add(_BreedingRoom[1]);
                    _BreedingRoom.RemoveAt(1);
                    _BreedingRoom.RemoveAt(0);
                    //Add a new random individual to the population (an inmigrant)
                    _NewGeneration.Add(new Individual(1));
                }
                
                //repeat breeding proccess until the new generation is born

            } while (_NewGeneration.Count < PopulationSize);

            return _NewGeneration;
        }

        public static List<Individual> BreedNextGeneration_Tournament()
        {
            List<Individual> _OldGeneration = Population;
            List<Individual> _Population_Poll = new List<Individual>();
            List<Individual> _NewGeneration = new List<Individual>();
            List<Individual> _BreedingRoom = new List<Individual>();
            List<Individual> _Tournament_Group = new List<Individual>();
            Individual Survivor;
            Individual _Breeded = new Individual();

            int iRandomPickIndex = 0;
            int iRandomTournamentSize = 0;

            int _EliteCount;

            //Add the elites to the new generation. Elites are carried trough generations to preserve the good traits
            foreach (Individual item in _OldGeneration)
            {
                if (item.bIsElite)
                {
                    _NewGeneration.Add(item);
                }

            }

            _EliteCount = _NewGeneration.Count;

            //Now breed until a new generation is born
            //For breeding each iteration a random 

            do
            {

                do
                {
                    foreach (Individual item in _OldGeneration)
                    {
                        _Population_Poll.Add(item);
                    }

                    iRandomTournamentSize = r.Next(2, (_Population_Poll.Count));
                    Thread.Sleep(100);
                    do
                    {
                        iRandomPickIndex = r.Next(0, (_Population_Poll.Count - 1));
                        Thread.Sleep(100);
                        _Tournament_Group.Add(_Population_Poll[iRandomPickIndex]);
                        _Population_Poll.RemoveAt(iRandomPickIndex);

                    } while (_Tournament_Group.Count < iRandomTournamentSize);
                   

                     Survivor = World.Tournament_Round(_Tournament_Group);
                    _BreedingRoom.Add(Survivor);
                    _Population_Poll = new List<Individual>();
                    _OldGeneration.Remove(Survivor);

                } while (_BreedingRoom.Count < 2);

                if (_BreedingRoom[0].DNA_Code != _BreedingRoom[1].DNA_Code | bAllowInnBreeding == true)
                {
                    //Mutate and Add breeded newborn to new generation
                    _Breeded = World.BreedIndividuals(0, 1, _BreedingRoom);
                    _Breeded = _Breeded.Mutate(_Breeded, dMutationChance);
                    _NewGeneration.Add(_Breeded);

                    //Return Parents to old generation
                    _OldGeneration.Add(_BreedingRoom[0]);
                    _OldGeneration.Add(_BreedingRoom[1]);
                    _BreedingRoom.RemoveAt(1);
                    _BreedingRoom.RemoveAt(0);

                    //Re-sort the old generation
                    _OldGeneration.Sort((x, y) => y.dFitnessScore.CompareTo(x.dFitnessScore));

                }
                else
                {
                    //Return Parents to old generation 
                    _OldGeneration.Add(_BreedingRoom[0]);
                    _OldGeneration.Add(_BreedingRoom[1]);
                    _BreedingRoom.RemoveAt(1);
                    _BreedingRoom.RemoveAt(0);
                    //Add a new random individual to the population (an inmigrant)
                    _NewGeneration.Add(new Individual(1));
                }

                //repeat breeding proccess until the new generation is born

            } while (_NewGeneration.Count < PopulationSize);

            return _NewGeneration;
        }

        public static List<Individual> BreedNextGeneration_T_R()
        {
            List<Individual> _OldGeneration = Population;
            List<Individual> _NewGeneration = new List<Individual>();
            List<Individual> _BreedingRoom = new List<Individual>();
            Individual _Breeded = new Individual();

            return _NewGeneration;
        }

    }
}
