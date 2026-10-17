package org.gms.client.command.commands.gm3;

import org.gms.client.Character;
import org.gms.client.Client;
import org.gms.client.command.Command;

public class ResetStatCommand extends Command {
    {
        setDescription("重置所有能力值为4: !resetstat");
    }

    @Override
    public void execute(Client c, String[] params) {
        Character player = c.getPlayer();

        int strDiff = 4 - player.getStr();
        int dexDiff = 4 - player.getDex();
        int intDiff = 4 - player.getInt();
        int lukDiff = 4 - player.getLuk();

        boolean success = player.assignStrDexIntLuk(strDiff, dexDiff, intDiff, lukDiff);

        if (success) {
            player.dropMessage(5, "所有能力值已重置为4，AP已返还");
        } else {
            player.dropMessage(5, "重置失败");
        }
    }
}
