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
                _Generation_Read_Individuals.Add(_temp);
            }
            return _Generation_Read_Individuals;
        }
    }
}
