using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;


namespace LibraryMS.BLL.Services
{
    public sealed class BookReservationService
    {
        private readonly BookReservationRepository _repo;
        private readonly NotificationService _notifications;
        private readonly UserContactService _userContacts;

        public BookReservationService(
             BookReservationRepository repo,
             NotificationService notifications,
             UserContactService userContacts)
        {
            _repo = repo;
            _notifications = notifications;
            _userContacts = userContacts;
        }

        public Task<List<BookAvailRowDto>> SearchAvailableAsync(string locCode, string? text)
            => _repo.SearchAvailableAsync(locCode, text);

        public Task<List<ResMyRowDto>> GetMyAsync(string userCode, string locCode)
            => _repo.GetMyAsync(userCode, locCode);

        //public Task CreateRequestAsync(ReservationRequestDto dto)
        //    => _repo.CreateRequestAsync(dto);

        //public Task<int> CancelAsync(int resId, string userCode)
        //    => _repo.CancelAsync(resId, userCode);

        public Task<List<ResPendingRowDto>> GetPendingAsync(string locCode, bool loadAll)
            => _repo.GetPendingAsync(locCode, loadAll);

        //public Task ApproveAsync(int resId, string adminUserCode)
        //    => _repo.ApproveAsync(resId, adminUserCode);

        //public Task RejectAsync(int resId, string adminUserCode, string? remark)
        //    => _repo.RejectAsync(resId, adminUserCode, remark);

        //public Task ProcessAsync(int resId, string adminUserCode)
        //    => _repo.ProcessAsync(resId, adminUserCode);

    //    public Task<int> CancelByUserAsync(int resId, string userCode)
    //=> _repo.CancelByUserAsync(resId, userCode);

        //public Task<int> CancelByAdminAsync(int resId, string adminUserCode, string? remark)
        //    => _repo.CancelByAdminAsync(resId, adminUserCode, remark);

        public Task<List<ResMyRowDto>> GetActiveByUserAsync(string userCode, string locCode)
    => _repo.GetActiveByUserAsync(userCode, locCode);
        public async Task CreateRequestAsync(ReservationRequestDto dto)
        {
            await _repo.CreateRequestAsync(dto);

            var contact = await _userContacts.GetByUserCodeAsync(dto.UserCode);
            if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
            {
                await _notifications.QueueBothAsync(
                    eventType: "RESERVATION_REQUESTED",
                    refDocNo: null,
                    userCode: dto.UserCode,
                    emailTo: contact.Email,
                    subject: "Book reservation request submitted",
                    body: $"Your reservation request has been submitted for Book Code: {dto.BookCode}, Qty: {dto.Qty}."
                );
                await _notifications.ProcessPendingAsync();
            }
        }

        // ---------------- APPROVE ----------------
        public async Task ApproveAsync(int resId, string adminUserCode)
        {
            var row = await _repo.GetByIdAsync(resId);

            await _repo.ApproveAsync(resId, adminUserCode);

            if (row != null)
            {
                var contact = await _userContacts.GetByUserCodeAsync(row.UserCode);
                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        "RESERVATION_APPROVED",
                        row.ResId.ToString(),
                        row.UserCode,
                        contact.Email,
                        "Book reservation approved",
                        $"Your reservation has been APPROVED.\nBook: {row.BookCode}\nQty: {row.Qty}"
                    );
                    await _notifications.ProcessPendingAsync();
                }
            }
        }

        public async Task RejectAsync(int resId, string adminUserCode, string? remark)
        {
            var row = await GetReservationByIdAsync(resId);
            await _repo.RejectAsync(resId, adminUserCode, remark);

            if (row != null)
            {
                var contact = await _userContacts.GetByUserCodeAsync(row.UserCode);
                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        eventType: "RESERVATION_REJECTED",
                        refDocNo: row.ResId.ToString(),
                        userCode: row.UserCode,
                        emailTo: contact.Email,
                        subject: "Book reservation rejected",
                        body: $"Your reservation for Book Code: {row.BookCode} was rejected. {remark}"
                    );
                    await _notifications.ProcessPendingAsync();
                }
            }
        }

        public async Task ProcessAsync(int resId, string adminUserCode)
        {
            var row = await GetReservationByIdAsync(resId);
            await _repo.ProcessAsync(resId, adminUserCode);

            if (row != null)
            {
                var contact = await _userContacts.GetByUserCodeAsync(row.UserCode);
                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        eventType: "RESERVATION_PROCESSED",
                        refDocNo: row.ResId.ToString(),
                        userCode: row.UserCode,
                        emailTo: contact.Email,
                        subject: "Book reservation processed",
                        body: $"Your reservation has been processed for Book Code: {row.BookCode}, Qty: {row.Qty}."
                    );
                    await _notifications.ProcessPendingAsync();
                }
            }
        }

        public async Task<int> CancelAsync(int resId, string userCode)
        {
            var rows = await _repo.CancelAsync(resId, userCode);

            if (rows > 0)
            {
                var contact = await _userContacts.GetByUserCodeAsync(userCode);
                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        eventType: "RESERVATION_CANCELLED",
                        refDocNo: resId.ToString(),
                        userCode: userCode,
                        emailTo: contact.Email,
                        subject: "Book reservation cancelled",
                        body: $"Your reservation #{resId} has been cancelled."
                    );
                    await _notifications.ProcessPendingAsync();
                }
            }

            return rows;
        }

        public async Task<int> CancelByUserAsync(int resId, string userCode)
        {
            var rows = await _repo.CancelByUserAsync(resId, userCode);

            if (rows > 0)
            {
                var contact = await _userContacts.GetByUserCodeAsync(userCode);
                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        eventType: "RESERVATION_CANCELLED_BY_USER",
                        refDocNo: resId.ToString(),
                        userCode: userCode,
                        emailTo: contact.Email,
                        subject: "Book reservation cancelled",
                        body: $"Your reservation #{resId} has been cancelled."
                    );
                    await _notifications.ProcessPendingAsync();
                }
            }

            return rows;
        }

        public async Task<int> CancelByAdminAsync(int resId, string adminUserCode, string? remark)
        {
            var row = await GetReservationByIdAsync(resId);
            var rows = await _repo.CancelByAdminAsync(resId, adminUserCode, remark);

            if (rows > 0 && row != null)
            {
                var contact = await _userContacts.GetByUserCodeAsync(row.UserCode);
                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        eventType: "RESERVATION_CANCELLED_BY_ADMIN",
                        refDocNo: row.ResId.ToString(),
                        userCode: row.UserCode,
                        emailTo: contact.Email,
                        subject: "Book reservation cancelled by admin",
                        body: $"Your reservation for Book Code: {row.BookCode} was cancelled by admin. {remark}"
                    );
                    await _notifications.ProcessPendingAsync();
                }
            }

            return rows;
        }

        // temporary helper until proper GetById is added
        private async Task<ResPendingRowDto?> GetReservationByIdAsync(int resId)
        {
            // Recommended: create repository method GetByIdAsync(resId)
            // This placeholder returns null until that method is added.
            await Task.CompletedTask;
            return null;
        }
    }
}