using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace SolidHitboxes
{
    [HarmonyPatch]
    class ProjectilePatch
    {
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.IsValidTarget))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ProjectileTargetPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codeLines = new List<CodeInstruction>(instructions);
            bool success = false;

            try
            {
                var flagAssignIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Stloc_1);

                if (flagAssignIndex > -1)
                {
                    // Double check that we have correct code block
                    if (codeLines[flagAssignIndex + 1].opcode == OpCodes.Ldarg_0 &&
                        codeLines[flagAssignIndex + 3].opcode == OpCodes.Callvirt &&
                        codeLines[flagAssignIndex + 3].operand.ToString().Contains("IsPlayer") &&
                        codeLines[flagAssignIndex + 5].opcode == OpCodes.Ldloc_1 &&
                        codeLines[flagAssignIndex + 8].opcode == OpCodes.Ret)
                    {
                        codeLines.RemoveRange(flagAssignIndex + 1, 8);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            if (!success) Debug.Log("Projectile targeting patch failed.");

            return codeLines.AsEnumerable();
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ProjectileDamageHandler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeLines = new List<CodeInstruction>(instructions);

            var applyDamageIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Callvirt && p.operand.ToString().Contains(" Damage(HitData)"));

            if (applyDamageIndex > -1)
            {
                var hookPatch = new List<CodeInstruction>();
                hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_3));
                hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 4));
                hookPatch.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(FFDamageHandler), nameof(FFDamageHandler.ModifyDamage))));

                // Apply damage method call takes 2 arguments, so move back 2 lines so we don't mess up params
                codeLines.InsertRange(applyDamageIndex - 2, hookPatch);
            }
            else
            {
                Debug.Log("Projectile friendly fire patch failed.");
            }

            return codeLines.AsEnumerable();
        }
    }
}
