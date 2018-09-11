using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections;
using System.Threading;

//This class represents 
namespace Genetic
{

    class Individual
    {
        //These are the parameters, the DNA code of the individual
        public int iMovementType { get; set; }
        public int iSpeed { get; set; }
        public int iZone { get; set; }
        public int iAcceleration { get; set; }
        public int iAcceleration_Ramp { get; set; }
        public decimal dFitnessScore { get; set;}
        public decimal dTime {get; set;}

        //Variable containing the genome of the individual
        public int[] DNA { get; set; }
        public string DNA_Code {get; set;}

        //Variable defining if the individual is an elite (doesn't mutate and goes to the next generation)
        public bool bIsElite { get; set; }

        //Variable with the weighted value of an individual
        public decimal dWeightedFitnessValue { get; set; }

        //Random numbers
        Random r = new Random();
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        //The constructor should initialize the individual with random variables
        //With RandomType = 1 we use the Random class
        //With RandomType = 2 we use the RNGCryptoServiceProvider class (which is slower but returns true random numbers)

        public Individual()
        {
            DNA = new int[5] {0,0,0,0,0};
        }
       
        public Individual(int RandomType)
        {

            if (RandomType == 1)
            {
                //The new individual shall be tested and obtain a fitness score to see if he meets the elite criteria
                bIsElite = false;

                //Movement type
                if (r.Next(100) <= 50)
                {
                    iMovementType = 1;
                }
                else
                {
                    iMovementType = 0;
                }

                //Speed
                iSpeed = r.Next(1,7000);

                //Zone
                iZone = r.Next(1,200);

                //First acceleration
                iAcceleration = r.Next(1,100);

                //Acceleration Ramp
                iAcceleration_Ramp = r.Next(1,100);

                //For the tests the completion time will be a product of an abstract equation
                //dTime = (iSpeed / 10 + iZone / 2 + iAcceleration * 2 + iAcceleration_Ramp * (3/2))/2; //MAXIMUM IS 725

                //Save the information in the DNA
                DNA = new int[5] {iMovementType, iSpeed,iZone,iAcceleration,iAcceleration_Ramp};
                
                for (int i = 0; i <= 4; i++)
                {
                    if (i > 4 | i == 0)
                    {
                        DNA_Code = DNA_Code + DNA[i].ToString();
                    }
                    else
                    {
                        DNA_Code = DNA_Code + "," + DNA[i].ToString();
                    }

                } 
            }
            else if (RandomType == 2)
            {
              
               // Buffer storage.
               byte[] data = new byte[4];


               // Fill buffer.
               rng.GetBytes(data);

               // Convert to int 32.
               int value = BitConverter.ToInt32(data, 0);
            }    
        }

        private Individual WriteDNA_ToParameters(int[] DNA_Sample)
        {
            Individual _Individual = new Individual();

            _Individual.iMovementType = DNA_Sample[0];
            _Individual.iSpeed = DNA_Sample[1];
            _Individual.iZone = DNA_Sample[2];
            _Individual.iAcceleration = DNA_Sample[3];
            _Individual.iAcceleration_Ramp = DNA_Sample[4];

            for (int i = 0; i <= 4; i++)
            {
                if (i > 4 | i == 0)
                {
                    _Individual.DNA_Code = _Individual.DNA_Code + DNA_Sample[i].ToString();
                }
                else
                {
                    _Individual.DNA_Code = _Individual.DNA_Code + "," + DNA_Sample[i].ToString();
                }

            }
            _Individual.DNA = DNA_Sample;

            //For the tests the completion time will be a product of an abstract equation
            _Individual.dTime = (_Individual.iSpeed / 10 + _Individual.iZone / 2 + _Individual.iAcceleration * 2 + _Individual.iAcceleration_Ramp * (3 / 2)) / 2; //MAXIMUM IS 725

            return _Individual;

        }

        public Individual SwapDna (Individual Individual_1, Individual Individual_2, int iPosition)
        {

            int _iPosition = iPosition;
            Individual _Individual_1 = Individual_1;
            Individual _Individual_2 = Individual_2;
            Individual newBreededIndividual = new Individual();

            if (_Individual_1 == null | _Individual_2 == null)
            {
                return newBreededIndividual;
            }

            if ( _Individual_1.DNA_Code != _Individual_2.DNA_Code)
            {
                
                //Load the genes of the first individual
                for (int i = 0; i <= 4; i++)
                {
                    if (i <= _iPosition)
                    {
                        newBreededIndividual.DNA[i] = _Individual_1.DNA[i];
                    }
                    else
                    {
                        newBreededIndividual.DNA[i] = _Individual_2.DNA[i];
                    }

                }

            }

            newBreededIndividual = WriteDNA_ToParameters(newBreededIndividual.DNA);
            return newBreededIndividual;

        }

        public Individual Mutate(Individual _IndividualAboutToMutate, double MutationChance)
        {
            Individual _Individual = _IndividualAboutToMutate;
            double _dMutationRoll = r.Next(0, 100);
            Thread.Sleep(100);
            double _dMutationChance = MutationChance;

            //Check if it is an elite. Elites are immune to mutation
            if (_Individual.bIsElite)
            {
                return _Individual;
            }

            if (_dMutationChance >= _dMutationRoll)
            {
                
                int _iRandomNumber = r.Next(0, 4);
                Thread.Sleep(100);

                switch (_iRandomNumber)
                {
                    case 0:
                        if (r.Next(100) <= 50)
                        {
                            _IndividualAboutToMutate.iMovementType = 1;
                        }
                        else
                        {
                            _IndividualAboutToMutate.iMovementType = 0;
                        }
                        _Individual.DNA[0] = _IndividualAboutToMutate.iMovementType;
                        break;

                    case 1:
                        _IndividualAboutToMutate.iSpeed = r.Next(1, 7000);
                        _Individual.DNA[1] = _IndividualAboutToMutate.iSpeed;
                        break;

                    case 2:
                        _IndividualAboutToMutate.iZone = r.Next(1, 200);
                        _Individual.DNA[2] = _IndividualAboutToMutate.iZone;
                        break;

                    case 3:
                        _IndividualAboutToMutate.iAcceleration = r.Next(1, 100);
                        _Individual.DNA[3] = _IndividualAboutToMutate.iAcceleration;
                        break;

                    case 4:
                        _IndividualAboutToMutate.iAcceleration_Ramp = r.Next(1, 100);
                        _Individual.DNA[4] = _IndividualAboutToMutate.iAcceleration_Ramp;
                        break;
                }
                //For the tests the completion time will be a product of an abstract equation
                _IndividualAboutToMutate.dTime = (_IndividualAboutToMutate.iSpeed / 10 + _IndividualAboutToMutate.iZone / 2 + _IndividualAboutToMutate.iAcceleration 
                 * 2 + _IndividualAboutToMutate.iAcceleration_Ramp * (3 / 2)) / 2; //MAXIMUM IS 725

                _Individual.DNA_Code = "";
                for (int i = 0; i <= 4; i++)
                {
                    if (i > 4 | i == 0)
                    {
                        _Individual.DNA_Code = _Individual.DNA_Code + _Individual.DNA[i].ToString();
                    }
                    else
                    {
                        _Individual.DNA_Code = _Individual.DNA_Code + "," + _Individual.DNA[i].ToString();
                    }

                }

            }
            return _Individual;
        }
    }
}
