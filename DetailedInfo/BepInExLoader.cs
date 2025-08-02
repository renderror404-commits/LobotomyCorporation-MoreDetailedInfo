using BepInEx;
using HarmonyLib;
using System;

namespace DetailedInfo
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class BepInExLoader : BaseUnityPlugin
    {
        public const string
            MODNAME = "DetailedInfo",
            AUTHOR = "Yentis",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.1.0";

        private void Awake()
        {
            Logger.LogMessage("Loading");
            var harmony = new Harmony(GUID);

            harmony.Patch(
                original: typeof(AgentInfoWindow.WorkerPrimaryStatUI).GetMethod(nameof(AgentInfoWindow.WorkerPrimaryStatUI.SetStat)),
                prefix: null,
                postfix: new HarmonyMethod(typeof(WorkerPrimaryStatUI_Patch).GetMethod(nameof(WorkerPrimaryStatUI_Patch.Postfix)))
            );

            harmony.Patch(
                original: typeof(UICommonTextConverter).GetMethod(nameof(UICommonTextConverter.GetPercentText), new Type[] { typeof(float) }),
                prefix: null,
                postfix: new HarmonyMethod(typeof(UICommonTextConverter_Patch).GetMethod(nameof(UICommonTextConverter_Patch.Postfix)))
            );

            var creatureUnitPostfix = new HarmonyMethod(typeof(CreatureUnit_Patch).GetMethod(nameof(CreatureUnit_Patch.Postfix)));

            harmony.Patch(
                original: typeof(CreatureUnit).GetMethod(nameof(CreatureUnit.Update)),
                prefix: null,
                postfix: creatureUnitPostfix
            );

            harmony.Patch(
                original: typeof(ChildCreatureUnit).GetMethod(nameof(ChildCreatureUnit.Update)),
                prefix: null,
                postfix: creatureUnitPostfix
            );

            harmony.Patch(
                original: typeof(AgentInfoWindow.InGameModeComponent).GetMethod(nameof(AgentInfoWindow.inGameModeComponent.SetUI)),
                prefix: null,
                postfix: new HarmonyMethod(typeof(SetUI_Patch).GetMethod(nameof(SetUI_Patch.Postfix)))
            );

            Logger.LogMessage("Loaded");
        }
    }
    
    public class WorkerPrimaryStatUI_Patch
    {
        public static void Postfix(AgentInfoWindow.WorkerPrimaryStatUI __instance, AgentModel agent)
        {
            string statText;
            int statGrade;
            int currentStatValue;
            float earnedStatValue;
            int titleBonus;
            
            switch (__instance.type)
            {
                case RwbpType.R:
                    {
                        statText = LocalizeTextDataModel.instance.GetText("Rstat");
                        statGrade = agent.Rstat;
                        currentStatValue = agent.primaryStat.maxHP;
                        earnedStatValue = agent.primaryStatExp.hp;
                        titleBonus = agent.titleBonus.maxHP;
                        
                        break;
                    }
                case RwbpType.W:
                    {
                        statText = LocalizeTextDataModel.instance.GetText("Wstat");
                        statGrade = agent.Wstat;
                        currentStatValue = agent.primaryStat.maxMental;
                        earnedStatValue = agent.primaryStatExp.mental;
                        titleBonus = agent.titleBonus.maxMental;
                        
                        break;
                    }
                case RwbpType.B:
                    {
                        statText = LocalizeTextDataModel.instance.GetText("Bstat");
                        statGrade = agent.Bstat;
                        currentStatValue = agent.primaryStat.workProb;
                        earnedStatValue = agent.primaryStatExp.work;
                        titleBonus = agent.titleBonus.workProb;
                        
                        break;
                    }
                case RwbpType.P:
                    {
                        statText = LocalizeTextDataModel.instance.GetText("Pstat");
                        statGrade = agent.Pstat;
                        currentStatValue = agent.primaryStat.attackSpeed;
                        earnedStatValue = agent.primaryStatExp.battle;
                        titleBonus = agent.titleBonus.attackSpeed;
                        
                        break;
                    }
                default:
                    return;
            }

            var statName = statText.Substring(0, Math.Min(statText.Length, 4));
            var statGradeText = AgentModel.GetLevelGradeText(statGrade);
            var earnedStatText = Math.Round((decimal)earnedStatValue, 0);
            var earnedValueColor = AgentInfoWindow.currentWindow.Additional_Plus_ValueColor;

            __instance.StatName.text = $"{statName} {statGradeText} ({currentStatValue}+
            {titleBonus}<color=#{earnedValueColor}>+{earnedStatValue:0.000}</color>)";
        }
    }
    
    public class UICommonTextConverter_Patch
    {
        public static void Postfix(ref string __result, float rate)
        {
            __result = $"{(rate * 100f):0.0}%";
        }
    }

    public class CreatureUnit_Patch
    {
        public static void Postfix(CreatureUnit __instance)
        {
            if (!__instance.hpSlider.gameObject.activeInHierarchy) return;

            var curHp = Math.Round((decimal)__instance.model.hp, 0);
            var maxHp = __instance.model.maxHp;

            var hpValueIndex = __instance.escapeCreatureName.text.IndexOf("(");
            if (hpValueIndex > 0)
            {
                __instance.escapeCreatureName.text = __instance.escapeCreatureName.text.Substring(0, hpValueIndex - 1);
            }

            var hpValue = $" ({curHp}/{maxHp})";
            __instance.escapeCreatureName.text += hpValue;
        }
    }

    public class SetUI_Patch
    {
        public static void Postfix(ref AgentInfoWindow.InGameModeComponent __instance, AgentModel agent)
        {
            __instance.AgentName.text = $"{agent.GetUnitName()}_{agent.instanceId}";
        }
    }
}
