using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        #region Properteis
        private DocumentService Document_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public DocumentController()
        {
            Document_S = new DocumentService();
        }
        #endregion

        #region Actions

        #endregion
    }
}
