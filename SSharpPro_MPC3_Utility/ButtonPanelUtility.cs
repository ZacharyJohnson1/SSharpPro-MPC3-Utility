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
        /// <summary>
        /// generic MPC3 touchscreen object
        /// </summary>
        private MPC3Basic _touchscreen { get; set; }

        /// <summary>
        /// stores the button name and user-definied method as key value pairs
        /// </summary>
        private Dictionary<eButtonName, Action> _buttonMap = new Dictionary<eButtonName, Action>();

        /// <summary>
        /// list of list of Feedback objects that stores user-definied 
        /// mutually exclusive sets.
        /// when a set is definied as mutually exclusive, only one button
        /// from that set can be in a "high" state at a time.
        /// </summary>
        private List<List<Feedback>> _mutuallyExclusiveSets = new List<List<Feedback>>();

        /// <summary>
        /// current value of bargraph
        /// </summary>
        private ushort _currentBargraphValue;

        /// <summary>
        /// specifies the touchscreen as a MPC3x201 type
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x201Touchscreen touchscreen)
        {
            this._touchscreen = touchscreen;
        }

        /// <summary>
        /// specifies the touchscreen as a MPC3x101 type
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x101Touchscreen touchscreen)
        {
            this._touchscreen = touchscreen;
        }

        /// <summary>
        /// specifies the touchscreen as a MPC3x102 type
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x102Touchscreen touchscreen)
        {
            this._touchscreen = touchscreen;
        }

        /// <summary>
        /// specifies the touchscreen as a MPC3x30x type
        /// </summary>
        /// <param name="touchscreen"></param>
        public ButtonPanelUtility(MPC3x30xTouchscreen touchscreen)
        {
            this._touchscreen = touchscreen;
        }

        /// <summary>
        /// Assigns button presses to actions
        /// </summary>
        public void AssignButton(eButtonName btnName, Action action)
        {
            _buttonMap.Add(btnName, action);
        }

        /// <summary>
        /// uses the button name as the key to execute the 
        /// method value associated with it in the _buttonMap dictionary
        /// if the key is not found, print to ErrorLog
        /// </summary>
        /// <param name="btnName">button name</param>
        public void ExecuteButtonAction(eButtonName btnName)
        {
            if (_buttonMap.ContainsKey(btnName))
            {
                var exeBtnDel = _buttonMap[btnName];
                if (exeBtnDel != null)
                {
                    exeBtnDel.Invoke();
                }
            }
            else
            {
                Debug.Log(">>> Error : " + btnName + " does not exist in dictionary.", Debug.ErrorLevel.Warning, true);
                throw new KeyNotFoundException("Error: " + btnName + " does not exist in dictionary");
            }
        }

        /// <summary>
        /// Sets button feedback to on or off
        /// </summary>
        /// <param name="btnNum">button number</param>
        /// <param name="state">state to set button</param>
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
            _touchscreen.Feedbacks[btnNum].State = state;
        }

        /// <summary>
        /// toggles a button's feedback depending on its current state
        /// given a button number
        /// </summary>
        /// <param name="btnNum">button number</param>
        public void ToggleFeedback(uint btnNum)
        {
            _touchscreen.Feedbacks[btnNum].State = !_touchscreen.Feedbacks[btnNum].State;
        }

        /// <summary>
        /// clears feedback from all buttons in Feedbacks list
        /// </summary>
        public void ClearButtonFb()
        {
            foreach (var btn in _touchscreen.Feedbacks)
            {
                btn.State = false;
            }
        }

        /// <summary>
        /// creates a user-defined mutually exclusive set of type List<Feedback>
        /// and add its to the _mutuallyExclusiveSets list
        /// </summary>
        /// <param name="set">List<Feedback</Feedback></param>
        public void CreateMutuallyExclusiveSet(List<Feedback> set)
        {
            _mutuallyExclusiveSets.Add(set);
        }

        /// <summary>
        /// remove a List of Feedback objects from the list of mutually exclusive sets
        /// given a list of type Feedback
        /// </summary>
        /// <param name="set">list to remove</param>
        public void RemoveMutuallyExclusiveSet(List<Feedback> set)
        {
            _mutuallyExclusiveSets.Remove(set);
        }

        /// <summary>
        /// remove a List of Feedback objects from the list of mutually exclusive sets
        /// given an index of type int
        /// </summary>
        /// <param name="index">index of list to remove</param>
        public void RemoveMutuallyExclusiveSet(int index)
        {
            _mutuallyExclusiveSets.RemoveAt(index);
        }

        /// <summary>
        /// checks if button is part of a user-defined mutually exclusive set 
        /// and returns that set
        /// </summary>
        /// <param name="btnNum">number of button</param>
        /// <returns>List<Feedback></returns>
        private List<Feedback> CheckForMutuallyExclusiveMembership(uint btnNum)
        {
            foreach (var set in _mutuallyExclusiveSets)
            {
                if (set.Contains(_touchscreen.Feedbacks[btnNum]))
                {
                    return set;
                }
            }
            return null;
        }

        /// <summary>
        /// sets volume bar graph
        /// range: 0 - 65535
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolumeBar(ushort volume)
        {
            _touchscreen.VolumeBargraph.UShortValue = volume;
            _currentBargraphValue = _touchscreen.VolumeBargraph.UShortValue;
        }

        /// <summary>
        /// return volume bars current value
        /// range: 0 - 65535
        /// </summary>
        /// <returns></returns>
        public ushort GetVolumeBar()
        {
            return _touchscreen.VolumeBargraph.UShortValue;
        }

        /// <summary>
        /// increment volume bar graph by user-defined offset
        /// range: 0 - 65535
        /// </summary>
        /// <param name="offset"></param>
        public void IncrementVolumeBar(ushort offset)
        {
            if (65535 - _currentBargraphValue > offset)
                _currentBargraphValue += offset;
            else
                _currentBargraphValue = 65535;
            SetVolumeBar((_currentBargraphValue));
        }

        /// <summary>
        /// decrement volume bar graph by user-defined offset
        /// range: 0 - 65535
        /// </summary>
        /// <param name="offset"></param>
        public void DecrementVolumeBar(ushort offset)
        {
            if (_currentBargraphValue > offset)
                _currentBargraphValue -= offset;
            else
                _currentBargraphValue = 0;
            SetVolumeBar((_currentBargraphValue));
        }

        /// <summary>
        /// enables a numerical button given the button number
        /// </summary>
        /// <param name="btnNum"></param>
        public void EnableNumericalButton(uint btnNum)
        {
            _touchscreen.EnableNumericalButton(btnNum);
        }

        /// <summary>
        /// disables a numerical button given the button number
        /// </summary>
        /// <param name="btnNum"></param>
        public void DisableNumericalButton(uint btnNum)
        {
            _touchscreen.DisableNumericalButton(btnNum);
        }

        /// <summary>
        /// Enable range of Numerical buttons depending on touchscreen type
        /// </summary>
        public void EnableNumericalButtons(uint btnStart, uint btnStop)
        {
            for (uint btn = btnStart; btn <= btnStop; btn++)
            {
                _touchscreen.EnableNumericalButton(btn);
            }
        }

        /// <summary>
        /// sets active brightness of buttons
        /// range: 0 - 65535
        /// </summary>
        /// <param name="value">brightness value</param>
        public void SetActiveBrightness(ushort value)
        {
            _touchscreen.ActiveBrightness.UShortValue = value;
        }

        /// <summary>
        /// returns the active brightness of buttons
        /// range: 0 - 65535
        /// </summary>
        /// <returns>brightness value</returns>
        public ushort GetActiveBrightness()
        {
            return _touchscreen.ActiveBrightnessFeedBack.UShortValue;
        }

        /// <summary>
        /// sets active timeout in minutes
        /// range: 0 - 65535
        /// </summary>
        /// <param name="minutes">the number of minutes before timeout</param>
        public void SetActiveTimeout(ushort minutes)
        {
            _touchscreen.ActiveTimeout.UShortValue = minutes;
        }

        /// <summary>
        /// gets the active timeout in minutes
        /// range: 0 - 65535
        /// </summary>
        /// <returns>active timeout in minutes</returns>
        public ushort GetActiveTimeout()
        {
            return _touchscreen.ActiveTimeoutFeedBack.UShortValue;
        }

        /// <summary>
        /// sets the auto brightness of buttons on or off
        /// range: 0 - 65535
        /// </summary>
        /// <param name="state">enable or disable autobrightness</param>
        public void SetAutoBrightness(bool state)
        {
            _touchscreen.AutoBrightnessEnabled.BoolValue = state;
        }

        /// <summary>
        /// gets the auto brightness value
        /// range: 0 - 65535
        /// </summary>
        /// <returns>true if autobrightness is enable, otherwise false</returns>
        public bool GetAutoBrightness()
        {
            return _touchscreen.AutoBrightnessEnabledFeedBack.BoolValue;
        }

        /// <summary>
        /// sets LED brightness of buttons
        /// range: 0 - 65535
        /// </summary>
        /// <param name="value">brightness value</param>
        public void SetLEDBrightness(ushort value)
        {
            _touchscreen.LEDBrightness.UShortValue = value;
        }

        /// <summary>
        /// gets LED brightness of buttons
        /// range: 0 - 65535
        /// </summary>
        /// <returns></returns>
        public ushort GetLEDBrightness()
        {
            return _touchscreen.LEDBrightnessFeedBack.UShortValue;
        }

        /// <summary>
        /// sets standby brightness (brightness of buttons when off)
        /// range: 0 - 65535
        /// </summary>
        /// <param name="value">brightness value</param>
        public void SetStandbyBrightness(ushort value)
        {
            _touchscreen.StandbyBrightness.UShortValue = value;
        }

        /// <summary>
        /// gets stanby brightness value
        /// range: 0 - 65535
        /// </summary>
        /// <returns>brightness value</returns>
        public ushort GetStandbyBrightness()
        {
            return _touchscreen.StandbyBrightnessFeedBack.UShortValue;
        }
    }
}
