using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SolidHitboxes
{
    [HarmonyPatch]
    class ProjectilePatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Projectile), "IsValidTarget")]
        private static IEnumerable<CodeInstruction> ProjectileTargetPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codeLines = new List<CodeInstruction>(instructions);
            bool success = false;

            try
            {
                var flagAssignIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Stloc_1);

                if (flagAssignIndex > -1)
                {
                    if (codeLines[flagAssignIndex + 3].opcode == OpCodes.Callvirt &&
                        codeLines[flagAssignIndex + 3].operand.ToString().Contains("IsPlayer") &&
                        codeLines[flagAssignIndex + 5].opcode == OpCodes.Ldloc_1 &&
                        IsTrue(codeLines[flagAssignIndex + 6].opcode))
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

            if (!success) Debug.LogError("Projectile.IsValidTarget patch failed.");

            return codeLines.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
        private static IEnumerable<CodeInstruction> ProjectileDamageHandler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeLines = new List<CodeInstruction>(instructions);
            var applyDamageIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Callvirt && p.operand.ToString().Contains(" Damage(HitData)"));

            if (applyDamageIndex > -1)
            {
                var hookPatch = new List<CodeInstruction>();
                hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 6));
                hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 15));
                hookPatch.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(FFDamageHandler), nameof(FFDamageHandler.ModifyDamage))));

                // Apply damage method call takes 2 arguments, so move back 2 lines so we don't mess up params
                codeLines.InsertRange(applyDamageIndex - 2, hookPatch);
            }
            else
            {
                Debug.LogError("Projectile friendly fire patch failed.");
            }

            return codeLines.AsEnumerable();
        }

        private static bool IsTrue(OpCode code)
        {
            return code == OpCodes.Brtrue || code == OpCodes.Brtrue_S;
        }
    }
}
