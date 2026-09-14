using System;
using UnityEngine;

namespace SolidHitboxes
{
    public static class FFDamageHandler
    {
        public static void ModifyDamage(IDestructible target, HitData hit)
        {
            try
            {
                var attacker = hit?.GetAttacker();
                var targetChar = target as Character;

                if (attacker?.IsTamed() == true && (targetChar?.IsPlayer() == true || targetChar?.IsTamed() == true))
                {
                    hit.ApplyModifier(0);
                    return;
                }

                if (attacker?.GetBaseAI() is not MonsterAI || targetChar?.GetBaseAI() is not MonsterAI) return;

                var isEnemy = !attacker.IsPlayer() && BaseAI.IsEnemy(attacker, targetChar);

                if (attacker.IsPlayer() || isEnemy) return;

                if (!Plugin.FFEnabled)
                {
                    hit.ApplyModifier(0);
                }
                else
                {
                    hit.ApplyModifier(Plugin.FFDamageModifier);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"FFDamageHandler error: Attacker: {hit?.GetAttacker()}, Target ({target}).\nError message: {e.Message}");
            }
        } 
    }
}
