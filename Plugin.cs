﻿using BepInEx;

namespace AutoDeposit
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            R.Init();
            Settings.Init(Config);

            new AddInventoryButtonsPatch().Enable();
            new AddTransferButtonPatch().Enable();
        }
    }
}
