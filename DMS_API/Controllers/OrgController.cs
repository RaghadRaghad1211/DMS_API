using DMS_API.ModelsView;
using DMS_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgController : ControllerBase
    {
        #region Properteis
        private OrgService Org_S;
        private ResponseModelView Response_MV { get; set; }
        #endregion

        #region Constructor
        public OrgController()
        {
           Org_S = new OrgService();
        }
        #endregion

        #region Actions
        [HttpGet]
        [Route("GetOrgsParentWithChilds")]
        public async Task<IActionResult> GetOrgsParentWithChilds([FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.GetOrgsParentWithChilds(RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetOrgsParentWithChilds_General")]
        public async Task<IActionResult> GetOrgsParentWithChilds_General([FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.GetOrgsParentWithChilds(RequestHeader,true);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("GetOrgsParentWithChilds_Table")]
        public async Task<IActionResult> GetOrgsParentWithChilds_Table([FromBody] PaginationModelView Pagination_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.GetOrgsParentWithChilds_Table(Pagination_MV,RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("GetOrgByID/{OrgID}")]
        public async Task<IActionResult> GetOrgByID([FromRoute] int OrgID, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.GetOrgByID(OrgID,RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpGet]
        [Route("SearchOrgsByArName/{OrgArName}")]
        public async Task<IActionResult> SearchOrgsByArName([FromRoute] string OrgArName, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.SearchOrgsByArName(OrgArName, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPost]
        [Route("AddOrg")]
        public async Task<IActionResult> AddOrg([FromBody] AddOrgModelView AddOrg_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.AddOrg(AddOrg_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }

        [HttpPut]
        [Route("EditOrg")]
        public async Task<IActionResult> EditOrg([FromBody] EditOrgModelView EditOrg_MV, [FromHeader] RequestHeaderModelView RequestHeader)
        {
            Response_MV = await Org_S.EditOrg(EditOrg_MV, RequestHeader);
            return Response_MV.Success == true ? Ok(Response_MV) : StatusCode((int)Response_MV.Data, Response_MV);
        }
        #endregion
    }
}