using System;
using System.Collections.Generic;

//ABB domain
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;

//IO Read Write domain
using ABB_RW;

//ControllerInfo domain
using Controller_Props;

namespace Network_Scanner_Domain
{
    public class Network_Scanner
    {
        // Variables used to connect to controlers on the network
        public NetworkScanner scanner { get; set; }
        public Controller controller { get; set;} 
        public ABB.Robotics.Controllers.RapidDomain.Task[] tasks { get; set; }
        public NetworkWatcher networkwatcher { get; set; }
        public UserAuthorizationSystem uas { get; set; }

        public Network_Scanner()
        {
            //InitializeComponent();
            scanner = new NetworkScanner();
        }

        public List<Controller_Properties> Scann_For_Controllers()
        {          
            scanner.Scan();
            ControllerInfoCollection controllers = scanner.Controllers;

            List<Controller_Properties> _ControllersInfoList = new List<Controller_Properties>();

            foreach (ControllerInfo controllerInfo in controllers)
            {

                Controller_Properties _ControllerInfo = new Controller_Properties();

                //Get all the data neccesary from the controllerinfo object
                _ControllerInfo.Id = controllerInfo.Id;
                _ControllerInfo.Availability = controllerInfo.Availability.ToString();
                _ControllerInfo.IsVirtual = controllerInfo.IsVirtual.ToString();
                _ControllerInfo.SystemName = controllerInfo.SystemName;
                _ControllerInfo.Version = controllerInfo.Version.ToString();
                _ControllerInfo.ControllerName = controllerInfo.ControllerName;
                _ControllerInfo.IPAddress = controllerInfo.IPAddress.ToString();
                _ControllerInfo.ControllerInfo = controllerInfo;

                //add it to the list
                _ControllersInfoList.Add(_ControllerInfo);

            }
            return _ControllersInfoList;
        }

        private void AddEvents()
        {
            //Event handlers for refreshing the list
            this.networkwatcher = new NetworkWatcher(scanner.Controllers);

            this.networkwatcher.Found += new EventHandler<NetworkWatcherEventArgs>(HandleFoundEvent);
            this.networkwatcher.Lost += new EventHandler<NetworkWatcherEventArgs>(HandleLostEvent);

            this.networkwatcher.EnableRaisingEvents = true;

        }


        // If new controler found add it to the list
        void HandleFoundEvent(object sender, NetworkWatcherEventArgs e)
        {
           // this.Invoke(new EventHandler<NetworkWatcherEventArgs>(AddControllerToListView), new Object[] { this, e });
        }

        // If  controler lost, delete it to the list
        void HandleLostEvent(object sender, NetworkWatcherEventArgs e)
        {
            //this.Invoke(new EventHandler<NetworkWatcherEventArgs>(DeleteControllerToListView), new Object[] { this, e });
        }

        private void Invoke(EventHandler<NetworkWatcherEventArgs> eventHandler, object[] v)
        {
            throw new NotImplementedException();
        }

        private void AddControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            //ControllerInfo controllerInfo = e.Controller;
            //ListViewItem item = null;

            //item = new ListViewItem(controllerInfo.Id);
            //item.SubItems.Add(controllerInfo.Availability.ToString());
            //item.SubItems.Add(controllerInfo.IsVirtual.ToString());
            //item.SubItems.Add(controllerInfo.SystemName);
            //item.SubItems.Add(controllerInfo.Version.ToString());
            //item.SubItems.Add(controllerInfo.ControllerName);
            //item.SubItems.Add(controllerInfo.IPAddress.ToString());
            //this.Network_View.Items.Add(item);
            //item.Tag = controllerInfo;
        }

        

        private void DeleteControllerToListView(object sender, NetworkWatcherEventArgs e)
        {
            //ControllerInfo controllerInfo = e.Controller;
            ////Get the IP adress of the lost controler
            //string sSysteName = controllerInfo.SystemName;

            ////Search the controler in the list
            //foreach (ListViewItem item in Network_View.Items)
            //{
            //    if (item.SubItems[3].Text.Contains(sSysteName))
            //    {
            //        Network_View.Items.Remove(item);
            //    }
            //}

        }

        //Method to connect to the controller
        public string Network_Connect_To_Cotroller(Object ControllerInfo)
        {
            string _infomessage = "";

            if (ControllerInfo != null)
            {
                ControllerInfo controllerInfo = (ControllerInfo)ControllerInfo;
                if (controllerInfo.Availability == Availability.Available)
                {
                    if (controller != null)
                    {
                        controller.Logoff();
                        controller.Dispose();
                        controller = null;
                    }
                    controller = ControllerFactory.CreateFrom(controllerInfo);
                    controller.Logon(UserInfo.DefaultUser);

                    uas = controller.AuthenticationSystem;
                    _infomessage = "Connected to: " + controllerInfo.Name;
                    return _infomessage;

                }
                else
                {
                    if (controller != null)
                    {
                        controller.Logoff();
                        controller.Dispose();
                        controller = null;
                    }

                    _infomessage = "Selected controller not available/ready";
                    return _infomessage;
                }
            }
            else
            {
                _infomessage = "Selected item from list doesn't exist, well shit...";
                return _infomessage;
            }

        }

        //Starts the main program in the controler
        public string Program_Start()
        {
            string _infomessage = "";
            try
            {

                tasks = controller.Rapid.GetTasks();
                using (Mastership m = Mastership.Request(controller.Rapid))
                {

                    //Check if the user has the rights to execute RAPID program
                    if (uas.CheckDemandGrant(Grant.ExecuteRapid))
                    {
                        //Set PP to main an start
                        //if (StartAtMain.Checked)
                        //{
                            tasks[0].ResetProgramPointer();
                        //}
                        controller.Rapid.Start();
                        _infomessage = "The controller's, " + controller.Name + ", program has started";
                        return _infomessage;
                    }
                    _infomessage = "You do not have the grant to execute RAPID programs";
                    return _infomessage;

                }

            }
            catch (System.InvalidOperationException ex)
            {
                _infomessage = "Mastership is held by another client." + ex.Message;
                return _infomessage;
            }
            catch (System.Exception ex)
            {
                _infomessage = "Unexpected error occurred: " + ex.Message;
                return _infomessage;
            }


        }

        public string Program_Stop()
        {
            string _infomessage = "";
            try
            {
                tasks = this.controller.Rapid.GetTasks();
                using (Mastership m = Mastership.Request(this.controller.Rapid))
                {

                    //Check if the user has the rights to execute RAPID program
                    if (uas.CheckDemandGrant(Grant.ExecuteRapid))
                    {
                        this.controller.Rapid.Stop(StopMode.Cycle);
                        _infomessage = "The program has been stopped";
                        return _infomessage;
                    }
                    else _infomessage = "You do not have the grant to execute RAPID programs";
                    return _infomessage;
                }

            }
            catch (System.InvalidOperationException ex)
            {
                _infomessage ="Mastership is held by another client." + ex.Message;
                return _infomessage;
            }
            catch (System.Exception ex)
            {
                _infomessage= "Unexpected error occurred: " + ex.Message;
                return _infomessage;
            }
        }

        public string Set_Reset_Bool(string BoolName, Boolean BoolValue)
        {
            ABB_Data_Write Write = new ABB_Data_Write();
            string _infomessage = "";
            try
            {
                tasks = controller.Rapid.GetTasks();
                using (Mastership m = Mastership.Request(this.controller.Rapid))
                {

                    //Check if the user has the rights to execute RAPID program
                    if (uas.CheckDemandGrant(Grant.ModifyRapidDataValue))
                    {
                        //Modify the rapid value
                        Write.Write_ABB_Boolean(BoolName, "mMain", "T_MAIN", controller, BoolValue);
                        return _infomessage;
                    }
                    else return _infomessage;

                }
            }
            catch (System.InvalidOperationException ex)
            {
                return _infomessage = "Mastership is held by another client." + ex.Message;
            }
            catch (System.Exception ex)
            {
                return _infomessage = "Unexpected error occurred: " + ex.Message;
            }

        }

        public bool Read_Bool(string BoolName)
        {
            ABB_Data_Read Read = new ABB_Data_Read();
            bool bValue = false;
            try
            {
                tasks = controller.Rapid.GetTasks();
                using (Mastership m = Mastership.Request(this.controller.Rapid))
                {

                    //Check if the user has the rights to execute RAPID program
                    if (uas.CheckDemandGrant(Grant.ModifyRapidDataValue))
                    {
                        //Modify the rapid value
                        bValue = Read.Read_ABB_Bool("mMain", "T_MAIN", controller, BoolName);
                        return bValue;
                    }
                    else return bValue;

                }
            }
            catch (System.InvalidOperationException ex)
            {
                //return _infomessage = "Mastership is held by another client." + ex.Message;
                return false;
            }
            catch (System.Exception ex)
            {
                //return _infomessage = "Unexpected error occurred: " + ex.Message;
                return false;
            }

        }

        public string Read_Record_FromArray(int Individual_Index)
        {

            ABB_Data_Read Read_DataRecord = new ABB_Data_Read();
            string Individual_Time = "";

            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    //Check if the user has the rights to execute RAPID program
                    if (uas.CheckDemandGrant(Grant.ModifyRapidDataValue))
                    {

                        Individual_Time = Read_DataRecord.Read_ABB_DataRecord("RawIndividual", "mMain", "T_MAIN", controller, Individual_Index);
                        return Individual_Time;

                    }
                    else return Individual_Time;

                }
                
            }

            catch (System.NullReferenceException ex)
            {
                return Individual_Time;
            }
        }

        public string Write_Record_In_Array(int[] Parameters, int Individual_Index)
        {
            ABB_Data_Write Write_DataRecord = new ABB_Data_Write();
            string _infomessage = "";
            List<string> _StringParameters = new List<string>();
            int[] _Parameters = Parameters;


            //Write the parameters to the string list
            for (int i = 0; i <= 4; i++)
            {
                _StringParameters.Add(_Parameters[i].ToString());
            }
            try
            {
                using (Mastership m = Mastership.Request(controller.Rapid))
                {
                    //Check if the user has the rights to execute RAPID program
                    if (uas.CheckDemandGrant(Grant.ModifyRapidDataValue))
                    {
                        //Modify the rapid record
                        Write_DataRecord.Write_ABB_DataRecord("RawIndividual", "mMain", "T_MAIN", controller, _StringParameters, Individual_Index);
                        _infomessage = "Data written succesfully";
                        return _infomessage;

                    }
                    else return _infomessage = "You do not have the grant to modify RAPID Data";

                }

            }

            catch (System.NullReferenceException ex)
            {
                return _infomessage = "No data in the user defined data type or not logged in a controller";
            }

        }
    }
}