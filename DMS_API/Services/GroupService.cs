using ArchiveAPI.Services;
using DMS_API.Models;
using DMS_API.ModelsView;
using System.Data;

namespace DMS_API.Services
{
    public class GroupService
    {
        #region Properteis
        private readonly DataAccessService dam;
        private SessionService Session_S { get; set; }
        private DataTable dt { get; set; }
        private UserModel User_M { get; set; }
        private List<UserModel> User_Mlist { get; set; }
        private ResponseModelView Response_MV { get; set; }
        #endregion
    }
}
