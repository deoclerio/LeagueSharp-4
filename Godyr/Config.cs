#region

using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Godyr
{
    internal class Config
    {
        public static Menu Menu { get; private set; }
        public static Orbwalking.Orbwalker Orbwalker { get; private set; }

        public static void Load()
        {
            Menu = new Menu("Godyr", "madk_godyr", true);

            TargetSelector.AddToMenu(Menu.AddSubMenu(new Menu("Target Selector", "ts")));

            Orbwalker = new Orbwalking.Orbwalker(Menu.AddSubMenu(new Menu("Orbwalker", "orbwalker")));

            var menuItems = new List<MenuItem>
            {
                new MenuItem("mode", "Mode").SetValue(new StringList(new[] { "Tiger", "Phoenix" }, 1)),
                new MenuItem("info1", "-- Turtle Settings --"),
                new MenuItem("w_combo", "Combo").SetValue(
                    new StringList(new[] { "Never", "Always", "Under Towers" }, 2)),
                new MenuItem("w_farm", "Farm").SetValue(new StringList(new[] { "Never", "Always", "Epic Monsters" }, 2)),
                new MenuItem("info2", "-- Phoenix Settings --"),
                new MenuItem("r_stacks", "Max Stacks").SetValue(new Slider(2, 0, 3)),
                new MenuItem("r_flames", "Min Flames").SetValue(new Slider(2, 0, 5)),
                new MenuItem("info3", "-- Smite Settings --"),
                new MenuItem("s_enemy", "Enemies (% HP)").SetValue(new Slider(40)),
                new MenuItem("s_small", "Small Camps").SetValue(false),
                new MenuItem("s_buffs", "Buffs").SetValue(true),
                new MenuItem("s_epic", "Epic Monsters").SetValue(true),
            };

            menuItems.ForEach(i => Menu.AddItem(i));

            Menu.AddToMainMenu();
        }

        public static bool GetBool(string item)
        {
            return Menu.Item(item).GetValue<bool>();
        }

        public static int GetListIndex(string item)
        {
            return Menu.Item(item).GetValue<StringList>().SelectedIndex;
        }

        public static int GetSliderValue(string item)
        {
            return Menu.Item(item).GetValue<Slider>().Value;
        }

        public static bool CanUseShield()
        {
            if (Udyr.W.Level == 0)
            {
                return false;
            }
            
            var target = Orbwalker.GetTarget() as Obj_AI_Base;

            if (target == null)
            {
                return false;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                return GetListIndex("w_farm") == 1 ||
                       GetListIndex("w_farm") == 2 && Utils.EpicMonsters.Any(em => target.Name.Contains(em));
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                return GetListIndex("w_combo") == 1 ||
                       GetListIndex("w_combo") == 2 && target.UnderTurret();
            }

            return false;
        }

        public static bool CanLeavePhoenix()
        {
            return Udyr.StanceBuffInstance.Count <= GetSliderValue("r_stacks") &&
                   Udyr.FlameCount >= GetSliderValue("r_flames");
        }
    }
}