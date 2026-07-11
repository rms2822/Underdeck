using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Underdeck.Core;

namespace Underdeck.Presentation
{
    /// <summary>
    /// The entire game, wired up procedurally: attach this to one empty
    /// GameObject in an otherwise-empty scene and press Play. Builds its own
    /// Canvas/EventSystem and every screen (title, room, action sheets, boss,
    /// shop, relic draft, end) from <see cref="Underdeck.Core.GameEngine"/>.
    ///
    /// This favors correctness and completeness of the game LOOP over visual
    /// fidelity to the reference web prototype — it uses plain legacy
    /// UI.Text/Image/Button (no TextMeshPro, no art assets) so the project
    /// has zero package-import steps. Treat it as a functional first pass to
    /// build on, not a finished look.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private RunState _state;
        private string _lastSeed;
        private RectTransform _screenRoot;
        private RectTransform _overlayRoot;
        private Text _toastText;

        private void Awake()
        {
            BuildCanvas();
            ShowTitle();
        }

        // ================= canvas / plumbing =================

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("UnderdeckCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            UIFactory.CreatePanel(canvasGo.transform, "Background", UIFactory.Ground);
            _screenRoot = UIFactory.NewChild(canvasGo.transform, "Screen");
            UIFactory.Stretch(_screenRoot);
            _overlayRoot = UIFactory.NewChild(canvasGo.transform, "Overlay");
            UIFactory.Stretch(_overlayRoot);

            _toastText = UIFactory.CreateText(canvasGo.transform, "", 30, UIFactory.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            var toastRt = _toastText.GetComponent<RectTransform>();
            toastRt.anchorMin = new Vector2(0.5f, 1f);
            toastRt.anchorMax = new Vector2(0.5f, 1f);
            toastRt.pivot = new Vector2(0.5f, 1f);
            toastRt.sizeDelta = new Vector2(1000, 90);
            toastRt.anchoredPosition = new Vector2(0, -160);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                Destroy(t.GetChild(i).gameObject);
        }

        private void CloseOverlay() => ClearChildren(_overlayRoot);

        private void ShowToast(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            _toastText.text = text;
            CancelInvoke(nameof(ClearToast));
            Invoke(nameof(ClearToast), 1.3f);
        }
        private void ClearToast() => _toastText.text = "";

        private static string RandomSeed()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            return BitConverter.ToUInt64(bytes, 0).ToString();
        }

        private static string DailySeed()
        {
            var d = DateTime.UtcNow;
            return (d.Year * 10000 + d.Month * 100 + d.Day).ToString();
        }

        // ================= title =================

        private void ShowTitle()
        {
            ClearChildren(_screenRoot);
            CloseOverlay();

            var panel = UIFactory.NewChild(_screenRoot, "Title");
            UIFactory.Stretch(panel);
            UIFactory.AddVertical(panel.gameObject, spacing: 18, align: TextAnchor.MiddleCenter,
                padding: new RectOffset(60, 60, 220, 60));

            UIFactory.CreateText(panel, "UNDERDECK", 64, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.CreateText(panel, "the dungeon is a deck that wants you dead", 22, UIFactory.BoneDim,
                TextAnchor.MiddleCenter, FontStyle.Italic);

            var spacer = UIFactory.NewChild(panel, "Spacer");
            UIFactory.Fixed(spacer.gameObject, height: 40);

            UIFactory.CreateOptionButton(panel, "Begin the Descent", null, UIFactory.Gold, true,
                () => StartRun(RandomSeed()));
            UIFactory.CreateOptionButton(panel, "Daily Descent", null, UIFactory.PanelLight, true,
                () => StartRun(DailySeed()));

            float best = PlayerPrefs.GetFloat("underdeck.best", -1);
            if (best >= 0)
                UIFactory.CreateText(panel, $"best score {(int)best}", 18, UIFactory.BoneDim);
        }

        private void StartRun(string seed)
        {
            _lastSeed = seed;
            _state = GameEngine.NewRun(seed);
            Refresh();
        }

        // ================= main dispatcher =================

        private void Refresh()
        {
            CloseOverlay();
            switch (_state.Phase)
            {
                case RunPhase.Room: RenderRoomScreen(); break;
                case RunPhase.BossIntro: RenderRoomScreen(); ShowBossIntro(); break;
                case RunPhase.Boss: RenderRoomScreen(); break;
                case RunPhase.Over: RenderEndScreen(); break;
            }
        }

        // ================= room / boss screen =================

        private static string Corner(Card c) => c.Kind switch
        {
            CardKind.Monster => GameEngine.ThreatOf(c).ToString(),
            CardKind.Potion => c.Rank.ToString(),
            CardKind.Treasure => c.Rank.ToString(),
            CardKind.Cache => "?",
            CardKind.Curse => "!",
            _ => "",
        };

        private void RenderRoomScreen()
        {
            ClearChildren(_screenRoot);
            var root = UIFactory.NewChild(_screenRoot, "Room");
            UIFactory.Stretch(root);
            UIFactory.AddVertical(root.gameObject, spacing: 14, align: TextAnchor.UpperCenter,
                padding: new RectOffset(24, 24, 40, 24));

            // top bar
            var topBar = UIFactory.NewChild(root, "TopBar");
            UIFactory.Fixed(topBar.gameObject, height: 44);
            UIFactory.AddHorizontal(topBar.gameObject, spacing: 10);
            var depth = Content.Depths[_state.Depth - 1];
            UIFactory.CreateText(topBar, $"DEPTH {depth.Numeral} · {depth.Name}", 20, UIFactory.BoneDim,
                TextAnchor.MiddleLeft);
            UIFactory.CreateText(topBar, $"🪙 {_state.Gold}", 22, UIFactory.Gold, TextAnchor.MiddleRight);

            // cancel-aim button, shown only while a targeting skill is armed
            if (_state.Mode != InputMode.None)
            {
                UIFactory.CreateOptionButton(root, "✕ Cancel Aim", null, UIFactory.PanelLight, true, () =>
                {
                    GameEngine.CancelMode(_state);
                    Refresh();
                });
            }

            // room grid (or the boss tile)
            var gridHolder = UIFactory.NewChild(root, "GridHolder");
            UIFactory.Fixed(gridHolder.gameObject, height: 460);
            if (_state.Phase == RunPhase.Boss && _state.Boss != null)
            {
                UIFactory.AddVertical(gridHolder.gameObject, align: TextAnchor.MiddleCenter);
                string label = _state.Boss.Name + (_state.Boss.HitsLeft > 1 ? $" · {_state.Boss.HitsLeft} lives left" : "");
                var bossTile = UIFactory.CreateTile(gridHolder, _state.Boss.Art, label, _state.Boss.Threat.ToString(),
                    UIFactory.Blood, false, OpenBossSheet);
                UIFactory.Fixed(bossTile.gameObject, height: 420, width: 420);
            }
            else
            {
                UIFactory.AddGrid(gridHolder.gameObject, new Vector2(480, 210), 2, spacing: 12);
                foreach (var c in _state.Room)
                {
                    bool selected = _state.Mode == InputMode.Smoke && _state.SmokePicks.Contains(c);
                    var bg = c.Kind == CardKind.Monster ? UIFactory.Panel
                        : c.Kind == CardKind.Curse ? new Color32(60, 40, 80, 255) : UIFactory.PanelLight;
                    UIFactory.CreateTile(gridHolder, CardInfo.Art(c), CardInfo.Name(c), Corner(c), bg, selected,
                        () => OnRoomCardTapped(c));
                }
            }

            // hero row
            var heroRow = UIFactory.NewChild(root, "HeroRow");
            UIFactory.Fixed(heroRow.gameObject, height: 70);
            UIFactory.AddVertical(heroRow.gameObject, spacing: 4);
            string weaponLine = _state.Weapon == null ? "unarmed"
                : $"{_state.Weapon.Name} · pwr {GameEngine.EffPower(_state)}" +
                  (_state.WeaponLimit != null ? $" · ≤ {GameEngine.EffLimit(_state)}" : " · keen");
            UIFactory.CreateText(heroRow, weaponLine, 18, UIFactory.Steel);
            UIFactory.CreateText(heroRow, $"♥ {_state.Hp}/{_state.MaxHp}   ◆ {_state.Focus}   relics {_state.Relics.Count}",
                18, UIFactory.Bone);
            UIFactory.CreateHpBar(heroRow, _state.MaxHp > 0 ? (float)_state.Hp / _state.MaxHp : 0);

            if (GameEngine.HasRelic(_state, "bloodpact") && _state.Phase != RunPhase.Over)
            {
                UIFactory.CreateOptionButton(root, "Bloodpact: +1 ◆ for 2 ♥", null, UIFactory.PanelLight,
                    _state.Hp > 2, () => { GameEngine.UseBloodpact(_state); Refresh(); });
            }

            // hand row
            var handRow = UIFactory.NewChild(root, "HandRow");
            UIFactory.Fixed(handRow.gameObject, height: 150);
            UIFactory.AddHorizontal(handRow.gameObject, spacing: 8);
            for (int i = 0; i < _state.Skills.Hand.Count; i++)
            {
                int idx = i;
                string id = _state.Skills.Hand[i];
                var sk = Content.Skills[id];
                bool locked = i == _state.LockedIdx;
                var (usable, _) = GameEngine.SkillUsability(_state, id);
                string cost = sk.Cost == 0 ? "free" : new string('◆', sk.Cost);
                var tile = UIFactory.CreateTile(handRow, locked ? "⛓️" : sk.Art, sk.Name, locked ? "" : cost,
                    UIFactory.Panel, false, () => OnSkillTapped(idx));
                tile.interactable = !locked && usable;
            }
            UIFactory.CreateTile(handRow, "🏃", "FLEE", "", UIFactory.Blood, false, OnFleeTapped)
                .interactable = GameEngine.CanFleeNow(_state) && _state.Mode == InputMode.None;

            UIFactory.CreateText(root, HintText(), 16, UIFactory.BoneDim, TextAnchor.MiddleCenter, FontStyle.Italic);
        }

        private string HintText()
        {
            if (_state.Phase == RunPhase.Over) return "";
            if (_state.Mode == InputMode.Smoke)
                return _state.SmokePicks.Count >= 2 ? "" : $"Choose {2 - _state.SmokePicks.Count} more card(s) to banish.";
            if (_state.Mode != InputMode.None) return "Choose a target in the room.";
            if (_state.Phase == RunPhase.Boss) return "Tap the boss. Spend skills first if you need them.";
            if (_state.FledLast && !GameEngine.HasRelic(_state, "boots") && !_state.SlipActive)
                return "You fled the last room — no escape from this one.";
            if (_state.ActedThisRoom > 0) return "You have acted — see this room through.";
            return "Face 3 of 4. The last card follows you down.";
        }

        private void OnRoomCardTapped(Card card)
        {
            switch (_state.Mode)
            {
                case InputMode.Bash:
                case InputMode.Revenant:
                    if (!GameEngine.ResolveBashRevenant(_state, card)) ShowToast("not a valid target");
                    Refresh();
                    break;
                case InputMode.Knife:
                    GameEngine.ResolveKnifeThrow(_state, card);
                    Refresh();
                    break;
                case InputMode.Snipe:
                    GameEngine.ArmSnipe(_state, card);
                    OpenCardSheet(card); // resolving normally from here commits the snipe
                    break;
                case InputMode.Smoke:
                    GameEngine.ToggleSmokePick(_state, card);
                    Refresh();
                    break;
                default:
                    OpenCardSheet(card);
                    break;
            }
        }

        private void OnSkillTapped(int idx)
        {
            string id = _state.Skills.Hand[idx];
            var (usable, why) = GameEngine.SkillUsability(_state, id);
            if (!usable) { ShowToast(why); return; }

            switch (id)
            {
                case "scout":
                    GameEngine.PlayScout(_state, idx);
                    ShowScoutSheet();
                    break;
                case "necromancy":
                    var six = GameEngine.PlayNecromancy(_state, idx);
                    ShowNecromancySheet(six);
                    break;
                case "bash": case "revenant": case "knifethrow": case "snipe": case "smokebomb":
                    GameEngine.BeginTargetMode(_state, idx);
                    Refresh();
                    break;
                default:
                    var fx = GameEngine.PlayInstant(_state, idx);
                    ShowToast(fx.Label);
                    Refresh();
                    break;
            }
        }

        private void OnFleeTapped()
        {
            if (!GameEngine.Flee(_state)) ShowToast("can't flee");
            Refresh();
        }

        // ================= card action sheet =================

        private string DeathNote(int dmg) =>
            (dmg >= _state.Hp && !(GameEngine.HasRelic(_state, "phoenix") && !_state.PhoenixUsed)) ? " — you will die" : "";

        private void OpenCardSheet(Card card)
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            var backdropBtn = backdrop.gameObject.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(CloseOverlay);

            var sheet = BeginSheet();

            switch (card.Kind)
            {
                case CardKind.Monster:
                    BuildMonsterSheet(sheet, card);
                    break;
                case CardKind.Potion:
                    BuildPotionSheet(sheet, card);
                    break;
                case CardKind.Treasure:
                    UIFactory.CreateText(sheet, $"Treasure — {card.Rank} gold", 24, UIFactory.Bone);
                    UIFactory.CreateOptionButton(sheet, "Pocket it",
                        $"+{(GameEngine.HasRelic(_state, "glasssigil") ? card.Rank * 2 : card.Rank)} gold",
                        UIFactory.PanelLight, true, () =>
                        {
                            GameEngine.PocketTreasure(_state, card);
                            CloseOverlay(); Refresh();
                        });
                    break;
                case CardKind.Cache:
                    UIFactory.CreateText(sheet, "Loot Cache — a skill waits inside", 24, UIFactory.Bone);
                    UIFactory.CreateOptionButton(sheet, "Pry it open", "draft 1 of 3 skills, or take gold instead",
                        UIFactory.PanelLight, true, () =>
                        {
                            GameEngine.OpenCache(_state, card);
                            ShowSkillDraftSheet(() => { GameEngine.AfterResolve(_state); Refresh(); });
                        });
                    break;
                case CardKind.Curse:
                    var cu = Content.Curses[card.Curse];
                    UIFactory.CreateText(sheet, cu.Name, 24, UIFactory.Blood);
                    UIFactory.CreateText(sheet, cu.Text, 17, UIFactory.BoneDim, TextAnchor.MiddleCenter, FontStyle.Italic);
                    UIFactory.CreateOptionButton(sheet, "Suffer it", "curses cannot be ignored", UIFactory.PanelLight, true,
                        () =>
                        {
                            GameEngine.SufferCurse(_state, card);
                            CloseOverlay(); Refresh();
                        });
                    break;
            }

            UIFactory.CreateOptionButton(sheet, "Leave it", null, UIFactory.Panel, true, CloseOverlay);
        }

        private void BuildMonsterSheet(Transform sheet, Card card)
        {
            string elite = card.Elite != EliteKind.None ? $" · {Content.Elites[card.Elite].Text}" : "";
            UIFactory.CreateText(sheet, $"{CardInfo.Name(card)} — threat {GameEngine.ThreatOf(card)}{elite}",
                24, UIFactory.Bone);

            if (_state.Weapon != null)
            {
                if (GameEngine.CanUseWeapon(_state, card))
                {
                    var pv = GameEngine.PreviewFight(_state, card, true);
                    UIFactory.CreateOptionButton(sheet, $"Strike with the {_state.Weapon.Name} ({GameEngine.EffPower(_state)})",
                        $"take {pv.Dmg} damage{DeathNote(pv.Dmg)}", UIFactory.PanelLight, true, () =>
                        {
                            GameEngine.FightMonster(_state, card, true);
                            CloseOverlay(); Refresh();
                        });
                }
                else
                {
                    UIFactory.CreateOptionButton(sheet, $"{_state.Weapon.Name} is too dull",
                        $"only cuts threat {GameEngine.EffLimit(_state)} or less", UIFactory.Panel, false, null);
                }
            }

            var pvBare = GameEngine.PreviewFight(_state, card, false);
            UIFactory.CreateOptionButton(sheet, "Fight bare-handed", $"take {pvBare.Dmg} damage{DeathNote(pvBare.Dmg)}",
                UIFactory.PanelLight, true, () =>
                {
                    GameEngine.FightMonster(_state, card, false);
                    CloseOverlay(); Refresh();
                });
        }

        private void BuildPotionSheet(Transform sheet, Card card)
        {
            bool allowed = !_state.PotionDrunk || _state.SecondPotion;
            int healed = allowed ? Mathf.Min(_state.MaxHp - _state.Hp, card.Rank) : 0;
            string sub = !allowed ? "no effect — one potion per room"
                : healed == 0 ? "no effect — health is full"
                : $"restore {healed} health";
            UIFactory.CreateText(sheet, CardInfo.Name(card) + $" — restores {card.Rank}", 24, UIFactory.Bone);
            UIFactory.CreateOptionButton(sheet, "Drink it", sub, UIFactory.PanelLight, true, () =>
            {
                GameEngine.DrinkPotion(_state, card);
                CloseOverlay(); Refresh();
            });
        }

        private void OpenBossSheet()
        {
            if (_state.Boss == null) return;
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            var backdropBtn = backdrop.gameObject.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            backdropBtn.onClick.AddListener(CloseOverlay);

            var sheet = BeginSheet();
            var boss = _state.Boss;
            UIFactory.CreateText(sheet, $"{boss.Name} — threat {boss.Threat}" + (boss.HitsLeft > 1 ? " · two lives" : ""),
                24, UIFactory.Bone);
            UIFactory.CreateText(sheet, boss.Text, 17, UIFactory.BoneDim, TextAnchor.MiddleCenter, FontStyle.Italic);

            var bossCard = Card.Monster('♠', boss.Threat);
            if (_state.Weapon != null && GameEngine.CanUseWeapon(_state, bossCard))
            {
                var pv = GameEngine.PreviewFight(_state, bossCard, true);
                UIFactory.CreateOptionButton(sheet, $"Strike with the {_state.Weapon.Name} ({GameEngine.EffPower(_state)})",
                    $"take {pv.Dmg} damage{DeathNote(pv.Dmg)}", UIFactory.PanelLight, true,
                    () => ResolveBossFight(true));
            }
            var pvBare = GameEngine.PreviewFight(_state, bossCard, false);
            UIFactory.CreateOptionButton(sheet, "Fight bare-handed", $"take {pvBare.Dmg} damage{DeathNote(pvBare.Dmg)}",
                UIFactory.PanelLight, true, () => ResolveBossFight(false));

            UIFactory.CreateOptionButton(sheet, "Not yet", null, UIFactory.Panel, true, CloseOverlay);
        }

        private void ResolveBossFight(bool useWeapon)
        {
            var r = GameEngine.FightBoss(_state, useWeapon);
            CloseOverlay();
            if (r.BossOutcome == BossOutcome.Dead)
            {
                GameEngine.EndRun(_state, false);
                Refresh();
            }
            else if (r.BossOutcome == BossOutcome.Slain)
            {
                if (GameEngine.TryCompleteRun(_state)) { Refresh(); return; }
                ShowRelicDraft();
            }
            else
            {
                Refresh();
            }
        }

        // ================= sub-sheets: scout / necromancy / drafts / shop =================

        private RectTransform BeginSheet()
        {
            var sheet = UIFactory.NewChild(_overlayRoot, "Sheet");
            sheet.anchorMin = new Vector2(0, 0);
            sheet.anchorMax = new Vector2(1, 0);
            sheet.pivot = new Vector2(0.5f, 0);
            sheet.sizeDelta = new Vector2(0, 1400);
            sheet.anchoredPosition = Vector2.zero;
            sheet.gameObject.AddComponent<Image>().color = UIFactory.Panel;
            UIFactory.AddVertical(sheet.gameObject, spacing: 10, padding: new RectOffset(24, 24, 24, 40));
            return sheet;
        }

        private void ShowScoutSheet()
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            backdrop.gameObject.AddComponent<Button>().onClick.AddListener(CloseOverlay);

            var sheet = BeginSheet();
            UIFactory.CreateText(sheet, "Scout — the next two cards", 24, UIFactory.Bone);
            var top = GameEngine.PeekDeckTop(_state);
            if (top.Count == 0)
            {
                UIFactory.CreateText(sheet, "The dungeon is spent.", 17, UIFactory.BoneDim);
            }
            else
            {
                string line = string.Join("   ·   ", top.ConvertAll(c => $"{CardInfo.Art(c)} {CardInfo.Name(c)}"));
                UIFactory.CreateText(sheet, line, 18, UIFactory.BoneDim);
                if (top.Count == 2)
                {
                    UIFactory.CreateOptionButton(sheet, "Swap their order", "the second will come first",
                        UIFactory.PanelLight, true, () =>
                        {
                            GameEngine.SwapDeckTopTwo(_state);
                            CloseOverlay(); Refresh();
                        });
                }
            }
            UIFactory.CreateOptionButton(sheet, "Leave them", null, UIFactory.Panel, true, () => { CloseOverlay(); Refresh(); });
        }

        private void ShowNecromancySheet(List<Card> six)
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            backdrop.gameObject.AddComponent<Button>().onClick.AddListener(CloseOverlay);

            var sheet = BeginSheet();
            UIFactory.CreateText(sheet, "Necromancy — claim one red card", 24, UIFactory.Bone);
            bool anyRed = false;
            foreach (var c in six)
            {
                if (c.Kind == CardKind.Potion)
                {
                    anyRed = true;
                    bool allowed = !_state.PotionDrunk || _state.SecondPotion;
                    int preview = allowed ? Mathf.Min(_state.MaxHp - _state.Hp, c.Rank) : 0;
                    UIFactory.CreateOptionButton(sheet, $"Raise the {CardInfo.Name(c)} ({c.Rank}♥)",
                        preview > 0 ? $"restore {preview} health" : "no effect", UIFactory.PanelLight, true, () =>
                        {
                            GameEngine.NecromancyChoosePotion(_state, c);
                            CloseOverlay(); Refresh();
                        });
                }
                else if (c.Kind == CardKind.Treasure || c.Kind == CardKind.Cache)
                {
                    anyRed = true;
                    int val = c.Kind == CardKind.Cache ? 6 : c.Rank;
                    int shown = GameEngine.HasRelic(_state, "glasssigil") ? val * 2 : val;
                    UIFactory.CreateOptionButton(sheet, $"Raise the {CardInfo.Name(c)} (♦)", $"+{shown} gold",
                        UIFactory.PanelLight, true, () =>
                        {
                            GameEngine.NecromancyChooseGold(_state, c);
                            CloseOverlay(); Refresh();
                        });
                }
                else
                {
                    UIFactory.CreateOptionButton(sheet, CardInfo.Name(c), "the dead offer no aid", UIFactory.Panel, false, null);
                }
            }
            if (!anyRed) UIFactory.CreateText(sheet, "Only bones answered.", 17, UIFactory.BoneDim);
            UIFactory.CreateOptionButton(sheet, anyRed ? "Take nothing" : "So be it", null, UIFactory.Panel, true,
                () => { CloseOverlay(); Refresh(); });
        }

        private void ShowSkillDraftSheet(Action onDone)
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            backdrop.gameObject.AddComponent<Button>().onClick.AddListener(() => { CloseOverlay(); onDone(); });

            var sheet = BeginSheet();
            UIFactory.CreateText(sheet, "Loot Cache — take one", 24, UIFactory.Bone);
            foreach (var id in GameEngine.GenerateSkillDraft(_state))
            {
                var sk = Content.Skills[id];
                UIFactory.CreateOptionButton(sheet, $"{sk.Art} {sk.Name} ({(sk.Cost == 0 ? "free" : sk.Cost + "◆")})",
                    sk.Text, UIFactory.PanelLight, true, () =>
                    {
                        GameEngine.AddSkillFromDraft(_state, id);
                        CloseOverlay(); onDone();
                    });
            }
            UIFactory.CreateOptionButton(sheet, "Take the coin instead",
                $"+{(GameEngine.HasRelic(_state, "glasssigil") ? 10 : 5)} gold", UIFactory.PanelLight, true, () =>
                {
                    GameEngine.TakeDraftGold(_state);
                    CloseOverlay(); onDone();
                });
        }

        private void ShowRelicDraft()
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);

            var picks = GameEngine.GenerateRelicDraft(_state);
            if (picks.Count == 0) { CloseOverlay(); ShowShop(); return; }

            var sheet = BeginSheet();
            UIFactory.CreateText(sheet, "Boss Trophy — choose a relic", 24, UIFactory.Gold);
            foreach (var id in picks)
            {
                var r = Content.Relics[id];
                UIFactory.CreateOptionButton(sheet, $"{r.Art} {r.Name}", r.Text, UIFactory.PanelLight, true, () =>
                {
                    GameEngine.GainRelic(_state, id);
                    CloseOverlay(); ShowShop();
                });
            }
        }

        private void ShowShop()
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            var stock = GameEngine.GenerateShopStock(_state);
            RenderShop(stock);
        }

        private void RenderShop(ShopStock stock)
        {
            // Remove any previous sheet but keep the backdrop, so repeat purchases
            // rebuild the shop in place instead of flickering the whole overlay.
            var existingSheet = _overlayRoot.Find("Sheet");
            if (existingSheet != null) Destroy(existingSheet.gameObject);

            var sheet = BeginSheet();
            UIFactory.CreateText(sheet, $"The Between-Depths Shop — 🪙 {_state.Gold}", 24, UIFactory.Gold);
            UIFactory.CreateText(sheet, "Spend before descending. Gold stays; regrets don't.", 15, UIFactory.BoneDim,
                TextAnchor.MiddleCenter, FontStyle.Italic);

            foreach (var slot in stock.Weapons)
            {
                var w = Content.Weapons[slot.Id];
                UIFactory.CreateOptionButton(sheet, $"{w.Art} {w.Name} ({w.Power}) — 🪙 {w.Price}", w.Text,
                    UIFactory.PanelLight, !slot.SoldOut && _state.Gold >= w.Price, () =>
                    {
                        GameEngine.BuyWeapon(_state, slot);
                        RenderShop(stock);
                    });
            }
            foreach (var slot in stock.Relics)
            {
                var r = Content.Relics[slot.Id];
                UIFactory.CreateOptionButton(sheet, $"{r.Art} {r.Name} — 🪙 {r.Price}", r.Text,
                    UIFactory.PanelLight, !slot.SoldOut && _state.Gold >= r.Price, () =>
                    {
                        GameEngine.BuyRelic(_state, slot);
                        RenderShop(stock);
                    });
            }
            foreach (var slot in stock.Skills)
            {
                var sk = Content.Skills[slot.Id];
                UIFactory.CreateOptionButton(sheet, $"{sk.Art} {sk.Name} (skill) — 🪙 {GameEngine.ShopSkillPrice}", sk.Text,
                    UIFactory.PanelLight, !slot.SoldOut && _state.Gold >= GameEngine.ShopSkillPrice, () =>
                    {
                        GameEngine.BuySkill(_state, slot);
                        RenderShop(stock);
                    });
            }
            UIFactory.CreateOptionButton(sheet, $"🧉 Tonic — 🪙 {GameEngine.TonicPrice}", $"Heal {GameEngine.TonicHeal} on the spot.",
                UIFactory.PanelLight, _state.Gold >= GameEngine.TonicPrice, () =>
                {
                    GameEngine.BuyTonic(_state);
                    RenderShop(stock);
                });

            int removalPrice = GameEngine.RemovalPrice(_state);
            UIFactory.CreateOptionButton(sheet, $"✂️ Remove a skill card — 🪙 {removalPrice}",
                "Thin your deck — the fee rises each time.", UIFactory.PanelLight, _state.Gold >= removalPrice,
                () => ShowRemoval(stock));

            var depth = Content.Depths[Mathf.Min(2, _state.Depth)];
            UIFactory.CreateOptionButton(sheet, $"Descend to Depth {depth.Numeral} →", null, UIFactory.Gold, true, () =>
            {
                CloseOverlay();
                GameEngine.AdvanceToNextDepth(_state);
                Refresh();
            });
        }

        private void ShowRemoval(ShopStock stock)
        {
            var existingSheet = _overlayRoot.Find("Sheet");
            if (existingSheet != null) Destroy(existingSheet.gameObject);

            var sheet = BeginSheet();
            UIFactory.CreateText(sheet, "Remove a card — it leaves the run", 24, UIFactory.Bone);
            var counts = GameEngine.RemovableSkillCounts(_state);
            foreach (var kv in counts)
            {
                var sk = Content.Skills[kv.Key];
                UIFactory.CreateOptionButton(sheet, $"{sk.Art} {sk.Name} ×{kv.Value}", sk.Text, UIFactory.PanelLight, true,
                    () =>
                    {
                        GameEngine.RemoveSkill(_state, kv.Key);
                        RenderShop(stock);
                    });
            }
            UIFactory.CreateOptionButton(sheet, "Never mind", null, UIFactory.Panel, true, () => RenderShop(stock));
        }

        // ================= boss intro =================

        private void ShowBossIntro()
        {
            CloseOverlay();
            var backdrop = UIFactory.CreatePanel(_overlayRoot, "Backdrop", UIFactory.Backdrop);
            var sheet = BeginSheet();
            var b = Content.Bosses[_state.Depth - 1];
            UIFactory.CreateText(sheet, b.Art, 60, UIFactory.Bone);
            UIFactory.CreateText(sheet, b.Name.ToUpperInvariant(), 30, UIFactory.Blood, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.CreateText(sheet, $"{b.Text} Threat {b.Threat}.", 18, UIFactory.BoneDim,
                TextAnchor.MiddleCenter, FontStyle.Italic);
            UIFactory.CreateOptionButton(sheet, "Face Him", null, UIFactory.Gold, true, () =>
            {
                GameEngine.EnterBossFight(_state);
                CloseOverlay();
                Refresh();
            });
        }

        // ================= end screen =================

        private void RenderEndScreen()
        {
            ClearChildren(_screenRoot);
            var panel = UIFactory.NewChild(_screenRoot, "End");
            UIFactory.Stretch(panel);
            UIFactory.AddVertical(panel.gameObject, spacing: 14, align: TextAnchor.MiddleCenter,
                padding: new RectOffset(60, 60, 160, 60));

            var sc = GameEngine.ScoreParts(_state);
            bool won = _state.Won;

            UIFactory.CreateText(panel, won ? "ESCAPED" : "SLAIN", 54,
                won ? UIFactory.Gold : UIFactory.Blood, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIFactory.CreateText(panel, won ? "The Deep Tyrant falls. You climb into daylight."
                : $"Depth {_state.Depth} keeps your bones.", 18, UIFactory.BoneDim,
                TextAnchor.MiddleCenter, FontStyle.Italic);

            UIFactory.CreateText(panel,
                $"health ×10 · {sc.Hp}\ngold · {sc.Gold}\ndepth ×100 · {sc.Depth}\nbosses ×50 · {sc.Boss}\n\nscore {sc.Total}",
                20, UIFactory.Bone);

            float prevBest = PlayerPrefs.GetFloat("underdeck.best", -1);
            if (sc.Total > prevBest) PlayerPrefs.SetFloat("underdeck.best", sc.Total);
            PlayerPrefs.Save();

            var spacer = UIFactory.NewChild(panel, "Spacer");
            UIFactory.Fixed(spacer.gameObject, height: 20);

            UIFactory.CreateOptionButton(panel, "Descend Again", null, UIFactory.Gold, true, () => StartRun(RandomSeed()));
            UIFactory.CreateOptionButton(panel, "Retry This Seed", null, UIFactory.PanelLight, true, () => StartRun(_lastSeed));
            UIFactory.CreateOptionButton(panel, "Daily Descent", null, UIFactory.PanelLight, true, () => StartRun(DailySeed()));
        }
    }
}
