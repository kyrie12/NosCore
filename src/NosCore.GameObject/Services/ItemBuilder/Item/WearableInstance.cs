﻿//  __  _  __    __   ___ __  ___ ___  
// |  \| |/__\ /' _/ / _//__\| _ \ __| 
// | | ' | \/ |`._`.| \_| \/ | v / _|  
// |_|\__|\__/ |___/ \__/\__/|_|_\___| 
// 
// Copyright (C) 2018 - NosCore
// 
// NosCore is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using NosCore.GameObject.Helper;
using NosCore.GameObject.Networking;
using NosCore.Shared;
using NosCore.Shared.Enumerations.Items;
using NosCore.Shared.I18N;
using Serilog;

namespace NosCore.GameObject.Services.ItemBuilder.Item
{
    public class WearableInstance : ItemInstance
    {
        private readonly ILogger _logger = Logger.GetLoggerConfiguration().CreateLogger();
        public WearableInstance(Item item) : base(item)
        {
        }
        public WearableInstance()
        {
        }
        public byte Ammo { get; set; }

        public byte Cellon { get; set; }

        public short CloseDefence { get; set; }

        public short Concentrate { get; set; }

        public short CriticalDodge { get; set; }

        public byte CriticalLuckRate { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public byte DarkElement { get; set; }

        public short DarkResistance { get; set; }

        public short DefenceDodge { get; set; }

        public short DistanceDefence { get; set; }

        public short DistanceDefenceDodge { get; set; }

        //public List<EquipmentOptionDTO> EquipmentOptions { get; set; }

        public short ElementRate { get; set; }

        public byte FireElement { get; set; }

        public short FireResistance { get; set; }

        public short HitRate { get; set; }

        public short Hp { get; set; }

        public bool IsEmpty { get; set; }

        public bool IsFixed { get; set; }

        public byte LightElement { get; set; }

        public short LightResistance { get; set; }

        public short MagicDefence { get; set; }

        public byte MaxElementRate { get; set; }

        public short Mp { get; set; }

        public sbyte? ShellRarity { get; set; }

        public byte WaterElement { get; set; }

        public short WaterResistance { get; set; }

        public long Xp { get; set; }

        public void SetRarityPoint()
        {
            switch (Item.EquipmentSlot)
            {
                case EquipmentType.MainWeapon:
                case EquipmentType.SecondaryWeapon:
                    {
                        int point = CharacterHelper.Instance.RarityPoint(Rare,
                            Item.IsHeroic ? (short)(95 + Item.LevelMinimum) : Item.LevelMinimum);
                        Concentrate = 0;
                        HitRate = 0;
                        DamageMinimum = 0;
                        DamageMaximum = 0;
                        if (Rare >= 0)
                        {
                            for (int i = 0; i < point; i++)
                            {
                                int rndn = RandomFactory.Instance.RandomNumber(0, 3);
                                if (rndn == 0)
                                {
                                    Concentrate++;
                                    HitRate++;
                                }
                                else
                                {
                                    DamageMinimum++;
                                    DamageMaximum++;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i > Rare * 10; i--)
                            {
                                DamageMinimum--;
                                DamageMaximum--;
                            }
                        }
                    }
                    break;

                case EquipmentType.Armor:
                    {
                        int point = CharacterHelper.Instance.RarityPoint(Rare,
                            Item.IsHeroic ? (short)(95 + Item.LevelMinimum) : Item.LevelMinimum);
                        DefenceDodge = 0;
                        DistanceDefenceDodge = 0;
                        DistanceDefence = 0;
                        MagicDefence = 0;
                        CloseDefence = 0;
                        if (Rare < 0)
                        {
                            for (int i = 0; i > Rare * 10; i--)
                            {
                                DistanceDefence--;
                                MagicDefence--;
                                CloseDefence--;
                            }

                            return;
                        }

                        for (int i = 0; i < point; i++)
                        {
                            int rndn = RandomFactory.Instance.RandomNumber(0, 3);
                            if (rndn == 0)
                            {
                                DefenceDodge++;
                                DistanceDefenceDodge++;
                            }
                            else
                            {
                                DistanceDefence++;
                                MagicDefence++;
                                CloseDefence++;
                            }
                        }
                    }
                    break;

                default:
                    _logger.Error(LogLanguage.Instance.GetMessageFromKey(LanguageKey.UNKNOWN_EQUIPMENTTYPE), Item.EquipmentSlot);
                    break;
            }
        }
    }
}