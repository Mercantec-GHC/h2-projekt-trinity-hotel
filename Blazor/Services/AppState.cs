using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.IdentityModel.JsonWebTokens;




    public class AppState
    {
        private bool _loggedIn;
        private int _userId;
        private bool _isAdmin;

        public event Action? OnChange;

        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                if (_loggedIn != value)
                {
                    _loggedIn = value;
                    NotifyStateChanged();
                }
            }
        }

        public int UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    NotifyStateChanged();
                }
            }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    NotifyStateChanged();
                }
            }
        }

        public void Logout()
        {
            LoggedIn = false;
            UserId = 0;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task InitializeStateAsync(IJSRuntime JSRuntime)
        {
            var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                var claims = DecodeToken(token);
                LoggedIn = true;
                UserId = claims.UserId;
                IsAdmin = claims.IsAdmin;
            }
            else
            {
                LoggedIn = false;
                UserId = 0;
                IsAdmin = false;
            }
            NotifyStateChanged();
        }

        private (int UserId, bool IsAdmin) DecodeToken(string token)
        {
            var handler = new JsonWebTokenHandler();
            var jsonToken = handler.ReadJsonWebToken(token);

            var userIdClaim = jsonToken.GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            var roleClaim = jsonToken.GetClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            int userId = int.Parse(userIdClaim?.Value ?? "0");
            bool isAdmin = roleClaim?.Value == "Administrator";

            return (userId, isAdmin);
        }
    public int LoggedInUserId { get; set; }

}

