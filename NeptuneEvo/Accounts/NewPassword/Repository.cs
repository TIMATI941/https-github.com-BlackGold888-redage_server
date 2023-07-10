using GTANetworkAPI;
using NeptuneEvo.Handles;
using NeptuneEvo.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEvo.Accounts.NewPassword
{
    class Repository
    {
        public static void changePassword(ExtPlayer player, string newPass)
        {
            var accountData = player.GetAccountData();
            if (accountData == null) return;
            accountData.Password = Accounts.Repository.GetMD5(newPass);
            GameLog.AccountLog(accountData.Login, accountData.HWID, accountData.IP, accountData.SocialClub, "Passwort ändern");
        }
    }
}
