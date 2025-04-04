using Jumia_Clone.Models.DTOs.AuthenticationDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IAuthRepo
    {
        bool Login(LoginDTO loginData);
        

        
    }
}
