using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    //public sealed class UserLockService
    //{
    //    private readonly UserLockApprovalRepository _repo;
    //    public UserLockService(UserLockApprovalRepository repo) => _repo = repo;

    //    public Task<List<UserLockRowDto>> GetPendingAsync() => _repo.GetPendingAsync();
    //    public Task UnlockAsync(string ulId, string adminUserCode) => _repo.UnlockAsync(ulId, adminUserCode);
    //}
    public sealed class UserLockService
    {
        private readonly UserLockApprovalRepository _repo;
        private readonly NotificationService _notifications;
        private readonly UserContactService _userContacts;

        public UserLockService(
            UserLockApprovalRepository repo,
            NotificationService notifications,
            UserContactService userContacts)
        {
            _repo = repo;
            _notifications = notifications;
            _userContacts = userContacts;
        }

        public Task<List<UserLockRowDto>> GetPendingAsync()
            => _repo.GetPendingAsync();

        public async Task UnlockAsync(string ulId, string adminUserCode)
        {
            var row = await _repo.GetByIdAsync(ulId);

            await _repo.UnlockAsync(ulId, adminUserCode);

            if (row != null)
            {
                var contact = await _userContacts.GetByUserCodeAsync(row.UserCode);

                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    await _notifications.QueueBothAsync(
                        eventType: "UNLOCK_APPROVED",
                        refDocNo: ulId,
                        userCode: row.UserCode,
                        emailTo: contact.Email,
                        subject: "Account unlocked successfully",
                        body: $"Your account has been unlocked successfully. You can now log in again."
                    );

                    await _notifications.ProcessPendingAsync();
                }
            }
        }
    }
}