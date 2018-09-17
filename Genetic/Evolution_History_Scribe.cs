using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public int Add_Individual_Record(Individual Individual_Recorded, int Generation_Number, int World_Number)
        {
            //Copy method inputs to local variables
            int _Generation_Number = Generation_Number;
            int _World_Number = World_Number;
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
    
                context.Evolution_History.Add(_History_List);

                context.SaveChanges();
            }
            return 0;
        }
    }
}
