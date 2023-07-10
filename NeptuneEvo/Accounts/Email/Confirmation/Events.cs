using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTANetworkAPI;
using NeptuneEvo.Accounts.Email.Confirmation.Models;
using NeptuneEvo.Handles;
using Redage.SDK;

namespace NeptuneEvo.Accounts.Email.Confirmation
{
    public class Events : Script
    {

        [Command("emailconfirm")]
        public void emailconfirm(ExtPlayer player, string email)
        {        
            EmailConfirm(player, email);
        }

        [RemoteEvent("server.email.confirm")]
        public void EmailConfirm(ExtPlayer player, string email)
        {                
            var accountData = player.GetAccountData();
            if (accountData == null) 
                return;
            
            var rg = new Regex(@"[0-9]{8,11}[.][0-9]{8,11}", RegexOptions.IgnoreCase);
            
            if (rg.IsMatch(accountData.Ga))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Du hast bereits deine E-Mail bestätigt!", 4500);
                return;
            }

            Trigger.SetTask(async () =>
            {
                var result = await Repository.Confirm(player, email);

                if (result == EmailConfirmEnum.Error)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ein unvorhergesehener Fehler!", 5000);
                else if (result == EmailConfirmEnum.LoadingError)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter,
                        "Warten Sie ein paar Sekunden und versuchen Sie es erneut...", 5000);
                else if (result == EmailConfirmEnum.EmailReg)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Diese E-Mail Adresse wird bereits verwenden!", 5000);
                else if (result == EmailConfirmEnum.DataError)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Fehler beim Ausfüllen vom Feld!",
                        5000);
                else if (result == EmailConfirmEnum.Success)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter,
                        "Sie erhalten eine Bestätigungsmail mit ein Link, mit dem Sie Ihre E-Mail-Adresse bestätigen. ",
                        5000);
            });
        }
    }
}