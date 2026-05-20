using ClickAndCollect.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Globalization;
using System.Text;

namespace ClickAndCollect.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string firstname, string lastname, string email)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Smtp:FromName"],
                _config["Smtp:FromEmail"]));
            message.To.Add(new MailboxAddress($"{firstname} {lastname}", toEmail));
            message.Subject = $"Bienvenue sur Click & Collect, {firstname} !";

            var builder = new BodyBuilder
            {
                HtmlBody = BuildHtmlBody(firstname, lastname, email)
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Smtp:Host"],
                int.Parse(_config["Smtp:Port"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["Smtp:Username"],
                _config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendOrderConfirmationEmailAsync(string toEmail, string firstname, string lastname, Order order, Store store, TimeSlot slot)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Smtp:FromName"],
                _config["Smtp:FromEmail"]));
            message.To.Add(new MailboxAddress($"{firstname} {lastname}", toEmail));
            message.Subject = "Votre commande Click & Collect est confirmée !";

            var builder = new BodyBuilder
            {
                HtmlBody = BuildOrderConfirmationHtml(firstname, lastname, order, store, slot)
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Smtp:Host"],
                int.Parse(_config["Smtp:Port"]!),
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(
                _config["Smtp:Username"],
                _config["Smtp:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string BuildOrderConfirmationHtml(string firstname, string lastname, Order order, Store store, TimeSlot slot)
        {
            var cultureInfo = new CultureInfo("fr-FR");
            string dateLabel = slot.DateSlot.ToString("dddd d MMMM yyyy", cultureInfo);
            string timeLabel = $"{slot.StartTime:hh\\:mm} → {slot.EndTime:hh\\:mm}";

            var linesHtml = new StringBuilder();
            foreach (var line in order.Lines)
            {
                string subtotal = line.GetSubTotal().ToString("0.00", CultureInfo.InvariantCulture);
                linesHtml.Append($"""
                                  <tr>
                                    <td style="padding:10px 0;border-bottom:1px solid #e9ecef;color:#212529;font-size:14px;">{line.Product.Name}</td>
                                    <td style="padding:10px 0;border-bottom:1px solid #e9ecef;color:#6c757d;font-size:14px;text-align:center;">{line.Quantity}</td>
                                    <td style="padding:10px 0;border-bottom:1px solid #e9ecef;color:#198754;font-size:14px;text-align:right;font-weight:bold;">{subtotal} €</td>
                                  </tr>
                                  """);
            }

            string total       = order.TotalAmount().ToString("0.00", CultureInfo.InvariantCulture);
            string serviceFee  = Order.DefaultServiceFee.ToString("0.00", CultureInfo.InvariantCulture);
            string grandTotal  = (order.TotalAmount() + Order.DefaultServiceFee).ToString("0.00", CultureInfo.InvariantCulture);

            return $"""
            <!DOCTYPE html>
            <html lang="fr">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
              <title>Confirmation de commande</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">

              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4;padding:30px 0;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">

                      <!-- HEADER -->
                      <tr>
                        <td style="background-color:#198754;padding:36px 40px;text-align:center;">
                          <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:bold;letter-spacing:1px;">
                            🛒 Click &amp; Collect
                          </h1>
                          <p style="margin:8px 0 0;color:#d1f5e0;font-size:14px;">
                            Commandez en ligne, récupérez en magasin
                          </p>
                        </td>
                      </tr>

                      <!-- CONFIRMATION BANNER -->
                      <tr>
                        <td style="background-color:#d1f5e0;padding:20px 40px;text-align:center;border-bottom:2px solid #198754;">
                          <p style="margin:0;color:#0a3d20;font-size:17px;font-weight:bold;">
                            ✅ Votre commande est bien enregistrée !
                          </p>
                        </td>
                      </tr>

                      <!-- BODY -->
                      <tr>
                        <td style="padding:36px 40px;">

                          <h2 style="margin:0 0 16px;color:#212529;font-size:20px;">
                            Bonjour {firstname} !
                          </h2>
                          <p style="margin:0 0 28px;color:#495057;font-size:15px;line-height:1.6;">
                            Merci pour votre commande. Vous trouverez ci-dessous le récapitulatif de votre retrait.
                          </p>

                          <!-- INFOS RETRAIT -->
                          <table width="100%" cellpadding="0" cellspacing="0"
                                 style="background-color:#f8f9fa;border-radius:6px;border:1px solid #e9ecef;margin-bottom:28px;">
                            <tr>
                              <td style="padding:20px 24px;">
                                <p style="margin:0 0 12px;color:#6c757d;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;font-weight:bold;">
                                  Informations de retrait
                                </p>
                                <table width="100%" cellpadding="0" cellspacing="0">
                                  <tr>
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;width:130px;">📍 Magasin</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;font-weight:bold;">{store.Name}</td>
                                  </tr>
                                  <tr style="border-top:1px solid #e9ecef;">
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;">🏠 Adresse</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;">{store.FullAddress()}</td>
                                  </tr>
                                  <tr style="border-top:1px solid #e9ecef;">
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;">📅 Date</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;font-weight:bold;text-transform:capitalize;">{dateLabel}</td>
                                  </tr>
                                  <tr style="border-top:1px solid #e9ecef;">
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;">🕐 Créneau</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;font-weight:bold;">{timeLabel}</td>
                                  </tr>
                                </table>
                              </td>
                            </tr>
                          </table>

                          <!-- ARTICLES -->
                          <p style="margin:0 0 12px;color:#6c757d;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;font-weight:bold;">
                            Détail de la commande
                          </p>
                          <table width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:8px;">
                            <thead>
                              <tr>
                                <th style="padding:8px 0;color:#6c757d;font-size:12px;text-align:left;font-weight:normal;border-bottom:2px solid #dee2e6;">Article</th>
                                <th style="padding:8px 0;color:#6c757d;font-size:12px;text-align:center;font-weight:normal;border-bottom:2px solid #dee2e6;">Qté</th>
                                <th style="padding:8px 0;color:#6c757d;font-size:12px;text-align:right;font-weight:normal;border-bottom:2px solid #dee2e6;">Sous-total</th>
                              </tr>
                            </thead>
                            <tbody>
                              {linesHtml}
                            </tbody>
                          </table>

                          <!-- TOTAUX -->
                          <table width="100%" cellpadding="0" cellspacing="0" style="margin-top:12px;margin-bottom:32px;">
                            <tr>
                              <td style="padding:6px 0;color:#6c757d;font-size:14px;">Sous-total</td>
                              <td style="padding:6px 0;color:#212529;font-size:14px;text-align:right;">{total} €</td>
                            </tr>
                            <tr>
                              <td style="padding:6px 0;color:#6c757d;font-size:14px;">Frais de service</td>
                              <td style="padding:6px 0;color:#212529;font-size:14px;text-align:right;">{serviceFee} €</td>
                            </tr>
                            <tr>
                              <td style="padding:12px 0 0;color:#212529;font-size:16px;font-weight:bold;border-top:2px solid #dee2e6;">Total</td>
                              <td style="padding:12px 0 0;color:#198754;font-size:18px;font-weight:bold;text-align:right;border-top:2px solid #dee2e6;">{grandTotal} €</td>
                            </tr>
                          </table>

                          <p style="margin:0;color:#6c757d;font-size:13px;line-height:1.6;">
                            Merci de vous présenter au magasin muni de cet email ou de votre numéro de commande pendant le créneau sélectionné.
                          </p>

                        </td>
                      </tr>

                      <!-- FOOTER -->
                      <tr>
                        <td style="background-color:#212529;padding:28px 40px;text-align:center;">
                          <p style="margin:0 0 8px;color:#adb5bd;font-size:13px;">
                            Click &amp; Collect — Haute École Condorcet, Charleroi
                          </p>
                          <p style="margin:0;color:#6c757d;font-size:12px;">
                            ✉️ contact@clickcollect.be &nbsp;|&nbsp; 📍 Charleroi, Belgique
                          </p>
                          <p style="margin:12px 0 0;color:#495057;font-size:11px;">
                            &copy; 2026 Click &amp; Collect. Tous droits réservés.
                          </p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>

            </body>
            </html>
            """;
        }

        private static string BuildHtmlBody(string firstname, string lastname, string email)
        {
            return $"""
            <!DOCTYPE html>
            <html lang="fr">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
              <title>Bienvenue sur Click &amp; Collect</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">

              <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4;padding:30px 0;">
                <tr>
                  <td align="center">
                    <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">

                      <!-- HEADER -->
                      <tr>
                        <td style="background-color:#198754;padding:36px 40px;text-align:center;">
                          <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:bold;letter-spacing:1px;">
                            🛒 Click &amp; Collect
                          </h1>
                          <p style="margin:8px 0 0;color:#d1f5e0;font-size:14px;">
                            Commandez en ligne, récupérez en magasin
                          </p>
                        </td>
                      </tr>

                      <!-- BODY -->
                      <tr>
                        <td style="padding:40px;">

                          <h2 style="margin:0 0 16px;color:#212529;font-size:22px;">
                            Bienvenue, {firstname} ! 👋
                          </h2>
                          <p style="margin:0 0 24px;color:#495057;font-size:15px;line-height:1.6;">
                            Votre compte a bien été créé sur <strong>Click &amp; Collect</strong>.
                            Vous pouvez dès maintenant parcourir notre catalogue et passer vos premières commandes.
                          </p>

                          <!-- RÉCAPITULATIF -->
                          <table width="100%" cellpadding="0" cellspacing="0"
                                 style="background-color:#f8f9fa;border-radius:6px;border:1px solid #e9ecef;margin-bottom:32px;">
                            <tr>
                              <td style="padding:20px 24px;">
                                <p style="margin:0 0 6px;color:#6c757d;font-size:12px;text-transform:uppercase;letter-spacing:0.5px;font-weight:bold;">
                                  Récapitulatif de votre compte
                                </p>
                                <table width="100%" cellpadding="0" cellspacing="0" style="margin-top:12px;">
                                  <tr>
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;width:120px;">Prénom</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;font-weight:bold;">{firstname}</td>
                                  </tr>
                                  <tr style="border-top:1px solid #e9ecef;">
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;">Nom</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;font-weight:bold;">{lastname}</td>
                                  </tr>
                                  <tr style="border-top:1px solid #e9ecef;">
                                    <td style="padding:8px 0;color:#6c757d;font-size:14px;">Email</td>
                                    <td style="padding:8px 0;color:#212529;font-size:14px;font-weight:bold;">{email}</td>
                                  </tr>
                                </table>
                              </td>
                            </tr>
                          </table>

                          <p style="margin:0 0 24px;color:#495057;font-size:15px;line-height:1.6;">
                            Si vous n'êtes pas à l'origine de cette inscription, ignorez simplement cet email.
                          </p>

                        </td>
                      </tr>

                      <!-- FOOTER -->
                      <tr>
                        <td style="background-color:#212529;padding:28px 40px;text-align:center;">
                          <p style="margin:0 0 8px;color:#adb5bd;font-size:13px;">
                            Click &amp; Collect — Haute École Condorcet, Charleroi
                          </p>
                          <p style="margin:0;color:#6c757d;font-size:12px;">
                            ✉️ contact@clickcollect.be &nbsp;|&nbsp; 📍 Charleroi, Belgique
                          </p>
                          <p style="margin:12px 0 0;color:#495057;font-size:11px;">
                            &copy; 2026 Click &amp; Collect. Tous droits réservés.
                          </p>
                        </td>
                      </tr>

                    </table>
                  </td>
                </tr>
              </table>

            </body>
            </html>
            """;
        }
    }
}
