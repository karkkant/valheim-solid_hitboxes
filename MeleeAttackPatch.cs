using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace SolidHitboxes
{
    [HarmonyPatch]
    internal static class MeleeAttackPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
        private static void StartAttack_Patch(ref Attack __instance)
        {
            __instance.m_hitFriendly = true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
        private static IEnumerable<CodeInstruction> DoMeleeAttack_Patch(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeLines = new List<CodeInstruction>(instructions);
            bool success = false;

            try
            {
                var applyDamageIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Callvirt && p.operand.ToString().Contains(" Damage(HitData)"));

                if (applyDamageIndex > -1)
                {
                    var hookPatch = new List<CodeInstruction>();
                    hookPatch.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(FFDamageHandler), nameof(FFDamageHandler.ModifyDamage))));
                    hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 34));
                    hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 39));

                    codeLines.InsertRange(applyDamageIndex, hookPatch);

                    success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            if (!success) Debug.LogError("Patch DoMeleeAttack failed.");

            return codeLines.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Attack), "<DoAreaAttack>g__checkHits|27_0")]
        private static IEnumerable<CodeInstruction> DoAreaAttack_Patch(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeLines = new List<CodeInstruction>(instructions);
            bool success = false;

            try
            {
                var applyDamageIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Callvirt && p.operand.ToString().Contains(" Damage(HitData)"));

                if (applyDamageIndex > -1)
                {
                    var hookPatch = new List<CodeInstruction>();
                    hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 4));
                    hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 7));
                    hookPatch.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(FFDamageHandler), nameof(FFDamageHandler.ModifyDamage))));

                    codeLines.InsertRange(applyDamageIndex - 2, hookPatch);

                    success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            if (!success) Debug.LogError("Patch DoAreaAttack failed.");

            return codeLines.AsEnumerable();
        }
    }
}
