using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;

namespace LibraryMS.BLL.Services
{
    public sealed class NotificationService
    {
        private readonly NotificationRepository _repo;
        private readonly EmailSenderService _email;

        public NotificationService(NotificationRepository repo, EmailSenderService email)
        {
            _repo = repo;
            _email = email;
        }

        public Task QueueEmailAsync(string eventType, string? refDocNo, string? userCode, string? emailTo, string subject, string body)
            => _repo.QueueAsync(eventType, refDocNo, userCode, emailTo, subject, body, "EMAIL");

        public async Task QueueInAppAsync(string eventType, string? refDocNo, string userCode, string title, string message)
        {
            await _repo.QueueAsync(eventType, refDocNo, userCode, null, title, message, "INAPP");
            await _repo.AddInAppAsync(userCode, title, message, refDocNo, eventType);
        }

        public async Task QueueBothAsync(string eventType, string? refDocNo, string userCode, string? emailTo, string subject, string body)
        {
            await _repo.QueueAsync(eventType, refDocNo, userCode, emailTo, subject, body, "BOTH");
            await _repo.AddInAppAsync(userCode, subject, body, refDocNo, eventType);
        }

        public async Task ProcessPendingAsync()
        {
            var rows = await _repo.GetPendingAsync();

            foreach (var row in rows)
            {
                try
                {
                    if ((row.Channel == "EMAIL" || row.Channel == "BOTH") &&
                        !string.IsNullOrWhiteSpace(row.EmailTo))
                    {
                        await _email.SendAsync(row.EmailTo!, row.Subject, row.Body);
                    }

                    await _repo.MarkSentAsync(row.Id);
                }
                catch (Exception ex)
                {
                    await _repo.MarkFailedAsync(row.Id, ex.Message);
                }
            }
        }
    }
}
