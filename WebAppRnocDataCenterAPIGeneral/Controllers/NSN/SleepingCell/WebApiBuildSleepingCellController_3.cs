using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                /// || item.Password.ToString().Contains(strSearchStringNeed)

            ).ToList()
            });
        }
        // ket thuc ham public ActionResult funSleepingCellOutLookSearch(string strSearchStringNeed)
        //************************************************************************
        //========================================================================


        [HttpPost]
        [Route("ControllerOutLook/Insert")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]


        public ActionResult funSleepingCellOutLookInsert(Outlook objSleepingCell_OutLook)
        {

            try
            {

                objConnectionsInformationSleepingCellDbContext.Outlooks.Add(objSleepingCell_OutLook);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();

                return Ok(new { message = "Thêm thành công", objfunInsert = objSleepingCell_OutLook });
            }
            //========================================================================

            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", details = ex.Message });
            }
            //========================================================================

        }
        // ket thuc ham public ActionResult funSleepingCellOutLookInsert(Outlook objSleepingCell_OutLook)
        //************************************************************************
        //========================================================================




        [HttpPost]
        [Route("ControllerOutLook/Insert1")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult funSleepingCellOutLook_Insert1(string strOutLookEmail, string strOutLookPass, int intSTT)
        {
            try
            {

                Outlook objSleeping_Cell_OutLook = new Outlook();
                objSleeping_Cell_OutLook.Email = strOutLookEmail;
                objSleeping_Cell_OutLook.Password = strOutLookPass;
                objSleeping_Cell_OutLook.SttEmail = intSTT;

               objConnectionsInformationSleepingCellDbContext.Outlooks.Add(objSleeping_Cell_OutLook);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { objdata = objSleeping_Cell_OutLook });
            }
            //========================================================================

            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            //========================================================================

            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database" );
            }
            //========================================================================

            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            //========================================================================

            catch (Exception)
            {
                // Log chi tiết, nhưng chỉ trả về message chung

                return StatusCode(500, "Internal server error");
            }
            

        }
        // ket thuc ham public ActionResult funSleepingCellOutLookInsert(Outlook objSleepingCell_OutLook)
        //************************************************************************
        //========================================================================



        [HttpPost]
        [Route("ControllerOutLook/Update")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellOutLookUpdate(int intID, int intSTT, string strOutLookEmail, string strOutLookPass, Boolean strActiveOutlookEmail)
        {
            try
            {
                Outlook objSleepingCellOutLook = new Outlook();
                objSleepingCellOutLook.Id = intID;
                objSleepingCellOutLook.Email = strOutLookEmail;
                objSleepingCellOutLook.Password = strOutLookPass;
                objSleepingCellOutLook.SttEmail = intSTT;
                objSleepingCellOutLook.Active = strActiveOutlookEmail;

                objConnectionsInformationSleepingCellDbContext.Outlooks.Update(objSleepingCellOutLook);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { objdata = objSleepingCellOutLook });

            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            //========================================================================
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            //========================================================================

        }
        // ket thuc ham public ActionResult funSleepingCellOutLookInsert(Outlook objSleepingCell_OutLook)
        //************************************************************************
        //========================================================================


        [HttpPost]
        [Route("ControllerOutLook/Delete")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellOutLook_Delete(int intID_Destro)
        {
            try
            {
                Outlook objSleepingCellOutLook = new Outlook();
                objSleepingCellOutLook.Id = intID_Destro;

                objConnectionsInformationSleepingCellDbContext.Outlooks.Remove(objSleepingCellOutLook);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { objdata = objSleepingCellOutLook });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            //========================================================================
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            //========================================================================

            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database");
            }

            catch (Exception)
            {
                // Log chi tiết, nhưng chỉ trả về message chung

                return StatusCode(500, "Internal server error");
            }


        }
        // ket thuc ham public ActionResult funSleepingCellOutLook_Delete(int intID_Destro)
        //************************************************************************
        //========================================================================







    }
}// ket thuc public class WebAPIBuildSleepingCellController : ControllerBase
