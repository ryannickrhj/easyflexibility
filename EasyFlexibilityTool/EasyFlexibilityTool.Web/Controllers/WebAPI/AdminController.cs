using AutoMapper;
using EasyFlexibilityTool.Data;
using EasyFlexibilityTool.Data.Model;
using EasyFlexibilityTool.Web.Controllers.WebAPI.Base;
using EasyFlexibilityTool.Web.Models;
using EasyFlexibilityTool.Web.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Security;
using System.IO;
using System.Net.Http;

namespace EasyFlexibilityTool.Web.Controllers.WebAPI
{
    public class AdminController : BaseApiController
    {
        public async Task<IHttpActionResult> GetAsync()
        {
            var angleMeasurementDictionary = await DbContext.AngleMeasurements
                .GroupBy(am => am.User.Id)
                .ToDictionaryAsync(g => g.Key, g => new List<AngleMeasurement> {
                    g.OrderBy(am => am.DateTimeStamp).FirstOrDefault(),
                    g.OrderByDescending(am => am.Angle).FirstOrDefault(),
                    g.OrderBy(am => am.DateTimeStamp).LastOrDefault()
                });
            try
            {
                var models = new List<LeaderboardItemModel>();
                foreach (var user in DbContext.Users)
                {
                    var angleMeasurementOfUser = new List<AngleMeasurementModel>() { new AngleMeasurementModel(), new AngleMeasurementModel() };
                    if (angleMeasurementDictionary.ContainsKey(user.Id))
                    {
                        angleMeasurementOfUser = Mapper.Map<List<AngleMeasurementModel>>(angleMeasurementDictionary[user.Id]);
                    }
                    models.Add(new LeaderboardItemModel()
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstMeasurement = angleMeasurementOfUser[0],
                        BestMeasurement = angleMeasurementOfUser[1],
                        LastMeasurementDate = (angleMeasurementOfUser.Count < 3 ? angleMeasurementOfUser[0] : angleMeasurementOfUser[2]).DateTimeStamp,
                        RewardPoints = user.Redeemed ? 0 : Math.Round(angleMeasurementOfUser[1].Angle - angleMeasurementOfUser[0].Angle),
                        IsBlocked = user.IsBlocked
                    });
                }
                return Ok(models.OrderByDescending(i => i.Progress));
            }
            catch
            {
                throw;
            }
        }

        [HttpGet, Route("api/admin/getadminlist")]
        public IHttpActionResult GetAdminList()
        {
            try
            {
                var models = new List<LeaderboardItemModel>();
                foreach (var user in DbContext.Users)
                {
                    AngleMeasurementModel defaultAngleMeasurementModel = new AngleMeasurementModel();
                    models.Add(new LeaderboardItemModel()
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstMeasurement = defaultAngleMeasurementModel,
                        BestMeasurement = defaultAngleMeasurementModel,
                        IsAdmin = user.Role == "Admin"
                    });
                }
                return Ok(models);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet, Route("api/admin/userrole")]
        public IHttpActionResult UserRole()
        {
            return Ok(new { Role = getUserRole(DbContext) });
        }

        [HttpPost, Route("api/admin/updateuserrole")]
        public IHttpActionResult UpdateUserRole(string userId, string role)
        {
            try
            {
                var user = DbContext.Users.Where(u => u.Id == userId).SingleOrDefault();
                user.Role = role;
                DbContext.SaveChanges();
                return Ok("OK");
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost, Route("api/admin/block")]
        public IHttpActionResult Block(string email, bool isBlocked)
        {
            try
            {
                var user = DbContext.Users.SingleOrDefault(u => u.Email == email);
                user.IsBlocked = isBlocked;
                DbContext.SaveChanges();
                return Ok("OK");
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost, Route("api/admin/delete")]
        public IHttpActionResult Delete(string email)
        {
            var user = DbContext.Users.SingleOrDefault(u => u.Email == email);
            var angleMeasurements = DbContext.AngleMeasurements.Where(am => am.UserId == user.Id);
            foreach (var angleMeasurement in angleMeasurements)
            {
                DbContext.AngleMeasurements.Remove(angleMeasurement);
            }
            DbContext.Users.Remove(user);
            DbContext.SaveChanges();
            return Ok(new { UserName = user.UserName });
        }

        [HttpGet, Route("api/admin/messageusers")]
        public async Task<IHttpActionResult> MessageUsers(string MailSubject, string MailBody)
        {
            try
            {
                var emailFrom = AppSettings.ServiceEmailAddress;
                var emailTo = new List<string>();
                //emailTo.Add("raychangrhj@gmail.com");
                foreach (var user in DbContext.Users)
                {
                    emailTo.Add(user.Email);
                }
                await EmailService.SendAsync(emailFrom, emailTo, MailSubject, "", MailBody).ConfigureAwait(false);
                return Ok("Success");
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet, Route("api/admin/getspecialofferlist")]
        public IHttpActionResult GetSpecialOfferList()
        {
            try
            {
                var emailTemplates = DbContext.EmailTemplates.Where(et => et.SendCondition.StartsWith("OFFER#")).ToList();
                var emailTemplateModels = new List<EmailTemplateModel>();
                foreach(var emailTemplate in emailTemplates)
                {
                    emailTemplateModels.Add(new EmailTemplateModel()
                    {
                        TemplateName = emailTemplate.TemplateName,
                        SendCondition = emailTemplate.SendCondition.Substring(6),
                        TemplateContent = emailTemplate.TemplateContent
                    });
                }
                return Ok(emailTemplateModels);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet, Route("api/admin/getemailtemplate")]
        public IHttpActionResult GetEmailTemplate(string templateName)
        {
            try
            {
                var emailTemplate = DbContext.EmailTemplates.Where(et => et.TemplateName == templateName).FirstOrDefault();
                return Ok(emailTemplate);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost, Route("api/admin/addemailtemplate")]
        public IHttpActionResult AddEmailTemplate(EmailTemplateModel emailTemplateModel)
        {
            var emailTemplate = DbContext.EmailTemplates.Where(et => et.TemplateName == emailTemplateModel.TemplateName).FirstOrDefault();
            if (emailTemplate == null)
            {
                DbContext.EmailTemplates.Add(new EmailTemplate()
                {
                    TemplateName = emailTemplateModel.TemplateName,
                    SendCondition = emailTemplateModel.SendCondition,
                    TemplateContent = emailTemplateModel.TemplateContent
                });
            }
            else
            {
                emailTemplate.TemplateName = emailTemplateModel.TemplateName;
                emailTemplate.SendCondition = emailTemplateModel.SendCondition;
                emailTemplate.TemplateContent = emailTemplateModel.TemplateContent;
            }
            DbContext.SaveChanges();
            return Ok("OK");
        }

        [HttpPost, Route("api/admin/deleteemailtemplate")]
        public IHttpActionResult DeleteComment(string templateName)
        {
            var emailTemplate = DbContext.EmailTemplates.Where(et => et.TemplateName == templateName).SingleOrDefault();
            DbContext.EmailTemplates.Remove(emailTemplate);
            DbContext.SaveChanges();
            return Ok("OK");
        }

        [HttpPost, Route("api/admin/changeallowautoemail")]
        public IHttpActionResult ChangeAllowAutoEmail(string userId, string autoEmailType, bool allowed)
        {
            var user = DbContext.Users.Where(u => u.Id == userId).SingleOrDefault();
            if (autoEmailType.ToLower().StartsWith("training"))
            {
                user.AllowTrainingFollowUps = allowed;
            }
            else if (autoEmailType.ToLower().StartsWith("special"))
            {
                user.AllowSpecialOffers = allowed;
            }
            DbContext.SaveChanges();
            return Ok("OK");
        }

        [HttpPost, Route("api/admin/registeruser")]
        public IHttpActionResult RegisterUser(string email, string password)
        {
            if (DbContext.Users.Where(u => u.Email == email).ToList().Count() > 0)
            {
                return Ok("Email already exists");
            }
            DbContext.Users.Add(new ApplicationUser()
            {
                UserName = email,
                Email = email,
                AllowTrainingFollowUps = true,
                AllowSpecialOffers = true
            });
            try
            {
                DbContext.SaveChangesAsync();
                return Ok("Registered successfully");
            }
            catch
            {
                return Ok("Register Failed");
            }
        }

        public static string getUserRole(ApplicationDbContext dbContext)
        {
            try
            {
                var userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = dbContext.Users.Find(userId);
                var email = user.Email;
                var adminEmails = new List<string>()
                {
                    AppSettings.ServiceEmailAddress,
                    "dtkach@elasticsteel.com",
                    "raychangrhj@gmail.com"
                };
                return adminEmails.Contains(email) ? "SuperAdmin" : user.Role == null ? "User" : user.Role;
            }
            catch { }
            return "User";
        }
    }
}
