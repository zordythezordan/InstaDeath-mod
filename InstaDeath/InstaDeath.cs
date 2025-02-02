using BepInEx;
using HarmonyLib;
using UnityEngine;
using PluginConfig.API;
using PluginConfig.API.Fields;
using System;

#pragma warning disable IDE0051

namespace Instadeath
{
    [BepInPlugin("com.zord.instadeath", "InstaDeath", "2.0.1")]
    [BepInDependency("com.eternalUnion.pluginConfigurator")]
    public class InstaDeath : BaseUnityPlugin
    {
        enum RESTARTING_TYPE
        {
            Disabled,
            Checkpoint,
            RestartMission
        }

        private static PluginConfigurator config;
        private static EnumField<RESTARTING_TYPE> restartingType;
        private static BoolField shouldDoCybergrind;

        private void Awake()
        {
            new Harmony("zord.instadeath").PatchAll(typeof(InstaDeath));

            config = PluginConfigurator.Create("InstaDeath", "com.zord.instadeath");

            restartingType = new EnumField<RESTARTING_TYPE>(config.rootPanel, "Restarting Type", "instadeath_restarting_type", RESTARTING_TYPE.Checkpoint);
            restartingType.SetEnumDisplayName(RESTARTING_TYPE.RestartMission, "Restart Mission");

            shouldDoCybergrind = new BoolField(config.rootPanel, "Enable in Cybergrind", "instadeath_is_cybergrind", false);

            config.SetIconWithURL("file://" + System.IO.Path.GetDirectoryName(Info.Location) + "\\icon.png");
            // icon not included in solution, place icon.png from thunderstore zip file in the same folder as mod
        }

        [HarmonyPatch(typeof(NewMovement), "GetHurt")]
        [HarmonyPostfix]
        static void GetHurtPatch()
        {
            if (shouldDoCybergrind.value == false && GameObject.FindObjectOfType<StatsManager>().endlessMode)
            {
                Console.WriteLine("this IS endless mode");
                return;
            }

            if (NewMovement.Instance.dead && restartingType.value != RESTARTING_TYPE.Disabled)
            {
                OptionsMenuToManager manager = GameObject.FindObjectOfType<OptionsMenuToManager>();
                if (manager == null) return;

                switch (restartingType.value)
                {
                    case RESTARTING_TYPE.RestartMission:
                        manager.RestartMissionNoConfirm();
                        break;
                    case RESTARTING_TYPE.Checkpoint:
                        manager.RestartCheckpoint();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
