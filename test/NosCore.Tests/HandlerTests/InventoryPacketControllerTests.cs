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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.Configuration;
using NosCore.Controllers;
using NosCore.Core.Encryption;
using NosCore.Core.Serializing;
using NosCore.Data;
using NosCore.Data.AliveEntities;
using NosCore.Data.StaticEntities;
using NosCore.Database;
using NosCore.DAL;
using NosCore.GameObject;
using NosCore.GameObject.Map;
using NosCore.GameObject.Networking;
using NosCore.GameObject.Networking.ClientSession;
using NosCore.GameObject.Networking.Group;
using NosCore.GameObject.Services.CharacterBuilder;
using NosCore.GameObject.Services.Inventory;
using NosCore.GameObject.Services.ItemBuilder;
using NosCore.GameObject.Services.ItemBuilder.Item;
using NosCore.GameObject.Services.MapInstanceAccess;
using NosCore.Packets.ClientPackets;
using NosCore.Packets.ServerPackets;
using NosCore.Shared.Enumerations.Character;
using NosCore.Shared.Enumerations.Group;
using NosCore.Shared.Enumerations.Interaction;
using NosCore.Shared.Enumerations.Items;
using NosCore.Shared.Enumerations.Map;
using NosCore.Shared.I18N;

namespace NosCore.Tests.HandlerTests
{
    [TestClass]
    public class InventoryPacketControllerTests
    {
        private readonly ClientSession _session = new ClientSession(null,
            new List<PacketController> { new InventoryPacketController() }, null);

        private AccountDto _acc;
        private CharacterDto _chara;
        private InventoryPacketController _handler;
        private ItemBuilderService _itemBuilder;

        [TestInitialize]
        public void Setup()
        {
            PacketFactory.Initialize<NoS0575Packet>();
            var contextBuilder =
                new DbContextOptionsBuilder<NosCoreContext>().UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString());
            DataAccessHelper.Instance.InitializeForTest(contextBuilder.Options);
            _acc = new AccountDto { Name = "AccountTest", Password = EncryptionHelper.Sha512("test") };
            _chara = new CharacterDto
            {
                CharacterId = 1,
                Name = "TestExistingCharacter",
                Slot = 1,
                AccountId = _acc.AccountId,
                MapId = 1,
                State = CharacterState.Active
            };
            _session.InitializeAccount(_acc);

            var items = new List<Item>
            {
                new Item {Type = PocketType.Main, VNum = 1012},
                new Item {Type = PocketType.Main, VNum = 1013},
                new Item {Type = PocketType.Equipment, VNum = 1, ItemType = ItemType.Weapon},
                new Item {Type = PocketType.Equipment, VNum = 912, ItemType = ItemType.Specialist},
                new Item {Type = PocketType.Equipment, VNum = 924, ItemType = ItemType.Fashion}
            };
            var conf = new WorldConfiguration { BackpackSize = 5, MaxItemAmount = 999 };
            _itemBuilder = new ItemBuilderService(items);
            _handler = new InventoryPacketController(conf, _itemBuilder);

            _handler.RegisterSession(_session);
            _session.SetCharacter(_chara.Adapt<Character>());
            _session.Character.Inventory = new InventoryService(items, conf);
        }

        [TestMethod]
        public void Test_Delete_FromSlot()
        {
             _session.Character.Inventory.AddItemToPocket(_itemBuilder.Create(1012, 1, 999));
            _handler.AskToDelete(new BiPacket{Option = RequestDeletionType.Confirmed, Slot = 0, PocketType = PocketType.Main });
            var packet = (IvnPacket)_session.LastPacket;
            Assert.IsTrue(packet.IvnSubPackets.All(iv=>iv.Slot == 0 && iv.VNum == -1));
        }
        
        [TestMethod]
        public void Test_Delete_FromEquiment()
        {
            _session.Character.Inventory.AddItemToPocket(_itemBuilder.Create(1, 1));
            _handler.AskToDelete(new BiPacket { Option = RequestDeletionType.Confirmed, Slot = 0, PocketType = PocketType.Equipment });
            Assert.IsTrue(_session.Character.Inventory.Count == 0);
            var packet = (IvnPacket)_session.LastPacket;
            Assert.IsTrue(packet.IvnSubPackets.All(iv => iv.Slot == 0 && iv.VNum == -1));
        }
    }
}