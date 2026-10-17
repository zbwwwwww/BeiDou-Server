/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
    Copyleft (L) 2016 - 2019 RonanLana

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
   @Author: Arthur L - Refactored command content into modules
*/
package org.gms.client.command.commands.gm2;

import org.gms.client.Character;
import org.gms.client.Client;
import org.gms.client.command.Command;
import org.gms.util.I18nUtil;

public class SetStatCommand extends Command {
    {
        setDescription("设置能力值: !setstat 数值 (全部) 或 !setstat str/dex/int/luk 数值 (单个)");
    }

    @Override
    public void execute(Client c, String[] params) {
        Character player = c.getPlayer();
        if (params.length < 1) {
            player.yellowMessage("用法: !setstat 数值 (全部属性) 或 !setstat str/dex/int/luk 数值 (单个属性)");
            return;
        }

        try {
            // 用法1: !setstat str 100 — 单独设置某个属性
            if (params.length >= 2) {
                String statType = params[0].toLowerCase();
                int targetValue = Integer.parseInt(params[1]);

                if (targetValue > Short.MAX_VALUE) {
                    targetValue = Short.MAX_VALUE;
                } else if (targetValue < 4) {
                    targetValue = 4;
                }

                int currentValue;
                boolean success = false;
                String statName = "";
                int diff;

                switch (statType) {
                    case "str":
                        currentValue = player.getStr();
                        statName = "力量";
                        diff = targetValue - currentValue;
                        if (diff > 0) player.gainAp(diff, true);
                        success = player.assignStr(diff);
                        break;
                    case "dex":
                        currentValue = player.getDex();
                        statName = "敏捷";
                        diff = targetValue - currentValue;
                        if (diff > 0) player.gainAp(diff, true);
                        success = player.assignDex(diff);
                        break;
                    case "int":
                        currentValue = player.getInt();
                        statName = "智力";
                        diff = targetValue - currentValue;
                        if (diff > 0) player.gainAp(diff, true);
                        success = player.assignInt(diff);
                        break;
                    case "luk":
                        currentValue = player.getLuk();
                        statName = "运气";
                        diff = targetValue - currentValue;
                        if (diff > 0) player.gainAp(diff, true);
                        success = player.assignLuk(diff);
                        break;
                    default:
                        player.yellowMessage("属性类型必须是 str/dex/int/luk");
                        return;
                }

                if (success) {
                    player.yellowMessage(statName + " 已设置为 " + targetValue);
                } else {
                    player.yellowMessage("设置失败");
                }
                return;
            }

            // 用法2: !setstat 100 — 四个属性全部设为同一个值（原有功能）
            int x = Integer.parseInt(params[0]);

            if (x > Short.MAX_VALUE) {
                x = Short.MAX_VALUE;
            } else if (x < 4) {
                x = 4;
            }

            player.updateStrDexIntLuk(x);
            player.yellowMessage("所有属性已设置为 " + x);
        } catch (NumberFormatException nfe) {
            player.yellowMessage("数值必须是整数");
        }
    }
}
