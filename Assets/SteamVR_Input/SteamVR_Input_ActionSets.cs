//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Valve.VR
{
    using System;
    using UnityEngine;
    
    
    public partial class SteamVR_Actions
    {
        
        private static SteamVR_Input_ActionSet_main p_main;
        
        public static SteamVR_Input_ActionSet_main main
        {
            get
            {
                return SteamVR_Actions.p_main.GetCopy<SteamVR_Input_ActionSet_main>();
            }
        }
        
        private static void StartPreInitActionSets()
        {
            SteamVR_Actions.p_main = ((SteamVR_Input_ActionSet_main)(SteamVR_ActionSet.Create<SteamVR_Input_ActionSet_main>("/actions/main")));
            Valve.VR.SteamVR_Input.actionSets = new Valve.VR.SteamVR_ActionSet[] {
                    SteamVR_Actions.main};
        }
    }
}
