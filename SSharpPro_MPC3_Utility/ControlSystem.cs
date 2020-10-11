using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using System.Collections.Generic;

namespace SSharpPro_MPC3_Utility
{
    public class ControlSystem : CrestronControlSystem
    {
        #region Fields

        private MPC3x201Touchscreen tp01;
        private ButtonPanelUtility buttonPanelUtility;

        #endregion

        #region Control System
        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;
                
                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                Debug.Log(">>> Error in the constructor: " + e.Message, Debug.ErrorLevel.Error, true);
            }
        }

        #endregion

        #region Initialize System
        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {
                tp01 = this.MPC3x201TouchscreenSlot;

                if (tp01.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    Debug.Log(">>> MPC3 Touchscreen registered successfully.", Debug.ErrorLevel.None, true);
                }
                else
                {
                    Debug.Log(">>> MPC3 Touchscreen failed to register", Debug.ErrorLevel.Error, true);
                }

                tp01.ButtonStateChange += new ButtonEventHandler(tp01_ButtonStateChange);
                Mpc3Initialize(tp01);
            }
            catch (Exception e)
            {
                Debug.Log(">>> Error in InitializeSystem: " + e.Message, Debug.ErrorLevel.Error, true);
            }
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// tp01_ButtonStateChange - ButtonStateChange event handler
        /// </summary>
        /// <param name="device">GenericBase</param>
        /// <param name="args">ButtonEventArgs</param>
        void tp01_ButtonStateChange(GenericBase device, ButtonEventArgs args)
        {
            try
            {
                eButtonName btnName = args.Button.Name;
                uint btnNum = args.Button.Number;
                eButtonState btnState = args.Button.State;
                
                if (btnState == eButtonState.Pressed)
                {
                    CrestronConsole.PrintLine(">>> Button: {0} | State: {1} | Number: {2}", btnName, btnState, btnNum);

                    buttonPanelUtility.ExecuteButtonAction(btnName);

                    if (btnNum > 4)
                    {
                        buttonPanelUtility.SetButtonFb(btnNum, true);
                    }
                    else if (btnNum == 1 || btnNum == 4)
                    {
                        buttonPanelUtility.ToggleFeedback(btnNum);
                    }
                    else if (btnNum == 2)
                    {
                        buttonPanelUtility.DecrementVolumeBar(655);
                    }
                    else if (btnNum == 3)
                    {
                        buttonPanelUtility.IncrementVolumeBar(655);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(">>> Error in tp01_ButtonStateChange: " + e.Message, Debug.ErrorLevel.Error, true);
            }
        }

        /// <summary>
        /// ControlSystem_SerialDataReceived event handler
        /// </summary>
        /// <param name="ReceivingComPort"></param>
        /// <param name="args"></param>
        void ControlSystem_SerialDataReceived(ComPort ReceivingComPort, ComPortSerialDataEventArgs args)
        {

        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }
        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }
        }

        #endregion

        #region Utility Methods
        /// <summary>
        /// mpc3Initialize - sets initial settings for MPC3x201
        /// </summary>
        /// <param name="tp">MPC3x201Touchscreen</param>
        void Mpc3Initialize(MPC3x201Touchscreen tp)
        {
            //enable buttons
            tp.DisableButtonPressBeeping();
            tp.EnablePowerButton();
            tp.EnableMuteButton();
            tp.EnableVolumeUpButton();
            tp.EnableVolumeDownButton();

            //map buttons to methods
            buttonPanelUtility = new ButtonPanelUtility(tp);
            //enable numerical buttons
            buttonPanelUtility.EnableAllNumericalButtons(1,6);
            buttonPanelUtility.SetVolumeBar(32000);

            //assign buttons to methods
            buttonPanelUtility.AssignButton(eButtonName.Power, new Action(() => CrestronConsole.PrintLine("Power Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Mute, new Action(() => CrestronConsole.PrintLine("Mute Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.VolumeUp, new Action(() => CrestronConsole.PrintLine("VolumeUp Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.VolumeDown, new Action(() => CrestronConsole.PrintLine("VolumeDown Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Button1, new Action(() => CrestronConsole.PrintLine("Button1 Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Button2, new Action(() => CrestronConsole.PrintLine("Button2 Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Button3, new Action(() => CrestronConsole.PrintLine("Button3 Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Button4, new Action(() => CrestronConsole.PrintLine("Button4 Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Button5, new Action(() => CrestronConsole.PrintLine("Button5 Command Executed")));
            buttonPanelUtility.AssignButton(eButtonName.Button6, new Action(() => CrestronConsole.PrintLine("Button6 Command Executed")));

            //create mutually exclusive set so only one source button can have a high state at a time
            buttonPanelUtility.CreateMutuallyExclusiveSet(new List<Feedback> {tp.Feedback1, tp.Feedback2, tp.Feedback3, tp.Feedback4, tp.Feedback5, tp.Feedback6});

            //initialize ComPort 1
            if(this.SupportsComPort)
            {
                ComPort comPort01 = this.ComPorts[1];
                if (comPort01.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    Debug.Log(">>> Error registering " + comPort01.ToString(), Debug.ErrorLevel.Error, true);
                }
                else
                {
                    int comSetupSuccess = comPort01.SetComPortSpec(ComPort.eComBaudRates.ComspecBaudRate9600,
                        ComPort.eComDataBits.ComspecDataBits8,
                        ComPort.eComParityType.ComspecParityNone,
                        ComPort.eComStopBits.ComspecStopBits1,
                        ComPort.eComProtocolType.ComspecProtocolRS232,
                        ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                        ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                        false);
                    if (comSetupSuccess == 0)
                    {
                        Debug.Log(">>> [" + this.ToString() + "] comport setup succeeded.", Debug.ErrorLevel.None, true);
                        this.ComPorts[1].SerialDataReceived += new ComPortDataReceivedEvent(ControlSystem_SerialDataReceived);
                    }
                    else
                    {
                        Debug.Log(">>> [" + this.ToString() + "] comport setup failed.", Debug.ErrorLevel.None, true);
                    }
                }
            }
        }

        #endregion
    }
}