namespace SolidHitboxes
{
    public static class FFDamageHandler
    {
        public static bool AIFriendlyFireEnabled { get; set; }

        public static void ModifyDamage(Character attacker, Character target, HitData hit)
        {
            if (AIFriendlyFireEnabled || attacker == null || target == null) return;

            var isEnemy = !attacker.IsPlayer() && BaseAI.IsEnemy(attacker, target);

            if (!attacker.IsPlayer() && !isEnemy)
            {
                hit.ApplyModifier(0);
            }
        }
    }
}
