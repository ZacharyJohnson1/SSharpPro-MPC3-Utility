using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using System.Collections.Generic;

namespace SSharpPro_MPC3_Utility
{
    public class ButtonPanelUtility
    {
        private MPC3Basic touchscreen;
        private Dictionary<eButtonName, Action> buttonMap = new Dictionary<eButtonName, Action>();
        private List<List<Feedback>> mutuallyExclusiveSets = new List<List<Feedback>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x201Touchscreen touchscreen)
        {
            this.touchscreen = touchscreen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x101Touchscreen touchscreen)
        {
            this.touchscreen = touchscreen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x102Touchscreen touchscreen)
        {
            this.touchscreen = touchscreen;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x30xTouchscreen touchscreen)
        {
            this.touchscreen = touchscreen;
        }

        /// <summary>
        /// Assigns button presses to actions
        /// </summary>
        public void AssignButton(eButtonName btnName, Action action)
        {
            buttonMap.Add(btnName, action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="btnName"></param>
        public void ExecuteButtonAction(eButtonName btnName)
        {
            if (buttonMap.ContainsKey(btnName))
            {
                buttonMap[btnName].Invoke();
            }
            else
            {
                Debug.Log(">>> Error : " + btnName + " does not exist in dictionary.", Debug.ErrorLevel.Warning, true);
            }
        }

        /// <summary>
        /// Sets button feedback to on or off
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="state"></param>
        public void SetButtonFb(uint btnNum, bool state)
        {
            var set = CheckForMutuallyExclusiveMembership(btnNum);
            if (set != null)
            {
                foreach (var member in set)
                {
                    member.State = false;
                }
            }
            touchscreen.Feedbacks[btnNum].State = state;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="btnNum"></param>
        public void ToggleFeedback(uint btnNum)
        {
            touchscreen.Feedbacks[btnNum].State = !touchscreen.Feedbacks[btnNum].State;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearButtonFb()
        {
            foreach (var btn in touchscreen.Feedbacks)
            {
                btn.State = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="set"></param>
        public void CreateMutuallyExclusiveSet(List<Feedback> set)
        {
            mutuallyExclusiveSets.Add(set);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="btnNum"></param>
        /// <returns></returns>
        private List<Feedback> CheckForMutuallyExclusiveMembership(uint btnNum)
        {
            foreach (var set in mutuallyExclusiveSets)
            {
                if (set.Contains(touchscreen.Feedbacks[btnNum]))
                {
                    return set;
                }
            }
            return null;
        }
    }
}