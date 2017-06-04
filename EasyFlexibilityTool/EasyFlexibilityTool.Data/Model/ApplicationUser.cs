namespace EasyFlexibilityTool.Data.Model
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public bool IsAgreeToLeaderboard { get; set; }
        public bool Redeemed { get; set; }
        public bool AllowTrainingFollowUps { get; set; }
        public bool AllowSpecialOffers { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public virtual List<AngleMeasurement> AngleMeasurements { get; set; }
    }
}
