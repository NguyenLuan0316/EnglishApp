using System.ComponentModel.DataAnnotations;

namespace WordWave.Api.Models.Requests;

public class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128)]
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 2), RegularExpression(@".*\S.*")]
    public string DisplayName { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = "";
}

public class ChangePasswordRequest
{
    [Required, StringLength(128)]
    public string CurrentPassword { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 10)]
    public string NewPassword { get; set; } = "";
}

public class UserAccessRequest
{
    [Required, RegularExpression("^(Admin|Learner)$")]
    public string Role { get; set; } = "";
    [Required]
    public bool? IsActive { get; set; }
}

public class AdminCreateUserRequest : RegisterRequest
{
    [Required, RegularExpression("^(Admin|Learner)$")]
    public string Role { get; set; } = "Learner";
    public bool IsActive { get; set; } = true;
}

public class AdminUpdateUserRequest : UserAccessRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 2), RegularExpression(@".*\S.*")]
    public string DisplayName { get; set; } = "";
}

public class AdminResetPasswordRequest
{
    [Required, StringLength(128, MinimumLength = 10)]
    public string NewPassword { get; set; } = "";
}
