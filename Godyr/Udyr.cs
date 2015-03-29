#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Godyr
{
    internal static class Udyr
    {
        public enum Stances
        {
            None,
            Tiger,
            Turtle,
            Bear,
            Phoenix
        }

        public static Spell Q, W, E, R;

        public static Obj_AI_Hero Hero
        {
            get { return ObjectManager.Player; }
        }

        public static BuffInstance StanceBuffInstance
        {
            get { return Hero.Buffs.FirstOrDefault(b => b.DisplayName.Contains("Stance") && b.IsValidBuff()); }
        }

        public static Stances Stance
        {
            get
            {
                if (StanceBuffInstance == null)
                {
                    return Stances.None;
                }

                return
                    Enum.GetValues(typeof(Stances))
                        .Cast<Stances>()
                        .FirstOrDefault(stance => StanceBuffInstance.DisplayName.Contains(stance.ToString()));
            }
        }

        public static int FlameCount { get; private set; }

        public static void Load()
        {
            if (Hero.ChampionName != "Udyr")
            {
                return;
            }
            try
            {
                Config.Load();

                Q = new Spell(SpellSlot.Q);
                W = new Spell(SpellSlot.W);
                E = new Spell(SpellSlot.E);
                R = new Spell(SpellSlot.R);

                Notifications.AddNotification("Godyr loaded!", 5);

                Game.OnUpdate += OnUpdate;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            }
            catch (Exception)
            {
                Notifications.AddNotification("Unable to load Godyr", 5);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            switch (Config.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    AICombos.Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    AICombos.Farm();
                    break;
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (Stance != Stances.Phoenix)
            {
                FlameCount = 0;
                return;
            }

            if (StanceBuffInstance.Count == 3 && !args.SData.Name.Contains("Stance"))
            {
                FlameCount++;
            }
        }
    }
}