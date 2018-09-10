using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;

//ABB domain
using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;

namespace ABB_RW
{
    class ABB_Data_Write
    {

        //Data domain
        private Num rapid_num;
        private Bool rapid_bool;
        private RapidData rd;
        private RapidData rd_array;
        private RapidDataType rdt;
        private ArrayData ad;

        //Functions and Methods


        // WRITE --------------------------------------------------------------------------------------------------------------
        public void Write_ABB_Num(string Var_Name, string Module_Name, string Task_Name, Controller aController, int Value)
        {

            string L_Var_Name = Var_Name;
            string L_Module_Name = Module_Name;
            string L_Task_Name = Task_Name;
            Controller L_aController = aController;
            rapid_num.Value = Value;

            try
            {

                rd = L_aController.Rapid.GetRapidData(Task_Name, Module_Name, Var_Name);
                if (rd.Value is Num)
                {
                    //Write the value
                    rd.Value = rapid_num;

                }

            }

            catch (ABB.Robotics.Controllers.RapidDomain.RapidModuleNotFoundException ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            catch (ABB.Robotics.Controllers.RapidDomain.RapidSymbolNotFoundException ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            catch (ABB.Robotics.GenericControllerException ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            catch (System.Exception ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            finally
            {
                // Release resources
            }

        }

        public void Write_ABB_Boolean(string Var_Name, string Module_Name, string Task_Name, Controller aController, Boolean Bit)
        {

            string L_Var_Name = Var_Name;
            string L_Module_Name = Module_Name;
            string L_Task_Name = Task_Name;
            Controller L_aController = aController;
            rapid_bool.Value = Bit;

            try
            {

                rd = L_aController.Rapid.GetRapidData(Task_Name, Module_Name, Var_Name);
                if (rd.Value is Bool)
                {
                    //Write the value
                    rd.Value = rapid_bool;

                }

            }

            catch (ABB.Robotics.Controllers.RapidDomain.RapidModuleNotFoundException ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            catch (ABB.Robotics.Controllers.RapidDomain.RapidSymbolNotFoundException ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            catch (ABB.Robotics.GenericControllerException ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            catch (System.Exception ee)
            {
                // TODO: Add error handling
                //MessageBox.Show("Error: " + ee.Message);
            }
            finally
            {
                // Release resources
            }

        }

        public void Write_ABB_DataRecord(string Data_Record_Name, string Module_Name, string Task_Name, Controller aController, List<string> Variables, int ArrayIndex)
        {

            string L_Module_Name = Module_Name;
            string L_Task_Name = Task_Name;
            string L_Data_Record_Name = Data_Record_Name;
            List<string> _Variables = Variables;
            Controller L_aController = aController;
            int _ArrayIndex = ArrayIndex;
            string _info;

            try
            {

                //Get the array with the records
                rd_array = aController.Rapid.GetRapidData(Task_Name, Module_Name, "RawIndividuals");
                ad = (ArrayData)rd_array.Value;
                int aRank = ad.Rank;

                //Read the record
                rd = L_aController.Rapid.GetRapidData(Task_Name, Module_Name, L_Data_Record_Name);
                rdt = L_aController.Rapid.GetRapidDataType(L_Task_Name, L_Module_Name, L_Data_Record_Name);
                UserDefined processdata = new UserDefined(rdt);        

                //Prepare the parameters
                for (int i = 0; i < 5; i++)
                {
                    processdata.Components[i].FillFromString(_Variables[i]);
                }

                ////Apply the value
                //rd.Value = processdata;

                //Add the parameters to the array
                rd_array.Value = ad;
                ad[ArrayIndex] = processdata;

            }

            catch (ABB.Robotics.Controllers.RapidDomain.RapidModuleNotFoundException ee)
            {
                _info = "Error: " + ee.Message;  
            }
            catch (ABB.Robotics.Controllers.RapidDomain.RapidSymbolNotFoundException ee)
            {
                _info = "Error: " + ee.Message;
            }
            catch (ABB.Robotics.GenericControllerException ee)
            {
                _info = "Error: " + ee.Message;
            }
            catch (System.Exception ee)
            {
                _info = "Error: " + ee.Message;
            }
            finally
            {
                // Release resources
            }

        }
    }


    // READ --------------------------------------------------------------------------------------------------------------

    class ABB_Data_Read
    {

        //Data domain
        private RapidData rd;
        private RapidDataType rdt;

        //Functions


        public List<string> Read_ABB_DataRecord(string Data_Record_Name, string Module_Name, string Task_Name, Controller aController)
        {

            List<string> Data_Records_List = new List<string>();

            string L_Module_Name = Module_Name;
            string L_Task_Name = Task_Name;
            string L_Data_Record_Name = Data_Record_Name;
            Controller L_aController = aController;

            rd = L_aController.Rapid.GetRapidData(Task_Name, Module_Name, L_Data_Record_Name);
            rdt = L_aController.Rapid.GetRapidDataType(L_Task_Name, L_Module_Name, L_Data_Record_Name);

            UserDefined processdata = new UserDefined(rdt);
            processdata = (UserDefined)rd.Value;

            Data_Records_List.Add(processdata.Components[0].ToString());
            Data_Records_List.Add(processdata.Components[1].ToString());
            Data_Records_List.Add(processdata.Components[2].ToString());


            return Data_Records_List;
        }
    }
}
