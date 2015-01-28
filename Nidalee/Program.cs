#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;
using Color = System.Drawing.Color;

#endregion

namespace Nidalee
{
    internal class Program
    {
        
        private static Items.Item _bork, _cutlass;
        private static SpellSlot _igniteSlot;
        private static Menu _config;
        private static Obj_AI_Hero _player;

        #region Spells
        private static Spell Q
        {
            get
            {
                if (IsCougar)
                    return new Spell(SpellSlot.Q, 125f + 50f);

                var spell = new Spell(SpellSlot.Q, 1500f);
                spell.SetSkillshot(0.125f, 70f, 1300, true, SkillshotType.SkillshotLine);
                return spell;
            }
        }

        private static Spell W
        {
            get
            {
                if (IsCougar)
                    return new Spell(SpellSlot.W, 750f);

                var spell = new Spell(SpellSlot.W, 900f);
                spell.SetSkillshot(1.5f, 80f, float.MaxValue, false, SkillshotType.SkillshotCircle);
                return spell;
            }
        }

        private static Spell E
        {
            get
            {
                return IsCougar ? new Spell(SpellSlot.E, 300f) : new Spell(SpellSlot.E, 600f);
            }
        }

        private static Spell R
        {
            get
            {
                return new Spell(SpellSlot.R);
            }
        }
        #endregion

        private static Orbwalking.OrbwalkingMode ActiveMode
        {
            get
            {
                if (_config.Item("KeysCombo").GetValue<KeyBind>().Active)
                    return Orbwalking.OrbwalkingMode.Combo;

                if (_config.Item("KeysLaneClear").GetValue<KeyBind>().Active)
                    return Orbwalking.OrbwalkingMode.LaneClear;

                if (_config.Item("KeysMixed").GetValue<KeyBind>().Active)
                    return Orbwalking.OrbwalkingMode.Mixed;

                return Orbwalking.OrbwalkingMode.None;
            }
        }

        private static bool PacketCasting
        {
            get { return _config.Item("packetCasting").GetValue<bool>(); }
        }

        private static bool IsCougar
        {
            get { return _player.BaseSkinName != "Nidalee"; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != "Nidalee") return;

            Game.PrintChat(
                "<font color=\"#0066FF\">[<font color=\"#FFFFFF\">madk</font>]</font><font color=\"#FFFFFF\"> Nidalee assembly loaded!</font>");

            #region Items

            _bork = new Items.Item(3153, 450f);
            _cutlass = new Items.Item(3144, 450f);
            
            #endregion

            /* Summoner Spells */
            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            #region Create Menu

            _config = new Menu("Nidaleek", "Nidaleek", true);

            var menuTargetSelector = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(menuTargetSelector);

            var menuOrbwalker = new Menu("Orbwalker", "Orbwalker");
            LXOrbwalker.AddToMenu(menuOrbwalker);

            var menuKeyBindings = new Menu("Key Bindings", "KB");
            menuKeyBindings.AddItem(
                new MenuItem("KeysCombo", "Combo").SetValue(
                    new KeyBind(menuOrbwalker.Item("Combo_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menuKeyBindings.AddItem(
                new MenuItem("KeysMixed", "Harass").SetValue(
                    new KeyBind(menuOrbwalker.Item("Harass_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menuKeyBindings.AddItem(
                new MenuItem("KeysLaneClear", "Lane/Jungle Clear").SetValue(
                    new KeyBind(menuOrbwalker.Item("LaneClear_Key").GetValue<KeyBind>().Key, KeyBindType.Press)));

            var menuCombo = new Menu("Combo", "combo");
            menuCombo.AddItem(new MenuItem("combo_info1", "Human Form:"));
            menuCombo.AddItem(new MenuItem("combo_Q1", "Javelin Toss").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_W1", "Bushwhack").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_E1", "Primal Surge").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_blank1", ""));
            menuCombo.AddItem(new MenuItem("combo_info2", "Cougar Form:"));
            menuCombo.AddItem(new MenuItem("combo_Q2", "Takedown").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_W2", "Pounce").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_E2", "Swipe").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_blank2", ""));
            menuCombo.AddItem(new MenuItem("combo_info3", "Extra Functions:"));
            menuCombo.AddItem(new MenuItem("combo_R", "Auto Switch Forms").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_Items", "Use Items").SetValue(true));
            menuCombo.AddItem(new MenuItem("combo_UT", "Jump to turret range").SetValue(true));

            var menuHarass = new Menu("Harass", "harass");
            menuHarass.AddItem(new MenuItem("harass_info1", "Human Form:"));
            menuHarass.AddItem(new MenuItem("harass_Q1", "Javelin Toss").SetValue(true));
            menuHarass.AddItem(new MenuItem("harass_W1", "Bushwhack").SetValue(true));
            menuHarass.AddItem(new MenuItem("harass_blank1", ""));
            menuHarass.AddItem(new MenuItem("harass_info2:", "Cougar Form:"));
            menuHarass.AddItem(new MenuItem("harass_E2", "Swipe").SetValue(true));
            menuHarass.AddItem(new MenuItem("harass_blank2", ""));
            menuHarass.AddItem(new MenuItem("harass_mn", "Required Mana").SetValue(new Slider(40)));

            var menuFarm = new Menu("Lane Clear", "farm");
            menuFarm.AddItem(new MenuItem("farm_info1", "Human Form:"));
            menuFarm.AddItem(new MenuItem("farm_Q1", "Javelin Toss").SetValue(true));
            menuFarm.AddItem(new MenuItem("farm_E1", "Primal Surge").SetValue(true));
            menuFarm.AddItem(new MenuItem("farm_blank1", ""));
            menuFarm.AddItem(new MenuItem("farm_info2", "Cougar Form:"));
            menuFarm.AddItem(new MenuItem("farm_Q2", "Takedown").SetValue(true));
            menuFarm.AddItem(new MenuItem("farm_W2", "Pounce").SetValue(true));
            menuFarm.AddItem(new MenuItem("farm_E2", "Swipe").SetValue(true));
            menuFarm.AddItem(new MenuItem("farm_blank2", ""));
            menuFarm.AddItem(new MenuItem("farm_R", "Auto Swtich Forms").SetValue(false));

            var menuKillSteal = new Menu("Kill Steal", "killsteal");
            menuKillSteal.AddItem(new MenuItem("ks_enabled", "State").SetValue(true));
            menuKillSteal.AddItem(new MenuItem("ks_Q1", "Javelin Toss").SetValue(true));
            menuKillSteal.AddItem(new MenuItem("ks_dot", "Ignite").SetValue(true));

            var menuMisc = new Menu("Misc", "Misc");
            menuMisc.AddItem(
                new MenuItem("autoHealMode", "Auto Heal Mode").SetValue(new StringList(new[] {"OFF", "Self", "Allies"},
                    1)));
            menuMisc.AddItem(new MenuItem("autoHealPct", "Auto Heal %").SetValue(new Slider(50)));
            menuMisc.AddItem(new MenuItem("packetCasting", "Packet Casting").SetValue(true));

            var menuDrawings = new Menu("Drawings", "drawings");
            menuDrawings.AddItem(new MenuItem("draw_info1", "Human Form:"));
            menuDrawings.AddItem(new MenuItem("draw_Q1", "Javelin Toss").SetValue(new Circle(true, Color.White)));
            menuDrawings.AddItem(new MenuItem("draw_W1", "Bushwhack").SetValue(new Circle(true, Color.White)));
            menuDrawings.AddItem(new MenuItem("draw_E1", "Primal Surge").SetValue(new Circle(true, Color.White)));
            menuDrawings.AddItem(new MenuItem("draw_blank", ""));
            menuDrawings.AddItem(new MenuItem("draw_info2", "Cougar Form:"));
            menuDrawings.AddItem(new MenuItem("draw_W2", "Pounce").SetValue(new Circle(true, Color.White)));
            menuDrawings.AddItem(new MenuItem("draw_E2", "Swipe").SetValue(new Circle(true, Color.White)));
            menuDrawings.AddItem(new MenuItem("draw_blank2", ""));
            menuDrawings.AddItem(new MenuItem("draw_CF", "Current Form Only").SetValue(false));

            _config.AddSubMenu(menuTargetSelector);
            _config.AddSubMenu(menuOrbwalker);
            _config.AddSubMenu(menuKeyBindings);
            _config.AddSubMenu(menuCombo);
            _config.AddSubMenu(menuHarass);
            _config.AddSubMenu(menuKillSteal);
            _config.AddSubMenu(menuFarm);
            _config.AddSubMenu(menuMisc);
            _config.AddSubMenu(menuDrawings);

            _config.AddToMainMenu();

            #endregion

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_config.Item("ks_enabled").GetValue<bool>())
                KillSteal();

            if (_config.Item("autoHealMode").GetValue<StringList>().SelectedIndex > 0)
                AutoHeal(_config.Item("autoHealMode").GetValue<StringList>().SelectedIndex == 2);

            switch (ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm();
                    break;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ1 = _config.Item("draw_Q1").GetValue<Circle>();
            var drawW1 = _config.Item("draw_W1").GetValue<Circle>();
            var drawE1 = _config.Item("draw_E1").GetValue<Circle>();
            var drawW2 = _config.Item("draw_W2").GetValue<Circle>();
            var drawE2 = _config.Item("draw_E2").GetValue<Circle>();
            var drawCf = _config.Item("draw_CF").GetValue<bool>();

            if (IsCougar || !drawCf)
            {
                if (drawW2.Active)
                    Render.Circle.DrawCircle(_player.Position, 750f, drawW2.Color);

                if (drawE2.Active)
                    Render.Circle.DrawCircle(_player.Position, 300f, drawE2.Color);
            }

            if (!IsCougar || !drawCf)
            {
                if (drawQ1.Active)
                    Render.Circle.DrawCircle(_player.Position, 1500f, drawQ1.Color);

                if (drawW1.Active)
                    Render.Circle.DrawCircle(_player.Position, 900f, drawW1.Color);

                if (drawE1.Active)
                    Render.Circle.DrawCircle(_player.Position, 600f, drawE1.Color);
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1600f, TargetSelector.DamageType.Magical);
            if (target == null)
                return;

            var marked = target.HasBuff("nidaleepassivehunted", true);
            var hunting = _player.HasBuff("nidaleepassivehunting", true);
            var distance = _player.Distance(target);
            var useItems = _config.Item("combo_Items").GetValue<bool>();

            var useQ = _config.Item("combo_Q" + (IsCougar ? "2" : "1")).GetValue<bool>();
            var useW = _config.Item("combo_W" + (IsCougar ? "2" : "1")).GetValue<bool>();
            var useE = _config.Item("combo_E" + (IsCougar ? "2" : "1")).GetValue<bool>();
            var useR = _config.Item("combo_R").GetValue<bool>();
            var underT = _config.Item("combo_UT").GetValue<bool>();

            if (useItems)
            {
                if (Items.CanUseItem(_bork.Id)) 
                    _bork.Cast(target);

                if (Items.CanUseItem(_cutlass.Id)) 
                    _cutlass.Cast(target);
            }

            if (!IsCougar) /* Human Form */
            {
                if (useR && marked && R.IsReady() && distance < 750f ||
                    (!Q.IsReady() && !Q.IsReady(2500) && target.Distance(_player) < 300f) &&
                    (!target.UnderTurret(true) || underT))
                {
                    R.Cast(PacketCasting);
                }

                if (useQ && Q.IsReady())
                {
                    Q.Cast(target, PacketCasting);
                }

                if (useW && W.IsReady())
                {
                    W.Cast(target, PacketCasting);
                }

                if (useE && E.IsReady() && Utility.CountEnemiesInRange(_player.AttackRange) > 0)
                {
                    E.CastOnUnit(_player, PacketCasting);
                }
            }
            else /* Cougar Form */
            {
                if (!marked && useR && R.IsReady() && distance < W.Range + 75f)
                {
                    R.Cast(PacketCasting);
                    return;
                }

                if (useQ && Q.IsReady())
                    Q.CastOnUnit(_player, PacketCasting);

                if (useW && marked && hunting && W.IsReady() && distance < 750f && distance > 200f &&
                    (!target.UnderTurret(true) || underT))
                {
                    W.CastOnUnit(target, PacketCasting);
                }

                if (useE && E.IsReady() && distance < E.Range)
                {
                    var pred = Prediction.GetPrediction(target, 0.5f);
                    E.Cast(pred.CastPosition, PacketCasting);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var orbTarget = LXOrbwalker.GetPossibleTarget();

            var useQ = _config.Item("harass_Q1").GetValue<bool>();
            var useW = _config.Item("harass_W1").GetValue<bool>();
            var useE = _config.Item("harass_E2").GetValue<bool>();

            var minMn = _config.Item("harass_Mn").GetValue<Slider>().Value;

            if ((orbTarget != null && orbTarget.IsMinion)) 
                return;

            if (IsCougar)
            {
                if (useE && E.IsReady())
                {
                    E.Cast(target);
                }

                return;
            }

            if (_player.ManaPercentage() < minMn)
                return;

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, PacketCasting);
            }

            if (useW && W.IsReady())
            {
                W.Cast(target, PacketCasting);
            }
        }

        private static void Farm()
        {
            var minions = MinionManager.GetMinions(_player.Position, 750f, MinionTypes.All, MinionTeam.NotAlly).OrderByDescending(m => m.HasBuff("nidaleepassivehunted") ? float.MaxValue : m.MaxHealth );
            var target = minions.FirstOrDefault(m => m.IsValidTarget());

            if (target == null)
                return;
            
            var marked = target.HasBuff("nidaleepassivehunted", true);

            var useQ = _config.Item("farm_Q" + (IsCougar ? "2" : "1")).GetValue<bool>();
            var useE = _config.Item("farm_E" + (IsCougar ? "2" : "1")).GetValue<bool>();
            var useW = _config.Item("farm_W2").GetValue<bool>();
            var useR = _config.Item("farm_R").GetValue<bool>();


            if (!IsCougar)
            {
                if (useQ && Q.IsReady() && !target.IsMinion)
                {
                    Q.Cast(target, PacketCasting);
                }

                if (useE && E.IsReady())
                {
                    E.CastOnUnit(_player, PacketCasting);
                }

                if (useR && R.IsReady() && (marked || Q.Instance.Cooldown > 6f - (6f * _player.PercentCooldownMod)))
                {
                    R.Cast(PacketCasting);
                }
            }
            else
            {
                if (useQ && Q.IsReady())
                {
                    Q.Cast(PacketCasting);
                }

                if (useW && W.IsReady() && _player.Distance(target) > 200f)
                {
                    if (!marked && _player.Distance(target) < 300f)
                    {
                        W.Cast(target, PacketCasting);
                    }
                    else
                    {
                        W.CastOnUnit(target, PacketCasting);
                    }
                }

                if (useE && E.IsReady())
                {
                    E.Cast(target, PacketCasting);
                }
            }
        }

        private static void KillSteal()
        {
            if (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo)
                return;

            var ksQ = _config.Item("ks_Q1").GetValue<bool>();
            var ksDot = _config.Item("ks_dot").GetValue<bool>();

            if (ksQ)
            {
                var qTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(hero => hero.IsValidTarget(1500f) && hero.Health < Q.GetDamage(hero));

                if (qTarget != null && Q.IsReady() && qTarget.IsValid)
                {
                    Q.Cast(qTarget, PacketCasting);
                }
            }

            if (ksDot && _igniteSlot != SpellSlot.Unknown)
            {
                var dotEnemy =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            hero =>
                                hero.IsValidTarget(600f) &&
                                hero.Health < _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) &&
                                !hero.HasBuff("SummonerDot", true));
                if (dotEnemy != null && _player.Spellbook.GetSpell(_igniteSlot).State == SpellState.Ready && dotEnemy.IsValid)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, dotEnemy);
                }
            }
        }

        private static void AutoHeal(bool healAllies)
        {
            if (_player.HasBuff("Recall"))
                return;

            var target = healAllies
                ? ObjectManager.Get<Obj_AI_Hero>()
                    .OrderBy(hero => hero.Health)
                    .First(hero => hero.IsAlly && hero.IsValidTarget(E.Range, false))
                : _player;

            var minHp = _config.Item("autoHealPct").GetValue<Slider>().Value;

            if (E.IsReady() && _player.HealthPercentage() <= minHp)
                E.CastOnUnit(target, PacketCasting);
        }
    }
}