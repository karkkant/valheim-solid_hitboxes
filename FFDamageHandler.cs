using System;
using UnityEngine;

namespace SolidHitboxes
{
    public static class FFDamageHandler
    {
        public static bool AIFriendlyFireEnabled { get; set; }

        public static void ModifyDamage(IDestructible target, HitData hit)
        {
            try
            {
                var attacker = hit?.GetAttacker();
                var targetChar = target as Character;

                if (attacker?.IsTamed() == true && targetChar?.IsPlayer() == true)
                {
                    hit.ApplyModifier(0);
                    return;
                }
                
                if (AIFriendlyFireEnabled || 
                    attacker?.GetBaseAI() is not MonsterAI ||
                    targetChar?.GetBaseAI() is not MonsterAI) return;

                var isEnemy = !attacker.IsPlayer() && BaseAI.IsEnemy(attacker, targetChar);
                
                if (!attacker.IsPlayer() && !isEnemy)
                {
                    hit.ApplyModifier(0);
                }
            } catch(Exception e)
            {
                Debug.LogError($"AI FF error: Attacker: {hit?.GetAttacker()}, Target ({target}).\nError message: {e.Message}");
            }
        }
    }
}
