package org.gms.client.command.commands.gm2;

import org.gms.client.Character;
import org.gms.client.Client;
import org.gms.client.command.Command;
import org.gms.scripting.npc.NPCScriptManager;

/**
 * !run gm - 打开 GM 功能菜单（NPC 9010000 对话）
 * 入口移植自 MXDtestServer GM 菜单系统
 */
public class RunCommand extends Command {
    {
        setDescription("运行脚本对话，如 !run gm 打开 GM 功能菜单");
    }

    @Override
    public void execute(Client c, String[] params) {
        Character player = c.getPlayer();
        if (params.length > 0) {
            String arg = params[0];
            if ("gm".equalsIgnoreCase(arg) || "gm_menu".equalsIgnoreCase(arg) || "9010000".equals(arg)) {
                NPCScriptManager.getInstance().start(c, 9010000, player);
                return;
            }
        }
        player.dropMessage(5, "用法: !run gm  打开 GM 功能菜单");
    }
}
