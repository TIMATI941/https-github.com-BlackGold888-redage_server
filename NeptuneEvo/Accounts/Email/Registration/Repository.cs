using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using LinqToDB;
using NeptuneEvo.Accounts.Registration.Models;
using NeptuneEvo.Handles;
using NeptuneEvo.Players;
using Redage.SDK;

namespace NeptuneEvo.Accounts.Email.Registration
{
    public class Repository
    {
                
        private static readonly nLog Log = new nLog("Accounts.Email.Registration.Repository");

        public static async Task<RegistrationEnum> Verification(ExtPlayer player, string login, string password, string email, string promo)
        {
            try
            {
                var sessionData = player.GetSessionData();
                if (sessionData == null) return RegistrationEnum.LoadingError;
                if (player.IsAccountData()) return RegistrationEnum.LoadingError;
                if (sessionData.RealHWID.Equals("NONE") || sessionData.RealSocialClub.Equals("NONE")) return RegistrationEnum.LoadingError;
                if (login.Length < 1 || password.Length < 1 || email.Length < 1) return RegistrationEnum.DataError;

                login = login.ToLower();
                email = email.ToLower();

                await using var db = new ServerBD("MainDB");//В отдельном потоке

                var account = await db.Accounts
                    .Where(v => v.Login.ToLower() == login || v.Email.ToLower() == email || v.Socialclub == sessionData.SocialClubName || v.Socialclub == sessionData.RealSocialClub)
                    .FirstOrDefaultAsync();

                if (account != null)
                {
                    if (account.Login.ToLower() == login) return RegistrationEnum.UserReg;
                    if (account.Email.ToLower() == email) return RegistrationEnum.EmailReg;
                    if (Main.ServerNumber != 0 && (account.Socialclub == sessionData.SocialClubName || account.Socialclub == sessionData.RealSocialClub)) return RegistrationEnum.SocialReg;
                }
                promo = promo.ToLower();

                if (!string.IsNullOrEmpty(promo))
                {
                    if (!Main.PromoCodes.ContainsKey(promo))
                    {
                        if (int.TryParse(promo, out int refuid)) return RegistrationEnum.PromoError;
                        if (Main.UUIDs.Contains(refuid)) return RegistrationEnum.ReffError;
                        return RegistrationEnum.PromoError;
                    }
                    else
                    {
                        var pcdata = Main.PromoCodes[promo];
                        if (pcdata.RewardLimit != 0 && pcdata.RewardReceived >= pcdata.RewardLimit) return RegistrationEnum.PromoLimitError;
                    }
                }

                var hash = await Email.Repository.Add(player, login, password, email, promo, type: 0);

                Utils.Analytics.HelperThread.AddUrl($"verify?email={email}&name={login}&hash={hash}&sid={Main.ServerNumber}");
                
                Trigger.ClientEvent(player, "client.registration.sendEmail");
                return RegistrationEnum.Registered;
            }
            catch (Exception e)
            {
                Log.Write($"Register Exception: {e.ToString()}");
                return RegistrationEnum.Error;
            }
        }
        
        public static void VerificationConfirm(string hash, string ga)
        {
            var emailVerification = Email.Repository.GetVerification(hash, isRegistered: true);
            
            if (emailVerification != null)
            {
                var player = emailVerification.Player;
                
                var sessionData = player.GetSessionData();
                if (sessionData == null) return;
                
                Trigger.SetTask(async () =>
                {
                    try
                    {
                        var result = await Accounts.Registration.Repository.Register(player, emailVerification.Login, emailVerification.Password, emailVerification.Email, emailVerification.Promo, ga);
                        
                        if (result == RegistrationEnum.Error) Accounts.Registration.Repository.MessageError(player, "Unerwarteter Fehler!");
                        else if (result == RegistrationEnum.LoadingError) Accounts.Registration.Repository.MessageError(player, "Bitte warten Sie einige Sekunden und versuchen Sie es erneut...");
                        else if (result == RegistrationEnum.SocialReg) Accounts.Registration.Repository.MessageError(player, "Ein Account ist bereits auf diesem SocialClub registriert!");
                        else if (result == RegistrationEnum.UserReg) Accounts.Registration.Repository.MessageError(player, "Dieser Benutzername ist bereits vergeben!");
                        else if (result == RegistrationEnum.EmailReg) Accounts.Registration.Repository.MessageError(player, "Diese E-Mail-Adresse ist bereits registriert!");
                        else if (result == RegistrationEnum.DataError) Accounts.Registration.Repository.MessageError(player, "Es gab einen Fehler bei der Eingabe der Felder!");
                        else if (result == RegistrationEnum.PromoError) Accounts.Registration.Repository.MessageError(player, "Dieser Aktionscode existiert derzeit nicht. Bitte geben Sie einen gültigen Code ein oder lassen Sie das Feld leer!");
                        else if (result == RegistrationEnum.ReffError) Accounts.Registration.Repository.MessageError(player, "Wir haben bemerkt, dass Sie einen Freundes-Referenzcode anstelle des Promocodes eingegeben haben. Bitte lassen Sie das Feld Promocode jetzt leer. Sie können den Promocode später im Telefonmenü finden.");
                        else if (result == RegistrationEnum.PromoLimitError) Accounts.Registration.Repository.MessageError(player, "Dieser Aktionscode wurde bereits zu oft verwendet. Bitte geben Sie einen anderen Code ein!");
                        else if (result == RegistrationEnum.ABError) Accounts.Registration.Repository.MessageError(player, "Es gab einen Registrierungsfehler. Bitte verwenden Sie Ihren Haupt-SocialClub, um sich im Spiel anzumelden");
                        Log.Write($"{sessionData.Name} ({sessionData.SocialClubName} | {sessionData.RealSocialClub}) tryed to signup.");

                    }
                    catch (Exception e)
                    {
                        Log.Write($"ClientEvent_signup Exception: {e.ToString()}");
                    }
                });
            }
        }
        
    }
}