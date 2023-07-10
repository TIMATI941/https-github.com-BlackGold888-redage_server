using Database;
using GTANetworkAPI;
using NeptuneEvo.Handles;
using LinqToDB;
using NeptuneEvo.Character;
using NeptuneEvo.Chars;
using NeptuneEvo.Players;
using Redage.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Localization;

namespace NeptuneEvo.Accounts.Recovery
{
    class Repository
    {
        private static readonly nLog Log = new nLog("Accounts.Repository.Events");

        public static void SendEmail(ExtPlayer player, string login)
        {
            try
            {
	            var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var auntificationData = sessionData.AuntificationData;

                login = login.ToLower();

                if (!auntificationData.IsCreateAccount)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RecoveryCantFind), 4500);
                    Trigger.ClientEvent(player, "restorepassstep", 2);
                    return;
                }
                else if (!auntificationData.Login.ToLower().Equals(login) && !auntificationData.Email.ToLower().Equals(login))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RecoveryCantFind), 4500);
                    Trigger.ClientEvent(player, "restorepassstep", 2);
                    return;
                }
                else if (!auntificationData.Email.Contains("@"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RecoveryEmailCant), 4500);
                    Trigger.ClientEvent(player, "restorepassstep", 2);
                    return;
                }
                sessionData.RecoveryCode = Generate.RandomOneTimePassword();
                if (sessionData.RecoveryCode == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RecoveryError), 5000);
                    Trigger.ClientEvent(player, "restorepassstep", 2);
                    return;
                }
                Utils.Analytics.HelperThread.AddUrl($"recovery?email={auntificationData.Email}&name={login}&code={sessionData.RecoveryCode}&ip={sessionData.Address}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Eine Nachricht zum Wiederherstellen des Passworts wurde erfolgreich an {Generate.ObfuscateEmail(auntificationData.Email)} gesendet.", 5000);
                Trigger.ClientEvent(player, "restorepassstep", 1);
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }
        public static async void RecoveryPassword(ExtPlayer player, string code)
        {
            try
            {
	            var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                var auntificationData = sessionData.AuntificationData;

                if (!auntificationData.IsCreateAccount)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RecoveryCantFind), 4500);
                    Trigger.ClientEvent(player, "restorepassstep", 2);
                    return;
                }
                if (code == sessionData.RecoveryCode)
                {
                    Log.Debug($"{sessionData.RealSocialClub} passwort erfolgreich wiederhergestellt!", nLog.Type.Info);
                    sessionData.RecoveryCode = null;
                    string newPassword = Generate.RandomString(9);
                    if (newPassword == null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.RecoveryError), 5000);
                        Trigger.ClientEvent(player, "restorepassstep", 2);
                        return;
                    }
                    
                    Utils.Analytics.HelperThread.AddUrl($"newpassword?email={auntificationData.Email}&name={auntificationData.Login}&pass={newPassword}&ip={sessionData.Address}");
                    
                    auntificationData.Password = Accounts.Repository.GetMD5(newPassword.ToString());

                    await using var db = new ServerBD("MainDB");//В отдельном потоке

                    await db.Accounts
                        .Where(v => v.Login == auntificationData.Login)
                        .Set(v => v.Password, auntificationData.Password)
                        .UpdateAsync();

                    Autorization.Repository.AutorizationAccount(player, auntificationData.Login, auntificationData.Password).Wait();
                    
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Sie haben den Zugriff auf das Konto erfolgreich geändert. Ein neues Passwort wurde an Ihre E-Mail gesendet!", 5000);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Sie können Ihr Passwort im Spiel mit /password und einem Neuen Passwort ändern. Beispiel: [/password 123] ohne klammer!", 10000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, LangFunc.GetText(LangType.De, DataName.CodeDoesntMatter), 4500);
                    Trigger.ClientEvent(player, "restorepassstep", 1);
                }
            }
            catch (Exception e)
            {
                Debugs.Repository.Exception(e);
            }
        }
    }
}
