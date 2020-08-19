using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using PieterP.ScoreSheet.Model.Database;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.ViewModels.Helpers {
    public class MailClient {

        public async Task<bool> Send(string resultsFile, string matches) {
            try {
                using (var file = File.OpenRead(resultsFile)) {
                    var s = DatabaseManager.Current.Settings;
                    string from = s.SmtpUsername.Value.IndexOf('@') > 0 ? s.SmtpUsername.Value : s.FreeTimeMailFrom.Value;
                    var message = new MailMessage(from, s.FreeTimeMailTo.Value);
                    if (s.ClubResponsibleInCC.Value) {
                        message.CC.Add(s.FreeTimeMailFrom.Value);
                    }
                    message.Subject = $"Uitslag vrijetijdscompetitie { s.HomeClub.Value }";
                    message.IsBodyHtml = false;
                    message.Body = @"Geachte interclubverantwoordelijke,

In bijlage aan deze email vindt u de uitslagen van wedstrijden uit de vrijetijdscompetitie. Het betreft de volgende wedstrijd(en):
$MATCHES$

Mocht u nog vragen hebben over één van de resultaten, kan u ons altijd contacteren op het emailadres: $EMAIL$

Met vriendelijke groet,
$CLUB$

Deze email werd automatisch gegenereerd. Het emailadres waar deze email van gestuurd werd, wordt mogelijk niet gelezen.".Replace("$MATCHES$", matches.ToString()).Replace("$EMAIL$", s.FreeTimeMailFrom.Value).Replace("$CLUB$", s.HomeClub.Value ?? "");
                    message.Attachments.Add(new Attachment(file, $"Resultaten_{ s.HomeClubId.Value }_{ DateTime.Now.ToString("ddMMyyyy") }.pdf", "application/pdf"));
                    using (var client = CreateClient()) {
                        await client.SendMailAsync(message);
                        return true;
                    }
                }
            } catch (Exception e) {
                Logger.Log(e);
                return false;
            }
        }

        public async Task<bool> Test() {
            try {
                var s = DatabaseManager.Current.Settings;
                using (var client = CreateClient()) {
                    string from = s.SmtpUsername.Value.IndexOf('@') > 0 ? s.SmtpUsername.Value : s.FreeTimeMailFrom.Value;
                    await client.SendMailAsync(from, "score_fttest@pieterp.be", "SMTP test", $"This is a test of the SMTP settings for club { s.HomeClub.Value } ({ s.HomeClubId.Value }). Client ID = { s.UniqueId.Value }. This message is automatically generated.");
                    return true;
                }
            } catch (Exception e) {
                Logger.Log(e);
                return false;
            }
        }
        private SmtpClient CreateClient() {
            var s = DatabaseManager.Current.Settings;
            var client = new SmtpClient(s.SmtpHost.Value, s.SmtpPort.Value);
            if (s.SmtpUsername.Value != "" || s.SmtpPassword.Value != "")
                client.Credentials = new NetworkCredential(s.SmtpUsername.Value, s.SmtpPassword.Value);
            client.EnableSsl = s.SmtpUseStartTls.Value;
            return client;
        }
    }
}