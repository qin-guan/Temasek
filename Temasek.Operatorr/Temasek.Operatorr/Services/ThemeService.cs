namespace Temasek.Operatorr.Services;

public class ThemeService
{
    private const string CookieName = "theme";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public string PreferredTheme => _httpContextAccessor.HttpContext?.Request.Query[CookieName] ??
                                    _httpContextAccessor.HttpContext?.Request.Cookies[CookieName] ??
                                    "light";

    public ThemeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        if (!string.IsNullOrWhiteSpace(_httpContextAccessor.HttpContext?.Request.Query[CookieName]))
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                CookieName,
                _httpContextAccessor.HttpContext?.Request.Query[CookieName] 
            );
        }
    }
}