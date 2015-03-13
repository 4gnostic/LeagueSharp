using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace KassadinCarry
{
    class Program
    {
        public static string hero = "Kassadin";
        public static Obj_AI_Hero player;

        public static List<Spell> SpellList = new List<Spell>();

        // Skiller

        public static Spell Q, W, E, R;

        // Menü

        public static Menu menu;

        // Orbw

        public static Orbwalking.Orbwalker OW;

        // Items

        public static Items.Item zhonya, seraphs;

        // Spell
        private static SpellSlot tutustur;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        static void Game_OnGameLoad(EventArgs args)
        {
            player = ObjectManager.Player;
            if (player.ChampionName != hero) { return; }
            Q = new Spell(SpellSlot.Q, 650f);
            W = new Spell(SpellSlot.W, 150f);
            E = new Spell(SpellSlot.E, 400f);
            R = new Spell(SpellSlot.R, 500f); // 450 den 500 e.

            SpellList.AddRange(new[] { Q, W, R });

            Q.SetTargetted(0.5f, 1400f);
            E.SetSkillshot(0.5f, 10f, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.5f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);


            zhonya = new Items.Item(3157, 10);
            seraphs = new Items.Item(3040, 0);
            tutustur = player.GetSpellSlot("SummonerDot");

            // Menü Oluştur

            menu = new Menu("KassadinCarry", "KassadinCarry", true);
            // ORBw Menü
            var orbwmenu = new Menu("Orbwalking", "orbwalk");
            menu.AddSubMenu(orbwmenu);
            OW = new Orbwalking.Orbwalker(orbwmenu);


            // TS Menü
            var tsmenu = new Menu("T.Selector/ Hedef Seçici", "ts");
            menu.AddSubMenu(tsmenu);
            TargetSelector.AddToMenu(tsmenu);

            // Kombo

            var kombo = new Menu("Kombo Ayarları", "kombo");
            {
                kombo.AddItem(new MenuItem("useQ", "Q Kullan ? ").SetValue(true));
                kombo.AddItem(new MenuItem("useW", "W Kullan ? ").SetValue(true));
                kombo.AddItem(new MenuItem("useE", "E Kullan ? ").SetValue(true));
                kombo.AddItem(new MenuItem("useR", "R Kullan ? ").SetValue(true));
                kombo.AddItem(new MenuItem("useQ", "Q Kullan ? ").SetValue(true));
                // items
                kombo.AddItem(new MenuItem("useZhonya", "Zhonya Kullan ?(0=Kullanma) Can: % ").SetValue(new Slider(25, 100, 0)));
                kombo.AddItem(new MenuItem("useSeraph", "Seraph Kullan ?(0=Kullanma) Can: % ").SetValue(new Slider(35, 100, 0)));
                // spell
                //kombo.AddItem(new MenuItem("useIgnate", "Tutuştur Kullan ? ").SetValue(new StringList(new[] { "KS İçin", "Her Komboda", "Kullanma" }, 1)));
                kombo.AddItem(new MenuItem("useIgnate", "Tutuştur Kullan ? ").SetValue(true));
                //kombo.AddItem(new MenuItem("useHeal", "Heal Kullan ? Can: % ").SetValue(new Slider(25, 100, 0)));
                //kombo.AddItem(new MenuItem("useBar", "Bariyer Kullan ? Can: % ").SetValue(new Slider(25, 100, 0)));
                kombo.AddItem(new MenuItem("useCombo", "Kombo").SetValue(new KeyBind(32, KeyBindType.Press)));

            }

            // Ekstra

            var ekstra = new Menu("Ekstra", "ekstra");
            {
                ekstra.AddItem(new MenuItem("autoQ", "Otomatik Q Kullan ? ").SetValue(false));
                ekstra.AddItem(new MenuItem("autoE", "Otomatik E Kullan ? ").SetValue(false));
                ekstra.AddItem(new MenuItem("ks", "Otomatik KS At(Aktif Değil) ? ").SetValue(true));
                ekstra.AddItem(new MenuItem("buffks", "Güçlendirmelere KS At(Aktif Değil) ? ").SetValue(true));

            }

            // Farm&Jung

            var farm = new Menu("Farm & Jung", "farm");
            {
                //Lane Clear
                farm.AddItem(new MenuItem("pp", "LaneClear/Koridor Temizle"));
                farm.AddItem(new MenuItem("lcQ", "Q Ayarı: ").SetValue(new StringList(new[] { "Son Vuruş", "Temizle", "Kullanma" }, 1)));
                farm.AddItem(new MenuItem("lcW", "W Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("lcW", "E Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("lcW", "R Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("useCombo", "Combo").SetValue(new KeyBind('V', KeyBindType.Press)));

                //Lane Freeze
                farm.AddItem(new MenuItem("pp2", "LaneClear/Koridor Temizle"));
                farm.AddItem(new MenuItem("lfQ", "Q İle Son Vuruş?").SetValue(true));
                farm.AddItem(new MenuItem("useLf", "Combo").SetValue(new KeyBind('X', KeyBindType.Press)));


                //Jung
                farm.AddItem(new MenuItem("pp3", "Orman Temizle (V)"));
                farm.AddItem(new MenuItem("jQ", "Q Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("jW", "W Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("jE", "E Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("jR", "R Kullan ? ").SetValue(true));
                farm.AddItem(new MenuItem("useCombo", "Combo").SetValue(new KeyBind('V', KeyBindType.Press)));



            }

            // Harass

            var harass = new Menu("Harass", "harass");
            {
                harass.AddItem(new MenuItem("hrQ", "Q Kullan ? ").SetValue(true));
                harass.AddItem(new MenuItem("hrE", "E Kullan ? ").SetValue(true));
                harass.AddItem(new MenuItem("useHarass", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));

            }


            Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat("KassadinCarry - PotansiyelTanrı - Yüklendi.");
        }
        private static float GetDistance(AttackableUnit target)
		{
			return Vector3.Distance(player.Position, target.Position);
		}
        private static double hasarhespla(Obj_AI_Base target)
        {
            double dmg = 0;
            if (Q.IsReady() && GetDistance(target) <= Q.Range)
            { dmg += player.GetSpellDamage(target, SpellSlot.Q) * 2; }
            if (W.IsReady() && GetDistance(target) <= W.Range)
            { dmg += player.GetSpellDamage(target, SpellSlot.W); }
            if (E.IsReady() && GetDistance(target) <= E.Range)
            { dmg += player.GetSpellDamage(target, SpellSlot.E); }
            if (menu.Item("useIgnate").GetValue<bool>() && GetDistance(target) <= 600)
            { dmg += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite); }
            return dmg;
        }
        private static bool tutusturhazir()
        {
            return (tutustur != SpellSlot.Unknown && player.Spellbook.CanUseSpell(tutustur) == SpellState.Ready);
        }
        static void Game_OnGameUpdate(EventArgs args)
        {

            var targetQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var targetW = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var targetE = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var targetR = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

            //Harass
            if (menu.Item("useHarass").GetValue<KeyBind>().Active)
            {
                if (menu.Item("hrQ").GetValue<bool>() && Q.IsReady() && targetQ.CountEnemiesInRange(650) > 0)
                {
                    Q.Cast(targetQ);
                }
                if (menu.Item("hrE").GetValue<bool>() && E.IsReady() && targetE.CountEnemiesInRange(400) > 0)
                {
                    E.Cast(targetE);
                }
            }
            //Harass - Son
            //Auto Q-E
            if (menu.Item("autoQ").GetValue<bool>() && Q.IsReady() && targetQ.CountEnemiesInRange(650) > 0)
            {
                Q.Cast(targetQ);
            }
            if (menu.Item("autoE").GetValue<bool>() && E.IsReady() && targetE.CountEnemiesInRange(400) > 0)
            {
                E.Cast(targetE);
            }
            // KS
            if (menu.Item("ks").GetValue<bool>())
            {
                if (R.IsReady() && R.IsKillable(targetR))
                {
                    R.Cast(targetR);
                }
                if (Q.IsReady() && Q.IsKillable(targetQ))
                {
                    Q.Cast(targetQ);
                }
                if (W.IsReady() && W.IsKillable(targetW))
                {
                    W.Cast(targetW);
                }
                if (E.IsReady() && E.IsKillable(targetE))
                {
                    E.Cast(targetE);
                }
                // KS - SON

                // Kombo
                if (menu.Item("useCombo").GetValue<KeyBind>().Active)
                {

                    if (menu.Item("useR").GetValue<bool>() && R.IsReady() && targetR.CountEnemiesInRange(500) > 0 && targetR.IsValidTarget(R.Range))
                    {
                        R.Cast(targetR);
                    }

                    if (menu.Item("useQ").GetValue<bool>() && Q.IsReady())
                    {
                        Q.Cast(targetQ);
                    }

                    if (menu.Item("useW").GetValue<bool>() && W.IsReady())
                    {
                        W.Cast(targetW);
                    }

                    if (menu.Item("useE").GetValue<bool>() && E.IsReady())
                    {
                        E.Cast(targetE);
                    }

                    /*
                     * 100 ----- maxhealt
                     * 25  ----- use
                     * use = (25*maxhealt)/1000
                     * 
                     *
                     * Heal - Bariyer - Zhonya - Serap
                     */
                    if (player.Health < (player.MaxHealth * menu.Item("useSeraph").GetValue<Slider>().Value) / 100 + 1 && Items.HasItem(seraphs.Id) && seraphs.IsReady() && player.CountEnemiesInRange(600) > 1 && !player.InFountain())
                    {
                        seraphs.Cast(player); // Seraph Kullan

                    }
                    if (player.Health < (player.MaxHealth * menu.Item("useZhonya").GetValue<Slider>().Value) / 100 + 1 && Items.HasItem(zhonya.Id) && zhonya.IsReady() && player.CountEnemiesInRange(600) > 1 && !player.InFountain())
                    {
                        zhonya.Cast(player); // Zhonya Kullan

                    }

                   // Tutuştur

                    if (menu.Item("useIgnate").GetValue<bool>() && tutusturhazir() && targetR.CountEnemiesInRange(500)>0 && hasarhespla(targetR) >= targetR.Health)
                    {
                        player.Spellbook.CastSpell(tutustur, targetR);
                    }


                }
                // Kombo - Son



            }
        }
    }
}
