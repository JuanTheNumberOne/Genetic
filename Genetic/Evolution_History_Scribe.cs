using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genetic;

namespace Evolution_History_Scribe_Space
{
    class Evolution_History_Scribe
    {
        private Evolution_History _History_List;
        public Evolution_History History_List
        {
            get
            {
                return _History_List;
            }
            set
            {
                _History_List = History_List;
            }
        }

        public Evolution_History_Scribe()
        {
            _History_List = new Evolution_History();
            History_List = new Evolution_History();
        }


        public int Add_Individual_Record(Individual Individual_Recorded, int Generation_Number, int World_Number, int Session_Number)
        {
            //Copy method inputs to local variables
            int _Generation_Number = Generation_Number;
            int _World_Number = World_Number;
            int _Session_Number = Session_Number;
            Individual _Individual_Recorded = Individual_Recorded;

            //Create an instance of the entity model
            using (var context = new Machine_Learning_Entities())
            {
                History_List.DNA = _Individual_Recorded.DNA_Code;
                History_List.Generation_Number = _Generation_Number;
                History_List.World_Number = _World_Number;
                History_List.Elapsed_Time = _Individual_Recorded.dTime;
                History_List.Fitness = _Individual_Recorded.dFitnessScore;
                History_List.Weighted_Fitness = _Individual_Recorded.dWeightedFitnessValue;
                History_List.Session_Number = _Session_Number; 


                context.Evolution_History.Add(_History_List);

                context.SaveChanges();
            }
            return 0;
        }

        public List<Individual> Read_Generation_Record (int Generation_Number, int World_Number, int Session_Number)
        {
            //Copy method inputs to local variables
            int _Generation_Number = Generation_Number;
            int _World_Number = World_Number;
            int _Session_Number = Session_Number;
            List<Individual> _Generation_Read_Individuals = new List<Individual>();
            Individual _temp = new Individual();
            List<Evolution_History> _Generation_Read_List = new List<Evolution_History>();

            //Create a list of the retrieved records from the database
            using (var context = new Machine_Learning_Entities())
            {
                //First fetch a copy of the database
                _Generation_Read_List = context.Evolution_History.ToList();

                //Apply filters to get desired records
                _Generation_Read_List = _Generation_Read_List.Where(b => b.Generation_Number == _Generation_Number && b.World_Number == _World_Number && b.Session_Number == _Session_Number)
                                        .OrderByDescending(x => x.Fitness).ThenByDescending(y => y.Elapsed_Time).ToList();
            }

            //Parse that to the list of individuals
            foreach (var item in _Generation_Read_List)
            {
                _temp.DNA_Code = item.DNA;
                _temp.DNA = _temp.Write_DNA_String_To_DNA_Array(_temp.DNA_Code);
                _temp = _temp.WriteDNA_ToParameters(_temp.DNA);
                _temp.dTime =  (decimal)item.Elapsed_Time;
                _temp.dFitnessScore = (decimal)item.Fitness;
                _temp.dWeightedFitnessValue = (decimal)item.Weighted_Fitness;
                _Generation_Read_Individuals.Add(_temp);
            }
            return _Generation_Read_Individuals;
        }

        public List<int> Retrieve_Available_Sessions()
        {
            List<int> Session_Numbers = new List<int>();
            List<Evolution_History> _Session_Read_List = new List<Evolution_History>();

            //Create a list of the retrieved records from the database
            using (var context = new Machine_Learning_Entities())
            {
                //First fetch a copy of the database
                _Session_Read_List = context.Evolution_History.ToList();

                //Apply filters to get desired records
                var Session_Numbers_Linq = _Session_Read_List.OrderBy(x => x.Session_Number).Where(b => b.Session_Number >= 1).Select(u => u.Session_Number).Distinct().ToList();
                
                foreach (var item in Session_Numbers_Linq)
                {
                    Session_Numbers.Add((int)item);
                }

            }

            return Session_Numbers;
        }

        public List<int> Retrieve_Available_Worlds()
        {
            List<int> World_Numbers = new List<int>();
            List<Evolution_History> _World_Read_List = new List<Evolution_History>();

            //Create a list of the retrieved records from the database
            using (var context = new Machine_Learning_Entities())
            {
                //First fetch a copy of the database
                _World_Read_List = context.Evolution_History.ToList();

                //Apply filters to get desired records
                var World_Numbers_Linq = _World_Read_List.OrderBy(x => x.World_Number).Where(b => b.World_Number >= 1).Select(u => u.World_Number).Distinct().ToList();

                foreach (var item in World_Numbers_Linq)
                {
                    World_Numbers.Add((int)item);
                }

            }

            return World_Numbers;
        }

        public List<int> Retrieve_Available_Generations()
        {
            List<int> Generations_Numbers = new List<int>();
            List<Evolution_History> _Generation_Read_List = new List<Evolution_History>();

            //Create a list of the retrieved records from the database
            using (var context = new Machine_Learning_Entities())
            {
                //First fetch a copy of the database
                _Generation_Read_List = context.Evolution_History.ToList();

                //Apply filters to get desired records
                var World_Numbers_Linq = _Generation_Read_List.OrderBy(x => x.Generation_Number).Where(b => b.Generation_Number >= 1).Select(u => u.Generation_Number).Distinct().ToList();

                foreach (var item in World_Numbers_Linq)
                {
                    Generations_Numbers.Add((int)item);
                }

            }

            return Generations_Numbers;
        }
    }
}
