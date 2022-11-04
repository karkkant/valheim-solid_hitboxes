using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace SolidHitboxes
{
    [HarmonyPatch(typeof(Attack))]
    [HarmonyPatch("DoMeleeAttack")]
    static class MeleeAttackPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codeLines = new List<CodeInstruction>(instructions);
            bool collisionPatchSuccess = false;
            bool success = false;

            try
            {
                var flagAssignIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Call && p.operand.ToString().Contains("IsEnemy"));
                var flagOperand = (LocalBuilder)codeLines[flagAssignIndex + 1].operand;

                // Step 1: Remove check which prevents AI friendly fire hits from registering
                if (flagAssignIndex > -1)
                {
                    if (codeLines[flagAssignIndex + 2].opcode == OpCodes.Ldarg_0 &&
                        codeLines[flagAssignIndex + 4].opcode == OpCodes.Callvirt &&
                        codeLines[flagAssignIndex + 4].operand.ToString().Contains("IsPlayer") &&
                        IsTrue(codeLines[flagAssignIndex + 5].opcode))
                    {
                        codeLines.RemoveRange(flagAssignIndex + 2, 4);

                        // Remove IsEnemy check
                        var flagCheckIndex = codeLines.FindIndex(flagAssignIndex, p => p.IsLdloc(flagOperand));

                        if (flagCheckIndex > -1)
                        {
                            codeLines.RemoveRange(flagCheckIndex, 2);
                            collisionPatchSuccess = true;
                        }
                    }
                }

                if (collisionPatchSuccess)
                {
                    // Step 2: Add our own hook for toggling AI friendly fire on/off
                    // This is done as a last step before damage is applied
                    var applyDamageIndex = codeLines.FindIndex(p => p.opcode == OpCodes.Callvirt && p.operand.ToString().Contains(" Damage(HitData)"));

                    if (applyDamageIndex > -1)
                    {
                        var hookPatch = new List<CodeInstruction>();
                        hookPatch.Add(new CodeInstruction(OpCodes.Ldarg_0));
                        hookPatch.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Attack), nameof(Attack.m_character))));
                        hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 34));
                        hookPatch.Add(new CodeInstruction(OpCodes.Isinst, typeof(Character)));
                        hookPatch.Add(new CodeInstruction(OpCodes.Ldloc_S, 39));
                        hookPatch.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(FFDamageHandler), nameof(FFDamageHandler.ModifyDamage))));

                        // Apply damage method call takes 2 arguments, so move back 2 lines so we don't mess up params
                        codeLines.InsertRange(applyDamageIndex - 2, hookPatch);

                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            if (!success) Debug.Log("Patch failed.");

            return codeLines.AsEnumerable();
        }

        private static bool IsTrue(OpCode code)
        {
            return code == OpCodes.Brtrue || code == OpCodes.Brtrue_S;
        }

    }
}
