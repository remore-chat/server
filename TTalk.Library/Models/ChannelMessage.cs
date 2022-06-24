using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TTalk.Library.Models
{
    public class ChannelMessage
    {
        public string Id { get; set; }
        public string ChannelId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public List<MessageAttachment> Attachments { get; set; }
        public DateTime CreatedAt { get; set; }

        public ChannelMessage()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class MessageAttachment
    {
        [Key]
        public string Id { get; }
        public MessageAttachment()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string ContentType { get; set; }
        public ChannelMessage Message { get; set; }
        public string FileId { get; set; }
    }
}
