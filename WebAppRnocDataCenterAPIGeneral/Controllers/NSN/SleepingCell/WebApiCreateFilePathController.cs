using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

namespace WebAppRnocDataCenterAPIGeneral.Controllers.NSN.SleepingCell
{
    [Route("API/Systems/NSN/SleepingCell")]
    [ApiController]

    public class WebApiBuildSleepingCellFilePathController : ControllerBase
    {
        readonly ConnectionsInformationSleepingCellDbContext objConnectionsInformationSleepingCellDbContext;

        public WebApiBuildSleepingCellFilePathController(ConnectionsInformationSleepingCellDbContext ConnectionsInformationSleepingCellDbContext_Process)
        {
            objConnectionsInformationSleepingCellDbContext = ConnectionsInformationSleepingCellDbContext_Process;
        }

        //************************************************************************
        //========================================================================
        [HttpGet]
        [Route("ControllerFilePath/GetList")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellFilePathRead()
        {
            try
            {
                // Thay đổi từ Outlooks sang FilePaths khi có model FilePath
                var filePaths = objConnectionsInformationSleepingCellDbContext.Tablefilepaths.ToList();
                return Ok(new { objfunSleeping_CellFilePathReadGetList = filePaths });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        // ket thuc ham public ActionResult funSleepingCellFilePathRead()
        //************************************************************************
        //========================================================================

        [HttpGet]
        [Route("ControllerFilePath/Search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellFilePathSearch(string strSearchStringNeed)
        {
            try
            {
                return Ok(new
                {
                    objfunSearch = objConnectionsInformationSleepingCellDbContext.Tablefilepaths
                        .Where(item => item.Oss.Contains(strSearchStringNeed) ||
                                      item.Username.Contains(strSearchStringNeed) ||
                                      item.Host.Contains(strSearchStringNeed) ||
                                      item.Protocol.Contains(strSearchStringNeed) ||
                                      item.Filepath.Contains(strSearchStringNeed))
                        .ToList()
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        // ket thuc ham public ActionResult funSleepingCellFilePathSearch(string strSearchStringNeed)
        //************************************************************************
        //========================================================================

        [HttpPost]
        [Route("ControllerFilePath/Insert")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellFilePathInsert(Tablefilepath objSleepingCell_FilePath)
        {
            try
            {
                objConnectionsInformationSleepingCellDbContext.Tablefilepaths.Add(objSleepingCell_FilePath);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { message = "Thêm thành công", objfunInsert = objSleepingCell_FilePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", details = ex.Message });
            }
        }
        // ket thuc ham public ActionResult funSleepingCellFilePathInsert(FilePath objSleepingCell_FilePath)
        //************************************************************************
        //========================================================================

        [HttpPost]
        [Route("ControllerFilePath/Insert1")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellFilePath_Insert1(int intSTT, string strOSS, string strUser, string strPassword,
            string strHost, int intPort, string strFilePath, string strProtocol, bool strActive)
        {
            try
            {
                Tablefilepath objSleeping_Cell_FilePath = new Tablefilepath();
                objSleeping_Cell_FilePath.SttFilePath = intSTT;
                objSleeping_Cell_FilePath.Oss = strOSS;
                objSleeping_Cell_FilePath.Username = strUser;
                objSleeping_Cell_FilePath.Password = strPassword;
                objSleeping_Cell_FilePath.Host = strHost;
                objSleeping_Cell_FilePath.Port = intPort;
                objSleeping_Cell_FilePath.Filepath = strFilePath;
                objSleeping_Cell_FilePath.Protocol = strProtocol;
                objSleeping_Cell_FilePath.Active = strActive;

                objConnectionsInformationSleepingCellDbContext.Tablefilepaths.Add(objSleeping_Cell_FilePath);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { objdata = objSleeping_Cell_FilePath });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        // ket thuc ham public ActionResult funSleepingCellFilePath_Insert1
        //************************************************************************
        //========================================================================

        [HttpPost]
        [Route("ControllerFilePath/Update")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellFilePathUpdate(int intID, int intSTT, string strOSS, string strUser,
            string strPassword, string strHost, int intPort, string strFilePath, string strProtocol, bool strActive)
        {
            try
            {
                Tablefilepath objSleepingCellFilePath = new Tablefilepath();
                objSleepingCellFilePath.Id = intID;
                objSleepingCellFilePath.SttFilePath = intSTT;
                objSleepingCellFilePath.Oss = strOSS;
                objSleepingCellFilePath.Username = strUser;
                objSleepingCellFilePath.Password = strPassword;
                objSleepingCellFilePath.Host = strHost;
                objSleepingCellFilePath.Port = intPort;
                objSleepingCellFilePath.Filepath = strFilePath;
                objSleepingCellFilePath.Protocol = strProtocol;
                objSleepingCellFilePath.Active = strActive;

                objConnectionsInformationSleepingCellDbContext.Tablefilepaths.Update(objSleepingCellFilePath);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { objdata = objSleepingCellFilePath });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        // ket thuc ham public ActionResult funSleepingCellFilePathUpdate
        //************************************************************************
        //========================================================================

        [HttpPost]
        [Route("ControllerFilePath/Delete")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult funSleepingCellFilePath_Delete(int intID_Destro)
        {
            try
            {
                Tablefilepath objSleepingCellFilePath = new Tablefilepath();
                objSleepingCellFilePath.Id = intID_Destro;

                objConnectionsInformationSleepingCellDbContext.Tablefilepaths.Remove(objSleepingCellFilePath);
                objConnectionsInformationSleepingCellDbContext.SaveChanges();
                return Ok(new { objdata = objSleepingCellFilePath });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Thao tác không hợp lệ: {ex.Message}");
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        // ket thuc ham public ActionResult funSleepingCellFilePath_Delete(int intID_Destro)
        //************************************************************************
        //========================================================================

    }
}// ket thuc public class FilePathController : ControllerBase