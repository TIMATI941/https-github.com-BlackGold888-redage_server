using System.Threading.Tasks;
using GTANetworkAPI;
using NeptuneEvo.Accounts.Registration.Models;
using NeptuneEvo.Handles;
using NeptuneEvo.Players;
using Redage.SDK;

namespace NeptuneEvo.Accounts.Email.Registration
{
    public class Events : Script
    {
        private static readonly nLog Log = new nLog("NeptuneEvo.Accounts.Email.Registration");
        
        [RemoteEvent("signup")]
        public void ClientEvent_signup(ExtPlayer player, string login_, string pass_, string email_, string promo_)
        {            
            if (Players.Queue.Repository.List.Contains(player))
                return;
            
            Trigger.SetTask(async () =>
            {
                if (!Main.ServerSettings.IsEmailConfirmed)
                {
                    var sessionData = player.GetSessionData();
                    if (sessionData == null) return;
                    
                    var result = await Accounts.Registration.Repository.Register(player, login_, pass_, email_, promo_, "");
                        
                    if (result == RegistrationEnum.Error) Accounts.Registration.Repository.MessageError(player,  "Unerwarteter Fehler!");
                    else if (result == RegistrationEnum.LoadingError) Accounts.Registration.Repository.MessageError(player,  "Bitte warten Sie einige Sekunden und versuchen Sie es erneut...");
                    else if (result == RegistrationEnum.SocialReg) Accounts.Registration.Repository.MessageError(player,  "Dieses SocialClub-Konto ist bereits mit einem Spielkonto registriert!");
                    else if (result == RegistrationEnum.UserReg) Accounts.Registration.Repository.MessageError(player,  "Dieser Benutzername ist bereits vergeben!");
                    else if (result == RegistrationEnum.EmailReg) Accounts.Registration.Repository.MessageError(player,  "Diese E-Mail-Adresse ist bereits vergeben!");
                    else if (result == RegistrationEnum.DataError) Accounts.Registration.Repository.MessageError(player,  "Fehler bei der Eingabe der Felder!");
                    else if (result == RegistrationEnum.PromoError) Accounts.Registration.Repository.MessageError(player,  "Dieser Promotionscode ist derzeit nicht gültig. Bitte geben Sie einen gültigen ein oder leeren Sie das Feld!");
                    else if (result == RegistrationEnum.ReffError) Accounts.Registration.Repository.MessageError(player,  "Es scheint, als hätten Sie einen Freundescode anstelle eines Streamercodes eingegeben. Bitte lassen Sie das Promotioncode-Feld leer und suchen Sie im Telefonmenü nach der entsprechenden Schaltfläche.");
                    else if (result == RegistrationEnum.PromoLimitError) Accounts.Registration.Repository.MessageError(player,  "Dieser Promotionscode hat das Aktivierungslimit erreicht. Bitte geben Sie einen anderen ein!");
                    else if (result == RegistrationEnum.ABError) Accounts.Registration.Repository.MessageError(player,  "Registrierungsfehler. Bitte melden Sie sich mit Ihrem offiziellen SocialClub-Konto an");
                    Log.Write($"{sessionData.Name} ({sessionData.SocialClubName} | {sessionData.RealSocialClub}) tryed to signup.");
                }
                else
                {
                    var result = await Repository.Verification(player, login_, pass_, email_, promo_);
                
                    if (result == RegistrationEnum.Error) Accounts.Registration.Repository.MessageError(player, "Unerwarteter Fehler!");
                    else if (result == RegistrationEnum.LoadingError) Accounts.Registration.Repository.MessageError(player, "Bitte warten Sie einige Sekunden und versuchen Sie es erneut...");
                    else if (result == RegistrationEnum.SocialReg) Accounts.Registration.Repository.MessageError(player, "Dieses SocialClub-Konto ist bereits mit einem Spielkonto registriert!");
                    else if (result == RegistrationEnum.UserReg) Accounts.Registration.Repository.MessageError(player, "Dieser Benutzername ist bereits vergeben!");
                    else if (result == RegistrationEnum.EmailReg) Accounts.Registration.Repository.MessageError(player, "Diese E-Mail-Adresse ist bereits vergeben!");
                    else if (result == RegistrationEnum.DataError) Accounts.Registration.Repository.MessageError(player, "Fehler bei der Eingabe der Felder!");
                    else if (result == RegistrationEnum.PromoError) Accounts.Registration.Repository.MessageError(player, "Dieser Promotionscode ist derzeit nicht gültig. Bitte geben Sie einen gültigen ein oder leeren Sie das Feld!");
                    else if (result == RegistrationEnum.ReffError) Accounts.Registration.Repository.MessageError(player, "Es scheint, als hätten Sie einen Freundescode anstelle eines Streamercodes eingegeben. Bitte lassen Sie das Promotioncode-Feld leer und suchen Sie im Telefonmenü nach der entsprechenden Schaltfläche.");
                    else if (result == RegistrationEnum.PromoLimitError) Accounts.Registration.Repository.MessageError(player, "Dieser Promotionscode hat das Aktivierungslimit erreicht. Bitte geben Sie einen anderen ein!");
                    else if (result == RegistrationEnum.ABError) Accounts.Registration.Repository.MessageError(player, "Registrierungsfehler. Bitte melden Sie sich mit Ihrem offiziellen SocialClub-Konto an");
                }
            });
        }
    }
}