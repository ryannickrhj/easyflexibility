﻿namespace EasyFlexibilityTool.Web.Controllers.MVC
{
    using System;
    using System.Data.Entity;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Base;
    using System.Linq;
    using System.Collections.Generic;

    public class LeaderboardController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                ViewBag.IsAgreeToLeaderboard = false;
                ViewBag.Redeemed = true;
                ViewBag.RewardPoints = 0;
                ViewBag.UserRole = WebAPI.AdminController.getUserRole(DbContext);
                if (Request.IsAuthenticated)
                {
                    var userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var user = await DbContext.Users.SingleOrDefaultAsync(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
                    if (user != null)
                    {
                        ViewBag.IsAgreeToLeaderboard = user.IsAgreeToLeaderboard;
                        ViewBag.Redeemed = user.Redeemed;

                        var measurements = await DbContext.AngleMeasurements
                            .Where(am => am.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase) && am.User.IsAgreeToLeaderboard)
                            .OrderBy(am => am.DateTimeStamp)
                            .ToListAsync();
                        if (measurements.Count >= 2)
                        {
                            var diffAngle = Math.Round(measurements.Last().Angle - measurements.First().Angle);
                            if (diffAngle > 0)
                            {
                                ViewBag.RewardPoints = user.Redeemed ? 0 : diffAngle;
                            }
                        }
                        ViewBag.Redeemed = ViewBag.RewardPoints == 0 ? true : false;
                    }
                }
                return View();
            }
            catch
            {
                throw;
            }
        }
    }
}
