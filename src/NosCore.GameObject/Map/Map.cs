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
using System.Collections.Generic;
using System.Linq;
using NosCore.Data.StaticEntities;
using NosCore.GameObject.Networking;
using NosCore.PathFinder;
using NosCore.Shared;

namespace NosCore.GameObject.Map
{
    public class Map : MapDto
    {
        private short _xLength;

        private short _yLength;

        public short XLength
        {
            get
            {
                if (_xLength == 0)
                {
                    _xLength = BitConverter.ToInt16(Data.AsSpan().Slice(0, 2).ToArray(), 0);
                }

                return _xLength;
            }
        }

        public short YLength
        {
            get
            {
                if (_yLength == 0)
                {
                    _yLength = BitConverter.ToInt16(Data.AsSpan().Slice(2, 2).ToArray(), 0);
                }

                return _yLength;
            }
        }

        public byte this[short x, short y] => Data.AsSpan().Slice(4 + (y * XLength) + x, 1)[0];

        internal bool GetFreePosition(ref short firstX, ref short firstY, byte xpoint, byte ypoint)
        {
            var minX = (short) (-xpoint + firstX);
            var maxX = (short) (xpoint + firstX);

            var minY = (short) (-ypoint + firstY);
            var maxY = (short) (ypoint + firstY);

            var cells = new List<MapCell>();
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if (x != firstX || y != firstY)
                    {
                        cells.Add(new MapCell {X = x, Y = y});
                    }
                }
            }

            foreach (var cell in cells.OrderBy(_ => RandomFactory.Instance.RandomNumber(0, int.MaxValue)))
            {
                if (IsBlockedZone(firstX, firstY, cell.X, cell.Y))
                {
                    continue;
                }

                firstX = cell.X;
                firstY = cell.Y;
                return true;
            }

            return false;
        }

        private bool IsBlockedZone(short firstX, short firstY, short mapX, short mapY)
        {
            for (var i = 1; i <= Math.Abs(mapX - firstX); i++)
            {
                if (!IsWalkable(this[(short) (firstX + (Math.Sign(mapX - firstX) * i)), firstY]))
                {
                    return true;
                }
            }

            for (var i = 1; i <= Math.Abs(mapY - firstY); i++)
            {
                if (!IsWalkable(this[firstX, (short) (firstY + (Math.Sign(mapY - firstY) * i))]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsWalkable(byte value)
        {
            return value == 0 || value == 2 || (value >= 16 && value <= 19);
        }
    }
}