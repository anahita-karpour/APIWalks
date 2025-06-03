namespace NZWalks.API.Repositories
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; } // The JWT ID it belongs to
        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool Used { get; set; }
        public bool Invalidated { get; set; }
        public string UserId { get; set; } // The user it belongs to
        public DateTime? RevokedAt { get; set; }

    }
}
