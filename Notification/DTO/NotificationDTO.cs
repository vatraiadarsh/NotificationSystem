namespace Notification.DTO
{
    public class NotificationDTO
    {
        public Guid Id { get; set; }
        public Guid FromUser { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Guid Recipient { get; set; }
        public DateTime DateReceived { get; set; }
        public DateTime? DateAlerted { get; set; }
        public bool MarkedAsRead { get; set; }
    }
}
