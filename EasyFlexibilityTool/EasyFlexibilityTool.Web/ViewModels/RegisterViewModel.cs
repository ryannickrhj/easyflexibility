namespace EasyFlexibilityTool.Web.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [StringLength(254)]
        public virtual string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Side Splits Beginner")]
        public bool SideSplitsBeginner { get; set; }

        [Display(Name = "Side Splits Intermediate")]
        public bool SideSplitsIntermediate { get; set; }

        [Display(Name = "Side Splits Advanced")]
        public bool SideSplitsAdvanced { get; set; }

        [Display(Name = "True Front Splits Beginner")]
        public bool TrueFrontSplitsBeginner { get; set; }

        [Display(Name = "True Front Splits Intermediate")]
        public bool TrueFrontSplitsIntermediate { get; set; }

        [Display(Name = "True Front Splits Advanced")]
        public bool TrueFrontSplitsAdvanced { get; set; }

        [Display(Name = "Open Front Splits Beginner")]
        public bool OpenFrontSplitsBeginner { get; set; }

        [Display(Name = "Open Front Splits Intermediate")]
        public bool OpenFrontSplitsIntermediate { get; set; }

        [Display(Name = "Open Front Splits Advanced")]
        public bool OpenFrontSplitsAdvanced { get; set; }

        [Display(Name = "Other")]
        public string Other { get; set; }
    }
}
