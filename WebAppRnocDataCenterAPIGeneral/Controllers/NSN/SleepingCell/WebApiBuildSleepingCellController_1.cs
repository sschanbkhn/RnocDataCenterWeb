using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
/// using WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

using WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [Route("API/Systems/NSN/SleepingCell")]
    [ApiController]

    public class WebApiBuildSleepingCellController : ControllerBase
    {

        readonly ConnectionsInformationSleepingCellDbContext objConnectionsInformationSleepingCellDbContext;
        
        public WebApiBuildSleepingCellController(ConnectionsInformationSleepingCellDbContext ConnectionsInformationSleepingCellDbContext_Process)
        {

            objConnectionsInformationSleepingCellDbContext = ConnectionsInformationSleepingCellDbContext_Process;
        }
        //************************************************************************
        //========================================================================

        [HttpGet]
        [Route("ControllerOutLook/GetList")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellsOutLookRead()
        {
            /// return Ok(new { objfunCellFaultsOutLookReadGetList = objConnectionsInformationSleepingCellDbContext.Outlooks.ToList() });
            
            try
            {
                var outlooks = objConnectionsInformationSleepingCellDbContext.Outlooks.ToList();
                return Ok(new { objfunCellFaultsOutLookReadGetList = outlooks });
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving outlook data", error = ex.Message });
            }
            //========================================================================
        }
        // ket thuc ham public ActionResult funSleepingCellsOutLookRead()
        //************************************************************************
        //========================================================================


        [HttpGet]
        [Route("ControllerOutLook/Search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellOutLookSearch(string strSearchStringNeed)
        {
            return Ok(new
            {
                objfunSearch = objConnectionsInformationSleepingCellDbContext.Outlooks.Where(item => item.Email.Contains(strSearchStringNeed)
                || item.Password.ToString().Contains(strSearchStringNeed)

            ).ToList()
            });
        }
        // ket thuc ham public ActionResult funSleepingCellOutLookSearch(string strSearchStringNeed)
        //************************************************************************
        //========================================================================


        [HttpPost]
        [Route("ControllerCellFaultsOutLook/Insert")]
        public ActionResult funInsert(Outlook objSleepingCell_OutLook)
        {

            /*

            objCellFaultsOutLook.Id = 0;
            objDataBaseKetNoiCSDLCellFaultsManagementApp.CellFaultsOutLooks.Add(objCellFaultsOutLook);
            objDataBaseKetNoiCSDLCellFaultsManagementApp.SaveChanges();
            return Ok(new { objfunInsert = objDataBaseKetNoiCSDLCellFaultsManagementApp });

            */

            try
            {
                // objCellFaultsOutLook.Id = 0; // ✅ Đảm bảo IDENTITY không bị truyền vào
                // if(objCellFaultsOutLook.Id == null || objCellFaultsOutLook.Id.ToString() == "")
                // {
                // objCellFaultsOutLook.Id = 0;
                // }

                objConnectionsInformationSleepingCellDbContext.Outlooks.Add(objSleepingCell_OutLook);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();

                return Ok(new { message = "Thêm thành công", objfunInsert = objSleepingCell_OutLook });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", details = ex.Message });
            }

        }














    }
}// ket thuc public class WebAPIBuildSleepingCellController : ControllerBase
