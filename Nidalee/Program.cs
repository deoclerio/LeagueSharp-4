#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Nidalee
{
    internal class Program
    {
        private static Items.Item _bork, _cutlass;
        private static SpellSlot _igniteSlot;
        private static Menu _config;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Obj_AI_Hero _player;

        #region Spells

        private static Spell Q
        {
            get
            {
                if (IsCougar)
                {
                    return new Spell(SpellSlot.Q, 125f + 50f);
                }

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
                {
                    return new Spell(SpellSlot.W, 750f);
                }

                var spell = new Spell(SpellSlot.W, 900f);
                spell.SetSkillshot(1.5f, 80f, float.MaxValue, false, SkillshotType.SkillshotCircle);
                return spell;
            }
        }

        private static Spell E
        {
            get { return IsCougar ? new Spell(SpellSlot.E, 300f) : new Spell(SpellSlot.E, 600f); }
        }

        private static Spell R
        {
            get { return new Spell(SpellSlot.R); }
        }

        #endregion

        private static ActiveModes ActiveMode
        {
            get
            {
                if (_config.Item("KeysCombo").GetValue<KeyBind>().Active)
                {
                    return ActiveModes.Combo;
                }

                if (_config.Item("KeysLaneClear").GetValue<KeyBind>().Active)
                {
                    return ActiveModes.LaneClear;
                }

                if (_config.Item("KeysMixed").GetValue<KeyBind>().Active)
                {
                    return ActiveModes.Mixed;
                }

                if (_config.Item("KeysLastHit").GetValue<KeyBind>().Active)
                {
                    return ActiveModes.LastHit;
                }

                if (_config.Item("KeysFlee").GetValue<KeyBind>().Active)
                {
                    return ActiveModes.Flee;
                }

                return ActiveModes.None;
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

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != "Nidalee")
            {
                return;
            }

            Game.PrintChat(
                "<font color=\"#0066FF\">[<font color=\"#FFFFFF\">madk</font>]</font><font color=\"#FFFFFF\"> Nidalee assembly loaded!</font>");

            #region Items

            _bork = new Items.Item(3153, 450f);
            _cutlass = new Items.Item(3144, 450f);

            #endregion

            _igniteSlot = _player.GetSpellSlot("SummonerDot");

            #region Create Menu

            _config = new Menu("Nidaleek", "Nidaleek", true);

            var menuTargetSelector = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(menuTargetSelector);

            var menuOrbwalker = new Menu("Orbwalker", "Orbwalker");

            _orbwalker = new Orbwalking.Orbwalker(menuOrbwalker);

            var menuKeyBindings = new Menu("Key Bindings", "KB");
            menuKeyBindings.AddItem(
                new MenuItem("KeysCombo", "Combo").SetValue(
                    new KeyBind(menuOrbwalker.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menuKeyBindings.AddItem(
                new MenuItem("KeysMixed", "Harass").SetValue(
                    new KeyBind(menuOrbwalker.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menuKeyBindings.AddItem(
                new MenuItem("KeysLaneClear", "Lane/Jungle Clear").SetValue(
                    new KeyBind(menuOrbwalker.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menuKeyBindings.AddItem(
                new MenuItem("KeysLastHit", "Last Hit").SetValue(
                    new KeyBind(menuOrbwalker.Item("LastHit").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menuKeyBindings.AddItem(
                new MenuItem("KeysFlee", "Flee (not done yet)").SetValue(new KeyBind('T', KeyBindType.Press)));

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
            menuHarass.AddItem(new MenuItem("harass_Mn", "Required Mana").SetValue(new Slider(40)));

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

            var menuLastHit = new Menu("Last Hit", "lasthit");
            menuLastHit.AddItem(new MenuItem("lh_JungleCamps", "Jungle Camps").SetValue(true));
            menuLastHit.AddItem(new MenuItem("lh_Cannons", "Cannon Minions").SetValue(false));
            menuLastHit.AddItem(new MenuItem("lh_OoRMinions", "Minions Out of Range").SetValue(true));

            var menuKillSteal = new Menu("Kill Steal", "killsteal");
            menuKillSteal.AddItem(new MenuItem("ks_enabled", "State").SetValue(true));
            menuKillSteal.AddItem(new MenuItem("ks_Q1", "Javelin Toss").SetValue(true));
            menuKillSteal.AddItem(new MenuItem("ks_dot", "Ignite").SetValue(true));

            var menuMisc = new Menu("Misc", "Misc");
            menuMisc.AddItem(
                new MenuItem("autoHealMode", "Auto Heal Mode").SetValue(
                    new StringList(new[] { "OFF", "Self", "Allies" }, 1)));
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
            var drawCd = menuDrawings.AddItem(new MenuItem("draw_Cd", "Combo Damage").SetValue(true));


            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = drawCd.GetValue<bool>();

            drawCd.ValueChanged +=
                (sender, eventArgs) => Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();

            _config.AddSubMenu(menuTargetSelector);
            _config.AddSubMenu(menuOrbwalker);
            _config.AddSubMenu(menuKeyBindings);
            _config.AddSubMenu(menuCombo);
            _config.AddSubMenu(menuHarass);
            _config.AddSubMenu(menuKillSteal);
            _config.AddSubMenu(menuFarm);
            _config.AddSubMenu(menuLastHit);
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
            {
                KillSteal();
            }

            if (_config.Item("autoHealMode").GetValue<StringList>().SelectedIndex > 0)
            {
                AutoHeal(_config.Item("autoHealMode").GetValue<StringList>().SelectedIndex == 2);
            }

            switch (ActiveMode)
            {
                case ActiveModes.Combo:
                    Combo();
                    break;
                case ActiveModes.Mixed:
                    Harass();
                    break;
                case ActiveModes.LaneClear:
                    Farm();
                    break;
                case ActiveModes.LastHit:
                    LastHit();
                    break;
                case ActiveModes.Flee:
                    Flee();
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
                {
                    Render.Circle.DrawCircle(_player.Position, 750f, drawW2.Color);
                }

                if (drawE2.Active)
                {
                    Render.Circle.DrawCircle(_player.Position, 300f, drawE2.Color);
                }
            }

            if (!IsCougar || !drawCf)
            {
                if (drawQ1.Active)
                {
                    Render.Circle.DrawCircle(_player.Position, 1500f, drawQ1.Color);
                }

                if (drawW1.Active)
                {
                    Render.Circle.DrawCircle(_player.Position, 900f, drawW1.Color);
                }

                if (drawE1.Active)
                {
                    Render.Circle.DrawCircle(_player.Position, 600f, drawE1.Color);
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1600f, TargetSelector.DamageType.Magical);
            if (target == null)
            {
                return;
            }

            var marked = target.HasBuff("NidaleePassiveHunted");
            var hunting = _player.HasBuff("NidaleePassiveHunting");
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
                {
                    _bork.Cast(target);
                }

                if (Items.CanUseItem(_cutlass.Id))
                {
                    _cutlass.Cast(target);
                }
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
                {
                    Q.CastOnUnit(_player, PacketCasting);
                }

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
            var target = TargetSelector.GetTarget(IsCougar ? E.Range : Q.Range, TargetSelector.DamageType.Magical);
            var orbTarget = _orbwalker.GetTarget();
            var useQ = _config.Item("harass_Q1").GetValue<bool>();
            var useW = _config.Item("harass_W1").GetValue<bool>();
            var useE = _config.Item("harass_E2").GetValue<bool>();

            var minMn = _config.Item("harass_Mn").GetValue<Slider>().Value;

            if ((orbTarget != null && orbTarget.IsValid<Obj_AI_Minion>()))
            {
                return;
            }

            if (IsCougar)
            {
                if (useE && E.IsReady())
                {
                    E.Cast(target);
                }

                return;
            }

            if (_player.ManaPercentage() < minMn)
            {
                return;
            }

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
            var minions =
                MinionManager.GetMinions(_player.Position, 750f, MinionTypes.All, MinionTeam.NotAlly)
                    .OrderByDescending(m => m.HasBuff("NidaleePassiveHunted") ? float.MaxValue : m.MaxHealth);
            var target = minions.FirstOrDefault(m => m.IsValidTarget());

            if (target == null)
            {
                return;
            }

            var marked = target.HasBuff("NidaleePassiveHunted");

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

        private static void LastHit()
        {
            if (IsCougar)
            {
                return;
            }

            var lastHitJungleCamps = _config.Item("lh_JungleCamps").GetValue<bool>();
            var lastHitCannons = _config.Item("lh_Cannons").GetValue<bool>();
            var lastHitOoRMinions = _config.Item("lh_OoRMinions").GetValue<bool>();

            var minions = MinionManager.GetMinions(_player.Position, 1500f);

            if (lastHitJungleCamps)
            {
                var jungleCamps = MinionManager.GetMinions(_player.Position, 1500f, MinionTypes.All, MinionTeam.Neutral);

                var neutralTarget =
                    jungleCamps.OrderByDescending(m => m.MaxHealth)
                        .FirstOrDefault(
                            m =>
                                HealthPrediction.GetHealthPrediction(m, (int) (Q.Speed * m.Distance(_player))) <=
                                Q.GetDamage(m));

                if (neutralTarget != null && Q.IsReady())
                {
                    Q.Cast(neutralTarget);
                    return;
                }
            }

            var target =
                minions.FirstOrDefault(
                    m =>
                        HealthPrediction.GetHealthPrediction(m, (int) (Q.Speed * m.Distance(_player))) <= Q.GetDamage(m) &&
                        ((lastHitCannons && m.BaseSkinName.Contains("MinionSiege")) ||
                         (lastHitOoRMinions && m.Distance(_player) >= Orbwalking.GetRealAutoAttackRange(_player))));


            if (target != null && Q.IsReady())
            {
                Q.Cast(target);
            }
        }

        private static void Flee()
        {
            // todo
        }

        private static void KillSteal()
        {
            if (ActiveMode == ActiveModes.Combo)
            {
                return;
            }

            var useQ = _config.Item("ks_Q1").GetValue<bool>();
            var useI = _config.Item("ks_dot").GetValue<bool>();

            if (useQ && !IsCougar)
            {
                var qTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(hero => hero.IsValidTarget(1500f) && hero.Health < Q.GetDamage(hero));

                if (qTarget != null && Q.IsReady() && qTarget.IsValid)
                {
                    Q.Cast(qTarget, PacketCasting);
                }
            }

            if (useI && _igniteSlot != SpellSlot.Unknown)
            {
                var igniteTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .FirstOrDefault(
                            hero =>
                                hero.IsValidTarget(600f) &&
                                hero.Health < _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite) &&
                                !hero.HasBuff("SummonerDot", true));

                if (igniteTarget != null && _player.Spellbook.GetSpell(_igniteSlot).State == SpellState.Ready)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, igniteTarget);
                }
            }
        }

        private static void AutoHeal(bool healAllies)
        {
            if (_player.HasBuff("Recall"))
            {
                return;
            }

            var minHealthPct = _config.Item("autoHealPct").GetValue<Slider>().Value;

            var target = _player;

            if (healAllies && target.HealthPercentage() > minHealthPct)
            {
                target =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .OrderBy(hero => hero.Health)
                        .First(
                            hero =>
                                hero.IsAlly && hero.IsValidTarget(E.Range, false) &&
                                hero.HealthPercentage() <= minHealthPct);
            }

            if (E.IsReady() && target.HealthPercentage() <= minHealthPct)
            {
                E.CastOnUnit(target, PacketCasting);
            }
        }

        private static float GetComboDamage(Obj_AI_Base target)
        {
            var dmg = 0f;
            var hunted = target.HasBuff("NidaleePassiveHunted");

            if (!IsCougar && Q.IsReady())
            {
                dmg += Q.GetDamage(target);
            }

            if (Q.IsReady())
            {
                dmg += hunted ? Q.GetDamage(target, 1) * 1.33f : Q.GetDamage(target, 1);
            }

            if (W.IsReady())
            {
                dmg += W.GetDamage(target, 1);
            }

            if (E.IsReady())
            {
                dmg += E.GetDamage(target, 1);
            }

            return dmg;
        }

        private enum ActiveModes
        {
            Combo,
            Mixed,
            LaneClear,
            LastHit,
            Flee,
            None
        }
    }
}